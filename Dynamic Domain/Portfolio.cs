using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using StarSimulator.Annotations;
using System;
using Newtonsoft.Json;

namespace StarSimulator
{
    public class Portfolio : NotifyBase
    {
        Star _star;
        private Dictionary<Process, Industry> _industries;
        [JsonIgnore]
        public Dictionary<Process, Industry> Industries
        {
            get { return _industries; }
            set
            {
                if (_industries == value) return;
                _industries = value;
                OnPropertyChanged();
                OnPropertyChanged("RawIndustries");
                OnPropertyChanged("CompositeIndustries");
            }
        }

        [JsonProperty("Industries")]
        public Dictionary<string, Industry> IndustriesByName
        {
            get { return _industries.Values.ToDictionary(x => x.SupportingProcess.Name); }
        }

        [JsonIgnore]
        public IEnumerable<Industry> RawIndustries
        {
            get
            {
                return Industries.Values.Where(x => x.SupportingProcess.IsRaw);
            }
        }
        [JsonIgnore]
        public IEnumerable<Industry> CompositeIndustries
        {
            get
            {
                return Industries.Values.Where(x => x.SupportingProcess.IsComposite);
            }
        }

        public void Invest(Timestamp at, Industry industry, decimal Amount)
        {
            var industryCoefficients = new Dictionary<Industry, decimal>();
            var industryStack = new Stack<Industry>();
            industryStack.Push(industry);
            industryCoefficients[industry] = 1;
            while(industryStack.Count > 0)
            {
                var current = industryStack.Pop();
                var coeff = industryCoefficients[current];
                foreach (var input in current.SupportingProcess.Inputs)
                {
                    if (input.Key.Raw) continue;
                    var inputProcess = input.Key.Producers.SingleOrDefault();
                    if (inputProcess == null) continue;
                    var nextIndustry = _industries[inputProcess];
                    industryStack.Push(nextIndustry);
                    if (!industryCoefficients.ContainsKey(nextIndustry))
                        industryCoefficients[nextIndustry] = 0;
                    industryCoefficients[nextIndustry] += coeff * input.Value;
                }
            }
            var totalCoeff = industryCoefficients.Values.Sum();
            var baseAmount = Amount / totalCoeff;
            foreach(var i in industryCoefficients)
            {
                var currValue = i.Key.Investment.At(at).Value;
                i.Key.Investment.Update(at, currValue + baseAmount * i.Value);
                i.Key.RaiseValueChanged();
            }
            var duplicates = _star.actions.OfType<InvestAction>().Where(x => x.At == at && x.Process == industry.SupportingProcess).ToList();
            foreach (var dup in duplicates) _star.actions.Remove(dup);
            _star.actions.Add(new InvestAction(at, Amount, industry.SupportingProcess));
            _star.Rebudget(at);
        }

        public Portfolio(Portfolio old, EconomyDefinition econ, Timestamp start, Star star)
        {
            Industries = econ.AllProcesses.Select(x =>
            {
                Industry oldIndustry;
                if(old.Industries.TryGetValue(x, out oldIndustry))
                {
                    return new Industry(this)
                    {
                        Investment = new FirstOrderHistory(start, oldIndustry.Investment.First.Value, oldIndustry.Investment.First.rateOfChange),
                        SupportingProcess = x,
                    };
                }
                return new Industry(this)
                {
                    Investment = new FirstOrderHistory(start, 0, Rate.Zero()),
                    SupportingProcess = x,
                };
            }).ToDictionary(x => x.SupportingProcess, new ProcessComparer());
            _star = star;
        }

        public Portfolio()
        {
            Industries = new Dictionary<Process, Industry>();
        }
    }
}

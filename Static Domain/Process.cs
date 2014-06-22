using System;
using System.Collections.Generic;
using System.Linq;

namespace StarSimulator
{
    public class ProcessComparer : IComparer<Process>, IEqualityComparer<Process>
    {
        public int Compare(Process x, Process y)
        {
            return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }

        public bool Equals(Process x, Process y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Process obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class Process
    {
        public Dictionary<Resource, decimal> Inputs { get; set; }
        public Dictionary<Resource, decimal> Outputs { get; set; }

        public bool IsRaw { get { return Inputs.Count <= 1 && Outputs.Count >= 1; } }
        public bool IsComposite {  get { return Inputs.Count >= 1 && Outputs.Count <= 1; } }

        public string Name { get; set; }

        public string Formula
        {
            get
            {
                return string.Join(" + ", Inputs.Select(x => x.Value + " x " + x.Key)) + " => " + string.Join(" + ", Outputs.Select(x => x.Value + " x " + x.Key));
            }
        }

        public override string ToString()
        {
            return Name;
        }
        public Process(string name, IEnumerable<Tuple<Resource, decimal>> inputs, IEnumerable<Tuple<Resource, decimal>> outputs)
        {
            Name = name;
            Inputs = inputs
                        .GroupBy(x => x.Item1)
                        .ToDictionary(x => x.Key,
                                      x => x.Sum(y => y.Item2));
            Outputs = outputs
                        .GroupBy(x => x.Item1)
                        .ToDictionary(x => x.Key,
                                      x => x.Sum(y => y.Item2));
        }
    }
}

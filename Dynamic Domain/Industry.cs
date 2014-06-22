using Newtonsoft.Json;
using System;

namespace StarSimulator
{
    public class Industry : NotifyBase
    {
        private Portfolio _portfolio;
        public Industry(Portfolio portfolio)
        {
            _portfolio = portfolio;
            Timestamp.TimeManager.PropertyChanged += TimeUpdated;
        }

        private void TimeUpdated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CurrentValue");
            OnPropertyChanged("CurrentRate");
        }

        [JsonIgnore]
        public Process SupportingProcess { get; set; }
        [JsonIgnore]
        public FirstOrderHistory Investment { get; set; }


        // For data binding and json serialization
        [JsonProperty("CurrentInvestment")]
        public decimal CurrentValue
        {
            get { return Investment.Current.Value; }
            set
            {
                _portfolio.Invest(Timestamp.Now(), this, value - Investment.Current.Value);
            }
        }

        public void RaiseValueChanged()
        {
            OnPropertyChanged("CurrentValue");
        }

        [JsonIgnore]
        public decimal CurrentRate
        {
            get { return Investment.Current.rateOfChange.PerTick; }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}

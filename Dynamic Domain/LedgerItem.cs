using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace StarSimulator
{
    public class LedgerItem : NotifyBase
    {
        Inventory _inventory;
        public LedgerItem(Inventory inv)
        {
            Timestamp.TimeManager.PropertyChanged += TimeUpdated;
            _inventory = inv;
        }

        private void TimeUpdated(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CurrentValue");
            OnPropertyChanged("CurrentRate");
        }

        private Resource _resource;
        [JsonIgnore]
        public Resource Resource
        {
            get { return _resource; }
            set
            {
                if (_resource == value) return;
                _resource = value;
                OnPropertyChanged();
            }
        }
        private FirstOrderHistory _quantity;
        [JsonIgnore]
        public FirstOrderHistory Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public void RaiseValueChanged()
        {
            OnPropertyChanged("CurrentValue");
            OnPropertyChanged("CurrentRate");
        }

        // For data binding and json serialization
        [JsonProperty("CurrentQuantity")]
        public decimal CurrentValue
        {
            get { return _quantity.Current.Value; }
            set
            {
                _quantity.Update(Timestamp.Now(), value - _quantity.Current.Value);
                OnPropertyChanged();
            }
        }

        public decimal CurrentRate
        {
            get { return _quantity.Current.rateOfChange.PerTick; }
            set
            {
                _inventory.Import(Timestamp.Now(), _resource, value / Duration.Epsilon());
            }
        }
    }
}
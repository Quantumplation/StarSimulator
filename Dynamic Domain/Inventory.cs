using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using StarSimulator.Annotations;
using System.Runtime.CompilerServices;
using System;
using Newtonsoft.Json;

namespace StarSimulator
{
    public class Inventory : NotifyBase
    {
        Star _star;
        private Dictionary<Resource, LedgerItem> _ledgerItems;
        [JsonIgnore]
        public Dictionary<Resource, LedgerItem> LedgerItems
        {
            get { return _ledgerItems; }
            set
            {
                if (_ledgerItems == value) return;
                _ledgerItems = value;
                OnPropertyChanged();
                OnPropertyChanged("Raw");
                OnPropertyChanged("Component");
                OnPropertyChanged("Product");
                OnPropertyChanged("ResourceQuantities");
            }
        }

        [JsonProperty("LedgerItems")]
        public Dictionary<string, LedgerItem> LedgerItemsByName
        {
            get { return _ledgerItems.Values.ToDictionary(x => x.Resource.Name); }
        }

        [JsonIgnore]
        public IEnumerable<LedgerItem> Raw
        {
            get { return _ledgerItems.Values.Where(x => x.Resource.Raw); }
        }
        [JsonIgnore]
        public IEnumerable<LedgerItem> Component
        {
            get { return _ledgerItems.Values.Where(x => x.Resource.Component); }
        }
        [JsonIgnore]
        public IEnumerable<LedgerItem> Product
        {
            get { return _ledgerItems.Values.Where(x => x.Resource.Product); }
        }

        public Inventory(Inventory old, EconomyDefinition newEcon, Timestamp start, Star star)
        {
            LedgerItems = newEcon.Resources.Values.Select(x =>
            {
                LedgerItem oldLedgerItem;
                if(old.LedgerItems.TryGetValue(x, out oldLedgerItem))
                {
                    // If it existed before, we roll back to the "initial state", to promote an iterative workflow
                    // Simulate, tweak, import, watch the same scenario again
                    return new LedgerItem(this)
                    {
                        Resource = x,
                        Quantity = new FirstOrderHistory(start, oldLedgerItem.Quantity.First.Value, oldLedgerItem.Quantity.First.rateOfChange),
                    };
                }
                else
                {
                    return new LedgerItem(this)
                    {
                        Resource = x,
                        Quantity = new FirstOrderHistory(start, 0, Rate.Zero()),
                    };
                }
            }).ToDictionary(x => x.Resource, new ResourceComparer());
            _star = star;
        }

        public void Import(Timestamp at, Resource resource, Rate rate)
        {
            _ledgerItems[resource].Quantity.Update(at, rate);
            var duplicates = _star.actions.OfType<ImportAction>().Where(x => x.At == at && x.Resource == resource).ToList();
            foreach (var dup in duplicates) _star.actions.Remove(dup);
            _star.actions.Add(new ImportAction(at, rate, resource));
            _star.Rebudget(at);
        }

        public Inventory()
        {
            LedgerItems = new Dictionary<Resource, LedgerItem>(new ResourceComparer());
        }
    }
}

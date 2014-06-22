using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using StarSimulator.Annotations;
using System;
using System.Linq;

namespace StarSimulator
{
    public class Star : NotifyBase
    {
        private EconomyDefinition _economy;
        private Portfolio _portfolio;
        private Inventory _inventory;

        public TimeManager TimeManager { get; private set; }

        public EconomyDefinition Economy
        {
            get { return _economy; }
            set
            {
                if (Equals(value, _economy)) return;
                _economy = value;
                OnPropertyChanged();

                var now = Timestamp.Now();
                Portfolio = new Portfolio(_portfolio, _economy, now, this);
                Inventory = new Inventory(_inventory, _economy, now, this);
                ReplayActions(actions);
            }
        }

        public Portfolio Portfolio
        {
            get { return _portfolio; }
            set
            {
                if (Equals(value, _portfolio)) return;
                _portfolio = value;
                OnPropertyChanged();
            }
        }

        public Inventory Inventory
        {
            get { return _inventory; }
            set
            {
                if (Equals(value, _inventory)) return;
                _inventory = value;
                OnPropertyChanged();
            }
        }

        internal List<StarAction> actions = new List<StarAction>();
        public IEnumerable<StarAction> Actions
        {
            get
            {
                return actions.OrderBy(x => x.At);
            }
        }

        public void ReplayActions(IEnumerable<StarAction> acts)
        {
            foreach (var action in acts.OrderBy(x => x.At))
            {
                action.Do(_portfolio, _inventory);
            }
            OnPropertyChanged("Actions");
            TimeManager.Pulse();
        }

        internal void Rebudget(Timestamp at)
        {
            foreach (var process in _economy.TopologicalComposites)
            {
                var investment = _portfolio.Industries[process].Investment.At(at).Value;
                var bottleneck = investment / Duration.Epsilon();
                foreach (var input in process.Inputs)
                {
                    var inputRate = _inventory.LedgerItems[input.Key].Quantity.At(at).rateOfChange;
                    if (inputRate / input.Value < bottleneck) bottleneck = inputRate / input.Value;
                }
                if (bottleneck.PerTick == 0) continue;
                foreach (var input in process.Inputs)
                {
                    var ledgerItem = _inventory.LedgerItems[input.Key];
                    ledgerItem.Quantity.Update(at, ledgerItem.Quantity.At(at).rateOfChange - bottleneck * input.Value);
                }
                foreach (var output in process.Outputs)
                {
                    var ledgerItem = _inventory.LedgerItems[output.Key];
                    ledgerItem.Quantity.Update(at, ledgerItem.Quantity.At(at).rateOfChange + bottleneck * output.Value);
                }
            }
            foreach (var item in _inventory.LedgerItems)
            {
                item.Value.RaiseValueChanged();
            }
            OnPropertyChanged("Actions");
        }

        public Star()
        {
            TimeManager = Timestamp.TimeManager;
            _economy = new EconomyDefinition();
            _inventory = new Inventory();
            _portfolio = new Portfolio();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSimulator
{
    public abstract class StarAction
    {
        public abstract string Description { get; }
        public Timestamp At { get; set; }
        public abstract void Do(Portfolio p, Inventory i);
    }

    public class InvestAction : StarAction
    {
        public override string Description
        {
            get
            {
                return String.Format("Invested {0} in {1} at T={2}", Amount, Process.Name, At.Ticks);
            }
        }

        public InvestAction(Timestamp t, decimal amount, Process proc)
        {
            At = t;
            Amount = amount;
            Process = proc;
        }

        public readonly decimal Amount;
        public readonly Process Process;

        public override void Do(Portfolio p, Inventory i)
        {
            p.Invest(At, p.Industries[Process], Amount);
        }
    }

    public class ImportAction : StarAction
    {
        public override string Description
        {
            get
            {
                return String.Format("Importing {0} units of {1} per tick as of T={2}", Rate.PerTick, Resource.Name, At.Ticks);
            }
        }

        public ImportAction(Timestamp t, Rate rate, Resource r)
        {
            At = t;
            Rate = rate;
            Resource = r;
        }

        public readonly Rate Rate;
        public readonly Resource Resource;

        public override void Do(Portfolio p, Inventory i)
        {
            i.Import(At, Resource, Rate);
        }
    }
}

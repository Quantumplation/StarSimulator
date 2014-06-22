using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSimulator
{
    public class FirstOrderHistory : NotifyBase
    {
        public class Snapshot
        {
            public readonly Timestamp Time;
            public readonly decimal Value;
            public readonly Rate rateOfChange;

            public Snapshot(Timestamp time, decimal val, Rate r)
            {
                Time = time;
                Value = val;
                rateOfChange = r;
            }
            
            public decimal Project(Duration d)
            {
                return Value + rateOfChange * d;
            }
        }

        public readonly SortedList<Timestamp, Snapshot> Moments;

        public FirstOrderHistory(Timestamp initialTime, decimal initialValue, Rate initialRate)
        {
            Moments = new SortedList<Timestamp, Snapshot>
            {
                { initialTime, new Snapshot(initialTime, initialValue, initialRate) }
            };
        }

        public Snapshot At(Timestamp t)
        {
            var current = Moments.First();
            
            foreach(var c in Moments)
            {
                if (c.Key > t) break;
                current = c;
            }

            return new Snapshot(t, current.Value.Project(t - current.Key), current.Value.rateOfChange);
        }

        public void Update(Timestamp t, Rate r)
        {
            Sanitize(t);
            var value = Latest.Project(t - Latest.Time);
            Update(t, value, r);
        }

        public void Update(Timestamp t, decimal value)
        {
            Sanitize(t);
            Update(t, value, Latest.rateOfChange);
        }

        public void Update(Timestamp t, decimal value, Rate r)
        {
            Sanitize(t, false);
            Moments.Add(t, new Snapshot(t, value, r));
            OnPropertyChanged("Latest");
        }

        private void Sanitize(Timestamp t, bool includeMin = true)
        {
            if (Latest.Time >= t)
            {
                // Clear out the times past this.  We're just making this
                // easy for the editor, in the game you wouldn't be able to accidentally ovewrrite the past.
                var futureMoments = Moments.Keys.Where(x => x >= t).ToList();
                futureMoments.ForEach(x => Moments.Remove(x));
            }
            if (Moments.Count == 0 && includeMin)
            {
                // We cleared out all history, so add back the 0,0 state
                Moments.Add(Timestamp.Min(), new Snapshot(Timestamp.Min(), 0, Rate.Zero()));
            }
        }

        public Snapshot Current
        {
            get
            {
                return At(Timestamp.Now());
            }
        }

        public Snapshot Latest
        {
            get { return Moments.Last().Value; }
        }

        public Snapshot First
        {
            get { return Moments.First().Value; }
        }
    }
}

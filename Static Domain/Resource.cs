using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StarSimulator
{
    public class ResourceComparer : IComparer<Resource>, IEqualityComparer<Resource>
    {
        public int Compare(Resource x, Resource y)
        {
            return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }

        public bool Equals(Resource x, Resource y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Resource obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class Resource
    {
        public string Name { get; set; }

        [JsonIgnore]
        public List<Process> Producers { get; set; }
        [JsonIgnore]
        public List<Process> Consumers { get; set; }

        public IEnumerable<string> ProducerNames { get { return Producers.Select(x => x.Name); } }
        public IEnumerable<string> ConsumerNames { get { return Consumers.Select(x => x.Name); } }

        public bool Raw { get { return Producers.Count == 0 ^ Producers.All(x => x.IsRaw); } }
        public bool Product { get { return Consumers.Count == 0; } }
        public bool Component { get { return !Raw && !Product; } }

        public Resource()
        {
            Producers = new List<Process>();
            Consumers = new List<Process>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

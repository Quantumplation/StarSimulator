using System;
using System.Collections.Generic;
using System.Linq;

namespace StarSimulator
{
    public class EconomyDefinition
    {
        public Dictionary<string, Resource> Resources;
        public Dictionary<string, Process> Processes;

        public EconomyDefinition()
        {
            Resources = new Dictionary<string, Resource>();
            Processes = new Dictionary<string, Process>();
        }

        public IEnumerable<Process> AllProcesses
        {
            get { return Processes.Values; }
        }

        public IEnumerable<Process> TopologicalComposites
        {
            get
            {
                var ordering = new Dictionary<Process, int>();
                foreach (var process in Processes.Values.Where(x => x.IsComposite))
                {
                    TopologicalSort(process, 0, ordering);
                }
                return ordering.OrderByDescending(x => x.Value).Select(x => x.Key);
            }
        }


        private void TopologicalSort(Process p, int depth, Dictionary<Process, int> ordering)
        {
            if (!ordering.ContainsKey(p)) ordering[p] = 0;
            ordering[p] = Math.Max(ordering[p], depth);
            foreach (var input in p.Inputs.Keys)
            {
                if (input.Raw) continue; // stop looking at the raw boundary
                if (input.Producers.Count == 0) continue;
                var other = input.Producers.Single();
                TopologicalSort(other, depth + 1, ordering);
            }
        }
    }
}

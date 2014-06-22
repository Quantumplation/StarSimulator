using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StarSimulator
{
    public static class Parser
    {
        public static EconomyDefinition Parse(string rawFile, string compositeFile)
        {
            var econDef = new EconomyDefinition();

            // Open the raw file
            var rawFileContents = File.ReadAllText(rawFile);
            var compositeFileContents = File.ReadAllText(compositeFile);

            ParseFile(rawFileContents, econDef,
                (name, energy, inputs, outputs) =>
                    econDef.Processes.Add(name, new Process(name, inputs, outputs)));
            ParseFile(compositeFileContents, econDef,
                (name, energy, inputs, outputs) => 
                    econDef.Processes.Add(name, new Process(name, inputs, outputs)));

            foreach (var process in econDef.AllProcesses)
            {
                foreach (var resource in process.Inputs.Keys.Where(x => x != null))
                    resource.Consumers.Add(process);
                foreach (var resource in process.Outputs.Keys.Where(x => x != null))
                    resource.Producers.Add(process);
            }

            // Ensure there are no cycles
            EnsureDAG(econDef);

            return econDef;
        }

        private static IEnumerable<Tuple<Resource, decimal>> ProcessResources(IEnumerable<string> columns, EconomyDefinition worldState)
        {
            foreach (var column in columns)
            {
                var idx = column.IndexOf('x');
                if (idx < 0)
                    continue;
                var inputQty = Decimal.Parse(column.Substring(0, idx));
                var inputResourceName = column.Substring(idx + 1);
                Resource inputResource;
                if (!worldState.Resources.TryGetValue(inputResourceName, out inputResource))
                {
                    inputResource = new Resource {Name = inputResourceName};
                    worldState.Resources.Add(inputResourceName, inputResource);
                }
                yield return Tuple.Create(inputResource, inputQty);
            }
        }

        private static void ParseFile(string fileConents, EconomyDefinition worldState, Action<string, string, IEnumerable<Tuple<Resource, decimal>>, IEnumerable<Tuple<Resource, decimal>>> emitProcess)
        {
            var lines = fileConents.Split('\n');
            foreach (var line in lines.Skip(1))
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');
                if (columns.All(String.IsNullOrWhiteSpace)) continue;

                var processName = columns[0];
                if (String.IsNullOrWhiteSpace(processName))
                    processName = "UNNAMED PROCESS " + sha256(line);

                var energyCost = columns[2];

                var inputs = columns.Skip(2).TakeWhile(x => x != "=>").ToArray();
                var outputs = columns.Skip(inputs.Length + 3).ToArray();

                emitProcess(processName, energyCost, 
                            ProcessResources(inputs, worldState), 
                            ProcessResources(outputs, worldState));
            }

        }
        private static string sha256(string password)
        {
            var crypt = new SHA256Managed();
            var hash = String.Empty;
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            return crypto.Aggregate(hash, (current, bit) => current + bit.ToString("x2"));
        }

        private static void EnsureDAG(EconomyDefinition econ)
        {
            // Ensure there are no loops.
            var allProcesses = new HashSet<Process>(econ.AllProcesses);

            foreach (var process in allProcesses)
            {
                RecursiveSearch(process, new Stack<Process>(), allProcesses);
            }
        }

        private static void RecursiveSearch(Process currentProcess, Stack<Process> chain, HashSet<Process> searchSpace)
        {
            if (chain.Contains(currentProcess)) throw new Exception(currentProcess.ToString());

            chain.Push(currentProcess);

            foreach (var output in currentProcess.Outputs.Keys.Where(x => x != null))
            {
                var outputClosure = output;
                foreach (var other in searchSpace.Where(x => x.Inputs.Keys.Contains(outputClosure)))
                {
                    RecursiveSearch(other, chain, searchSpace);
                }
            }
            chain.Pop();
        }
    }
}

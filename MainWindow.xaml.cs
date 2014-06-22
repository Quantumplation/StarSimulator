using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Fleck;

namespace StarSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TotalInvestmentLabel.DataContext = 0;
            DataContext = _star;
        }

        readonly Star _star = new Star();

        private void Econ_Import(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dlgRaw = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Raw Resource File"
            };
            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlgRaw.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result != true) return;
            var dlgComposite = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Composite Resource File"
            };

            result = dlgComposite.ShowDialog();

            if (result != true) return;
            // Open document 
            var rawFilename = dlgRaw.FileName;
            var compositeFilename = dlgComposite.FileName;
            var econ = Parser.Parse(rawFilename, compositeFilename);
            _star.Economy = econ;

            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () => Console.WriteLine("Open!");
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => DumpData(socket, message);
                Timestamp.TimeManager.PropertyChanged += (s, ev) => DumpData(socket, Timestamp.Now().Ticks.ToString());
            });
        }

        private void DumpData(IWebSocketConnection socket, string msg)
        {
            if (msg == "econ")
            {
                socket.Send(JsonConvert.SerializeObject(_star.Economy));
            }
            else
            {
                var at = long.Parse(msg);
                var old = Timestamp.TimeManager.CurrentTick;
                Timestamp.TimeManager.SilentOverride(at);
                var data = new
                {
                    Tick = at,
                    Inventory = _star.Inventory,
                    Portfolio = _star.Portfolio,
                };
                socket.Send(JsonConvert.SerializeObject(data));
                Timestamp.TimeManager.SilentOverride(old);
            }
        }

        System.Windows.Threading.DispatcherTimer simulationTimer;
        private void BeginSimulation(object sender, RoutedEventArgs e)
        {
            if(simulationTimer == null)
            {
                simulationTimer = new System.Windows.Threading.DispatcherTimer();
                simulationTimer.Tick += IncrementTime; ;
                simulationTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            }
            if(!simulationTimer.IsEnabled)
                simulationTimer.Start();
        }

        private void HaltSimulation(object sender, RoutedEventArgs e)
        {
            if (simulationTimer != null && simulationTimer.IsEnabled)
                simulationTimer.Stop();
        }

        private void IncrementTime(object sender, System.EventArgs e)
        {
            Timestamp.TimeManager.CurrentTick += 1;
        }

        private void SaveActions(object sender, RoutedEventArgs e)
        {
            var dlgSave = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Actions File"
            };
            // Display SaveFileDialog by calling ShowDialog method 
            var result = dlgSave.ShowDialog();
            if (result != true) return;

            var filename = dlgSave.FileName;
            var fileContents = new StringBuilder();
            fileContents.AppendLine("type,amount,supporting resource/process,time");
            foreach (var act in _star.Actions)
            {
                var imp = act as ImportAction;
                var inv = act as InvestAction;
                if (imp != null)
                {
                    fileContents.AppendFormat("import,{0},{1},{2}", imp.Rate.PerTick, imp.Resource.Name, imp.At.Ticks).AppendLine();
                }
                if (inv != null)
                {
                    fileContents.AppendFormat("invest,{0},{1},{2}", inv.Amount, inv.Process.Name, inv.At.Ticks).AppendLine();
                }
            }
            File.WriteAllText(filename, fileContents.ToString());
        }

        private void ImportActions(object sender, RoutedEventArgs e)
        {
            var dlgImport = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Actions File"
            };
            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlgImport.ShowDialog();
            if (result != true) return;

            var filename = dlgImport.FileName;

            var actionFile = File.ReadAllText(filename);
            var lines = actionFile.Split('\n');
            var actions = new List<StarAction>();
            foreach (var line in lines.Skip(1))
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');
                if (columns.All(String.IsNullOrWhiteSpace)) continue;

                string actionType = columns[0];
                string amount = columns[1];
                string support = columns[2];
                string time = columns[3];

                switch (actionType)
                {
                    case "import":
                        actions.Add(new ImportAction(Timestamp.FromString(time), Rate.FromString(amount), _star.Economy.Resources[support]));
                        break;
                    case "invest":
                        actions.Add(new InvestAction(Timestamp.FromString(time), decimal.Parse(amount), _star.Economy.Processes[support]));
                        break;
                }
            }
            _star.ReplayActions(actions);
        }
    }
}

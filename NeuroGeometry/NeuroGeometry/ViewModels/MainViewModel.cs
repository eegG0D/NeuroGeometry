using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using NeuroGeometry.Abstractions;
using NeuroGeometry.Services;

namespace NeuroGeometry.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private IBiosensor _biosensor;
        private PointCollection _geometryPoints;
        private PointCollection _rawWavePoints;
        private string _statusText;
        private Brush _geometryColor;

        // GAME VARIABLES
        private int _score;
        private int _level = 1;
        private int _currentXp;
        private int _maxXp = 100;
        private int _combo = 1;
        private int _overdriveMeter; // 0 to 100
        private bool _isOverdriveActive;
        private string _gameMessage;

        // Visuals
        private int _attention;
        private int _meditation;

        public RelayCommand ConnectCommand { get; set; }
        public ObservableCollection<ElectrodeViewModel> ElectrodeImpedanceMap { get; set; }

        // --- BINDINGS ---
        public PointCollection GeometryPoints { get => _geometryPoints; set { _geometryPoints = value; OnPropertyChanged("GeometryPoints"); } }
        public PointCollection RawWavePoints { get => _rawWavePoints; set { _rawWavePoints = value; OnPropertyChanged("RawWavePoints"); } }
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged("StatusText"); } }
        public Brush GeometryColor { get => _geometryColor; set { _geometryColor = value; OnPropertyChanged("GeometryColor"); } }

        public int Attention { get => _attention; set { _attention = value; OnPropertyChanged("Attention"); } }
        public int Meditation { get => _meditation; set { _meditation = value; OnPropertyChanged("Meditation"); } }

        // Game Bindings
        public int Score { get => _score; set { _score = value; OnPropertyChanged("Score"); } }
        public int Level { get => _level; set { _level = value; OnPropertyChanged("Level"); } }
        public int CurrentXp { get => _currentXp; set { _currentXp = value; OnPropertyChanged("CurrentXp"); } }
        public int MaxXp { get => _maxXp; set { _maxXp = value; OnPropertyChanged("MaxXp"); } }
        public int Combo { get => _combo; set { _combo = value; OnPropertyChanged("Combo"); } }
        public int OverdriveMeter { get => _overdriveMeter; set { _overdriveMeter = value; OnPropertyChanged("OverdriveMeter"); } }
        public string GameMessage { get => _gameMessage; set { _gameMessage = value; OnPropertyChanged("GameMessage"); } }
        public bool IsOverdriveActive { get => _isOverdriveActive; set { _isOverdriveActive = value; OnPropertyChanged("IsOverdriveActive"); } }

        public MainViewModel()
        {
            _biosensor = new ThinkGearService();
            _biosensor.DataReceived += OnBrainDataReceived;

            ElectrodeImpedanceMap = new ObservableCollection<ElectrodeViewModel>();
            for (int i = 0; i < 16; i++) ElectrodeImpedanceMap.Add(new ElectrodeViewModel { Name = $"N{i + 1}", Impedance = 0 });

            RawWavePoints = new PointCollection(Enumerable.Repeat(new System.Windows.Point(0, 50), 300));
            ConnectCommand = new RelayCommand(o => _biosensor.Connect());

            StatusText = "SYSTEM OFFLINE";
            GameMessage = "Connect Headset to Start";
            GeometryColor = Brushes.Cyan;
        }

        private void OnBrainDataReceived(EegData data)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Raw Wave (Visual Only)
                if (data.rawEeg != 0) UpdateRawWave(data.rawEeg);

                // Game Loop (Updates 1/sec usually)
                if (data.eSense != null)
                {
                    StatusText = data.PoorSignalLevel == 0 ? "LINK ESTABLISHED" : "SEARCHING FOR SIGNAL...";
                    Attention = data.eSense.attention;
                    Meditation = data.eSense.meditation;

                    if (data.PoorSignalLevel < 50)
                    {
                        RunGameLogic(data);
                        CalculateGeometry(data);
                    }

                    foreach (var node in ElectrodeImpedanceMap) node.Update(data.PoorSignalLevel);
                }
            });
        }

        private void RunGameLogic(EegData data)
        {
            // 1. COMBO SYSTEM
            // Maintain focus > 60 to build combo
            if (data.eSense.attention > 60)
            {
                if (Combo < 5) Combo++;
            }
            else if (data.eSense.attention < 40)
            {
                if (Combo > 1) GameMessage = "COMBO BROKEN!";
                Combo = 1;
            }

            // 2. OVERDRIVE SYSTEM (Mana)
            // Meditation charges your "Ultimate"
            if (!IsOverdriveActive)
            {
                OverdriveMeter += (data.eSense.meditation / 10);
                if (OverdriveMeter >= 100) ActivateOverdrive();
            }
            else
            {
                // Drain meter while active
                OverdriveMeter -= 10;
                if (OverdriveMeter <= 0) DeactivateOverdrive();
            }

            // 3. XP & SCORING
            int baseXpGain = (data.eSense.attention / 10);

            // Critical Hit check (Gamma waves)
            bool isCrit = data.eegPower.midGamma > 10000;
            if (isCrit)
            {
                GameMessage = "CRITICAL SYNAPSE!";
                baseXpGain *= 2;
            }

            int multiplier = IsOverdriveActive ? Combo * 2 : Combo;
            int totalGain = baseXpGain * multiplier;

            Score += totalGain * 10;
            CurrentXp += totalGain;

            // Level Up
            if (CurrentXp >= MaxXp)
            {
                Level++;
                CurrentXp = 0;
                MaxXp = (int)(MaxXp * 1.5);
                GameMessage = $"LEVEL UP! RANK {Level}";
                // Play Level Up Sound (System beep for simplicity)
                Task.Run(() => Console.Beep(600, 300));
            }

            // Visual Feedback Colors
            if (IsOverdriveActive) GeometryColor = Brushes.OrangeRed;
            else if (Combo >= 4) GeometryColor = Brushes.Gold;
            else GeometryColor = Brushes.Cyan;
        }

        private void ActivateOverdrive()
        {
            IsOverdriveActive = true;
            OverdriveMeter = 100;
            GameMessage = ">>> OVERDRIVE ACTIVATED <<<";
            Task.Run(() => { Console.Beep(400, 100); Console.Beep(500, 100); Console.Beep(600, 300); });
        }

        private void DeactivateOverdrive()
        {
            IsOverdriveActive = false;
            OverdriveMeter = 0;
            GameMessage = "Overdrive Depleted";
        }

        private void UpdateRawWave(int rawValue)
        {
            double y = 50 + (rawValue / 20.0);
            if (y < 0) y = 0; if (y > 100) y = 100;
            var points = new PointCollection(RawWavePoints);
            points.RemoveAt(0);
            points.Add(new System.Windows.Point(points.Count * 2, y));
            for (int i = 0; i < points.Count; i++) points[i] = new System.Windows.Point(i * 2, points[i].Y);
            RawWavePoints = points;
        }

        private void CalculateGeometry(EegData data)
        {
            if (data.eegPower == null) return;
            double cx = 150; double cy = 150; double baseRadius = 50 + (Combo * 5); // Grow with combo

            // Logarithmic scaling for visualization
            double[] powers = new double[] {
                Math.Log(data.eegPower.delta), Math.Log(data.eegPower.theta),
                Math.Log(data.eegPower.lowAlpha), Math.Log(data.eegPower.highAlpha),
                Math.Log(data.eegPower.lowBeta), Math.Log(data.eegPower.highBeta),
                Math.Log(data.eegPower.lowGamma), Math.Log(data.eegPower.highGamma)
            };
            PointCollection newPoints = new PointCollection();
            double angleStep = Math.PI * 2 / 8;
            for (int i = 0; i < 8; i++)
            {
                double r = baseRadius + (powers[i] * 5);
                newPoints.Add(new System.Windows.Point(cx + r * Math.Cos(i * angleStep), cy + r * Math.Sin(i * angleStep)));
            }
            GeometryPoints = newPoints;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Required Helpers
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private Action<object> _execute;
        public RelayCommand(Action<object> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged;
    }

    public class ElectrodeViewModel : INotifyPropertyChanged
    {
        private Brush _statusColor;
        public string Name { get; set; }
        public int Impedance { get; set; }
        public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged("StatusColor"); } }
        public void Update(int signalQuality)
        {
            if (signalQuality == 0) StatusColor = Brushes.LimeGreen;
            else if (signalQuality < 50) StatusColor = Brushes.Yellow;
            else StatusColor = Brushes.Red;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
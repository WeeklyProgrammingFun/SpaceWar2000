using GalaSoft.MvvmLight;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFRunner.Model;
using WPFRunner.ViewModel;

namespace WPFRunner.SpaceWar2K
{
    public class ViewModel : ViewModelBase
    {
        public ObservableCollection<string> Maps { get; } = new ObservableCollection<string>();
        public ObservableCollection<BotView> Bots { get; } = new ObservableCollection<BotView>();

        
        public ViewModel()
        {
            //Maps.Add("map1.txt");
            //Maps.Add("map2.txt");
            //Maps.Add("map3.txt");
            //Maps.Add("map4.txt");
            //SelectedMap = Maps[0];

            foreach (var player in MainViewModel.Players)
                Bots.Add(new BotView(player, BotCheck));
            if (Bots.Count > 0)
            {
                Bots[0].Player1 = true;
                if (Bots.Count > 1)
                    Bots[1].Player2 = true;
                else
                    Bots[0].Player2 = true;
                //Bots[0].Player2 = true;
            }

            timer.Tick += (o, e) => TimerTick();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Start();
        }

        void TimerTick()
        {
            if (Playback)
            {
                while (stateQueue.TryDequeue(out var state))
                    history.Add(state);
                if (planetDiameters == null && history.Any())
                    planetDiameters = CreatePlanetRadii(history[0]);
                if (Frame < history.Count)
                    Frame++;
            }
        }

        DispatcherTimer timer = new DispatcherTimer();

        // call after bot changed checkbox
        void BotCheck(BotView bot)
        {
            // uncheck all other bots with this
            foreach (var b in Bots)
            {
                if (b == bot)
                    continue;
                if (bot.Player1 && b.Player1)
                    b.Player1 = false;
                if (bot.Player2 && b.Player2)
                    b.Player2 = false;
            }
            StartGame();
        }

        public class BotView :ViewModelBase
        {
            public Player player;
            readonly Action<BotView> botAction;
            public BotView(Player player, Action<BotView> action)
            {
                this.player = player;
                this.botAction = action;
            }
            public override string ToString()
            {
                return player.Filename;
            }

            private bool player1;
            public bool Player1
            {
                get { return player1; }
                set { if (Set(ref player1, value)) { if (value) { botAction(this); } }; }
            }
            private bool player2;
            public bool Player2
            {
                get { return player2; }
                set { if (Set(ref player2, value)) { if (value) { botAction(this); } }; }
            }
        }

        #region Properties
        private bool playback = true;
        public bool Playback
        {
            get { return playback; }
            set { Set(ref playback, value); }
        }
        private string player1Text;
        public string Player1Text
        {
            get { return player1Text; }
            set { Set(ref player1Text, value); }
        }
        private string player2Text;
        public string Player2Text
        {
            get { return player2Text; }
            set { Set(ref player2Text, value); }
        }

        private string selectedMap;
        public string SelectedMap
        {
            get { return selectedMap; }
            set { if (Set(ref selectedMap, value)) StartGame(); }
        }

        private int frame = 0;
        public int Frame
        {
            get { return frame; }
            set { if (Set(ref frame, value)) DrawFrame(); }
        }

        private int frameMax = 200;
        public int FrameMax
        {
            get { return frameMax; }
            set { if (Set(ref frameMax, value)) StartGame(); }
        }

        private int randomSeed = 12345;
        public int RandomSeed
        {
            get { return randomSeed; }
            set { if (Set(ref randomSeed, value)) StartGame(); }
        }

        #endregion

        public ObservableCollection<Viewable> ViewItems { get; } = new ObservableCollection<Viewable>();

        Size viewSize = new Size(10, 10);
        internal void ViewSize(Size newSize)
        {
            viewSize = newSize;
            DrawFrame();
        }

        void ComputeScale()
        {
            var state = CurFrame();
            planetDiameters = CreatePlanetRadii(state);
            if (planetDiameters == null) return;
            var planets = state.planets_;

            // compute scaling for canvas
            if (planetDiameters.Count < 1) return;

            double w = viewSize.Width, h = viewSize.Height;
            if (w <= 0 || h <= 0 || state == null)
                return;
            if (planets.Count == 0)
                return;
            var xmin = planets.Select((p, i) => p.x_).Min();
            var xmax = planets.Select((p, i) => p.x_ + planetDiameters[i]).Max();
            var ymin = planets.Select((p, i) => p.y_).Min();
            var ymax = planets.Select((p, i) => p.y_ + planetDiameters[i]).Max();

            // scale min to origin (0,0)

            var pmax = Math.Max(xmax - xmin, ymax - ymin);
            var smin = Math.Min(h, w);

            //Debug.WriteLine(String.Format("{0},{1}  {2},{3}", xmin, ymin, xmax, ymax));

            Scale = 0.80*smin / pmax;

            // compute borders - todo has clipping to edge bug
            var xborder = ((w) - (xmax - xmin) * Scale) / 2;
            var yborder = ((h) - (ymax - ymin) * Scale) / 2;
            TranslatePoint = new Point(xborder, yborder);
        }

        private double scale=1;
        public double Scale
        {
            get { return scale; }
            set { Set(ref scale, value); }
        }
        private Point translatePoint = new Point(10, 10);
        public Point TranslatePoint
        {
            get { return translatePoint; }
            set { Set(ref translatePoint, value); }
        }

        State CurFrame()
        {
            int frame = Frame;
            if (frame >= history.Count)
                frame = history.Count - 1;
            if (frame < 0)
                return new State(1,1,new List<Planet>(), new List<Fleet>());
            var state = history[frame];
            return state;
        }

        void DrawFrame()
        {
            if (planetDiameters == null) return;

            var state = CurFrame();

            ComputeScale();

            ViewItems.Clear();
            var index = 0;
            foreach (var p in state.planets_)
            {
                double diameter = planetDiameters[index];
                int player1FleetsEnRoute = state.FleetsEnRoute(Owner.Player1,p);
                int player2FleetsEnRoute = state.FleetsEnRoute(Owner.Player2,p);
                ViewItems.Add(new ViewablePlanet(p,diameter*2, player1FleetsEnRoute, player2FleetsEnRoute));
                ++index;
            }
            foreach (var f in state.fleets_)
                ViewItems.Add(new ViewableFleet(state,f));

            DrawHistory();

            Player1Text = FormatPlayerText(state, Owner.Player1);
            Player2Text = FormatPlayerText(state, Owner.Player2);
        }
        string FormatPlayerText(State state, Owner owner)
        {
            var fleets = state.fleets_.Where(f => f.owner_ == owner).ToList();
            var planetCount = state.planets_.Count(p => p.owner_ == owner);
            return $"{state.Population((int)owner)}, growth {state.GrowthRate((int)owner)}, fleets {fleets.Count}, fleet pop {fleets.Sum(f=>f.population_)}, planets {planetCount}";
        }

        Dictionary<int, double> planetDiameters = new Dictionary<int, double>();

        private static Dictionary<int, double> CreatePlanetRadii(State state)
        {
            if (!state.planets_.Any())
                return null;
            // compute distances from planet to planet
            double dist(Planet a, Planet b)
            {
                var dx = a.x_ - b.x_;
                var dy = a.y_ - b.y_;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            var diams = new List<double>();
            // for each planet, find distance to closest neighbor
            var index = 0;
            foreach (var planet in state.planets_)
            {
                // find closest nonzero distance among all planets
                var min = state.planets_.Select((p,i) => i == index?Double.MaxValue : dist(p,planet)).Min();
                diams.Add(min);
                ++index;
            }

            // for each planet
            var closest = diams.Min(); // largest diameter possible
            var growthMax = state.planets_.Select(p => p.growthRate_).Max();
            double minr = 0.60, maxr = 1.30; // size of planet in percent terms of possible sizes

            // now walk diameters and change them
            index = 0;
            foreach (var planet in state.planets_)
            {
                var ratio = (double)planet.growthRate_ / growthMax; // in 0-1

                ratio = ratio * (maxr - minr) + minr; // in range min to max
                diams[index] = closest * ratio;
                ++index;
            }

            return diams.Select((v,i)=>(i,v)).ToDictionary(p=>p.Item1,p=>p.Item2);
        }

        Random rand = new Random(1234);
        /// <summary>
        /// Try to load a background
        /// </summary>
        void LoadBackground()
        {
            try
            {
                const string path = "Backgrounds";
                var files = Directory.GetFiles(path, "*.jpg");
                if (files.Length == 0)
                { // no images
                    BackgroundImage = Brushes.Black;
                    return;
                }
                else
                { // load random background
                    var filename = files[rand.Next(files.Length)];
                    var br = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(filename, UriKind.Relative))
                    };
                    BackgroundImage = br;
                }
            }
            catch // todo -note?
            {
                BackgroundImage = Brushes.Black;
            }
        }

        private Brush backgroundImage = null;
        public Brush BackgroundImage
        {
            get { return backgroundImage; }
            set { Set(ref backgroundImage, value); }
        }

        ConcurrentQueue<State> stateQueue = new ConcurrentQueue<State>();

        // fire off a game with selected parameters
        async void StartGame()
        {
            // check if two bots picked
            // check if map picked

            var p1 = Bots.Where(b => b.Player1).FirstOrDefault();
            var p2 = Bots.Where(b => b.Player2).FirstOrDefault();
            if (p1 == null || p2 == null)
            {
                // sageBox.Show("Select player 1 and 2");
                return;
            }

            LoadBackground();

            Frame = 0;
            planetDiameters = null;
            while (stateQueue.TryDequeue(out var s))
            { // nothing
            }
            history.Clear();

            var dispatcher = Dispatcher.CurrentDispatcher;

            var res = await Task.Factory.StartNew(() =>
            {
                return GameRunner.PlayOneGame(p1.player, p2.player, RandomSeed, stateQueue.Enqueue);
            });
        }

        List<State> history = new List<State>();

        #region History
        // HistoryLines to render on history canvas
        public ObservableCollection<Line> HistoryLines { get; } = new ObservableCollection<Line>();
        Size historySize = new Size(1, 1);
        void DrawHistory()
        {
            var border = 10.0;
            var states = history;
            if (states.Count < 2)
                return;
            var pop1 = states.Select(s => s.Population(1)).ToList();
            var pop2 = states.Select(s => s.Population(2)).ToList();
            var rate1 = states.Select(s => s.GrowthRate(1)).ToList();
            var rate2 = states.Select(s => s.GrowthRate(2)).ToList();
            var maxPop = Math.Max(pop1.Max(), pop2.Max());
            var maxRate = Math.Max(rate1.Max(), rate2.Max());
            HistoryLines.Clear();
            var line1 = new Line
            {
                X1 = border,
                X2 = border,
                Y1 = border,
                Y2 = historySize.Height - border,
                Stroke = Brushes.PowderBlue,
                StrokeThickness = 2
            };
            HistoryLines.Add(line1);
            DrawLines(rate2, maxRate, Brushes.Pink);
            DrawLines(rate1, maxRate, Brushes.LightGreen);
            DrawLines(pop2, maxPop, Brushes.Red, pop1.Last() == 0);
            DrawLines(pop1, maxPop, Brushes.Green, pop2.Last() == 0);

            // add cursor
            var alpha = (double) Math.Min(Frame,history.Count) / history.Count;
            var cy = (historySize.Height - 2 * border) * alpha + border;
            line1 = new Line
            {
                X1 = 5,
                X2 = historySize.Width-5,
                Y1 = cy,
                Y2 = cy,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 1
            };
            HistoryLines.Add(line1);

            void DrawLines(List<int> vals, int max, Brush color, bool showWin = false)
            {
                var w = historySize.Width;
                var h = historySize.Height;
                var dx = (w - 2 * border) / (max + 1);
                var dy = (h - 2 * border) / (vals.Count + 1);
                for (var i = 0; i < vals.Count - 1; ++i)
                {
                    var line = new Line
                    {
                        Stroke = color,
                        StrokeThickness = 2,
                        Opacity = 0.7,
                        X1 = vals[i] * dx + border,
                        X2 = vals[i + 1] * dx + border,
                        Y1 = i * dy + border,
                        Y2 = (i + 1) * dy + border
                    };
                    HistoryLines.Add(line);
                }
                if (showWin)
                {
                    var i = vals.Count - 1;
                    var line = new Line
                    {
                        Stroke = color,
                        StrokeThickness = 15,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        X1 = vals[i] * dx + border,
                        X2 = vals[i] * dx + border,
                        Y1 = i * dy + border,
                        Y2 = i * dy + border
                    };
                    HistoryLines.Add(line);
                }

            }
        }

        internal void HistorySize(Size newSize)
        {
            historySize = newSize;
            DrawHistory();
        }

#endregion

    }
}

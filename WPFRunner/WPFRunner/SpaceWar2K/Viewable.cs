using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPFRunner.SpaceWar2K
{
    public class Viewable : ViewModelBase
    {
        public string Text { get; set; }

        private double x = 0;
        public double X
        {
            get { return x; }
            set { Set(ref x, value); }
        }



        private double y = 0;
        public double Y
        {
            get { return y; }
            set { Set(ref y, value); }
        }
    }
    public class ViewableFleet :Viewable
    {
        public Fleet Item { get; set; }
        public Brush FillBrush { get; set; }

        public static Brush[] FleetBrushes = new Brush[]
                                                 {
                                                     null,
                                                     Brushes.LightGreen,
                                                     Brushes.Pink
                                                 };
        void LocateFleet(State state, Fleet fleet)
        {
            // location
            var x1 = state.planets_[fleet.src_].x_;
            var x2 = state.planets_[fleet.dst_].x_;
            var y1 = state.planets_[fleet.src_].y_;
            var y2 = state.planets_[fleet.dst_].y_;
            var alpha = 1.0-(double)fleet.turnsRemaining_/fleet.totalTurns_;
            X = (x2 - x1) * alpha + x1;
            Y = (y2 - y1) * alpha + y1;
        }


        public ViewableFleet(State state, Fleet fleet)
        {
            Item = fleet;
            LocateFleet(state, fleet);
            FillBrush = ViewableFleet.FleetBrushes[(int)fleet.owner_];
        }
    }

    public class ViewablePlanet : Viewable
    {
        public Planet Item { get; set; }

        public double Diameter { get; set; }
        public Brush StrokeBrush { get; set; }

        public Brush FillBrush
        {
            get;
            private set;
        }

        public static Brush[] PlanetBrushes = new Brush[]
                                           {
                                                       new RadialGradientBrush(Colors.LightGray, Colors.Black),
                                                       new RadialGradientBrush(Colors.LightGreen, Colors.DarkGreen),
                                                       new RadialGradientBrush(Colors.Pink, Colors.DarkRed),
                                                       new SolidColorBrush(Colors.White), // no one sent here
                                                       new SolidColorBrush(Colors.Green), // green sent here
                                                       new SolidColorBrush(Colors.Red),   // red sent here
                                                       // last brush is mixed brush for fleets sent to planet
                                                       new LinearGradientBrush(
                                                                    Colors.Red, // todo - make brushes from colors?
                                                                    Colors.Green,
                                                                    0)
                                           };

        public ViewablePlanet(Planet p, double diameter, int player1FleetsEnRoute, int player2FleetsEnRoute)
        {
            Item = p;
            Diameter = diameter;

            // set standard location for the object upper left, top location
            X = p.x_ - Diameter / 2;
            Y = p.y_ - Diameter / 2;
            FillBrush = PlanetBrushes[(int)Item.owner_];

            if ((player1FleetsEnRoute > 0) && (player2FleetsEnRoute > 0))
                StrokeBrush = PlanetBrushes[6]; // both
            else if (player1FleetsEnRoute > 0)
                StrokeBrush = PlanetBrushes[4]; // 
            else if (player2FleetsEnRoute > 0)
                StrokeBrush = PlanetBrushes[5]; // 
            else
                StrokeBrush = PlanetBrushes[3];
        }
    }

}

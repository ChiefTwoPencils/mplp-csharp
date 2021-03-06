using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

//using System.Drawing;
using System.Diagnostics;
using System.Windows.Shapes;

namespace FindFlows
{
    internal class Link
    {
        public Network Network { get; set; }
        public Node FromNode { get; set; }
        public Node ToNode { get; set; }
        public double Capacity { get; set; }
        public double Flow { get; set; }
        public Line MyLine { get; set; }

        private bool _isInTree;
        public bool IsInTree
        {
            get { return _isInTree; }
            set
            {
                _isInTree = value;
                SetLinkAppearance();
            }
        }

        private bool _isInPath;
        public bool IsInPath
        {
            get { return _isInPath; }
            set
            {
                _isInPath = value;
                SetLinkAppearance();
            }
        }



        public Link(Network network, Node fromNode, Node toNode, int capacity)
        {
            Network = network;
            FromNode = fromNode;
            ToNode = toNode;
            Capacity = capacity;
            Flow = 0;
            IsInTree = false;

            FromNode.AddLink(this);
            network.AddLink(this);
        }

        internal void Draw(Canvas canvas)
        {
            MyLine = canvas.DrawLine(FromNode.Center, ToNode.Center, Brushes.Green, 2);
        }

        internal void DrawLabel(Canvas canvas)
        {
            if (!Network.DrawLabels) return;

            static double WeightedCoord(double x, double y)
                => 0.67 * x + 0.33 * y;

            const int Radius = 10;
            const int Diameter = 2 * Radius;

            var fromCenter = FromNode.Center;
            var toCenter = ToNode.Center;
            var angleToPerp = 0;

            if (fromCenter.X == toCenter.X)
            {
                angleToPerp = 90;
            }
            else if (fromCenter.Y == toCenter.Y)
            {
                angleToPerp = -90;
            }

            var dxy = toCenter - fromCenter;
            var angle = Math.Atan2(dxy.X, dxy.Y) * 180 / Math.PI + angleToPerp;

            var x = WeightedCoord(fromCenter.X, toCenter.X);
            var y = WeightedCoord(fromCenter.Y, toCenter.Y);

            var bounds = new Rect(x - Radius, y - Radius, Diameter, Diameter);
            canvas.DrawEllipse(bounds, Brushes.White, Brushes.Transparent, 0);
            canvas.DrawString($"{Flow}/{Capacity}", Diameter, Diameter, new Point(x, y), angle, 12, Brushes.Black);
        }

        internal void SetLinkAppearance()
        {
            if (MyLine == null) return;

            MyLine.Stroke = Flow > 0
                ? Brushes.Red
                : Brushes.Black;

            MyLine.StrokeThickness = 2 * Flow + 1;
        }

        public void Reset()
        {
            ToNode.BackLinks.Add(this);
            Flow = 0;
        }

        public override string ToString() => $"[{FromNode}] --> [{ToNode}] ({Capacity})";
    }
}

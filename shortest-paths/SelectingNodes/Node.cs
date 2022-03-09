using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SelectingNodes
{
    internal class Node
    {
        public const double LargeRadius = 10;
        public const double SmallRadius = 3;

        public int Index { get; set; }
        public Point Center { get; set; }
        public string Text { get; set; }
        public Network Network { get; set; }
        public List<Link> Links { get; set; }
        public Ellipse MyEllipse { get; set; }
        public Label MyLabel { get; set; }

        private bool _isStartNode;
        public bool IsStartNode
        {
            get { return _isStartNode; }
            set 
            { 
                _isStartNode = value; 
                SetNodeAppearance();
            }
        }

        private bool _isEndNode;
        public bool IsEndNode
        {
            get { return _isEndNode; }
            set 
            {
                _isEndNode = value;
                SetNodeAppearance();
            }
        }

        public Node(Network network, Point center, string text)
        {
            Index = -1;
            Center = center;
            Text = text;
            Network = network;
            Links = new();

            network.AddNode(this);
        }

        public Node AddLink(Link link)
        {
            Links.Add(link);
            return this;
        }

        internal void Draw(Canvas canvas, bool drawLabels)
        {
            var radius = drawLabels ? LargeRadius : SmallRadius;
            var diameter = 2 * radius;
            var bounds = new Rect(Center.X - radius, Center.Y - radius, diameter, diameter);

            MyEllipse = canvas.DrawEllipse(bounds, Brushes.White, Brushes.Black, 1);
            MyEllipse.Tag = this;
            MyEllipse.MouseDown += Network.ellipse_MouseDown;

            if (drawLabels)
            {
                MyLabel = canvas.DrawLabel(
                    bounds, Text, Brushes.Transparent, Brushes.Blue, HorizontalAlignment.Center, VerticalAlignment.Center, 12, 0);
                MyLabel.Tag = this;
                MyLabel.MouseDown += Network.label_MouseDown;
            }

        }

        private void SetNodeAppearance()
        {
            if (MyEllipse == null) return;

            if (IsStartNode)
            {
                MyEllipse.Fill = Brushes.Pink;
                MyEllipse.Stroke = Brushes.Red;
                MyEllipse.StrokeThickness = 2;
            }
            else if (IsEndNode)
            {
                MyEllipse.Fill = Brushes.LightGreen;
                MyEllipse.Stroke = Brushes.Green;
                MyEllipse.StrokeThickness = 1;
            }
        }

        public override string ToString() => $"[{Text}]";
    }
}

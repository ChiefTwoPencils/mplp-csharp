using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawingNetworks
{
    internal class Node
    {
        public int Index { get; set; }
        public Point Center { get; set; }
        public string Text { get; set; }
        public Network Network { get; set; }
        public List<Link> Links { get; set; }

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

        internal void Draw(Canvas canvas)
        {
            var radius = 10;
            var diameter = 2 * radius;
            var bounds = new Rect(Center.X - radius, Center.Y - radius, diameter, diameter);

            canvas.DrawEllipse(bounds, Brushes.White, Brushes.Black, 1);

            canvas.DrawLabel(
                bounds, Text, Brushes.Transparent, Brushes.Blue, HorizontalAlignment.Center, VerticalAlignment.Center, 12, 0);
        }

        public override string ToString() => $"[{Text}]";
    }
}

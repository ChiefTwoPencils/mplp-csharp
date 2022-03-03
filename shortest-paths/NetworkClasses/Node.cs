using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClasses
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

        public override string ToString() => $"[{Text}]";
    }
}

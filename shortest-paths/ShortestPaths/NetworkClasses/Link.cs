using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkClasses
{
    internal class Link
    {
        public Network Network { get; set; }
        public Node FromNode { get; set; }
        public Node ToNode { get; set; }
        public int Cost { get; set; }

        public Link(Network network, Node fromNode, Node toNode, int cost)
        {
            Network = network;
            FromNode = fromNode;
            ToNode = toNode;
            Cost = cost;

            FromNode.AddLink(this);
            network.AddLink(this);
        }

        public override string ToString() => $"[{FromNode}] --> [{ToNode}] ({Cost})";
    }
}

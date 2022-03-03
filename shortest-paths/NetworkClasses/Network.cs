using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;

using static System.Linq.Enumerable;
using static System.Int32;

namespace NetworkClasses
{
    internal class Network
    {
        public List<Node> Nodes { get; set; }
        public List<Link> Links { get; set; }

        public Network() => Clear(); 

        public Network AddNode(Node node)
        {
            node.Index = Nodes.Count;
            Nodes.Add(node);
            return this;
        }

        public void AddLink(Link link) => Links.Add(link);

        public void Clear()
        {
            Nodes = new List<Node>();
            Links = new List<Link>();
        }

        public void SaveToFile(string path)
            => File.WriteAllText(path, Serialize());

        internal string Serialize()
        {
            static string SerializeNode(Node node)
                => $"{node.Center.X},{node.Center.Y},{node.Text}";

            static string SerializeLink(Link link)
                => $"{link.FromNode.Index},{link.ToNode.Index},{link.Cost}";

            var lines = new List<string>
            {
                Nodes.Count.ToString(),
                Links.Count.ToString()
            };

            lines.AddRange(Nodes.Select(SerializeNode));
            lines.AddRange(Links.Select(SerializeLink));

            return string.Join("\n", lines.ToArray());
        }

        public void LoadFromFile(string path)
            => Deserialize(File.ReadAllText(path));

        private void Deserialize(string network)
        {
            Clear();
            
            using var reader = new StringReader(network);

            var zero = "0";

            var nodeCount = Parse(ReadNextLine(reader) ?? zero);
            var linkCount = Parse(ReadNextLine(reader) ?? zero);

            foreach (var _ in Range(0, nodeCount).ToList())
            {
                var node = ReadNextLine(reader);
                if (node == null) return;

                var values = SplitLine(node);
                new Node(
                    this,
                    new Point(
                        Parse(values[0]),
                        Parse(values[1])),
                    values[2]);
            }

            foreach (var _ in Range(0, linkCount).ToList())
            {
                var link = ReadNextLine(reader);
                if (link == null) return;

                var values = SplitLine(link);
                new Link(
                    this,
                    Nodes.ElementAt(Parse(values[0])),
                    Nodes.ElementAt(Parse(values[1])),
                    Parse(values[2]));
            }
        }

        private string? ReadNextLine(StringReader reader)
        {
            string? line = null;
            for (line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (line == null) return null;

                line = Regex
                    .Replace(line, "#.*?\n", string.Empty)
                    .Trim();

                if (line != string.Empty) return line;
            }

            return line;
        }

        private string[] SplitLine(string data) => data.Split('(', ',', ')');
    }
}

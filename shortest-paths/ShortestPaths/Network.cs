using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static System.Linq.Enumerable;
using static System.Int32;

using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Diagnostics;

namespace ShortestPaths
{
    internal class Network
    {
        internal enum AlgorithmTypes
        {
            LabelSetting,
            LabelCorrecting
        }

        public List<Node> Nodes { get; set; }
        public List<Link> Links { get; set; }
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }

        private AlgorithmTypes _algorithmType;
        public AlgorithmTypes AlgorithmType 
        { 
            get => _algorithmType; 
            set 
            {
                _algorithmType = value;
                CheckForPath();
            } 
        }

        public bool DrawLabels { get => Nodes.Count < 100; }

        public Network() => Clear(); 

        public Network(string path)
        {
            Clear();
            LoadFromFile(path);
        }

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

            foreach (var _ in Range(0, linkCount))
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

        private Rect GetBounds() => Nodes.Aggregate(
            (double.MaxValue, double.MaxValue, -1.0, -1.0),
            (minmax, node) => 
            {
                var c = node.Center;
                var (minx, miny, maxx, maxy) = minmax;

                return (
                    Math.Min(minx, c.X), Math.Min(miny, c.Y),
                    Math.Max(maxx, c.X), Math.Max(maxy, c.Y)
                );
            }, 
            minmax => new Rect(
                new Point(minmax.Item1, minmax.Item2),
                new Point(minmax.Item3, minmax.Item4)));

        internal void Draw(Canvas canvas)
        {
            var bounds = GetBounds();
            canvas.Height = bounds.Height;
            canvas.Width = bounds.Width;

            Links.ForEach(link => link.Draw(canvas));

            Links.ForEach(link => link.DrawLabel(canvas));

            Nodes.ForEach(node => node.Draw(canvas, DrawLabels));
        }

        internal void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = sender as Ellipse;
            var node = ellipse!.Tag as Node;
            NodeClicked(node!, e);
        }

        internal void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var label = sender as Label;
            var node = label!.Tag as Node;
            NodeClicked(node!, e);
        }

        private void NodeClicked(Node node, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (StartNode != null) StartNode.IsStartNode = false;
                                
                StartNode = node;
                node.IsStartNode = true;
            }
            else
            {
                if (EndNode != null) EndNode.IsEndNode = false;
                               
                EndNode = node;
                node.IsEndNode = true;
            }
            CheckForPath();
        }

        private void ToggleHighlights(List<Link> links, bool toggle = false)
            => links.ForEach(link => link.IsInTree = link.IsInPath = toggle);

        private void FindPathTreeLabelSetting()
        {
            ResetNodesAndLinks();

            StartNode.TotalCost = 0;
            var candidates = new List<Node> { StartNode };

            var pops = 0;
            var checks = 0;
            while (candidates.Any())
            {
                var bestCost = double.PositiveInfinity;
                var bestIndex = -1;
                for (var i = 0; i < candidates.Count; ++i)
                {
                    ++checks;
                    if (bestCost > candidates[i].TotalCost)
                    {
                        bestCost = candidates[i].TotalCost;
                        bestIndex = i;
                    }
                }

                var best = candidates[bestIndex];
                candidates.RemoveAt(bestIndex);
                best.Visited = true;
                ++pops;

                best.Links
                    .ForEach(link =>
                    {
                        var other = link.ToNode;
                        if (other.Visited) return;

                        var newCost = best.TotalCost + link.Cost;
                        if (newCost < other.TotalCost)
                        {
                            other.TotalCost = newCost;
                            other.ShortestPathLink = link;
                            candidates.Add(other);
                        }
                    });
            }

            Trace.WriteLine($"Checks: {checks}");
            Trace.WriteLine($"Pops: {pops}");

            Nodes
                .Where(n => n.ShortestPathLink != null)
                .ToList()
                .ForEach(n => n.ShortestPathLink.IsInTree = true);
        }

        private void FindPathTreeLabelCorrecting()
        {
            ResetNodesAndLinks();

            StartNode.TotalCost = 0;
            var candidates = new List<Node> { StartNode };

            var pops = 0;
            while (candidates.Any())
            {
                var best = candidates.First();
                candidates.RemoveAt(0);
                ++pops;

                best.Links
                    .ForEach(link =>
                    {
                        var other = link.ToNode;
                        var newCost = best.TotalCost + link.Cost;

                        if (newCost < other.TotalCost)
                        {
                            other.TotalCost = newCost;
                            other.ShortestPathLink = link;
                            candidates.Add(other);
                        }
                    });
            }
            Trace.WriteLine($"Pops: {pops}");

            Nodes
                .Where(n => n.ShortestPathLink != null)
                .ToList()
                .ForEach(n => n.ShortestPathLink.IsInTree = true);
        }

        private void FindPath()
        {
            for (var node = EndNode; node != StartNode; node = node.ShortestPathLink.FromNode)
                node.ShortestPathLink.IsInPath = true;

            Trace.WriteLine($"Total cost: {EndNode.TotalCost}");
        }

        private void CheckForPath()
        {
            if (StartNode != null)
            {
                switch (AlgorithmType)
                {
                    case AlgorithmTypes.LabelSetting:
                        FindPathTreeLabelSetting();
                        break;
                    case AlgorithmTypes.LabelCorrecting:
                        FindPathTreeLabelCorrecting();
                        break;
                    default: throw new ArgumentException("No such algo.");
                }

                if (EndNode != null) FindPath();
            }
        }

        private void ResetNodesAndLinks()
        {
            ResetNodes();
            ToggleHighlights(Links);
        }

        private void ResetNodes() => Nodes.ForEach(ResetNode);

        private void ResetNode(Node node)
        {
            node.TotalCost = double.PositiveInfinity;
            node.IsInPath = false;
            node.ShortestPathLink = null;
            node.Visited = false;
        }

        private string[] SplitLine(string data) => data.Split('(', ',', ')');
    }
}

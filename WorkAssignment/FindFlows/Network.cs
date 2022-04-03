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
using System.Windows.Markup.Localizer;
using FindFlows;

namespace FindFlows
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
                // CheckForPath();
                CalculateFlows();
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
                => $"{link.FromNode.Index},{link.ToNode.Index},{link.Capacity}";

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
            // CheckForPath();
            CalculateFlows();
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

                        var newCost = best.TotalCost + link.Capacity;
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
                        var newCost = best.TotalCost + link.Capacity;

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
        
        private void CalculateFlows()
        {
            if (StartNode == null || EndNode == null) return;

            // Calculate maximal flows.

            // Prepare the links and nodes.
            Nodes.ForEach(node => node.Reset());
            Links.ForEach(link => link.Reset());

            // Repeat until we can find no more improvements:
            for (;;)
            {
                // Add the source node to the candidate list.
                var candidates = new List<Node> {StartNode};
                StartNode.Visited = true;
                // Repeat until the candidate list is empty:
                while (candidates.Any())
                {
                    // Get the next candidate.
                    var candidate = candidates.Pop();

                    // See if we can add flow to the node's links.
                    candidate
                        .Links
                        .ForEach(link =>
                        {
                            var neighbor = link.ToNode;
                            // See if we should add this neighbor to the candidate list.
                            if (!neighbor.Visited && link.Flow < link.Capacity)
                            {
                                // Add this neighbor to the candidate list.
                                candidates.Add(neighbor);
                                neighbor.Visited = true;
                                // Record the node and link that got to the neighbor.
                                neighbor.FromNode = candidate;
                                neighbor.FromLink = link;
                            }
                        });
                    
                    // See if we can subtract flow from the node's back links.
                    candidate
                        .BackLinks
                        .ForEach(link =>
                        {
                            var neighbor = link.FromNode;
                            if (!neighbor.Visited && link.Flow > 0)
                            {
                                // Add this neighbor to the candidate list.
                                candidates.Add(neighbor);
                                neighbor.Visited = true;
                                // Record the node and link that got to the neighbor.
                                neighbor.FromNode = candidate;
                                neighbor.FromLink = link;
                            }
                        });

                    // If we have reached the sink node, break out
                    // of the while len(candidateList) > 0 loop.
                    if (EndNode.Visited) break;
                }
                // If we didn't visit the sink, then we didn't find
                // an augmenting path so break out of the for(;;) loop.
                if (!EndNode.Visited) break;
                
                // Work back through the augmenting path updating the link flows.
                // First find the smallest unused capacity on the augmenting path.
                var test = EndNode;
                var smallestCapacity = double.PositiveInfinity;
                while (test != StartNode)
                {
                    // Get the link that got us to this node.
                    var link = test.FromLink;

                    // See if this link was used as a normal link or a backlink.
                    var unusedCapacity = link.ToNode == test
                        ? link.Capacity - link.Flow // Normal link.
                        : link.Flow; // Backlink.
                    
                    if (smallestCapacity > unusedCapacity) smallestCapacity = unusedCapacity;

                    // Go to the previous node in the path.
                    test = test.FromNode;
                }
                
                // To update the augmenting path, follow the path
                // again, this time updating the flows.
                test = EndNode;
                while (test != StartNode)
                {
                    // Get the link that got us to this node.
                    var link = test.FromLink;

                    // See if this link was used as a
                    // normal link or a reverse link.
                    link.Flow = link.ToNode == test
                        ? link.Flow + smallestCapacity
                        : link.Flow - smallestCapacity;

                    // Go to the previous node in the path.
                    test = test.FromNode;
                }
                // Reset the nodes' visited flags for the
                // next attempt at finding an augmenting path.
                Nodes.ForEach(node => node.Visited = false);
            }
            // We're done. The total flow equals the
            // total flow out of the source. (Or into the sink.)
            var flow = StartNode
                .Links
                .Aggregate(0.0, (flow, link) => flow + link.Flow);
            Console.WriteLine($"Total flow: {flow}");

            // Update the link colors and thicnesses.
            Links.ForEach(link => link.SetLinkAppearance());
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

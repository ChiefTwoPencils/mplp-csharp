using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace nary_node5
{
    public class NaryNode<T>
    {
        #region Ctors, Consts, and Props

        public const double Radius = 20.00;
        public const double XSpacing = 40.00;
        public const double YSpacing = 75.00;

        public Point Center { get; private set; }
        public Rect SubtreeBounds { get; private set; }
        public double Diameter { get; } = Radius * 2;
        public T Value { get; set; }
        public List<NaryNode<T>> Children { get; }

        public NaryNode(T value)
        {
            Value = value;
            Children = new List<NaryNode<T>>();
        }

        #endregion

        #region Node Ops

        public NaryNode<T> AddChild(NaryNode<T> child)
        {
            if (child == null)
            {
                throw new ArgumentNullException($"{nameof(child)} must not be null");
            }

            Children.Add(child);
            return this;
        }

        public NaryNode<T> AddChildren(params NaryNode<T>[] children)
        {
            if (children == null || children.Any(child => child == null))
            {
                throw new ArgumentException($"{nameof(children)} and its elements must not be null");
            }

            Children.AddRange(children);
            return this;
        }

        public NaryNode<T> FindNode(T value) => FindNode(this, value);

        public NaryNode<T> FindNode(NaryNode<T> root, T value)
        {
            if (root == null || Equals(root.Value, value)) return root;

            foreach (var child in root.Children)
            {
                if (child.FindNode(value) != null) return child;
            }

            return null;
        }

        #endregion

        #region Traversals

        public List<NaryNode<T>> TraversePreOrder() => TraversePreOrder(this);

        private List<NaryNode<T>> TraversePreOrder(NaryNode<T> root)
        {
            if (root == null) return new List<NaryNode<T>>();

            var list = new List<NaryNode<T>> { root };
            list.AddRange(root.Children.SelectMany(c => TraversePreOrder(c)));
            return list;
        }

        public List<NaryNode<T>> TraversePostOrder() => TraversePostOrder(this);

        private List<NaryNode<T>> TraversePostOrder(NaryNode<T> root)
        {
            if (root == null) return new List<NaryNode<T>>();

            var list = root.Children
                .SelectMany(c => TraversePostOrder(c))
                .ToList();
            list.Add(root);
            return list;
        }

        public Queue<NaryNode<T>> TraverseBreadthFirst()
        {
            var queue = new Queue<NaryNode<T>>();
            queue.Enqueue(this);
            return TraverseBreadthFirst(queue, this.Children);
        }

        private Queue<NaryNode<T>> TraverseBreadthFirst(Queue<NaryNode<T>> queue, List<NaryNode<T>> children)
        {
            if (children == null || !children.Any()) return queue;

            children.ForEach(c => queue.Enqueue(c));
            TraverseBreadthFirst(queue, children.SelectMany(c => c.Children).ToList());

            return queue;
        }

        #endregion

        #region Drawing

        public void ArrangeAndDrawSubtree(Canvas canvas, double minX, double minY)
        {
            ArrangeSubtree(minX, minY);
            DrawSubtreeLinks(canvas);
            DrawSubtreeNodes(canvas);
        }

        private void ArrangeSubtree(double minX, double minY)
        {
            if (HasEmptyNest())
            {
                SubtreeBounds = new Rect(minX, minY, Diameter, Diameter);
                Center = new Point(minX + Radius, minY + Radius);
                return;
            }

            var childMinY = minY + YSpacing;
            var height = 0.0;
            var width = 0.0;
            var totalChildWidth = 0.0;

            var count = Children.Count;
            for (var i = 0; i < count; ++i)
            {
                if (i > 0)
                {
                    totalChildWidth += Children[i - 1].SubtreeBounds.Width;
                }
                var child = Children[i];
                var childMinX = minX + totalChildWidth + i * XSpacing;
                child.ArrangeSubtree(childMinX, childMinY);
            }

            width = Children.Sum(c => c.SubtreeBounds.Width) + (count - 1) * XSpacing;
            height = Children.Max(c => c.SubtreeBounds.Height) + YSpacing;

            SubtreeBounds = new Rect(minX, minY, width, height);
            Center = new Point(minX + SubtreeBounds.Width / 2, minY + Radius);
        }

        private void DrawSubtreeNodes(Canvas canvas)
        {
            TraverseBreadthFirst()
                .ToList()
                .ForEach(n =>
                {
                    var subBox = canvas.DrawRectangle(
                        n.SubtreeBounds, Brushes.Transparent, Brushes.Red, 1);

                    var node = canvas.DrawEllipse(
                        new Rect(n.Center.X - Radius, n.Center.Y - Radius, Diameter, Diameter), Brushes.White, Brushes.Green, 1);

                    var label = canvas.DrawLabel(
                        new Rect(
                            n.Center.X - Radius,
                            n.Center.Y - Radius,
                            Diameter,
                            Diameter),
                        n.Value,
                        Brushes.Transparent,
                        Brushes.Red,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center,
                        Radius,
                        1);
                });
        }

        private void DrawSubtreeLinks(Canvas canvas)
        {
            void Connector(Point start, Point end) => canvas.DrawLine(start, end, Brushes.Black, 1);

            void Line(NaryNode<T> node, bool isChild = true)
            {
                var center = node.Center;
                var y = center.Y;
                var halfWay = YSpacing / 2;

                y = isChild ? y - halfWay : y + halfWay;

                Connector(center, new Point(center.X, y));
            }

            TraverseBreadthFirst()
                .ToList()
                .ForEach(n =>
                {
                    if (n.Children.Any())
                    {
                        Line(n, false);
                        n.Children.ForEach(c => Line(c));
                        var startX = n.Children.First().Center.X;
                        var endX = n.Children.Last().Center.X;
                        var y = n.Center.Y + YSpacing / 2;
                        Connector(new Point(startX, y), new Point(endX, y));
                    }                    
                });
        }

        private bool HasEmptyNest() => !Children.Any();

        private bool HasOneChild() => Children.Count == 1;

        #endregion

        #region Helpers

        public override string ToString() => ToString(string.Empty);

        public string ToString(string spaces)
        {
            var thisValue = $"{spaces}{Value}:";
            if (!Children.Any())
            {
                return thisValue;
            }

            var indentation = $"{spaces}  ";
            var childrenValues = Children
                .Select(child => child.ToString(indentation));

            return $"{thisValue}\n{string.Join("\n", childrenValues)}";
        }

        #endregion
    }
}
    
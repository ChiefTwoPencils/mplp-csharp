using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace nary_node6
{
    public class NaryNode<T>
    {
        #region Ctors, Consts, and Props

        private const double Width = 80;
        private const double Height = 40;
        private const double Indent = 20;
        private const double XSpacing = 40.00;
        private const double YSpacing = 20.00;

        private double HalfWidth { get; set; } = Width / 2;
        private double HalfHeight { get; set; } = Height / 2;
        public Point Center { get; private set; }
        public Rect SubtreeBounds { get; private set; }
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

        public NaryNode<T> AddChildren(params T[] values)
            => AddChildren(values.Select(v => new NaryNode<T>(v)).ToArray());

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

        public bool IsLeaf() => HasEmptyNest();

        public bool IsTwig() => Children.All(child => child.IsLeaf());

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
            if (IsLeaf())
            {
                SubtreeBounds = new Rect(minX, minY, Width, Height);
                Center = new Point(minX + HalfWidth, minY + HalfHeight);
                return;
            }

            var baseY = minY + Height + YSpacing;

            if (IsTwig())
            {                
                var x = minX + Indent;
                var y = baseY;

                var count = Children.Count;
                for (var i = 0; i < count; i++)
                {
                    var child = Children[i];
                    child.ArrangeSubtree(x, y);
                    y = child.SubtreeBounds.Bottom + YSpacing;
                }

                SubtreeBounds = new Rect(minX, minY, Width + Indent, y - YSpacing - minY);
                Center = new Point(minX + HalfWidth, minY + HalfHeight);
            }
            else
            {
                var totalChildWidth = 0.0;
                var count = Children.Count;
                var height = 0.0;

                for (var i = 0; i < count; ++i)
                {
                    var child = Children[i];
                    var childMinX = minX + totalChildWidth + i * XSpacing;
                    child.ArrangeSubtree(childMinX, baseY);
                    totalChildWidth += child.SubtreeBounds.Width;
                    height = Math.Max(height, child.SubtreeBounds.Height);
                }

                SubtreeBounds = new Rect(minX, minY, totalChildWidth + (count - 1) * XSpacing, height);
                Center = new Point(minX + SubtreeBounds.Width / 2, minY + HalfHeight);
            }
        }

        private void DrawSubtreeNodes(Canvas canvas)
        {
            TraverseBreadthFirst()
                .ToList()
                .ForEach(n =>
                {
                    #region Debug Node Bounds
                    //var subBox = canvas.DrawRectangle(
                    //    n.SubtreeBounds, Brushes.Transparent, Brushes.Red, 1);
                    #endregion

                    var fill = n.HasEmptyNest() ? Brushes.White : Brushes.LightPink;
                    var node = canvas.DrawRectangle(
                        new Rect(n.Center.X - HalfWidth, n.Center.Y - HalfHeight, Width, Height), fill, Brushes.Black, 1);

                    string value = n.Value.ToString();
                    value = value.Length > 10
                        ? string.Join("\n", value.Split(' '))
                        : value;

                    var label = canvas.DrawLabel(
                        new Rect(
                            n.Center.X - HalfWidth,
                            n.Center.Y - HalfHeight,
                            Width,
                            Height),
                        value,
                        Brushes.Transparent,
                        Brushes.Red,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center,
                        HalfHeight / 2,
                        1);
                });
        }

        private void DrawSubtreeLinks(Canvas canvas)
        {
            void Connector(Point start, Point end) => canvas.DrawLine(start, end, Brushes.Green, 1);

            TraverseBreadthFirst()
                .ToList()
                .ForEach(n =>
                {
                    if (n.IsLeaf())
                    {
                        return;
                    }

                    if (n.IsTwig())
                    {
                        var lastPoint = new Point(0, 0);
                        n.Children.ForEach(child =>
                        {
                            var x = child.SubtreeBounds.Left;
                            var y = child.SubtreeBounds.Top + HalfHeight;
                            lastPoint = new Point(x - Indent / 2, y);
                            Connector(new Point(x, y), lastPoint);
                        });
                         
                        var startX = n.SubtreeBounds.Left + Indent / 2;
                        var startY = n.SubtreeBounds.Top + Height;
                        Connector(new Point(startX, startY), lastPoint);
                    }
                    else
                    {
                        var center = n.Center;
                        var halfWay = HalfHeight + YSpacing / 2;
                        var startX = double.MaxValue;
                        var endX = double.MinValue;
                        var startEndY = 0.0;                        

                        n.Children.ForEach(child =>
                        {
                            var childCenter = child.Center;
                            startX = Math.Min(startX, childCenter.X);
                            endX = Math.Max(endX, childCenter.X);
                            startEndY = childCenter.Y - halfWay;
                            Connector(childCenter, new Point(childCenter.X, childCenter.Y - halfWay));
                        });

                        Connector(center, new Point(center.X, center.Y + halfWay));
                        Connector(new Point(startX, startEndY), new Point(endX, startEndY));
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
    
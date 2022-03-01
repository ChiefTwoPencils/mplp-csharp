using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace binary_node6
{
    public class BinaryNode<T>
    {

        #region Ctors, Consts, and Props

        public const double Radius = 20.00;
        public const double XSpacing = 42.00;
        public const double YSpacing = 84.00;

        public Point Center { get; private set; }
        public Rect SubtreeBounds { get; private set; }
        public double Diameter { get; } = Radius * 2;

        public T Value { get; set; }
        public BinaryNode<T> LeftChild { get; set; }
        public BinaryNode<T> RightChild { get; set; }

        public BinaryNode(T value)
        {
            Value = value;
            LeftChild = null;
            RightChild = null;
        }

        #endregion

        #region Left

        public BinaryNode<T> AddLeft(BinaryNode<T> left)
        {
            LeftChild = left;
            return this;
        }

        public BinaryNode<T> PutLeft(BinaryNode<T> left)
        {
            LeftChild = left;
            return left;
        }

        #endregion

        #region Right

        public BinaryNode<T> AddRight(BinaryNode<T> right)
        {
            RightChild = right;
            return this;
        }

        public BinaryNode<T> PutRight(BinaryNode<T> right)
        {
            RightChild = right;
            return right;
        }

        #endregion

        #region Both

        public BinaryNode<T> FindNode(T value) => FindNode(this, value);

        public BinaryNode<T> FindNode(BinaryNode<T> root, T value)
        {
            if (root == null || Equals(root.Value, value)) return root;

            return FindNode(root.LeftChild, value)
                ?? FindNode(root.RightChild, value);
        }

        #endregion

        #region Traversals

        public List<BinaryNode<T>> TraversePreOrder() => TraversePreOrder(this);

        private List<BinaryNode<T>> TraversePreOrder(BinaryNode<T> root)
        {
            if (root == null) return new List<BinaryNode<T>>();

            var list = new List<BinaryNode<T>> { root };
            list.AddRange(TraversePreOrder(root.LeftChild));
            list.AddRange(TraversePreOrder(root.RightChild));
            return list;
        }

        public List<BinaryNode<T>> TraverseInOrder() => TraverseInOrder(this);

        private List<BinaryNode<T>> TraverseInOrder(BinaryNode<T> root)
        {
            if (root == null) return new List<BinaryNode<T>>();

            var list = TraverseInOrder(root.LeftChild);
            list.Add(root);
            list.AddRange(TraverseInOrder(root.RightChild));
            return list;
        }

        public List<BinaryNode<T>> TraversePostOrder() => TraversePostOrder(this);

        public List<BinaryNode<T>> TraversePostOrder(BinaryNode<T> root)
        {
            if (root == null) return new List<BinaryNode<T>>();

            var list = TraversePostOrder(root.LeftChild);
            list.AddRange(TraversePostOrder(root.RightChild));
            list.Add(root);
            return list;
        }

        public Queue<BinaryNode<T>> TraverseBreadthFirst()
        {
            var queue = new Queue<BinaryNode<T>>();
            queue.Enqueue(this);
            return TraverseBreadthFirst(queue, this);
        }

        private Queue<BinaryNode<T>> TraverseBreadthFirst(Queue<BinaryNode<T>> queue, BinaryNode<T> root)
        {
            if (root == null) return queue;

            if (root.LeftChild != null)
            {
                queue.Enqueue(root.LeftChild);
            }

            if (root.RightChild != null)
            {
                queue.Enqueue(root.RightChild);
            }

            TraverseBreadthFirst(queue, root.LeftChild);
            TraverseBreadthFirst(queue, root.RightChild);

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

            if (HasOneChild())
            {
                var child = LeftChild ?? RightChild;
                child.ArrangeSubtree(minX, childMinY);
                SubtreeBounds = new Rect(minX, minY, child.SubtreeBounds.Width, child.SubtreeBounds.Height + YSpacing);
            }
            else // Has Left and Right children
            {                               
                LeftChild.ArrangeSubtree(minX, childMinY);

                var rightMinX = LeftChild.SubtreeBounds.Width + XSpacing;
                RightChild.ArrangeSubtree(minX + rightMinX, childMinY);
                SubtreeBounds = new Rect(
                    minX,
                    minY,
                    rightMinX + RightChild.SubtreeBounds.Width,
                    Math.Max(LeftChild.SubtreeBounds.Height, RightChild.SubtreeBounds.Height) + YSpacing); ;           
            }

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
            void Line(BinaryNode<T> node, BinaryNode<T> child) 
                => canvas.DrawLine(node.Center, child.Center, Brushes.Black, 1);

            TraverseBreadthFirst()
                .ToList()
                .ForEach(n =>
                {
                    if (n.LeftChild != null) Line(n, n.LeftChild);
                    if (n.RightChild != null) Line(n, n.RightChild);
                });
        }

        #endregion

        #region Helpers

        private bool HasEmptyNest() => LeftChild == null && RightChild == null;

        private bool HasOneChild() => LeftChild != null ^ RightChild != null;

        public override string ToString() => ToString(string.Empty);

        public string ToString(string spaces)
        {
            var thisValue = $"{spaces}{Value}:";
            if (LeftChild == null && RightChild == null)
            {
                return thisValue;
            }

            var indentation = $"{spaces}  ";
            var left = GetChildStringOrNone(LeftChild, indentation);
            var right = GetChildStringOrNone(RightChild, indentation);
            return $"{thisValue}\n{left}\n{right}";
        }

        private string GetChildStringOrNone(BinaryNode<T> child, string indentation)
            => child?.ToString(indentation) ?? $"{indentation}None";

        #endregion
    }
}

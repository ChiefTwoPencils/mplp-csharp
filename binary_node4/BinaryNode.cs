namespace binary_node4
{
    public class BinaryNode<T>
    {
        public T Value { get; set; }
        public BinaryNode<T>? LeftChild { get; set; }
        public BinaryNode<T>? RightChild { get; set; }

        public BinaryNode(T value)
        {
            Value = value;
            LeftChild = null;
            RightChild = null;
        }            

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

        public BinaryNode<T>? FindNode(T value) => FindNode(this, value);

        public BinaryNode<T>? FindNode(BinaryNode<T>? root, T value)
        {
            if (root == null || Equals(root.Value, value)) return root;

            return FindNode(root.LeftChild, value)
                ?? FindNode(root.RightChild, value);
        }

        public List<BinaryNode<T>>? TraversePreOrder() => TraversePreOrder(this);

        private List<BinaryNode<T>> TraversePreOrder(BinaryNode<T>? root)
        {
            if (root == null) return new List<BinaryNode<T>>();

            var list = new List<BinaryNode<T>> { root };
            list.AddRange(TraversePreOrder(root.LeftChild));
            list.AddRange(TraversePreOrder(root.RightChild));
            return list;
        }

        public List<BinaryNode<T>> TraverseInOrder() => TraverseInOrder(this);

        private List<BinaryNode<T>> TraverseInOrder(BinaryNode<T>? root)
        {
            if (root == null) return new List<BinaryNode<T>>();

            var list =  TraverseInOrder(root.LeftChild);
            list.Add(root);
            list.AddRange(TraverseInOrder(root.RightChild));
            return list;
        }

        public List<BinaryNode<T>> TraversePostOrder() => TraversePostOrder(this);

        public List<BinaryNode<T>> TraversePostOrder(BinaryNode<T>? root) 
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

        private Queue<BinaryNode<T>> TraverseBreadthFirst(Queue<BinaryNode<T>> queue, BinaryNode<T>? root)
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

        private string GetChildStringOrNone(BinaryNode<T>? child, string indentation)
            => child?.ToString(indentation) ?? $"{indentation}None";
    }
}

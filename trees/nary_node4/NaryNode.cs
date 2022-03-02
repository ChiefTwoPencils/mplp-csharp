namespace nary_node4
{
    public class NaryNode<T>
    {
        public T Value { get; set; }

        public List<NaryNode<T>> Children { get; set; }             

        public NaryNode(T value)
        {
            Value = value;
            Children = new List<NaryNode<T>>();
        }

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

        public NaryNode<T>? FindNode(T value) => FindNode(this, value);

        public NaryNode<T>? FindNode(NaryNode<T>? root, T value)
        {
            if (root == null || Equals(root.Value, value)) return root;

            foreach (var child in root.Children)
            {
                if (child.FindNode(value) != null) return child;
            }

            return null;
        }

        public List<NaryNode<T>> TraversePreOrder() => TraversePreOrder(this);

        private List<NaryNode<T>> TraversePreOrder(NaryNode<T>? root)
        {
            if (root == null) return new List<NaryNode<T>>();

            var list = new List<NaryNode<T>> { root };
            list.AddRange(root.Children.SelectMany(c => TraversePreOrder(c)));
            return list;
        }

        public List<NaryNode<T>> TraversePostOrder() => TraversePostOrder(this);

        private List<NaryNode<T>> TraversePostOrder(NaryNode<T>? root)
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

        private Queue<NaryNode<T>> TraverseBreadthFirst(Queue<NaryNode<T>> queue, List<NaryNode<T>>? children)
        {
            if (children == null || !children.Any()) return queue;

            children.ForEach(c => queue.Enqueue(c));
            TraverseBreadthFirst(queue, children.SelectMany(c => c.Children).ToList());

            return queue;
        }

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
    }
}

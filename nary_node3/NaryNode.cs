namespace nary_node3
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

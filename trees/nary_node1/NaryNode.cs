namespace nary_node1
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

        public override string ToString()
            => $"{Value}: {string.Join(" ", Children.Select(c => c.Value))}"; 
    }
}

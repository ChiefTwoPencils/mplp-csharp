namespace binary_node1
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

        public override string ToString()
        {
            static string ChildValue(BinaryNode<T>? child)
                => child?.Value?.ToString() ?? "null";

            return $"{Value}: {ChildValue(LeftChild)} {ChildValue(RightChild)}";
        }
    }
}

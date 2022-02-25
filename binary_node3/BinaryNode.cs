namespace binary_node3
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

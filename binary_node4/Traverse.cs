namespace binary_node4
{
    internal static class Traverse
    {
        public static void PreOrder<T>(this BinaryNode<T>? root, Action<BinaryNode<T>> action)
        {
            if (root == null) return;

            action(root);
            PreOrder(root.LeftChild, action);
            PreOrder(root.RightChild, action);
        }

        public static void InOrder<T>(this BinaryNode<T>? root, Action<BinaryNode<T>> action)
        {
            if (root == null) return;

            InOrder(root.LeftChild, action);
            action(root);
            InOrder(root.RightChild, action);
        }

        public static void PostOrder<T>(this BinaryNode<T>? root, Action<BinaryNode<T>> action)
        {
            if (root == null) return;

            PostOrder(root.LeftChild, action);
            PostOrder(root.RightChild, action);
            action(root);
        }

        public static void BreadthFirst<T>(this BinaryNode<T>? root, Action<BinaryNode<T>> action)
        {
            if (root == null) return;

            action(root);
            BreadthFirstRec(root, action);
        }

        private static void BreadthFirstRec<T>(BinaryNode<T>? root, Action<BinaryNode<T>> action)
        {
            if (root == null) return;

            if (root.LeftChild != null)
            {
                action(root.LeftChild);
            }

            if (root.RightChild != null)
            {
                action(root.RightChild);
            }

            BreadthFirstRec(root.LeftChild, action);
            BreadthFirstRec(root.RightChild, action);
        }
    }
}

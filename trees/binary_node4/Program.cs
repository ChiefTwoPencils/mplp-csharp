using binary_node4;

var root = new BinaryNode<string>("Root");
var a = new BinaryNode<string>("A");
var b = new BinaryNode<string>("B");
var c = new BinaryNode<string>("C");
var d = new BinaryNode<string>("D");
var e = new BinaryNode<string>("E");
var f = new BinaryNode<string>("F");

root
    .PutLeft(a)
    .AddLeft(c)
    .AddRight(d);

root
    .PutRight(b)
    .PutRight(e)
    .AddLeft(f);

var format = "{0, -15}";

Console.WriteLine("- Traversals using Node collections...");
Traverse("Preorder:", root.TraversePreOrder());
Traverse("Inorder:", root.TraverseInOrder());
Traverse("Postorder:", root.TraversePostOrder());
Traverse("Breadth-first:", root.TraverseBreadthFirst().ToList());

Console.WriteLine();

Console.WriteLine("- Traversals using Actions on Nodes...");
Console.Write(format, "Preorder:");
root.PreOrder(n => Console.Write($"{n.Value} "));
Console.WriteLine();

Console.Write(format, "Inorder:");
root.InOrder(n => Console.Write($"{n.Value} "));
Console.WriteLine();

Console.Write(format, "Postorder:");
root.PostOrder(n => Console.Write($"{n.Value} "));
Console.WriteLine();

Console.Write(format, "Breadth-first:");
root.BreadthFirst(n => Console.Write($"{n.Value} "));
Console.WriteLine();

void Traverse(string type, List<BinaryNode<string>> nodes)
    => Console.WriteLine($"{format}{{1}}", type, string.Join(' ', nodes.Select(n => n.Value)));

using binary_node3;

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

FindValue(root, "Root");
FindValue(root, "E");
FindValue(root, "F");
FindValue(root, "Q");
FindValue(b, "F");

static void FindValue(BinaryNode<string>? node, string value)
{
    var result = node?.FindNode(value) == null
        ? $"Value {value} not found"
        : $"Found {value}";

    Console.WriteLine(result);
}

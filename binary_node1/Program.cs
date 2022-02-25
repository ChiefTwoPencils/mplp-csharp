using binary_node1;

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

new List<BinaryNode<string>>
{
    root, a , b, c, d, e, f
}
.ForEach(Console.WriteLine);

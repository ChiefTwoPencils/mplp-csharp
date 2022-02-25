using nary_node4;

var nroot = new NaryNode<string>("Root");
var na = new NaryNode<string>("A");
var nb = new NaryNode<string>("B");
var nc = new NaryNode<string>("C");
var nd = new NaryNode<string>("D");
var ne = new NaryNode<string>("E");
var nf = new NaryNode<string>("F");
var ng = new NaryNode<string>("G");
var nh = new NaryNode<string>("H");
var ni = new NaryNode<string>("I");

nroot.AddChildren(na, nb, nc);
na.AddChildren(nd, ne);
nd.AddChild(ng);
nc.AddChild(nf);
nf.AddChildren(nh, ni);

var format = "{0, -15}";

Traverse("Preorder:", nroot.TraversePreOrder());
Traverse("Postorder:", nroot.TraversePostOrder());
Traverse("Breadth-first:", nroot.TraverseBreadthFirst().ToList());

void Traverse(string type, List<NaryNode<string>> nodes)
    => Console.WriteLine($"{format}{{1}}", type, string.Join(' ', nodes.Select(n => n.Value)));
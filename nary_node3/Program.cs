using nary_node3;

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

FindValue(nroot, "Root");
FindValue(nroot, "E");
FindValue(nroot, "F");
FindValue(nroot, "Q");
FindValue(nc, "F");

static void FindValue(NaryNode<string>? node, string value)
{
    var result = node?.FindNode(value) == null
        ? $"Value {value} not found"
        : $"Found {value}";

    Console.WriteLine(result);
}
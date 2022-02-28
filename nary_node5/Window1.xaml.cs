using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nary_node5
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Height = 350;
            Width = 500;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Build a test tree.
            // A
            //         |
            //     +---+---+
            // B   C   D
            //     |       |
            //    +-+      +
            // E F      G
            //    |        |
            //    +      +-+-+
            // H      I J K
            NaryNode<string> node_a = new NaryNode<string>("A");
            NaryNode<string> node_b = new NaryNode<string>("B");
            NaryNode<string> node_c = new NaryNode<string>("C");
            NaryNode<string> node_d = new NaryNode<string>("D");
            NaryNode<string> node_e = new NaryNode<string>("E");
            NaryNode<string> node_f = new NaryNode<string>("F");
            NaryNode<string> node_g = new NaryNode<string>("G");
            NaryNode<string> node_h = new NaryNode<string>("H");
            NaryNode<string> node_i = new NaryNode<string>("I");
            NaryNode<string> node_j = new NaryNode<string>("J");
            NaryNode<string> node_k = new NaryNode<string>("K");

            node_a.AddChild(node_b);
            node_a.AddChild(node_c);
            node_a.AddChild(node_d);
            node_b.AddChild(node_e);
            node_b.AddChild(node_f);
            node_d.AddChild(node_g);
            node_e.AddChild(node_h);
            node_g.AddChild(node_i);
            node_g.AddChild(node_j);
            node_g.AddChild(node_k);

            // Draw the tree.
            node_a.ArrangeAndDrawSubtree(mainCanvas, 10, 10);
        }
    }
}

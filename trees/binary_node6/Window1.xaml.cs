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

namespace binary_node6
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Height = 450;
            Width = 500;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Build a test tree.
            //         A
            //        / \
            //       /   \
            //      /     \
            //     B       C
            //    / \     / \
            //   D   E   F   G
            //      / \     /
            //     H   I   J
            //            / \
            //           K   L
            BinaryNode<string> node_a = new BinaryNode<string>("A");
            BinaryNode<string> node_b = new BinaryNode<string>("B");
            BinaryNode<string> node_c = new BinaryNode<string>("C");
            BinaryNode<string> node_d = new BinaryNode<string>("D");
            BinaryNode<string> node_e = new BinaryNode<string>("E");
            BinaryNode<string> node_f = new BinaryNode<string>("F");
            BinaryNode<string> node_g = new BinaryNode<string>("G");
            BinaryNode<string> node_h = new BinaryNode<string>("H");
            BinaryNode<string> node_i = new BinaryNode<string>("I");
            BinaryNode<string> node_j = new BinaryNode<string>("J");
            BinaryNode<string> node_k = new BinaryNode<string>("K");
            BinaryNode<string> node_l = new BinaryNode<string>("L");

            node_a.AddLeft(node_b);
            node_a.AddRight(node_c);
            node_b.AddLeft(node_d);
            node_b.AddRight(node_e);
            node_c.AddLeft(node_f);
            node_c.AddRight(node_g);
            node_e.AddLeft(node_h);
            node_e.AddRight(node_i);
            node_g.AddLeft(node_j);
            node_j.AddLeft(node_k);
            node_j.AddRight(node_l);

            // Arrange and draw the tree.
            node_a.ArrangeAndDrawSubtree(mainCanvas, 10, 10);
        }
    }
}

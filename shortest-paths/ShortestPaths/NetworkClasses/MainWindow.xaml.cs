using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Point = System.Drawing.Point;

namespace NetworkClasses
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void ValidateNetwork(Network network, string path)
        {
            var given = network.Serialize();
            network.SaveToFile(path);
            network.LoadFromFile(path);
            var loaded = network.Serialize();

            netTextBox.Text = loaded;

            statusLabel.Content = given == loaded
                ? "OK"
                : "Serializations do not match";
        }

        private void one_Click(object sender, RoutedEventArgs e)
        {
            var network = new Network();
            var a = new Node(network, new Point(20, 20), "A");
            var b = new Node(network, new Point(120, 120), "B");
            var ab = new Link(network, a, b, 10);

            ValidateNetwork(network, "one.net");
        }

        private void two_Click(object sender, RoutedEventArgs e)
        {
            var network = new Network();
            var a = new Node(network, new Point(20, 20), "A");
            var b = new Node(network, new Point(120, 20), "B");
            var c = new Node(network, new Point(20, 120), "C");
            var d = new Node(network, new Point(120, 120), "D");
            var ab = new Link(network, a, b, 10);
            var bd = new Link(network, b, d, 15);
            var ac = new Link(network, a, c, 20);
            var cd = new Link(network, c, d, 25);

            ValidateNetwork(network, "two.net");
        }

        private void three_Click(object sender, RoutedEventArgs e)
        {
            var network = new Network();
            var a = new Node(network, new Point(20, 20), "A");
            var b = new Node(network, new Point(120, 20), "B");
            var c = new Node(network, new Point(20, 120), "C");
            var d = new Node(network, new Point(120, 120), "D");
            var ab = new Link(network, a, b, 10);
            var bd = new Link(network, b, d, 15);
            var ac = new Link(network, a, c, 20);
            var cd = new Link(network, c, d, 25);
            var ba = new Link(network, b, a, 11);
            var db = new Link(network, d, b, 16);
            var ca = new Link(network, c, a, 21);
            var dc = new Link(network, d, c, 26);

            ValidateNetwork(network, "three.net");
        }
    }
}

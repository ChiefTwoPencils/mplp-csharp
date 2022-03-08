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
using Microsoft.Win32;

namespace DrawingNetworks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Network network;

        public MainWindow()
        {
            InitializeComponent();
            network = new Network();
        }

        private void one_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.DrawRectangle(new Rect(0, 0, 300, 300), Brushes.Black, Brushes.Pink, 1);
            var network = new Network();
            var a = new Node(network, new Point(0, 0), "A");
            var b = new Node(network, new Point(120, 120), "B");
            var ab = new Link(network, a, b, 10);
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
        }

        private void DrawNetwork()
        {
            mainCanvas.Children.Clear();

            network.Draw(mainCanvas);
        }

        private void exit_Click(object sender, RoutedEventArgs e) => Close();

        private void open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.DefaultExt = ".net";
                fileDialog.Filter = "Network Files|*.net|All Files|*.*";

                // Display the dialog.
                bool? result = fileDialog.ShowDialog();
                if (result ?? false)
                {
                    network = new Network(fileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                network = new Network();
            }

            DrawNetwork();
        }
    }
}

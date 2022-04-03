using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace FindFlows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Network network;
        private Network.AlgorithmTypes algorithm;
        private readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            network = new Network();
            BuildGridNetwork("3x3_grid.net", 300, 300, 3, 3);
            BuildGridNetwork("4x4_grid.net", 300, 300, 4, 4);
            BuildGridNetwork("5x8_grid.net", 600, 400, 5, 8);
            BuildGridNetwork("6x10_grid.net", 600, 400, 6, 10);
            BuildGridNetwork("10x15_grid.net", 600, 400, 10, 15);
            BuildGridNetwork("20x30_grid.net", 600, 400, 20, 30);
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
                    network.AlgorithmType = algorithm;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                network = new Network();
            }

            DrawNetwork();
        }

        private double Distance(Point first, Point second)
        {
            var diff = first - second;            
            return Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y, 2));
        }

        private Link MakeRandomizedLink(Network network, (Node, Node) fromTo)
        {
            var min = 1.0;
            var max = 1.2;
            var (from, to) = fromTo;
            var cost = (int) Math.Round(Distance(from.Center, to.Center) * (random.Next(10, 13) * .1f));
            return new Link(network, from, to, cost);
        }

        private void LinkNodes(Network network, (Node, Node) fromTo)
            => LinkNodes(network, fromTo.Item1, fromTo.Item2);

        private void LinkNodes(Network network, Node from, Node to)
        {
            MakeRandomizedLink(network, (from, to));
            MakeRandomizedLink(network, (to, from));
        }

        private Network BuildGridNetwork(string fileName, double width, double height, int rows, int cols)
        {
            const int margin = 10;
            var xspace = (int) Math.Floor((width - 2 * margin) / cols);
            var yspace = (int) Math.Floor((height - 2 * margin) / rows);
            var nodeRows = new List<List<Node>>();
            var grid = new Network();
            var count = 0;

            for (var r = 0; r < rows; r++)
            {
                var x = margin;
                var y = margin + r * yspace;

                var row = new List<Node>();
                for (var c = 0; c < cols; c++)
                {
                    row.Add(new Node(grid, new Point(x, y), $"{++count}"));
                    x += xspace;
                }

                nodeRows.Add(row);
            }            

            var previous = nodeRows.First();
            foreach (var row in nodeRows.Skip(1))
            {
                previous
                    .Zip(row, (p, r) => (p, r))
                    .ToList()
                    .ForEach(pair => LinkNodes(grid, pair));
                previous = row;
            }

            foreach (var row in nodeRows)
            {
                var first = row.First();
                row
                    .Skip(1)
                    .Aggregate(first, (a, b) =>
                    {
                        LinkNodes(grid, (a, b));
                        return b;
                    });
            }

            grid.SaveToFile(fileName);
             
            return grid;
        }

        private void algorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var item = (ComboBoxItem)comboBox.SelectedItem;
            algorithm = item.Content.ToString() == "Label Setting"
                ? Network.AlgorithmTypes.LabelSetting
                : Network.AlgorithmTypes.LabelCorrecting;
        }
    }
}

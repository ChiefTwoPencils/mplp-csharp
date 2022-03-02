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

using System.Diagnostics;
using System.Threading.Tasks;

namespace sorted_binary_node1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private SortedBinaryNode<int> _root;

        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RunTests();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var value = GetValue();
            if (_root == null)
            {
                _root = new SortedBinaryNode<int>(value);
            }
            else
            {
                _root.AddNode(value);
            }

            ClearAndDraw();
        }

        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            if (_root != null)
            {
                var value = GetValue();
                var caption = "Find Node";
                var message = $"We found {value}";
                var button = MessageBoxButton.OK;
                var icon = MessageBoxImage.None;

                var result = _root.FindNode(value);

                if (result == null)
                {
                    message = $"No such value: {value}";
                    icon = MessageBoxImage.Error;
                }

                MessageBox.Show(message, caption, button, icon);
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_root != null)
            {
                _root = _root.RemoveNode(GetValue());

                if (_root == null) resetButton_Click(sender, e);
                else ClearAndDraw();
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            _root = null;
            Clear();
        }

        private int GetValue()
        {
            if (int.TryParse(ValueTextBox.Text, out int value))
            {
                return value;
            }

            throw new ArgumentException("Textbox must have an integer value.");
        }

        private void Clear()
        {
            mainCanvas.Children.Clear();
            ValueTextBox.Text = string.Empty;
        }

        private void ClearAndDraw()
        {
            Clear();
            _root.ArrangeAndDrawSubtree(mainCanvas, 10, 10);
        }

        private void RunTests()
        {
            var values = new List<int>() 
            { 60, 35, 76, 21, 42, 71, 89, 17, 24, 74, 11, 23, 72, 75 };

            _root = new SortedBinaryNode<int>(values.First());

            values
                .Skip(1)
                .ToList()
                .ForEach(n => _root.AddNode(n));

            TestAdd(values);
            TestFind(values);
            TestRemove(values);

            ClearAndDraw();
        }

        private void TestAdd(List<int> expected)
        {
            var actual = _root.TraverseBreadthFirst()
                .Select(n => n.Value);

            var sameSize = actual.Count() == expected.Count;

            var all = ZipCheck(actual, expected);

            if (!(sameSize && all)) throw new Exception("Add FAILED!");
        }

        private void TestFind(List<int> expected)
        {
            var anyNull = expected
                .Select(i => _root.FindNode(i))
                .Any(n => n == null);

            if (anyNull) throw new Exception("Find FAILED!");
        }

        private void TestRemove(List<int> initials)
        {
            var removals = new List<int> { 42, 35, 71, 21, 60, 76 };

            removals.ForEach(n => _root.RemoveNode(n));

            var actual = _root.TraverseBreadthFirst()
                .Select(n => n.Value)
                .ToList();

            var expected = initials
                .Where(v => !removals.Contains(v));

            var sameSize = actual.Count == expected.Count();

            var all = ZipCheck(expected, actual);

            if (!(sameSize && all)) throw new Exception("Remove FAILED!");
        }

        private bool ZipCheck(IEnumerable<int> first, List<int> second)
            => first.Zip(second, (f, s) => f == s).All(b => true);
    }
}

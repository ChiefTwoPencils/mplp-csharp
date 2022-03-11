using Microsoft.Win32;
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

namespace TopologicalSorting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Task> unsortedTasks = new();
        private PoSorter sorter = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) => Close();

        private void sortButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedTasks = sorter.TopoSort(unsortedTasks);
            sortedListBox.ItemsSource = sortedTasks;

            var sortedCount = sortedTasks.Count;
            var unsortedCount = unsortedTasks.Count;
            var stats = $"{sortedCount} of {unsortedCount}";

            var result = sortedCount == unsortedCount
                ? "Successfully"
                : "Only";

            var message = $"{result} sorted {stats}.";
            var caption = "Sort Results";
            var button = MessageBoxButton.OK;
            var icon = MessageBoxImage.Information;
            MessageBox.Show(message, caption, button, icon);
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
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
                    unsortedTasks = sorter.LoadPoFile(fileDialog.FileName);
                    unsortedListBox.ItemsSource = unsortedTasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            sortButton.IsEnabled = true;
        }
    }
}

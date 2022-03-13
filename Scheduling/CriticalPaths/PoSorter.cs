using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CriticalPaths
{
    internal class PoSorter
    {
        internal List<List<Task>> Columns { get; private set; } = new();
        internal List<Task> TopoSort(List<Task> tasks)
        {
            PrepareTasks(tasks); 

            // Create an empty SortedTasks list.
            var sortedTasks = new List<Task>();

            // Create a readyTasks queue.
            // Add tasks with no prerequisites to readyTasks.
            var readyTasks = new Queue<Task>(tasks.Where(t => !t.PrereqTasks.Any()));

            // While readyTasks is not empty:
            while (readyTasks.Count > 0)
            {
                // Remove a task from the list. Call it readyTask.
                var readyTask = readyTasks.Dequeue();
                readyTask.Index = sortedTasks.Count;
                sortedTasks.Add(readyTask);

                // Loop through readyTask’s followers:
                readyTask.Followers
                    .ForEach(task =>
                    {
                        // Decrement the follower’s prereqCount.
                        // If the follower’s prereqCount is now 0, add it to readyTasks.
                        if ((--task.PrereqCount) == 0) readyTasks.Enqueue(task);                        
                    });                
            }

            VerifySort(sortedTasks);
            return sortedTasks;
        }

        internal List<List<Task>> BuildPertChart(List<Task> tasks)
        {
            PrepareTasks(tasks);
            var finish = tasks.Last();

            // Create an empty SortedTasks list.
            var sortedColumns = new List<List<Task>>();

            // Create a readyTasks queue.
            // Add tasks with no prerequisites to readyTasks.
            var readyTasks = new Queue<Task>(tasks.Where(t => !t.PrereqTasks.Any()));
            var newReadyTasks = new Queue<Task>();

            // While readyTasks is not empty:
            var batchList = new List<Task>();
            while (readyTasks.Count > 0)
            {
                // Remove a task from the list. Call it readyTask.
                var readyTask = readyTasks.Dequeue();
                readyTask.SetTimes();
                batchList.Add(readyTask);

                // Loop through readyTask’s followers:
                readyTask.Followers
                    .ForEach(task =>
                    {
                        // Decrement the follower’s prereqCount.
                        // If the follower’s prereqCount is now 0, add it to readyTasks.
                        if ((--task.PrereqCount) == 0) newReadyTasks.Enqueue(task);
                    });

                if(!readyTasks.Any())
                {
                    readyTasks = newReadyTasks;
                    sortedColumns.Add(batchList);
                    batchList = new List<Task>();
                    newReadyTasks = new Queue<Task>();
                }
            }

            finish.MarkCritical();
            Columns = sortedColumns;
            return Columns;
        }

        internal void DrawPertChart(Canvas canvas)
        {
            // Clear the canvas
            canvas.Children.Clear();

            const int margin = 20;
            const int xspace = 50; 
            const int yspace = 10;
            const int width = 60;
            const int height = 90;

            // Set each task in each column's bounds
            var drawables = new List<Task>();
            for (var i = 0; i < Columns.Count; i++)
            {
                var x = margin + i * (width + xspace);
                var y = yspace;

                foreach (var element in Columns[i])
                {                    
                    var bounds = new Rect(new Point(x, y), new Point(x + width, y + height));
                    element.Bounds = bounds;
                    drawables.Add(element);
                    y += height + yspace;
                }
            }

            // Draw lines from each task to its prerequisites
            drawables
                .ForEach(drawable => drawable.PrereqTasks
                .ForEach(prereq =>
                {
                    var isTaskCritical = drawable.StartTime == prereq.EndTime;
                    var pixels = isTaskCritical  ? 3 : 1;
                    var from = new Point(drawable.Bounds.Left, drawable.Center.Y);
                    var to = new Point(prereq.Bounds.Right, prereq.Center.Y);
                    var color = isTaskCritical && drawable.IsCritical && prereq.IsCritical
                        ? Brushes.Red
                        : Brushes.Black;

                    canvas.DrawLine(from, to, color, pixels);
                }));


            // Use DrawString/Rectangle to draw the task and its index
            drawables.ForEach(drawable => DrawTask(canvas, drawable));
        }

        internal void VerifySort(List<Task> sortedTasks)
        {
            var isSorted = sortedTasks
                .All(t => t.PrereqTasks
                    .All(pt => pt.Index < t.Index));

            if (!isSorted) throw new Exception("Sort could not be verified.");
        }

        internal Task? ReadTask(StreamReader reader)
        {
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                if (line == string.Empty) continue;

                var parts = line
                    .Split(new[] { ',', '[', ']' }, StringSplitOptions.TrimEntries)
                    .Where(s => s != string.Empty)
                    .ToList();

                var index = int.Parse(parts[0]);
                var duration = int.Parse(parts[1]);
                var name = parts[2];
                var prereqNumbers = parts
                    .ToArray()[3..]
                    .Select(int.Parse)
                    .ToList();

                return new Task(name, index, duration, prereqNumbers);
            }

            return null;
        }

        internal List<Task> LoadPoFile(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            using var reader = new StreamReader(stream);

            var taskList = new List<Task>();
            while (true)
            {
                var task = ReadTask(reader);
                if (task == null) break;
                taskList.Add(task);
            }

            taskList.ForEach(t => t.NumbersToTasks(taskList));

            return taskList;
        }

        private void PrepareTasks(List<Task> tasks)
        {
            // Initialize the tasks:                
            tasks.ForEach(task =>
            {
                // Give each task a followers list that holds
                // references to the tasks that must follow it.
                // I.e. this task is a prerequisite for tasks in the followers list.
                task.PrereqTasks.ForEach(prereq => prereq.Followers.Add(task));

                // Set PrereqCount to the number of prerequisites this task has.
                task.ResetPrereqCount();

                // Reset critical
                task.IsCritical = false;
            });
        }

        private void DrawTask(Canvas canvas, Task drawable)
        {
            var width = drawable.Bounds.Width;
            var height = drawable.Bounds.Height;

            var index = $"Task: {drawable.Index}";
            var duration = $"Dur: {drawable.Duration}";
            var start = $"Start: {drawable.StartTime}";
            var end = $"End: {drawable.EndTime}";

            var lines = string.Join("\n", new List<string> { index, duration, start, end });

            var fill = Brushes.LightBlue;
            var stroke = Brushes.Black;

            if (drawable.IsCritical)
            {
                fill = Brushes.Pink;
                stroke = Brushes.Red;
            }

            canvas.DrawRectangle(drawable.Bounds, fill, stroke, 1);
            canvas.DrawString(lines, width, height, drawable.Center, 0, 12, stroke);
        }
    }
}

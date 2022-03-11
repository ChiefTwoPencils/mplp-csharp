using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologicalSorting
{
    internal class PoSorter
    {
        internal List<Task> TopoSort(List<Task> tasks)
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
            });            

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
                var name = parts[1];
                var prereqNumbers = parts
                    .ToArray()[2..]
                    .Select(int.Parse)
                    .ToList();

                return new Task(name, index, prereqNumbers);
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prereqs = System.Collections.Generic.List<CriticalPaths.Task>;
using Followers = System.Collections.Generic.List<CriticalPaths.Task>;
using System.Windows;

namespace CriticalPaths
{
    internal class Task
    {
        internal string Name { get; set; }
        internal int Index { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        internal int Duration { get; private set; }
        internal bool IsCritical { get; set; }
        internal int PrereqCount { get; set; }
        public List<int> PrereqNumbers { get; set; } = new();
        internal Prereqs PrereqTasks  { get; set; } = new ();
        internal Followers Followers { get; set; } = new ();

        private Rect _bounds;
        internal Rect Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;

                var halfWidth = _bounds.Width / 2;
                var halfHeight = _bounds.Height / 2;
                var x = _bounds.Left + halfWidth;
                var y = _bounds.Top + halfHeight;

                Center = new Point(x, y);
            }
        }
        internal Point Center { get; private set; }

        internal Task(string name, int index, int duration, List<int> prereqNumbers)
        {
            Name = name;
            Index = index;
            Duration = duration;
            PrereqNumbers = prereqNumbers;
        }

        internal void NumbersToTasks(List<Task> tasks)
            => PrereqTasks = PrereqNumbers
            .Select(n => tasks[n])
            .ToList();

        internal void ResetPrereqCount() => PrereqCount = PrereqTasks.Count;

        internal void SetTimes()
        {
            StartTime = 0;
            PrereqTasks.ForEach(task =>
            {
                StartTime = Math.Max(StartTime, task.EndTime);
                task.EndTime = task.StartTime + task.Duration;
            });
            EndTime = StartTime + Duration;
        }

        internal void MarkCritical()
        {
            IsCritical = true;
            PrereqTasks
                .Where(pt => pt.EndTime == StartTime)
                .ToList()
                .ForEach(pt => pt.MarkCritical());
        }

        public override string ToString() => Name;
    }
}

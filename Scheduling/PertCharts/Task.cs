using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prereqs = System.Collections.Generic.List<PertCharts.Task>;
using Followers = System.Collections.Generic.List<PertCharts.Task>;
using System.Windows;

namespace PertCharts
{
    internal class Task
    {
        internal string Name { get; set; }
        internal int Index { get; set; }
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

        internal Task(string name, int index, List<int> prereqNumbers)
        {
            Name = name;
            Index = index;
            PrereqNumbers = prereqNumbers;
        }

        internal void NumbersToTasks(List<Task> tasks)
            => PrereqTasks = PrereqNumbers
            .Select(n => tasks[n])
            .ToList();

        internal void ResetPrereqCount() => PrereqCount = PrereqTasks.Count;

        public override string ToString() => Name;
    }
}

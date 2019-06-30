using FlowScriptDrawControl.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FlowScriptDrawControl.Model
{
    class PositionAdjuster
    {
        private SourceControl[] sources;
        private Dictionary<Source, SourceControl> sourceDict;
        private Command.CommandManager commandManager;

        public PositionAdjuster(SourceControl[] sources, Command.CommandManager commandManager)
        {
            this.sources = sources;
            this.commandManager = commandManager;
            sourceDict = sources.ToDictionary(s => s.CurrentSource, s => s);
        }

        public void Adjust()
        {
            var points = sources.ToDictionary(s => s, s => 0);
            foreach (SourceControl source in sources)
            {
                IncrementPoint(points, source);
            }

            var pointList = new Dictionary<int, List<SourceControl>>();
            foreach (KeyValuePair<SourceControl, int> pair in points)
            {
                if (!pointList.TryGetValue(pair.Value, out List<SourceControl> srcs))
                {
                    srcs = new List<SourceControl>();
                    pointList.Add(pair.Value, srcs);
                }
                srcs.Add(pair.Key);
            }

            var positions = sources.ToDictionary(s => s, s => s.CurrentSource.Position);
            foreach (KeyValuePair<int, List<SourceControl>> pair in pointList.OrderByDescending(p => p.Key))
            {
                Reverse(pair.Value[0], positions);
            }
        }

        private void Reverse(SourceControl source, Dictionary<SourceControl, Point> positions)
        {
            var widths = new Dictionary<int, double>();
            Reverse(0, source, new HashSet<SourceControl>(), widths, new Dictionary<int, double>(), positions);
            var posXs = new Dictionary<int, double>();
            double sum = 0;
            foreach (KeyValuePair<int, double> pair in widths.OrderBy(p => p.Key))
            {
                posXs[pair.Key] = sum;
                sum += pair.Value + 30;
            }
            SetWidth(0, source, new HashSet<SourceControl>(), posXs, positions);
            var selectables = new List<Selectable>();
            var tempPositions = new List<Point>();
            foreach (KeyValuePair<SourceControl, Point> pair in positions)
            {
                selectables.Add(pair.Key.CurrentSource);
                tempPositions.Add(pair.Value);
            }
            var command = new Command.MovePositionablesCommand(selectables.ToArray(), tempPositions.ToArray());
            commandManager.AddCommand(command);
        }

        private void SetWidth(int level, SourceControl source, HashSet<SourceControl> usedList,
            Dictionary<int, double> posXs, Dictionary<SourceControl, Point> positions)
        {
            positions[source] = new Point(posXs[level], positions[source].Y);
            usedList.Add(source);

            foreach (Item item in source.CurrentSource.InItems)
            {
                if (item.InConnection != null)
                {
                    if (usedList.Contains(sourceDict[item.InConnection.Target.Source]))
                    {
                        continue;
                    }

                    SetWidth(level - 1, sourceDict[item.InConnection.Target.Source], usedList, posXs, positions);
                }
            }
            foreach (Item item in source.CurrentSource.OutItems)
            {
                foreach (Connection connection in item.OutConnections)
                {
                    if (usedList.Contains(sourceDict[connection.Target.Source]))
                    {
                        continue;
                    }

                    SetWidth(level + 1, sourceDict[connection.Target.Source], usedList, posXs, positions);
                }
            }
        }

        private void Reverse(int level, SourceControl source, HashSet<SourceControl> usedList,
            Dictionary<int, double> widths, Dictionary<int, double> heights, Dictionary<SourceControl, Point> positions)
        {
            usedList.Add(source);
            if (!heights.ContainsKey(level))
            {
                positions[source] = new Point(level, 0);
                heights[level] = source.ActualHeight + 5;
            }
            else
            {
                positions[source] = new Point(level * 100, heights[level]);
                heights[level] += source.ActualHeight + 5;
            }
            if (!widths.ContainsKey(level))
            {
                widths.Add(level, source.ActualWidth);
            }
            else
            {
                widths[level] = Math.Max(widths[level], source.ActualWidth);
            }

            foreach (Item item in source.CurrentSource.InItems)
            {
                if (item.InConnection != null)
                {
                    if (usedList.Contains(sourceDict[item.InConnection.Target.Source]))
                    {
                        continue;
                    }

                    Reverse(level - 1, sourceDict[item.InConnection.Target.Source], usedList, widths, heights, positions);
                }
            }
            foreach (Item item in source.CurrentSource.OutItems)
            {
                foreach (Connection connection in item.OutConnections)
                {
                    if (usedList.Contains(sourceDict[connection.Target.Source]))
                    {
                        continue;
                    }

                    Reverse(level + 1, sourceDict[connection.Target.Source], usedList, widths, heights, positions);
                }
            }
        }

        private void IncrementPoint(Dictionary<SourceControl, int> points, SourceControl source)
        {
            IncrementPoint(points, source, new HashSet<SourceControl>());
        }

        private void IncrementPoint(Dictionary<SourceControl, int> points, SourceControl source, HashSet<SourceControl> usedList)
        {
            usedList.Add(source);
            points[source] += 1;
            foreach (Item item in source.CurrentSource.InItems)
            {
                if (item.InConnection != null)
                {
                    if (usedList.Contains(sourceDict[item.InConnection.Target.Source]))
                    {
                        continue;
                    }

                    IncrementPoint(points, sourceDict[item.InConnection.Target.Source], usedList);
                }
            }
        }
    }
}

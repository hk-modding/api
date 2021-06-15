using UnityEngine.UI;
using System.Collections.Generic;

namespace Modding.Menu
{
    /// <summary>
    /// An interface that assembles <c>Selectable</c>s into a coherent navigation graph.
    /// </summary>
    public interface INavigationGraph
    {
        /// <summary>
        /// Registers a <c>Selectable</c> into the current navigation graph.
        /// </summary>
        /// <param name="selectable">The selectable to add.</param>
        public void AddNavigationNode(Selectable selectable);

        /// <summary>
        /// Builds the currently registered navigation nodes into a complete graph
        /// and returns the selectable to start selected, or null if there are none.<br/>
        /// If the navigation graph implementation sets it in place this method may not do anything.
        /// </summary>
        public Selectable BuildNavigation();
    }

    /// <summary>
    /// A navigation graph that does nothing.
    /// </summary>
    public struct NullNavigationGraph : INavigationGraph
    {
        /// <summary>
        /// Do nothing with the passed in selectable.
        /// </summary>
        /// <param name="selectable"></param>
        public void AddNavigationNode(Selectable selectable) { }

        /// <inheritdoc/>
        public Selectable BuildNavigation() => null;
    }

    /// <summary>
    /// A navigation graph that chains selectables like a circular linked list.
    /// </summary>
    public class ChainedNavGraph : INavigationGraph
    {
        private Selectable first;
        private Selectable last;
        private ChainDir dir;

        /// <summary>
        /// Creates a new chained navigation graph.
        /// </summary>
        /// <param name="dir">The direction to place successive selectables.</param>
        public ChainedNavGraph(ChainDir dir = ChainDir.Down)
        {
            this.dir = dir;
        }

        /// <inheritdoc/>
        public void AddNavigationNode(Selectable selectable)
        {
            if (first == null)
            {
                first = selectable;
                last = selectable;
                first.navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit
                };
                return;
            }
            switch (this.dir)
            {
                // idk how to make this code better
                case ChainDir.Down:
                    {
                        selectable.navigation = new Navigation
                        {
                            selectOnDown = this.first,
                            selectOnUp = this.last,
                            mode = Navigation.Mode.Explicit
                        };

                        var firstNav = this.first.navigation;
                        firstNav.selectOnUp = selectable;
                        this.first.navigation = firstNav;

                        var lastNav = this.last.navigation;
                        lastNav.selectOnDown = selectable;
                        this.last.navigation = lastNav;

                        this.last = selectable;
                    }
                    break;
                case ChainDir.Up:
                    {
                        selectable.navigation = new Navigation
                        {
                            selectOnUp = this.first,
                            selectOnDown = this.last,
                            mode = Navigation.Mode.Explicit
                        };

                        var firstNav = this.first.navigation;
                        firstNav.selectOnDown = selectable;
                        this.first.navigation = firstNav;

                        var lastNav = this.last.navigation;
                        lastNav.selectOnUp = selectable;
                        this.last.navigation = lastNav;

                        this.last = selectable;
                    }
                    break;
                case ChainDir.Right:
                    {
                        selectable.navigation = new Navigation
                        {
                            selectOnRight = this.first,
                            selectOnLeft = this.last,
                            mode = Navigation.Mode.Explicit
                        };

                        var firstNav = this.first.navigation;
                        firstNav.selectOnLeft = selectable;
                        this.first.navigation = firstNav;

                        var lastNav = this.last.navigation;
                        lastNav.selectOnRight = selectable;
                        this.last.navigation = lastNav;

                        this.last = selectable;
                    }
                    break;
                case ChainDir.Left:
                    {
                        selectable.navigation = new Navigation
                        {
                            selectOnLeft = this.first,
                            selectOnRight = this.last,
                            mode = Navigation.Mode.Explicit
                        };

                        var firstNav = this.first.navigation;
                        firstNav.selectOnRight = selectable;
                        this.first.navigation = firstNav;

                        var lastNav = this.last.navigation;
                        lastNav.selectOnLeft = selectable;
                        this.last.navigation = lastNav;

                        this.last = selectable;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public Selectable BuildNavigation() => this.first;

        /// <summary>
        /// The direction to chain selectables.
        /// </summary>
        public enum ChainDir
        {
            /// <summary>
            /// Place successive selectables downwards.
            /// </summary>
            Down,
            /// <summary>
            /// Place successive selectables upwards.
            /// </summary>
            Up,
            /// <summary>
            /// Place successive selectables rightwards.
            /// </summary>
            Right,
            /// <summary>
            /// Place successive selectables leftwards.
            /// </summary>
            Left
        }
    }

    /// <summary>
    /// A navigation graph that connects selectables in a grid.
    /// </summary>
    public class GridNavGraph : INavigationGraph
    {
        /// <summary>
        /// The number of columns in the current row.
        /// </summary>
        public int Columns { get; private set; }

        private List<List<Selectable>> grid;

        /// <summary>
        /// Creates a new grid navigation graph.
        /// </summary>
        /// <param name="columns">The number of columns in the grid.</param>
        public GridNavGraph(int columns)
        {
            this.Columns = columns;
            this.grid = new List<List<Selectable>>() { new List<Selectable>() };
        }

        /// <summary>
        /// Starts a new row and changes the number of columns in the subsequent grid rows.
        /// </summary>
        /// <param name="columns">The new number of columns.</param>
        public void ChangeColumns(int columns)
        {
            this.Columns = columns;
            this.grid.Add(new List<Selectable>());
        }

        /// <inheritdoc/>
        public void AddNavigationNode(Selectable selectable)
        {
            var end = this.grid[this.grid.Count - 1];
            if (end == null || end.Count >= Columns)
            {
                end = new List<Selectable>();
                this.grid.Add(end);
            }
            end.Add(selectable);
        }

        /// <inheritdoc/>
        public Selectable BuildNavigation()
        {
            for (var i = 0; i < this.grid.Count; i++)
            {
                var upRow = GetWrapped(this.grid, i - 1);
                var thisRow = this.grid[i];
                var downRow = GetWrapped(this.grid, i + 1);

                var singleUp = upRow.Count != thisRow.Count ? upRow[0] : null;
                var singleDown = downRow.Count != thisRow.Count ? downRow[0] : null;

                for (var c = 0; c < thisRow.Count; c++)
                {
                    var thisNode = thisRow[c];
                    thisNode.navigation = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                        selectOnUp = singleUp ?? upRow[c],
                        selectOnDown = singleDown ?? downRow[c],
                        selectOnLeft = GetWrapped(thisRow, c - 1),
                        selectOnRight = GetWrapped(thisRow, c + 1)
                    };
                }
            }
            return this.grid[0]?[0];
        }

        private static T GetWrapped<T>(List<T> list, int index)
        {
            if (index == -1)
            {
                return list[list.Count - 1];
            }
            else if (index == list.Count)
            {
                return list[0];
            }
            else
            {
                return list[index];
            }
        }
    }
}
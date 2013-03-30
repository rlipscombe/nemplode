using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ViewTags
{
    public class DataGridBehaviors
    {
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.RegisterAttached(
                "BindableColumns",
                typeof (ObservableCollection<DataGridColumn>),
                typeof (DataGridBehaviors),
                new UIPropertyMetadata(null, BindableColumnsPropertyChanged));

        private static void BindableColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid == null)
                return;

            NotifyCollectionChangedEventHandler columnsCollectionChanged =
                (sender, nccea) => ColumnsCollectionChanged(dataGrid, sender, nccea);

            dataGrid.Columns.Clear();

            var oldColumns = e.OldValue as ObservableCollection<DataGridColumn>;
            if (oldColumns != null)
                oldColumns.CollectionChanged -= columnsCollectionChanged;

            var newColumns = e.NewValue as ObservableCollection<DataGridColumn>;
            if (newColumns != null)
            {
                foreach (var column in newColumns)
                {
                    dataGrid.Columns.Add(column);
                }

                newColumns.CollectionChanged += columnsCollectionChanged;
            }
        }

        private static void ColumnsCollectionChanged(DataGrid dataGrid, object sender,
                                                     NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    dataGrid.Columns.AddRange(e.NewItems.Cast<DataGridColumn>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (DataGridColumn dataGridColumn in e.NewItems)
                    {
                        dataGrid.Columns.Remove(dataGridColumn);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    dataGrid.Columns[e.NewStartingIndex] = (DataGridColumn) e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Move:
                    dataGrid.Columns.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        dataGrid.Columns.Clear();
                        dataGrid.Columns.AddRange(e.NewItems.Cast<DataGridColumn>());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ObservableCollection<DataGridColumn> GetBindableColumns(UIElement element)
        {
            return (ObservableCollection<DataGridColumn>) element.GetValue(BindableColumnsProperty);
        }

        public static void SetBindableColumns(UIElement element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }
    }
}
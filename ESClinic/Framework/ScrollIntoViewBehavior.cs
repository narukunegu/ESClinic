using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ESClinic.Framework
{
    public class ScrollIntoViewBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private static void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            if (dataGrid.SelectedItem != null)
            {
                dataGrid.Dispatcher.BeginInvoke((Action)(() =>
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem, null);
                }));
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;
        }
    }
}
using System.Windows.Controls;
using System.Windows;

namespace MesAdmin.Common.Common
{
    public class ColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Column column = (Column)item;

            return (DataTemplate)((Control)container).FindResource(column.Settings + "ColumnTemplate");

        }
    }

    public enum SettingsType { Default, Combo, Image, DateTime }
    public class Column
    {
        public string FieldName { get; set; }
        public string Header { get; set; }
        public int Width { get; set; }
        public string Alignment { get; set; }
        public SettingsType Settings { get; set; }
    }
}

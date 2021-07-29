using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using System.Data.Common;
using System.ComponentModel;

namespace MesAdmin.Common.Common
{
    public class CellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            GridCellData data = (GridCellData)item;
            PropertyDescriptor property = TypeDescriptor.GetProperties(data.Data)["Editor"];
            if (property == null)
                return null;

            string editorType = TypeDescriptor.GetProperties(data.Data)["Editor"].GetValue(data.Data) as string;

            if (editorType == "Pass/Fail")
                return (DataTemplate)((FrameworkElement)container).FindResource("PassFailTemplate");
            else return null;
        }
    }
}

using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid.TreeList;

namespace MesAdmin.Common.Utils
{
    class HiddenTreeListEditorEventArgsConverter : EventArgsConverterBase<TreeListEditorEventArgs>
    {
        protected override object Convert(object sender, TreeListEditorEventArgs e)
        {
            return new HiddenTreeListEditorEvent { sender = sender, e = e };
        }
    }

    public class HiddenTreeListEditorEvent
    {
        public object sender;
        public TreeListEditorEventArgs e;
    }
}
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace MesAdmin.Common.Utils
{
    class HiddenEditorEventArgsConverter : EventArgsConverterBase<EditorEventArgs>
    {
        protected override object Convert(object sender, EditorEventArgs e)
        {
            return new HiddenEditorEvent { sender = sender, e = e };
        }
    }

    public class HiddenEditorEvent
    {
        public object sender;
        public EditorEventArgs e;
    }
}

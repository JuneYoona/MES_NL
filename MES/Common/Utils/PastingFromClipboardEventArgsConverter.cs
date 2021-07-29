using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace MesAdmin.Common.Utils
{
    class PastingFromClipboardEventArgsConverter : EventArgsConverterBase<PastingFromClipboardEventArgs>
    {
        protected override object Convert(object sender, PastingFromClipboardEventArgs e)
        {
            return new PastingFromClipboardEvent { sender = sender, e = e };
        }
    }

    public class PastingFromClipboardEvent
    {
        public object sender;
        public PastingFromClipboardEventArgs e;
    }
}

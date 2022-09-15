using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace MesAdmin.Common.Utils
{
    class CellMergeEventArgsConverter : EventArgsConverterBase<CellMergeEventArgs>
    {
        protected override object Convert(object sender, CellMergeEventArgs e)
        {
            return new CellMergeEvent { sender = sender, e = e };
        }
    }

    public class CellMergeEvent
    {
        public object sender;
        public CellMergeEventArgs e;
    }
}

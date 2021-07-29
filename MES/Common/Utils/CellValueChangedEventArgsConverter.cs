using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace MesAdmin.Common.Utils
{
    class CellValueChangedEventArgsConverter : EventArgsConverterBase<CellValueChangedEventArgs>
    {
        protected override object Convert(object sender, CellValueChangedEventArgs e)
        {
            return new CellValueChangedEvent { sender = sender, e = e };
        }
    }

    public class CellValueChangedEvent
    {
        public object sender;
        public CellValueChangedEventArgs e;
    }
}

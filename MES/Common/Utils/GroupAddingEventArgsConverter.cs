using DevExpress.Mvvm.UI;
using DevExpress.Xpf.NavBar;

namespace MesAdmin.Common.Utils
{
    class GroupAddingEventArgsConverter : EventArgsConverterBase<GroupAddingEventArgs>
    {
        protected override object Convert(object sender, GroupAddingEventArgs e)
        {
            return new GroupAddingEvent { sender = sender, e = e };
        }
    }

    public class GroupAddingEvent
    {
        public object sender;
        public GroupAddingEventArgs e;
    }
}

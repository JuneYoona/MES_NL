using DevExpress.Xpf.Editors.DateNavigator;
using DevExpress.Xpf.Editors;

namespace MesAdmin.Common.CustomControl
{
    /// <summary>
    /// CalendarYears.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CalendarYears : DateNavigator
    {
        public CalendarYears()
        {
            InitializeComponent();
        }
        protected override void CalendarViewChanged(DateNavigatorCalendarView oldState, DateNavigatorCalendarView newState)
        {
            if (newState == DateNavigatorCalendarView.Year)
            {
                newState = DateNavigatorCalendarView.Years;
            }
            base.CalendarViewChanged(oldState, newState);
        }

        protected override void OnCalendarButtonClick(object sender, DevExpress.Xpf.Editors.DateNavigator.Controls.DateNavigatorCalendarButtonClickEventArgs e)
        {
            base.OnCalendarButtonClick(sender, e);
            var editor = this.GetValue(BaseEdit.OwnerEditProperty) as PopupBaseEdit;
            if (this.CalendarView == DateNavigatorCalendarView.Years && editor != null)
                editor.ClosePopup();
        }
    }
}

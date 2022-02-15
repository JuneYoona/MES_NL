using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Docking.VisualElements;
using System.Windows;
using System.Windows.Input;

namespace MesAdmin.Common.Utils
{
    public class MouseOverBehavior : Behavior<DocumentPaneItem>
    {
        public static bool GetIsMouseOver(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseOverProperty);
        }

        public static void SetIsMouseOver(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMouseOverProperty, value);
        }

        public static readonly DependencyProperty IsMouseOverProperty =
            DependencyProperty.RegisterAttached("IsMouseOver", typeof(bool), typeof(MouseOverBehavior), new PropertyMetadata(false));


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            var dataContext = this.AssociatedObject.DataContext as DependencyObject;
            if (dataContext != null)
                SetIsMouseOver(dataContext, false);
        }

        void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            var dataContext = this.AssociatedObject.DataContext as DependencyObject;
            if (dataContext != null)
                SetIsMouseOver(dataContext, true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            base.OnDetaching();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Docking;

namespace MesAdmin
{
    /// <summary>
    /// MainView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
            if (treeViewItem != null)
            {
                Models.NetMenu menu = treeViewItem.Header as Models.NetMenu;
                if (menu != null)
                {
                    menu.IsExpanded = true;
                    Properties.Settings.Default.ExpandedMenus.Add(menu.MenuId.ToString().ToUpper());
                }
            }
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
            if (treeViewItem != null)
            {
                Models.NetMenu menu = treeViewItem.Header as Models.NetMenu;
                if (menu != null)
                {
                    menu.IsExpanded = false;
                    Properties.Settings.Default.ExpandedMenus.Remove(menu.MenuId.ToString().ToUpper());
                }
            }
        }
    }
}

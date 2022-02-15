﻿using System;
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

namespace MesAdmin.Views
{
    /// <summary>
    /// ProductionWorkOrderBAC90View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BAC90PP001C : UserControl
    {
        public BAC90PP001C()
        {
            InitializeComponent();
            DataContext = new ViewModels.BAC90PP001CVM();
        }
    }
}
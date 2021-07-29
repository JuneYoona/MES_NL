﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Mvvm;

namespace MesAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DevExpress.Xpf.Core.ThemedWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand HelpCmd { get; set; }

        public MainWindowViewModel()
        {
            HelpCmd = new DelegateCommand(OnHelp);
        }

        public void OnHelp()
        {
            System.Windows.Forms.Help.ShowHelp(null, System.Windows.Forms.Application.StartupPath + @"\MES-NL.chm");
        }
    }
}

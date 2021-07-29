using System;
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
using MesAdmin.Models;

namespace MesAdmin.Common.CustomControl
{
    /// <summary>
    /// Equipment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Equipment : UserControl
    {
        private string _eqpCode;
        private string _status;
        private string _lt;
        private string _pause;

        public string EqpCode
        {
            get { return _eqpCode; }
            set
            {
                _eqpCode = value;
                lblCode.Content = value;
            }
        }
        public string LeadTime
        {
            get { return _lt; }
            set
            {
                _lt = value;
                lblLt.Content = value;
            }
        }
        public string Pause
        {
            get { return _pause; }
            set
            {
                _pause = value;
                lblPause.Content = value;
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                switch (_status)
                {
                    case "S":
                        Border.Background = Brushes.LightSteelBlue;
                        this.lblStatus.Content = "Working..";
                        break;
                    case "W":
                        Border.Background = Brushes.White;
                        this.lblStatus.Content = "Stopping..";
                        this.lblLt.Foreground = Brushes.White;
                        this.lblPause.Foreground = Brushes.White;
                        this.lbl1.Foreground = Brushes.White;
                        this.lbl2.Foreground = Brushes.White;
                        break;
                    default:
                        Border.Background = Brushes.LightSalmon;
                        this.lblStatus.Content = "Pausing..";
                        break;
                }
            }
        }
        public string EqpName { get; set; }
        public bool Break { get; set; }
        public ProductionEqpInList EqpInList { get; set; }

        public Equipment()
        {
            InitializeComponent();
        }
    }
}

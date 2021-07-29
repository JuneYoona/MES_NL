using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace MesAdmin.Reports
{
    public partial class Invoice : DevExpress.XtraReports.UI.XtraReport
    {
        public Invoice()
        {
            InitializeComponent();
        }

        private void XRLabel_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRLabel)sender).Text = ((XRLabel)sender).Text.Replace(@"\n", Environment.NewLine);
        }
    }
}

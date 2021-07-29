using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Drawing.Printing;
using System.Data;

namespace MesAdmin.Reports
{
    public partial class MaterialDispense : DevExpress.XtraReports.UI.XtraReport
    {
        public MaterialDispense()
        {
            InitializeComponent();
        }
        
        private void calculatedField1_GetValue(object sender, GetValueEventArgs e)
        {
            e.Value = (int)e.GetColumnValue("RowNo") / 11;
        }

        private void xrSubreport2_BeforePrint(object sender, PrintEventArgs e)
        {
            int i = Convert.ToInt32(this.GetCurrentColumnValue("calculatedField1"));
            XtraReport report = ((XRSubreport)sender).ReportSource;
            report.FilterString = "[RowNo]>" + (i * 10).ToString() + " AND [RowNo]<= " + (i * 10 + 10).ToString();
        }
    }
}

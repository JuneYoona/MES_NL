using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace MesAdmin.Reports
{
    public partial class BottleRepackingCard : DevExpress.XtraReports.UI.XtraReport
    {
        public BottleRepackingCard()
        {
            InitializeComponent();
        }

        private void Cell_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                XRTableCell cell = sender as XRTableCell;

                string cardColor = GetCurrentColumnValue("CardColor").ToString();
                int[] intArray = cardColor.Split(',').Select(x => int.Parse(x)).ToArray();

                cell.BackColor = Color.FromArgb(intArray[0], intArray[1], intArray[2]);
            }
            catch { }
        }
    }
}

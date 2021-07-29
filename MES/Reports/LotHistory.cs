using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using System.Windows;
using System.Linq;

namespace MesAdmin.Reports
{
    public partial class LotHistory : DevExpress.XtraReports.UI.XtraReport
    {
        public LotHistory()
        {
            InitializeComponent();
        }

        private void TableCell_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRTableCell)sender).Text = ((XRTableCell)sender).Text.Replace(@"\n", Environment.NewLine);
        }

        float mergedRowHeight = 0;
        private void xrTableRow2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                BrickGraphics brickGraph = this.PrintingSystem.Graph;
                XRTableCell mergedCell = xrTableCell10;
                string currValue = GetCurrentColumnValue("WE30").ToString();
                float mergedCellWidth = GraphicsUnitConverter.Convert(mergedCell.WidthF, ReportUnit.ToDpi(), GraphicsUnit.Document.ToDpi());
                SizeF size = brickGraph.MeasureString(currValue, mergedCell.Font, Convert.ToInt32(mergedCellWidth), StringFormat.GenericDefault);

                float calculatedCellHeight = GraphicsUnitConverter.Convert(size.Height, GraphicsUnit.Document.ToDpi(), ReportUnit.ToDpi());
                bool isLastRow = !mergedCell.Report.GetCurrentColumnValue("WE30").Equals(mergedCell.Report.GetNextColumnValue("WE30")) || mergedCell.Report.GetCurrentRow().Equals(mergedCell.Report.GetNextRow());

                if (isLastRow)
                {
                    if (mergedRowHeight < calculatedCellHeight)
                        xrTableRow2.HeightF = calculatedCellHeight - mergedRowHeight;
                    if (calculatedCellHeight - mergedRowHeight < 100)
                        xrTableRow2.HeightF = 100;
                    
                    mergedRowHeight = 0;
                }
                else
                {
                    xrTableRow2.HeightF = 100;
                    Detail.HeightF = 0;
                    mergedRowHeight += xrTableRow2.HeightF;
                }
                mergedCell.CanGrow = false;
            }
            catch { }
        }

        private void LotHistory_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            // print preview center 잡기
            Window wnd = Application.Current.Windows.OfType<Window>().SingleOrDefault(o => o.GetType() == typeof(DevExpress.Xpf.Printing.DocumentPreviewWindow));
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
    }
}

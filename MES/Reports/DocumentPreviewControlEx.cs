using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraPrinting;
using DevExpress.Xpf.Printing.PreviewControl;
using System.IO;
using DevExpress.XtraPrinting.Native.ExportOptionsControllers;
using DevExpress.XtraReports.UI;
using MesAdmin.Models;
using MesAdmin.Reports;

namespace MesAdmin
{
    public class DocumentPreviewControlEx : DocumentPreviewControl
    {
        public DocumentPreviewControlEx()
        {
            this.CommandBarStyle = DevExpress.Xpf.DocumentViewer.CommandBarStyle.Bars;
            this.PreviewKeyDown += DocumentPreviewControlEx_PreviewKeyDown;
        }

        private void DocumentPreviewControlEx_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox tb = e.OriginalSource as System.Windows.Controls.TextBox;
            if (tb == null) return;
            
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                tb.AppendText(Environment.NewLine);
                tb.SelectionStart = tb.Text.Length;
                e.Handled = true;
            }
        }

        public override void Export(ExportFormat? format)
        {
            SaveEditingFields();
            base.Export(format);
        }

        public override void Print()
        {
            SaveEditingFields();
            base.Print();
        }

        public override void PrintDirect(string printerName = null)
        {
            SaveEditingFields();
            base.PrintDirect(printerName);
        }

        public void SaveEditingFields()
        {
            XtraReport report = (XtraReport)this.DocumentSource;

            // save editing fields
            string reqNo = report.Parameters["ReqNo"].Value.ToString();
            string boxCnt = string.Empty;
            string docDate = string.Empty;
            string remark = string.Empty;
            string grossWeight = string.Empty;

            if (report.GetType() == typeof(Invoice))
            {
                boxCnt = report.PrintingSystem.EditingFields[2].EditValue.ToString();
                docDate = report.PrintingSystem.EditingFields[0].EditValue.ToString();
                remark = report.PrintingSystem.EditingFields[1].EditValue.ToString();
            }
            else
            {
                boxCnt = report.PrintingSystem.EditingFields[2].EditValue.ToString();
                docDate = report.PrintingSystem.EditingFields[0].EditValue.ToString();
                remark = report.PrintingSystem.EditingFields[1].EditValue.ToString();
                grossWeight = report.PrintingSystem.EditingFields[3].EditValue.ToString();
            }

            SalesPrintDocument doc = new SalesPrintDocument();
            doc.SaveDocument(reqNo, boxCnt, docDate, remark, grossWeight);
        }
    }
}

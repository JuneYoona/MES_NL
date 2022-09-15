
namespace MesAdmin.Reports
{
    partial class MaterialBPDLBarCode
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DevExpress.XtraPrinting.BarCode.Code128Generator code128Generator1 = new DevExpress.XtraPrinting.BarCode.Code128Generator();
            DevExpress.XtraPrinting.BarCode.Code128Generator code128Generator2 = new DevExpress.XtraPrinting.BarCode.Code128Generator();
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.xrBarCode2 = new DevExpress.XtraReports.UI.XRBarCode();
            this.xrBarCode1 = new DevExpress.XtraReports.UI.XRBarCode();
            this.LotNo = new DevExpress.XtraReports.Parameters.Parameter();
            this.ItemName = new DevExpress.XtraReports.Parameters.Parameter();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.Dpi = 254F;
            this.TopMargin.HeightF = 0F;
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.Dpi = 254F;
            this.BottomMargin.HeightF = 0F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrBarCode2,
            this.xrBarCode1});
            this.Detail.Dpi = 254F;
            this.Detail.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Detail.HeightF = 620F;
            this.Detail.HierarchyPrintOptions.Indent = 50.8F;
            this.Detail.Name = "Detail";
            this.Detail.StylePriority.UseFont = false;
            // 
            // xrBarCode2
            // 
            this.xrBarCode2.AutoModule = true;
            this.xrBarCode2.Dpi = 254F;
            this.xrBarCode2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "PadRight(?LotNo, 18)\n")});
            this.xrBarCode2.LocationFloat = new DevExpress.Utils.PointFloat(25.00001F, 357.1458F);
            this.xrBarCode2.Module = 5.08F;
            this.xrBarCode2.Name = "xrBarCode2";
            this.xrBarCode2.Padding = new DevExpress.XtraPrinting.PaddingInfo(26, 26, 0, 0, 254F);
            this.xrBarCode2.SizeF = new System.Drawing.SizeF(951F, 177.2709F);
            this.xrBarCode2.StylePriority.UseTextAlignment = false;
            code128Generator1.CharacterSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetAuto;
            this.xrBarCode2.Symbology = code128Generator1;
            this.xrBarCode2.Text = "MB-001";
            this.xrBarCode2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
            // 
            // xrBarCode1
            // 
            this.xrBarCode1.AutoModule = true;
            this.xrBarCode1.Dpi = 254F;
            this.xrBarCode1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "PadRight(?ItemName, 10)")});
            this.xrBarCode1.LocationFloat = new DevExpress.Utils.PointFloat(25.00001F, 108.4375F);
            this.xrBarCode1.Module = 5.08F;
            this.xrBarCode1.Name = "xrBarCode1";
            this.xrBarCode1.Padding = new DevExpress.XtraPrinting.PaddingInfo(26, 26, 0, 0, 254F);
            this.xrBarCode1.SizeF = new System.Drawing.SizeF(951F, 177.2709F);
            this.xrBarCode1.StylePriority.UseTextAlignment = false;
            code128Generator2.CharacterSet = DevExpress.XtraPrinting.BarCode.Code128Charset.CharsetAuto;
            this.xrBarCode1.Symbology = code128Generator2;
            this.xrBarCode1.Text = "MB-001";
            this.xrBarCode1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter;
            // 
            // LotNo
            // 
            this.LotNo.Name = "LotNo";
            this.LotNo.Visible = false;
            // 
            // ItemName
            // 
            this.ItemName.Description = "Parameter1";
            this.ItemName.Name = "ItemName";
            this.ItemName.Visible = false;
            // 
            // MaterialBPDLBarCode
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail});
            this.Dpi = 254F;
            this.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "BackColor", "PadRight(?ItemName, 10)")});
            this.Font = new System.Drawing.Font("Arial", 9.75F);
            this.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            this.PageHeight = 620;
            this.PageWidth = 1000;
            this.PaperKind = System.Drawing.Printing.PaperKind.Custom;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.LotNo,
            this.ItemName});
            this.ReportUnit = DevExpress.XtraReports.UI.ReportUnit.TenthsOfAMillimeter;
            this.ShowPrintMarginsWarning = false;
            this.ShowPrintStatusDialog = false;
            this.SnapGridSize = 25F;
            this.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            this.Version = "21.2";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.Parameters.Parameter LotNo;
        private DevExpress.XtraReports.Parameters.Parameter ItemName;
        private DevExpress.XtraReports.UI.XRBarCode xrBarCode1;
        private DevExpress.XtraReports.UI.XRBarCode xrBarCode2;
    }
}

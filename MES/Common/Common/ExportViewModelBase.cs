using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using DevExpress.XtraPrinting;
using System;
using System.Windows;

namespace MesAdmin.Common.Common
{
    public class ExportViewModelBase : StateBusinessObject
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ISaveFileDialogService SaveFileDialogService { get { return GetService<ISaveFileDialogService>("SaveFileDialogServiceXlsx"); } }
        #endregion

        public void OnToExcel(object sender)
        {
            TableView view = sender as TableView;
            view.PrintAutoWidth = false;
            view.PrintCellStyle = (Style)Application.Current.FindResource("PrintCellStyle");


            XlsxExportOptionsEx exportOption = new XlsxExportOptionsEx
            {
                TextExportMode = TextExportMode.Value,
                ExportHyperlinks = false,
                AllowSortingAndFiltering = DevExpress.Utils.DefaultBoolean.False,
                ExportType = DevExpress.Export.ExportType.WYSIWYG,
            };

            Random rnd = new Random();
            SaveFileDialogService.DefaultFileName = rnd.Next() + ".xlsx";

            if (SaveFileDialogService.ShowDialog())
            {
                try
                {
                    view.ExportToXlsx(SaveFileDialogService.GetFullFileName(), exportOption);
                    System.Diagnostics.Process.Start(SaveFileDialogService.GetFullFileName());
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            }
        }
    }
}

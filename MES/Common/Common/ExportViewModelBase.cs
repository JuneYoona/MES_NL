using System;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Xpf.Grid;
using DevExpress.Mvvm;
using System.Diagnostics;

namespace MesAdmin.Common.Common
{
    public class ExportViewModelBase : StateBusinessObject
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        public void OnToExcel(object sender)
        {
            TableView view = sender as TableView;
            view.PrintAutoWidth = false;

            XlsxExportOptionsEx exportOption = new XlsxExportOptionsEx
            {
                TextExportMode = TextExportMode.Text,
                ExportHyperlinks = false,
                ExportType = DevExpress.Export.ExportType.WYSIWYG
            };

            Random rnd = new Random();
            string tempFileName = Path.Combine(Path.GetTempPath(), rnd.Next() + ".xlsx");

            try
            {
                view.ExportToXlsx(tempFileName, exportOption);
                var process = new Process();
                process.StartInfo = new ProcessStartInfo(tempFileName);
                process.EnableRaisingEvents = true;

                process.Exited += delegate
                {
                    if (File.Exists(tempFileName))
                        File.Delete(tempFileName);
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }
    }
}

using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using MesAdmin.Common.Common;
using System.Data;
using DevExpress.XtraReports.UI;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class StockDetailVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public StockDetailList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public StockDetail SelectedItem
        {
            get { return GetValue<StockDetail>(); }
            set { SetValue(value); }
        }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value, () => { WhCode = ""; ItemAccount = ""; }); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public bool AuthEdit { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        public ICommand PrintLabelCmd { get; set; }
        #endregion

        public StockDetailVM()
        {
            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            IEnumerable<CommonMinor> minor = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "ZZZ03");
            if (minor.Where(o => o.MinorCode == DSUser.Instance.UserID).Count() > 0) AuthEdit = true;

            SearchCmd = new AsyncCommand(OnSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            SaveCmd = new AsyncCommand(OnSave, () => Collections != null && Collections.Where(o => o.State != EntityState.Unchanged).Count() > 0);
            PrintLabelCmd = new DelegateCommand(OnPrintLabel, () => SelectedItem != null);
        }

        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(() =>
            {
                try
                {
                    Collections.Save();
                    SearchCore();
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() =>
                        MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error)
                    );
                }
            }).ContinueWith(t => IsBusy = false);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            string bizAreaCode = BizAreaCode;
            string whCode = WhCode;
            string itemAccount = ItemAccount;

            Collections = new StockDetailList(bizAreaCode, whCode, itemAccount);
        }

        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (SelectedItem == null) return;

            if (SelectedItem.WhCode == "PE10" && !AuthEdit)
            {
                TableView view = pm.sender as TableView;
                System.Reflection.PropertyInfo info = SelectedItem.GetType().GetProperty(pm.e.Column.FieldName);
                info.SetValue(SelectedItem, pm.e.OldValue);
                view.HideEditor();
                // editor hide 후 old value 가 바로 반영이 안되서 다시 editor show
                view.ShowEditor();
                MessageBoxService.ShowMessage("제품창고 재고는 담당자만 수정이 가능합니다!", "Information", MessageButton.OK, MessageIcon.Information);

                return;
            }

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        protected void OnPrintLabel()
        {
            XtraReport rpt = null;

            if (SelectedItem.BizAreaCode == "BAC60")
            {
                rpt = new Reports.MaterialOLED();

                rpt.Parameters["ItemCode"].Value = SelectedItem.ItemCode;
                rpt.Parameters["ItemName"].Value = SelectedItem.ItemName;
                rpt.Parameters["LotNo"].Value = SelectedItem.LotNo;
                rpt.Parameters["BasicUnit"].Value = SelectedItem.BasicUnit;
                rpt.CreateDocument();
            }
            else
            {
                rpt = new Reports.MaterialBPDL();

                rpt.AfterPrint += (s, e) =>
                {
                    XtraReport rpt2 = new Reports.MaterialBPDLBarCode();
                    rpt2.Parameters["ItemName"].Value = SelectedItem.ItemName;
                    rpt2.Parameters["LotNo"].Value = SelectedItem.LotNo;
                    rpt2.CreateDocument();

                    rpt.ModifyDocument(x => { x.AddPages(rpt2.Pages); });
                };

                rpt.Parameters["ItemCode"].Value = SelectedItem.ItemCode;
                rpt.Parameters["ItemName"].Value = SelectedItem.ItemName;
                rpt.Parameters["LotNo"].Value = SelectedItem.LotNo;
                rpt.Parameters["BasicUnit"].Value = SelectedItem.BasicUnit;
                rpt.CreateDocument();
            }
            
            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, rpt);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
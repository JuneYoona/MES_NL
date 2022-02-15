using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using MesAdmin.Reports;
using DevExpress.Xpf.Printing;

namespace MesAdmin.ViewModels
{
    public class SalesOrderReqNLVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupSalesOrderReqView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSalesOrderReqView"); } }
        DevExpress.Mvvm.IDialogService PopupItemView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public SalesOrderReqHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public SalesOrderReqDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<SalesOrderReqDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<CommonMinor> WhCode { get; set; }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public CommonBizPartner SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
        }
        public WorkerList WorkerList
        {
            get { return GetProperty(() => WorkerList); }
            set { SetProperty(() => WorkerList, value); }
        }
        public CommonItemList Items { get; set; }
        #endregion

        #region Commands
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        public AsyncCommand ConfirmCmd { get; set; }
        #endregion

        #region Report
        SalesOrderRequest report;
        #endregion

        public SalesOrderReqNLVM()
        {
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            AddCmd = new DelegateCommand(OnAdd, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ConfirmCmd = new AsyncCommand(OnConfirm, CanConfirm);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand<string>(OnShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            PrintCmd = new DelegateCommand(OnPrint, CanPrint);

            SelectedItems = new ObservableCollection<SalesOrderReqDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            Header = new SalesOrderReqHeader();
            Details = new SalesOrderReqDetailList();

            WorkerList = new WorkerList("Sales", "BAC60");
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            BindingBizPartnerList();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList);
            await task;

            if (task.IsCompleted)
            {
                BizPartnerList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return (new CommonBizPartnerList()).Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public bool CanAdd()
        {
            return IsNew && Header.ReqDate != null && !string.IsNullOrEmpty(Header.Remark1) && !string.IsNullOrEmpty(Header.ShipTo);
        }
        public void OnAdd()
        {
            Details.Insert(Details.Count, new SalesOrderReqDetail
            {
                State = EntityState.Added,
                CustItemCode = "",
                UnitPrice = 0,
                ExchangeRate = 1,
                SoNo = "",
                SoSeq   = 0
            });
        }

        public bool CanConfirm()
        {
            return !IsNew && Header.FinalFlag != "Y" && Details.Count > 0;
        }
        public Task OnConfirm()
        {
            IsBusy = true;
            return Task.Factory.StartNew(ConfirmCore);
        }
        public void ConfirmCore()
        {
            try
            { 
                Header.Confirm();
                Header.FinalFlag = "Y";
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }

            IsBusy = false;
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (var item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || string.IsNullOrEmpty(item.WhCode) || item.Qty <= 0)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Details.Count == 0) return false;
            }
            else
                ret = Details.Where(u => u.State == EntityState.Deleted).Count() > 0;

            return ret;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                if (IsNew)
                {
                    Header.Currency = "";
                    Header.SoType = "";
                    Header.MoveType = "";
                    Header.Save();
                    Details.ToList().ForEach(u => u.ReqNo = Header.ReqNo);
                }
                Details.Save();
                SearchCore();
                IsNew = false;
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public void OnShowDialog(string pm)
        {
            if (pm == "DocumentNo") // 수불현황조회
            {
                var vmReq = ViewModelSource.Create(() => new PopupSalesOrderReqVM());
                PopupSalesOrderReqView.ShowDialog(
                    dialogCommands: vmReq.DialogCmds,
                    title: "출하요청내역 조회",
                    viewModel: vmReq
                );

                if (vmReq.ConfirmHeader != null)
                {
                    Header.ReqNo = vmReq.ConfirmHeader.ReqNo;
                    OnSearch();
                }
            }
            else
            {
                var vmItem = ViewModelSource.Create(() => new PopupItemVM("15"));
                PopupItemView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "품목선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null)
                {
                    SelectedItems[0].ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItems[0].ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItems[0].ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    SelectedItems[0].BasicUnit = vmItem.ConfirmItem.BasicUnit;
                    SelectedItems[0].WhCode = "PE10";
                }
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Details.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void OnBizPartneChanged()
        {
            if (IsNew)
            {
                Details.Clear();
                Header.MoveType = "";
            }
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new SalesOrderReqHeader();
            Details.Clear();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.ReqNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string reqNo = Header.ReqNo;
            Header = new SalesOrderReqHeader(reqNo);
            Header.ReqNo = reqNo;
            Details = new SalesOrderReqDetailList(reqNo);
            IsBusy = false;

            if (Details.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("출하요청 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
        }

        public bool CanPrint()
        {
            return !IsNew;
        }
        public void OnPrint()
        {
            if (Header.ReqNo == null) return;

            report = new SalesOrderRequest();
            report.Parameters["ReqNo"].Value = Header.ReqNo;
            PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if (item == null)
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemCode", null);
                grid.SetCellValue(pm.e.RowHandle, "ItemName", null);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", null);
                grid.SetCellValue(pm.e.RowHandle, "BasicUnit", null);
                grid.SetCellValue(pm.e.RowHandle, "WhCode", null);
            }
            else
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                grid.SetCellValue(pm.e.RowHandle, "BasicUnit", item.BasicUnit);
                grid.SetCellValue(pm.e.RowHandle, "WhCode", "PE10");
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added)
            {
                IsNew = true;
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            }
            else
            {
                SalesOrderReqDetail item = pm.Item as SalesOrderReqDetail;
                Header.ReqNo = item.ReqNo;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
        }
    }
}

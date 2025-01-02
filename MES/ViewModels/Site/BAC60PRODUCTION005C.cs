using DevExpress.DataAccess.Sql;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraReports.UI;
using MesAdmin.Common.Common;
using MesAdmin.Common.Utils;
using MesAdmin.Models;
using MesAdmin.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60PRODUCTION005CVM : ViewModelBase
    {
        #region Services
        IDialogService PopupStockView { get { return GetService<IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public string OrderNo
        {
            get { return GetProperty(() => OrderNo); }
            set { SetProperty(() => OrderNo, value); }
        }
        public DateTime ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        public Z_BAC60_REPACKING Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public ObservableCollection<Z_BAC60_REPACKING_DETAIL> SelectedItems { get; } = new ObservableCollection<Z_BAC60_REPACKING_DETAIL>();
        public IEnumerable<CommonMinor> BtlTypeList { get; set; }
        public WorkerList InspectorList
        {
            get { return GetProperty(() => InspectorList); }
            set { SetProperty(() => InspectorList, value); }
        }
        public bool SalesRole { get { return DSUser.Instance.RoleName.Contains("Sales") || DSUser.Instance.RoleName.Contains("admin"); }}
        public bool QualityRole { get { return DSUser.Instance.RoleName.Contains("Quality");}}
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }

        #endregion

        #region Commands
        public ICommand ShowStockCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand WhValueChangedCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand PrintLabelCmd { get; set; }
        public ICommand PrintCardCmd { get; set; }
        #endregion

        public BAC60PRODUCTION005CVM()
        {
            IsNew = true;
            Header = new Z_BAC60_REPACKING();
            Header.ReqDate = DateTime.Now;
            InspectorList = new WorkerList("FQC", "BAC60");
            // 재소분 bottle type
            BtlTypeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "ZZZ16" && u.IsEnabled);

            AddCmd = new DelegateCommand(Add, () => IsNew && !string.IsNullOrEmpty(Header.LotNo) && Header.Qty > 0 && SalesRole);
            DelCmd = new DelegateCommand(Delete, () => SelectedItems.Count > 0 && string.IsNullOrEmpty(Header.InspectorId));
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            SearchCmd = new AsyncCommand(OnSearch, ()=> !string.IsNullOrEmpty(OrderNo) && IsNew);
            ShowStockCmd = new DelegateCommand(OnShowStock);
            PrintLabelCmd = new DelegateCommand(OnPrintLabel, () => !IsNew);
            PrintCardCmd = new DelegateCommand(OnPrintCard, () => !IsNew);
        }

        public Task OnSearch()
        {
            IsBusy = true;            
            return Task.Run(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            IsNew = false;
            Header = new Z_BAC60_REPACKING(OrderNo);

            if (string.IsNullOrEmpty(Header.OrderNo))
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("작업지시 정보가 없습니다!"
                                                            , "Information"
                                                            , MessageButton.OK
                                                            , MessageIcon.Information));
                OnNew();
            }
        }

        public void Delete()
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Header.Detail.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void Add()
        {
            Header.Detail = Header.Detail ?? new ObservableCollection<Z_BAC60_REPACKING_DETAIL>();

            Header.Detail.Add(new Z_BAC60_REPACKING_DETAIL
            {
                State = EntityState.Added,
                ItemCode = Header.ItemCode,
                LotNo = Header.LotNo,
                ItemName = Header.ItemName,
                ItemSpec = Header.ItemSpec,
                BasicUnit = Header.BasicUnit,
            });
        }

        public bool CanSave()
        {
            if (Header.Detail == null) return false;

            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (var item in Header.Detail.Where(u => u.State == EntityState.Added))
                {
                    if (item.Qty == null || item.Qty == 0)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Header.Detail.Count == 0) return false;
            }
            else ret = Header.Detail.Where(u => u.State == EntityState.Deleted).Count() > 0 || (!string.IsNullOrEmpty(Header.InspectorId) && Header.InspectDate != null);

            return ret;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(SaveCore).ContinueWith(t => IsBusy = false);
        }
        public void SaveCore()
        {
            try
            {
                OrderNo = Header.Save();
                OnSearch();
                IsNew = false;
                // send mesaage to parent view
                Messenger.Default.Send("Refresh");
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
            }
        }

        public void OnNew()
        {
            OrderNo = null;
            IsNew = true;
            Header = new Z_BAC60_REPACKING();
            Header.ReqDate = DateTime.Now;
        }

        public void OnShowStock()
        {
            CommonMinor whCode = GlobalCommonMinor.Instance.First(o => o.MajorCode == "I0011" && o.MinorCode == "PE10");
            var vmItem = ViewModelSource.Create(() => new PopupStockVM(whCode.MinorCode)); // 재고를 조회할 창고전달

            PopupStockView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "재고선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItems != null && vmItem.ConfirmItems.Count > 0)
            {
                try
                {
                    Header.ItemCode = vmItem.ConfirmItem.ItemCode;
                    Header.LotNo = vmItem.ConfirmItem.LotNo;
                    Header.ItemName = vmItem.ConfirmItem.ItemName;
                    Header.ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    Header.Qty = Convert.ToInt32(vmItem.ConfirmItem.Qty);
                    Header.BasicUnit = vmItem.ConfirmItem.BasicUnit;
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            }
        }

        protected void OnPrintLabel()
        {
            XtraReport rpt = new BottleRepacking();
            (rpt.DataSource as SqlDataSource).ConnectionName = DBInfo.Instance.Name;
            rpt.Parameters["OrderNo"].Value = OrderNo;
            rpt.Parameters["LotNo"].Value = Header.LotNo;
            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(Application.Current.MainWindow, rpt);

            // 출력기록 남기기
            rpt.PrintingSystem.EndPrint += (s, e) =>
            {
                try
                {
                    Header.Save_PrintHistory();
                }
                catch (Exception ex)
                {
                    // XtraReport 에서는 MessageBoxService 사용할 수 없어 MessageBox 대체
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        protected void OnPrintCard()
        {
            BottleRepackingCard rpt = new BottleRepackingCard();
            (rpt.DataSource as SqlDataSource).ConnectionName = DBInfo.Instance.Name;
            rpt.Parameters["OrderNo"].Value = OrderNo;
            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(Application.Current.MainWindow, rpt);
        }
        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            if (pm.Type == EntityMessageType.Changed)
            {
                OrderNo = pm.Item.ToString();
                Header.InspectDate = DateTime.Now;
                Task.Run(SearchCore).ContinueWith(t =>
                {
                    if (QualityRole) Header.InspectDate = Header.InspectDate ?? DateTime.Now;
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
            else ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
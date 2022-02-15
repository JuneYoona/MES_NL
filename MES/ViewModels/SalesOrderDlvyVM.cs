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
using System.Data.SqlClient;
using DevExpress.XtraReports.UI;

namespace MesAdmin.ViewModels
{
    public class SalesOrderDlvyVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupSalesOrderReqDetailView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSalesOrderReqDetailView"); } }
        DevExpress.Mvvm.IDialogService PopupSalesDlvyView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSalesDlvyView"); } }
        DevExpress.Mvvm.IDialogService PopupStockView { get { return GetService<DevExpress.Mvvm.IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public SalesOrderDlvyHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public SalesOrderReqDetail ReqDetail
        {
            get { return GetProperty(() => ReqDetail); }
            set { SetProperty(() => ReqDetail, value); }
        }
        public SalesOrderDlvyDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<SalesOrderDlvyDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public SalesOrderDlvyDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
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
        public IEnumerable<SalesOrderTypeConfig> OrderType { get; set; }
        public CommonItemList Items { get; set; }
        private List<StockDetail> ExceptStocks { get; set; }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public bool ShowCOA
        {
            get { return GetProperty(() => ShowCOA); }
            set { SetProperty(() => ShowCOA, value); }
        }
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
        public ICommand ShowDialogSalesCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        public ICommand ShowStockCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ReferSoCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        public ICommand PostDeliveryCmd { get; set; }
        public ICommand PostDeliveryCancelCmd { get; set; }
        public ICommand COAGFCmd { get; set; }
        #endregion

        public SalesOrderDlvyVM()
        {
            if (DSUser.Instance.BizAreaCode == "BAC95") ShowCOA = true;

            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            BindingBizPartnerList();
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            OrderType = new SalesOrderTypeConfigList(); // 수주형태
            IsNew = true;
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ReferSoCmd = new DelegateCommand(OnReferSo, CanReferSo);
            AddCmd = new DelegateCommand(OnAdd, CanAdd);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            ShowDialogSalesCmd = new DelegateCommand(OnShowDialogSales);
            ShowStockCmd = new DelegateCommand(OnShowStock);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            PrintCmd = new DelegateCommand(OnPrint);
            PostDeliveryCmd = new DelegateCommand(OnPostDelivery, CanPostDelivery);
            PostDeliveryCancelCmd = new DelegateCommand(OnPostDeliveryCancel, CanPostDeliveryCancel);
            COAGFCmd = new DelegateCommand(OnCOAGF, CanCOAGF);

            SelectedItems = new ObservableCollection<SalesOrderDlvyDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            Header = new SalesOrderDlvyHeader();
            Details = new SalesOrderDlvyDetailList();
            ExceptStocks = new List<StockDetail>();
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerLiss);
            await task;

            if (task.IsCompleted)
            {
                BizPartnerList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerLiss()
        {
            return (new CommonBizPartnerList()).Where(u => u.BizType.Substring(0, 1) == "C");
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanAdd()
        {
            return IsNew && ReqDetail != null;
        }
        public void OnAdd()
        {
            Details.Insert(Details.Count, new SalesOrderDlvyDetail
            {
                State = EntityState.Added,
                CustItemCode = ReqDetail.CustItemCode,
                SoType = ReqDetail.SoType,
                ShipTo = ReqDetail.ShipTo,
                UnitPrice = ReqDetail.UnitPrice,
                ExchangeRate = ReqDetail.ExchangeRate,
                WhCode = ReqDetail.WhCode,
                ReqNo = ReqDetail.ReqNo,
                ReqSeq = ReqDetail.Seq,
                SoNo = ReqDetail.SoNo,
                SoSeq = ReqDetail.SoSeq,
                VATRate = ReqDetail.VATRate,
                BasicUnit = ReqDetail.BasicUnit
            });
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                if (Details.Count == 0) return false;
                // 필수 입력값 처리
                foreach (var item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.Qty <= 0 || string.IsNullOrEmpty(item.LotNo))
                    {
                        ret = false;
                        break;
                    }
                }
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
                    // 로트번호, 가용재고 그룹핑 후 입력재고 비교
                    var temp = Details.GroupBy(row => new { row.LotNo, row.AvailableQty })
                                        .Select(g => new { LotNo = g.Key.LotNo, AvailableQty = g.Key.AvailableQty,  Qty = g.Sum(x => x.Qty)});
                    foreach (var item in temp)
                    {
                        if (item.Qty > item.AvailableQty)
                            throw new Exception(item.LotNo + "의 재고를 확인하세요!");
                    }

                    if(ReqDetail.Qty != Details.Sum(u => u.Qty))
                        throw new Exception("출하요청수량을 확인하세요!");

                    Header.Save();
                    Details.ToList().ForEach(u => u.DnNo = Header.DnNo);
                }

                Details.Save();
                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (ex is SqlException && IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanReferSo()
        {
            return IsNew;
        }
        public void OnReferSo()
        {
            var vmOrderReqDetail = ViewModelSource.Create(() => new PopupSalesOrderReqDetailVM());
            PopupSalesOrderReqDetailView.ShowDialog(
                dialogCommands: vmOrderReqDetail.DialogCmds,
                title: "출하요청내역참조",
                viewModel: vmOrderReqDetail
            );
            if (vmOrderReqDetail.ConfirmItem != null)
            {
                // header view
                ReqDetail = vmOrderReqDetail.ConfirmItem;

                // header 정보 저장을 위해서 setting
                Header.SoType = ReqDetail.SoType;
                Header.MoveType = ReqDetail.MoveType;
                Header.ShipTo = ReqDetail.ShipTo;
                Header.ReqDate = ReqDetail.ReqDate;
                Details.Clear();
            }
        }

        public void OnShowDialog()
        {
            var vmDlvy = ViewModelSource.Create(() => new PopupSalesOrderDlvyVM());
            PopupSalesDlvyView.ShowDialog(
                dialogCommands: vmDlvy.DialogCmds,
                title: "출고내역 조회",
                viewModel: vmDlvy
            );

            if (vmDlvy.ConfirmHeader != null)
            {
                Header.DnNo = vmDlvy.ConfirmHeader.DnNo;
                OnSearch();
                IsNew = false;
            }
        }
        public void OnShowDialogSales()
        {
            var vmDlvy = ViewModelSource.Create(() => new PopupSalesOrderDlvyVM("Y", "N"));
            PopupSalesDlvyView.ShowDialog(
                dialogCommands: vmDlvy.DialogCmds,
                title: "출하내역 조회",
                viewModel: vmDlvy
            );

            if (vmDlvy.ConfirmHeader != null)
            {
                Header.DnNo = vmDlvy.ConfirmHeader.DnNo;
                OnSearch();
            }
        }

        public bool CanPostDelivery()
        {
            return Header.ActualDate != null && Header.PostFlag == "N";
        }
        public void OnPostDelivery()
        {
            Header.PostDelivery();
            OnSearch();
        }

        public bool CanPostDeliveryCancel()
        {
            return Header.ActualDate != null && Header.PostFlag == "Y";
        }
        public void OnPostDeliveryCancel()
        {
            Header.PostDeliveryCancel();
            OnSearch();
        }
        
        public void OnShowStock()
        {
            // 선택된 로트는 재고 pop up에서 제외
            ExceptStocks.Clear();
            Details.Where(u => !string.IsNullOrEmpty(u.ItemCode)).ToList().ForEach(u => ExceptStocks.Add(new StockDetail
            {
                BizAreaCode = u.BizAreaCode,
                ItemCode = u.ItemCode,
                WhCode = u.WhCode,
                WaCode = u.WaCode,
                LotNo = u.LotNo,
            }));

            var vmItem = ViewModelSource.Create(() =>
            new PopupStockVM
            (
                GlobalCommonMinor.Instance.Where(u => u.MinorCode == ReqDetail.WhCode).FirstOrDefault(), ReqDetail.ItemCode, ExceptStocks
            )); // 재고를 조회할 창고전달


            PopupStockView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "재고선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItems != null && vmItem.ConfirmItems.Count > 0)
            {
                try
                {
                    int idx = Details.IndexOf(SelectedItem);
                    Details.RemoveAt(idx);

                    foreach (StockDetail item in vmItem.ConfirmItems)
                    {
                        Details.Insert(idx++,
                            new SalesOrderDlvyDetail
                            {
                                State = EntityState.Added,
                                BizAreaCode = item.BizAreaCode,
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                                ItemSpec = item.ItemSpec,
                                WhCode = item.WhCode,
                                WaCode = item.WaCode,
                                LotNo = item.LotNo,
                                Qty = item.Qty - item.PickingQty,
                                AvailableQty = item.Qty - item.PickingQty,
                                Memo = item.BottleLotNo,

                                // header 정보
                                CustItemCode = ReqDetail.CustItemCode,
                                SoType = ReqDetail.SoType,
                                ShipTo = ReqDetail.ShipTo,
                                UnitPrice = ReqDetail.UnitPrice,
                                ExchangeRate = ReqDetail.ExchangeRate,
                                ReqNo = ReqDetail.ReqNo,
                                ReqSeq = ReqDetail.Seq,
                                SoNo = ReqDetail.SoNo,
                                SoSeq = ReqDetail.SoSeq,
                                VATRate = ReqDetail.VATRate,
                                BasicUnit = ReqDetail.BasicUnit
                            }
                        );
                        // 추가된 재고들 선택된걸 보여주기위해
                        SelectedItems.Add(Details.ElementAt(idx - 1));
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            }
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            try
            {
                if (pm.e.Column.FieldName != "ItemCode") return;

                TableView view = pm.sender as TableView;
                GridControl grid = view.Grid;

                CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
                if (item == null)
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemCode", "");
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", "");
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", "");
                    grid.SetCellValue(pm.e.RowHandle, "BasicUnit", "");
                }
                else
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                    grid.SetCellValue(pm.e.RowHandle, "BasicUnit", item.BasicUnit);
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message);
            }
        }

        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (pm.e.Column.FieldName != "Qty") return;
            try
            {
                TableView view = pm.sender as TableView;
                GridControl grid = view.Grid;

                grid.UpdateTotalSummary();
            }
            catch (Exception ex) { MessageBoxService.ShowMessage(ex.Message); }
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

        public void OnNew()
        {
            IsNew = true;
            Header = new SalesOrderDlvyHeader();
            ReqDetail = null;;
            Details.Clear();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.DnNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string dnNo = Header.DnNo;
            
            Header.DnNo = dnNo;
            Details = new SalesOrderDlvyDetailList(dnNo);
            Header = new SalesOrderDlvyHeader(dnNo);

            SalesOrderDlvyDetail item = Details.FirstOrDefault();
            if (item != null)
                ReqDetail = new SalesOrderReqDetail(item.ReqNo, item.ReqSeq);

            IsBusy = false;
            // 출하처리 실제출고일용
            IsEnabled = Header.PostFlag == "N";
            if (Details.Count == 0)
            { 
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("출고내역 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
        }

        public void OnPrint()
        {
            //PackingOrder report = new PackingOrder();
            //PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        public bool CanCOAGF()
        {
            return !IsNew;
        }
        public void OnCOAGF()
        {
            //// Minor Code에 등록된 성적서를 가져온다
            //CommonMinor minor = new CommonMinorList(majorCode: "GFCOA").Where(u => u.Ref01 == Header.ShipTo).FirstOrDefault();

            //string reportName = minor == null || string.IsNullOrEmpty(minor.Ref02) ? "GlassFritCOADefault" : minor.Ref02;

            //Type type = Type.GetType("MesAdmin.Reports." + reportName, true);
            //XtraReport report = (XtraReport)Activator.CreateInstance(type);

            //string itemCode = string.Empty;
            //try
            //{
            //    foreach (var item in Details)
            //    {
            //        XtraReport temp = (XtraReport)Activator.CreateInstance(type);
            //        temp.Parameters["BizCode"].Value = Header.ShipTo;
            //        temp.Parameters["ItemCode"].Value = item.ItemCode;
            //        temp.Parameters["LotNo"].Value = item.LotNo;
            //        temp.Parameters["Qty"].Value = item.Qty;
            //        temp.CreateDocument();
            //        report.Pages.AddRange(temp.Pages);

            //        itemCode = item.ItemCode;
            //    }
            //}
            //catch { }

            //// Ag Paste 검사성적서 추가
            //if(minor != null && !string.IsNullOrEmpty(minor.Ref03))
            //{ 
            //    GlassFritCOAAg agPaste = new GlassFritCOAAg();
            //    agPaste.Parameters["DnNo"].Value = Header.DnNo;
            //    agPaste.Parameters["BizCode"].Value = Header.ShipTo;
            //    agPaste.Parameters["ItemCode"].Value = itemCode;
            //    agPaste.CreateDocument();
            //    report.Pages.AddRange(agPaste.Pages);
            //}

            //DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            Header.DnNo = pm.Item as string;
            OnSearch();
            IsNew = false;
        }
    }
}

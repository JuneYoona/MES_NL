using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports.UI;
using MesAdmin.Common.Common;
using MesAdmin.Common.Utils;
using MesAdmin.Models;
using MesAdmin.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class SalesDlvyNoteVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupSalesDlvyReqDetailView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSalesDlvyReqDetailView"); } }
        DevExpress.Mvvm.IDialogService PopupSalesDlvyNoteView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSalesDlvyNoteView"); } }
        DevExpress.Mvvm.IDialogService PopupStockView { get { return GetService<DevExpress.Mvvm.IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public SalesDlvyNoteHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public SalesDlvyReqDetail ReqDetail
        {
            get { return GetProperty(() => ReqDetail); }
            set { SetProperty(() => ReqDetail, value); }
        }
        public SalesDlvyNoteDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }        
        public ObservableCollection<SalesOrderDlvyDetail> SelectedItems { get; } = new ObservableCollection<SalesOrderDlvyDetail>();
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
        public bool BottleVisible
        {
            get { return (DSUser.Instance.BizAreaCode == "BAC10" || DSUser.Instance.BizAreaCode == "BAC20" || DSUser.Instance.BizAreaCode == "BAC16") && IsNew; }
        }
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set
            {
                SetProperty(() => IsNew, value, () => { RaisePropertyChanged("BottleVisible"); RaisePropertyChanged("EditQty"); });
            }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool IsPrinting
        {
            get { return GetProperty(() => IsPrinting); }
            set { SetProperty(() => IsPrinting, value); }
        }
        public bool EditQty
        {
            get { return DSUser.Instance.BizAreaCode != "BAC10" && DSUser.Instance.BizAreaCode != "BAC20" && DSUser.Instance.BizAreaCode != "BAC16" && IsNew; }
        }
        public string ProdDateHeaderName
        {
            get
            {
                CommonMinor minor = GlobalCommonMinor.Instance.Where(o => o.MajorCode == "ZA001" && o.MinorCode == DSUser.Instance.BizAreaCode).FirstOrDefault();
                if (minor == null) return "생산일";

                return minor.Ref02;
            }
        }
        public DateTime? RecentProdDate
        {
            get { return GetProperty(() => RecentProdDate); }
            set { SetProperty(() => RecentProdDate, value); }
        }
        #endregion

        #region Commands
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

        public SalesDlvyNoteVM()
        {
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            // 업체정보가져오기
            Task.Run(() => { return GlobalCommonBizPartner.Instance.Where(u => u.BizType == "C" || u.BizType == "CS" && u.IsEnabled == true); })
                .ContinueWith(t => { BizPartnerList = t.Result; });

            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            OrderType = new SalesOrderTypeConfigList(); // 수주형태
            DelCmd = new DelegateCommand<object>(Delete, (object obj) => Details.Count > 0);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(Header.DnNo));
            ReferSoCmd = new DelegateCommand(OnReferSo, () => IsNew);
            AddCmd = new DelegateCommand(OnAdd, () => IsNew && ReqDetail != null);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);

            ShowStockCmd = new DelegateCommand(OnShowStock);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            PrintCmd = new DelegateCommand(OnPrint, () => !IsNew && !string.IsNullOrEmpty(Header.DnNo) && !IsPrinting);
            PostDeliveryCmd = new DelegateCommand(OnPostDelivery, () => Header.ActualDate != null && Header.PostFlag == "N");
            PostDeliveryCancelCmd = new DelegateCommand(OnPostDeliveryCancel, () => Header.ActualDate != null && Header.PostFlag == "Y");

            Header = new SalesDlvyNoteHeader();
            Details = new SalesDlvyNoteDetailList();
            ExceptStocks = new List<StockDetail>();
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
            return Task.Run(SaveCore).ContinueWith(t => IsBusy = false);
        }
        public void SaveCore()
        {
            try
            {
                if (IsNew)
                {
                    #region 출하요청수량과 출하수량 비교
                    if (ReqDetail.Qty - ReqDetail.DlvyQty < Details.Sum(u => u.Qty))
                        throw new Exception("출하요청수량을 확인하세요!");
                    #endregion

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
                {
                    Header.Delete();
                    Header.DnNo = "";
                }

                string message;

                if (ex is SqlException)
                {
                    SqlException sqlEx = ex as SqlException;
                    message = sqlEx.Errors[0].Message;
                }
                else message = ex.Message;

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(message
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
            }
        }

        public void OnReferSo()
        {
            // System.ComponentModel.Win32Exception 때문에 DispatcherService 사용
            DispatcherService.BeginInvoke(() => {
                var vmOrderReqDetail = ViewModelSource.Create(() => new PopupSalesDlvyReqDetailVM());
                PopupSalesDlvyReqDetailView.ShowDialog(
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
            });
        }

        public void OnShowDialog()
        {
            var vmDlvy = ViewModelSource.Create(() => new PopupSalesDlvyNoteVM());
            PopupSalesDlvyNoteView.ShowDialog(
                dialogCommands: vmDlvy.DialogCmds,
                title: "출하내역 조회",
                viewModel: vmDlvy
            );

            if (vmDlvy.ConfirmHeader != null)
            {
                Header.DnNo = vmDlvy.ConfirmHeader.DnNo;
                OnSearch();
                IsNew = false;
            }
        }

        public void OnPostDelivery()
        {
            Header.PostDelivery();
            OnSearch();
        }

        public void OnPostDeliveryCancel()
        {
            Header.PostDeliveryCancel();
            OnSearch();
        }

        public void OnShowStock()
        {
            // 선택된 로트중 가용수량이 없는 재고 pop up에서 제외
            ExceptStocks.Clear();
            var temp = Details.GroupBy(row => new { row.BizAreaCode, row.ItemCode, row.WhCode, row.WaCode, row.LotNo, row.AvailableQty })
                                        .Select(g => new
                                        {
                                            BizAreaCode = g.Key.BizAreaCode,
                                            ItemCode = g.Key.ItemCode,
                                            WhCode = g.Key.WhCode,
                                            WaCode = g.Key.WaCode,
                                            LotNo = g.Key.LotNo,
                                            AvailableQty = g.Key.AvailableQty,
                                            Qty = g.Sum(x => x.Qty)
                                        });

            temp.Where(u => !string.IsNullOrEmpty(u.ItemCode) && u.AvailableQty <= u.Qty).ToList().ForEach(u => ExceptStocks.Add(new StockDetail
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
                GlobalCommonMinor.Instance.Where(u => u.MinorCode == ReqDetail.WhCode).FirstOrDefault().MinorCode, ReqDetail.ItemCode, ExceptStocks, ""
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
                                ProductDate = item.ProductDate,
                                Bottle = item.Bottle,

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
            if (pm.e.Column.FieldName != "Qty" && pm.e.Column.FieldName != "Bottle") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            grid.UpdateTotalSummary();
        }

        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                //if (u.State == EntityState.Added) Details.Remove(u);
                //else u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;

                // 등록된 내역 모두 삭제
                if (u.State == EntityState.Added) Details.Remove(u);
                else
                {
                    Details.ToList().ForEach(o => o.State = o.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted);
                }
            });
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new SalesDlvyNoteHeader();
            ReqDetail = null; ;
            Details.Clear();
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
            Details = new SalesDlvyNoteDetailList(dnNo);
            Header = new SalesDlvyNoteHeader(dnNo);

            SalesOrderDlvyDetail item = Details.FirstOrDefault();
            if (item != null)
                ReqDetail = new SalesDlvyReqDetail(item.ReqNo, item.ReqSeq);

            IsBusy = false;
            // 출하처리 실제출고일용
            IsEnabled = Header.PostFlag == "N";
            if (Details.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("출하내역 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
        }

        public void OnPrint()
        {
            IsPrinting = true; // prevent double click

            XtraReport report = new PackingOrderBAC40();
            report.Parameters["DnNo"].Value = Header.DnNo;

            Window wnd = PrintHelper.ShowPrintPreview(Application.Current.MainWindow, report);

            // 시간이 많이 걸리는 report loading panel 용
            wnd.ContentRendered += (o, e) => {
                DXSplashScreen.Show<SplashScreenView>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(wnd));
            };

            report.AfterPrint += (o, e) => {
                if (DXSplashScreen.IsActive) DXSplashScreen.Close();
            };

            IsPrinting = false;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added)
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                IsNew = true;
            }
            else
            {
                Header.DnNo = pm.Item as string;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                    IsNew = false;
                });
            }
        }
    }
}
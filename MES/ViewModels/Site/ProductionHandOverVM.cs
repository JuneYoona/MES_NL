using System;
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
using DevExpress.Xpf.Printing;
using MesAdmin.Reports;

namespace MesAdmin.ViewModels
{
    public class ProductionHandOverVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupStockView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupStockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public ProductionHandOverList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<ProductionHandOver> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ProductionHandOver SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public string HoNo
        {
            get { return GetProperty(() => HoNo); }
            set { SetProperty(() => HoNo, value); }
        }
        public DateTime OutDate
        {
            get { return GetProperty(() => OutDate); }
            set { SetProperty(() => OutDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
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
        private List<StockDetailHO> ExceptStocks { get; set; }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        #endregion

        public ProductionHandOverVM()
        {
            SelectedItems = new ObservableCollection<ProductionHandOver>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            OutDate = DateTime.Now;
            IsNew = true;

            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            PrintCmd = new DelegateCommand(OnPrint, CanPrint);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);

            ExceptStocks = new List<StockDetailHO>();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (var item in Collections.Where(u => u.State == Common.Common.EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.LotNo))
                    {
                        ret = false;
                        break;
                    }
                }
                if (Collections.Count == 0) return false;
            }
            else
                ret = Collections.Where(u => u.State == Common.Common.EntityState.Deleted).Count() > 0;

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
                // 품질검사 대기인 로트는 인계불가능
                Collections.ToList().ForEach(u => 
                {
                    if (u.Qty <= 0) { throw new Exception(u.LotNo + "의 재고가 부족합니다!"); }
                });

                Collections.ToList().ForEach(u => { u.OutDate = OutDate; u.Memo = Memo; });
                string hoNo = Collections.Save();
                if(!string.IsNullOrEmpty(hoNo))
                    HoNo = hoNo;
                
                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (IsNew)
                    HoNo = "";
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new ProductionHandOverList(hoNo: HoNo);
            SelectedItem = null;

            ProductionHandOver item = Collections.FirstOrDefault();
            if (item != null)
            {
                OutDate = item.OutDate;
                Memo = item.Memo;
            }

            IsBusy = false;
            if (Collections.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("출고내역 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
        }

        public void OnShowDialog()
        {
            try
            {
                // 선택된 로트는 재고 pop up에서 제외
                ExceptStocks.Clear();
                Collections.Where(u => !string.IsNullOrEmpty(u.ItemCode)).ToList().ForEach(u => ExceptStocks.Add(new StockDetailHO
                {
                    BizAreaCode = "BAC60",
                    ItemCode = u.ItemCode,
                    WhCode = "PE10",
                    WaCode = "WE60",
                    LotNo = u.LotNo,
                }));

                var vmItem = ViewModelSource.Create(() => new PopupStockHoVM(ExceptStocks));
                PopupStockView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "재고선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null && vmItem.ConfirmItems.Count > 0)
                {
                    int idx = Collections.IndexOf(SelectedItem);
                    Collections.RemoveAt(idx);

                    foreach (StockDetailHO item in vmItem.ConfirmItems)
                    {
                        Collections.Insert(idx++,
                            new ProductionHandOver
                            {
                                State = Common.Common.EntityState.Added,
                                ProductOrderNo = item.ProductOrderNo,
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                                ItemSpec = item.ItemSpec,
                                BasicUnit = item.BasicUnit,
                                LotNo = item.LotNo,
                                Qty = item.Qty,
                            }
                        );
                        // 추가된 재고들 선택된걸 보여주기위해
                        SelectedItems.Add(Collections.ElementAt(idx - 1));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        bool CanDel(object obj) { return SelectedItem != null; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == Common.Common.EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == Common.Common.EntityState.Deleted ? Common.Common.EntityState.Unchanged : Common.Common.EntityState.Deleted;
            });
        }

        public bool CanAdd()
        {
            return IsNew;
        }
        public void Add()
        {
            Collections.Insert(Collections.Count, new ProductionHandOver { State = EntityState.Added });
        }

        public bool CanPrint()
        {
            return !IsNew && Collections.Count() > 0;
        }
        public void OnPrint()
        {
            HandOverGoods report = new HandOverGoods();
            report.Parameters["HoNo"].Value = HoNo;
            PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        public void OnNew()
        {
            IsNew = true;
            HoNo = "";
            OutDate = DateTime.Now;
            Memo = "";
            Collections.Clear();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            if (Collections == null)
                Collections = new ProductionHandOverList();

            DocumentParamter pm = parameter as DocumentParamter;

            if (pm.Type == EntityMessageType.Added)
            {
                if (pm.Item != null)
                {
                    IEnumerable<StockDetailHO> items = pm.Item as IEnumerable<StockDetailHO>;

                    foreach (StockDetailHO item in items)
                    {
                        Collections.Insert(Collections.Count, new ProductionHandOver
                        {
                            State = EntityState.Added,
                            ProductOrderNo = item.ProductOrderNo,
                            ItemCode = item.ItemCode,
                            ItemName = item.ItemName,
                            ItemSpec = item.ItemSpec,
                            LotNo = item.LotNo,
                            Qty = item.Qty,
                            BasicUnit = item.BasicUnit,
                        });
                    }
                }

                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            }
            else
            {
                ProductionHandOver Item = pm.Item as ProductionHandOver;
                // header
                HoNo = Item.HoNo;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
        }
    }
}

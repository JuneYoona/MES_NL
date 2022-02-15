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
    public class PurcharseOrderVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupItemView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupItemView"); } }
        DevExpress.Mvvm.IDialogService PopupPOView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupPOView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
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
        public PurcharseOrderDetailList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<PurcharseOrderDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<CommonMinor> WhCode { get; set; }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public CommonBizPartner SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
        }
        public string PoNo
        {
            get { return GetProperty(() => PoNo); }
            set { SetProperty(() => PoNo, value); }
        }
        public DateTime? PoDate
        {
            get { return GetProperty(() => PoDate); }
            set { SetProperty(() => PoDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public CommonItemList Items { get; set; }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        #endregion

        public PurcharseOrderVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "V" || u.BizType == "CV");
            PoDate = DateTime.Now;
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            Items = new CommonItemList(); // 품목정보
            IsNew = true;
            Collections = new PurcharseOrderDetailList();
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });

            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            PrintCmd = new DelegateCommand(OnPrint);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            NewCmd = new DelegateCommand(OnNew);

            SelectedItems = new ObservableCollection<PurcharseOrderDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
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
                foreach (PurcharseOrderDetail item in Collections.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.DlvyDate == null || item.WhCode == null || item.PoQty <= 0)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Collections.Count == 0) return false;
            }
            else
                ret = Collections.Where(u => u.State == EntityState.Deleted).Count() > 0;

            return ret;
        }
        public Task OnSave() 
        {
            IsBusy = true;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            if (IsNew)
            {
                PurcharseOrderHeader header = new PurcharseOrderHeader
                {
                    PoNo = PoNo,
                    PoDate = PoDate,
                    BizCode = SelectedPartner.BizCode,
                    Memo = Memo
                };
                try
                {
                    PoNo = header.Save(header);
                    Collections.ToList().ForEach(u => u.PoNo = PoNo);
                    Collections.Save();
                    Collections = new PurcharseOrderDetailList(poNo: PoNo);
                    // send mesaage to parent view
                    Messenger.Default.Send<string>("Refresh");
                    IsNew = false;
                }
                catch (Exception ex)
                {
                    header.Delete(PoNo);
                    PoNo = "";
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                }
            }
            else
            {
                try
                {
                    Collections.Save();
                    Collections = new PurcharseOrderDetailList(poNo: PoNo);
                    // send mesaage to parent view
                    Messenger.Default.Send<string>("Refresh");
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                }
            }
            IsBusy = false;
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            if (SelectedItems != null)
            {
                if (IsNew) // 입력모드
                {
                    SelectedItems.ToList().ForEach(item => Collections.Remove(item));
                }
                else // 수정모드
                {
                    SelectedItems.ToList().ForEach(item =>
                        item.State = item.State == EntityState.Deleted ?
                        EntityState.Unchanged :
                        EntityState.Deleted
                    );
                }
            }
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;
    
            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if(item == null)
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemCode", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemName", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", "");
            }
            else
            { 
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                grid.SetCellValue(pm.e.RowHandle, "PoBasicUnit", item.BasicUnit);
            }
        }

        public bool CanAdd()
        {
            return SelectedPartner != null && PoDate != null && IsNew;
        }
        public void Add()
        {
            Collections.Insert(Collections.Count, new PurcharseOrderDetail
            {
                State = EntityState.Added,
            });
        }

        public void ShowDialog(string pm)
        {
            if (pm == "PopupItemView") // 품목조회
            {
                var vmItem = ViewModelSource.Create(() => new PopupItemVM());
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
                    SelectedItems[0].PoBasicUnit = vmItem.ConfirmItem.BasicUnit;
                    SelectedItems[0].WhCode = vmItem.ConfirmItem.InWhCode;
                }
            }
            else
            {
                var vmPO = ViewModelSource.Create(() => new PopupPurcharseOrderVM());
                PopupPOView.ShowDialog(
                    dialogCommands: vmPO.DialogCmds,
                    title: "발주현황 정보",
                    viewModel: vmPO
                );

                if (vmPO.ConfirmHeader != null)
                {
                    // header
                    PoNo = vmPO.ConfirmHeader.PoNo;
                    SelectedPartner = new CommonBizPartner { BizCode = vmPO.ConfirmHeader.BizCode };
                    PoDate = vmPO.ConfirmHeader.PoDate;
                    Memo = vmPO.ConfirmHeader.Memo;
                    // detail
                    Collections = vmPO.CollectionsDetail;
                    IsNew = false;
                }
            }
        }

        public void OnNew()
        {
            IsNew = true;
            PoNo = null;
            Collections.Clear();
            SelectedItems.Clear();
            SelectedPartner = null;
            PoDate = DateTime.Now;
            Memo = null;
        }

        public void OnPrint()
        {
            //PurchaseOrder report = new PurchaseOrder();
            //PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            PurcharseOrderDetail Item = pm.Item as PurcharseOrderDetail;
            // header
            PoNo = Item.PoNo;
            SelectedPartner = new CommonBizPartner { BizCode = Item.BizCode };
            PoDate = Item.PoDate;
            Memo = Item.Memo;
            // detail
            Collections = new PurcharseOrderDetailList(poNo: PoNo);
            IsNew = false;
        }
    }
}

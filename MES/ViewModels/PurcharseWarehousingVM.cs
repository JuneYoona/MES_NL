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
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using System.Windows;

namespace MesAdmin.ViewModels
{
    public class PurcharseWarehousingVM : ViewModelBase
    {
        #region Services
        IDialogService PopupPurcharseOrderDetailView { get { return GetService<IDialogService>("PopupPurcharseOrderDetailView"); } }
        IDialogService PopupPurcharseWarehousingView { get { return GetService<IDialogService>("PopupPurcharseWarehousingView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public PurcharseWarehousingList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<PurcharseWarehousing> SelectedItems
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
        public string GrNo
        {
            get { return GetProperty(() => GrNo); }
            set { SetProperty(() => GrNo, value); }
        }
        public DateTime InputDate
        {
            get { return GetProperty(() => InputDate); }
            set { SetProperty(() => InputDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
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
        public ICommand BizPartneChangedCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand ReferPoCmd { get; set; }
        #endregion

        public PurcharseWarehousingVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "V" || u.BizType == "CV");
            InputDate = DateTime.Now;
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            IsNew = true;
            Collections = new PurcharseWarehousingList();
            
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            ReferPoCmd = new DelegateCommand(OnReferPo, CanReferPo);
            BizPartneChangedCmd = new DelegateCommand(OnBizPartneChanged);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);

            SelectedItems = new ObservableCollection<PurcharseWarehousing>();
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
                foreach (var item in Collections.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.LotNo) || item.Qty <= 0)
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
            try
            {
                Collections.ToList().ForEach(u => { u.InputDate = InputDate; u.Memo = Memo; });
                string grNo = Collections.Save();
                if (IsNew)
                {
                    GrNo = grNo;
                    Collections = new PurcharseWarehousingList(grNo: GrNo);
                    IsNew = false;
                }
                else
                    Collections = new PurcharseWarehousingList(grNo: GrNo);

                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (IsNew)
                    GrNo = "";
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanReferPo()
        {
            return SelectedPartner != null && IsNew;
        }
        public void OnReferPo()
        {
            var vmOrderDetail = ViewModelSource.Create(() => new PopupPurcharseOrderDetailVM(SelectedPartner.BizCode));
            PopupPurcharseOrderDetailView.ShowDialog(
                dialogCommands: vmOrderDetail.DialogCmds,
                title: "발주내역참조",
                viewModel: vmOrderDetail
            );

            if (vmOrderDetail.ConfirmItems.Count > 0)
            {
                foreach (var item in vmOrderDetail.ConfirmItems)
                {
                    Collections.Add(item);
                }
            }
        }

        public void OnShowDialog()
        {
            var vmWarehousing = ViewModelSource.Create(() => new PopupPurcharseWarehousingVM());
            PopupPurcharseWarehousingView.ShowDialog(
                dialogCommands: vmWarehousing.DialogCmds,
                title: "입고내역 조회",
                viewModel: vmWarehousing
            );

            if (vmWarehousing.ConfirmHeader != null)
            {
                // header
                GrNo = vmWarehousing.ConfirmHeader.GrNo;
                SelectedPartner = new CommonBizPartner { BizCode = vmWarehousing.ConfirmHeader.BizCode };
                InputDate = vmWarehousing.ConfirmHeader.InputDate;
                Memo = vmWarehousing.ConfirmHeader.Memo;
                // detail
                Collections = vmWarehousing.CollectionsDetail;
                IsNew = false;
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void OnBizPartneChanged()
        {
            Collections.Clear();
        }

        public void OnNew()
        {
            IsNew = true;
            GrNo = "";
            SelectedPartner = null;
            InputDate = DateTime.Now;
            Memo = "";
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            PurcharseWarehousing Item = pm.Item as PurcharseWarehousing;
            // header
            GrNo = Item.GrNo;
            SelectedPartner = new CommonBizPartner { BizCode = Item.BizCode };
            InputDate = Item.InputDate;
            Memo = Item.Memo;
            // detail
            Collections = new PurcharseWarehousingList(grNo: Item.GrNo);
            IsNew = false;
        }
    }
}

using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.Xpf;
using MesAdmin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60SALES002RVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public ObservableCollection<SalesOrderDlvyHeader> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public SalesOrderDlvyHeader SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public IEnumerable<CommonBizPartner> BizCodeList
        {
            get { return GetProperty(() => BizCodeList); }
            set { SetProperty(() => BizCodeList, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public DateTime StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public DateTime EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowItemDialogCmd { get; set; }
        public AsyncCommand MouseDownCmd { get; set; }
        #endregion

        public BAC60SALES002RVM()
        {
            // 업체정보가져오기
            Task.Run(() => { return GlobalCommonBizPartner.Instance.Where(u => u.BizType == "C" || u.BizType == "CS" && u.IsEnabled == true); })
                .ContinueWith(t => { BizCodeList = t.Result; });

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);
            ShowItemDialogCmd = new DelegateCommand(OnShowItemDialog);
            SearchCmd = new AsyncCommand(OnSearch, () => IsBusy == false);
            MouseDownCmd = new AsyncCommand(OnMouseDown);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Run(SearchCore).ContinueWith(t => { Details = null; IsBusy = false; });
        }
        public void SearchCore()
        {
            Collections = new SalesOrderDlvyHeaderDetail(StartDate, EndDate, ItemCode, BizCode);
        }

        public Task OnMouseDown()
        {
            return Task.Run(() =>
            {
                if (SelectedItem != null)
                    Details = Commonsp.BAC60SALES002RS(SelectedItem.DnNo);
            });
        }

        public void OnShowItemDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM());
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                ItemCode = vmItem.ConfirmItem.ItemCode;
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            Task.Run(SearchCore).ContinueWith(t => MainViewModel.TabLoadingClose());
        }
    }
}
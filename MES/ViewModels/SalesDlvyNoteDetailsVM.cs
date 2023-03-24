using System;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using DevExpress.Mvvm.POCO;

namespace MesAdmin.ViewModels
{
    public class SalesDlvyNoteDetailsVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public DataRowView SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public SalesDlvyNoteDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public IEnumerable<CommonBizPartner> BizCodeList
        {
            get { return GetProperty(() => BizCodeList); }
            set { SetProperty(() => BizCodeList, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> SoTypeList { get; set; }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value, () => { RaisePropertyChanged("BottleView"); RaisePropertyChanged("PosView"); }); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
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
        public bool BottleView
        {
            get { return BizAreaCode == "BAC10" || BizAreaCode == "BAC20" || BizAreaCode == "BAC16"; }
        }
        public bool PosView
        {
            get { return BizAreaCode == "BAC10"; }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowItemDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public AsyncCommand MouseDownCmd { get; set; }
        #endregion

        public SalesDlvyNoteDetailsVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.IsEnabled == true);
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode)) BizAreaCode = DSUser.Instance.BizAreaCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            // 업체정보가져오기
            Task<IEnumerable<CommonBizPartner>>.Factory
                .StartNew(() => { return GlobalCommonBizPartner.Instance.Where(u => u.BizType == "C" || u.BizType == "CS"); })
                .ContinueWith(task => { BizCodeList = task.Result; });
            // 수주형태
            SoTypeList = (new SalesOrderTypeConfigList()).Where(u => u.IsEnabled == true);

            ShowItemDialogCmd = new DelegateCommand(OnShowItemDialog);
            SearchCmd = new AsyncCommand(OnSearch, () => IsBusy == false, true);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            MouseDownCmd = new AsyncCommand(OnMouseDown);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = new SalesDlvyNoteHeaderTable(StartDate, EndDate, SoType, ItemCode, BizCode, BizAreaCode).Collections;
            Details = null;
        }

        public Task OnMouseDown()
        {
            return Task.Factory.StartNew(() =>
            {
                if (SelectedItem != null) Details = new SalesDlvyNoteDetailList(dnNo: (string)SelectedItem.Row["DnNo"]);
            });
        }

        public void OnMouseDoubleClick()
        {
            try
            {
                if (SelectedItem == null) return;

                string viewName = string.Empty;
                string title = string.Empty;

                string pm = (string)SelectedItem["DnNo"];
                string documentId = (string)SelectedItem["DnNo"];
                IDocument document = MainViewModel.FindDocument(documentId);
                if (document == null)
                {
                    MainViewModel.TabLoadingOpen();
                    document = MainViewModel.CreateDocument("SalesDlvyNoteView", "출하내역 등록", new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel));
                    document.DestroyOnClose = true;
                    document.Id = documentId;
                }

                document.Show();
                SelectedItem = null;
            }
            catch
            {
                MainViewModel.TabLoadingClose();
                MessageBoxService.ShowMessage("교착상태가 발생했습니다! 다시시도하세요.", "Information", MessageButton.OK, MessageIcon.Information);
            }
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

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            Task.Run(SearchCore).ContinueWith(t => MainViewModel.TabLoadingClose());
        }
    }
}

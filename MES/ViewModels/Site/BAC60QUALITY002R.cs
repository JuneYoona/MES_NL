using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;

namespace MesAdmin.ViewModels
{
    public class BAC60QUALITY002RVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDialogService PopupQualityResultIMRView { get { return GetService<IDialogService>("QualityResultIMRView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get; set; }
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
        public ObservableCollection<Column> Columns { get; private set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public string QrType

        {
            get { return GetProperty(() => QrType); }
            set { SetProperty(() => QrType, value); }
        }
        public string LotNo

        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
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
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<DataRowView> MouseDoubleClickCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public BAC60QUALITY002RVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            Columns = new ObservableCollection<Column>();
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDoubleClickCmd = new DelegateCommand<DataRowView>(OnMouseDoubleClick, CanMouseDoubleClick);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
        }

        public bool CanSearch() { return !string.IsNullOrEmpty(QrType); }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            DataTable dt = Commonsp.BAC60QUALITY002RS(QrType, StartDate, EndDate, LotNo);

            Columns.Clear();
            if (dt != null)
            {
                foreach (DataColumn col in dt.Columns)
                {         
                    Columns.Add(
                        new Column
                        {
                            FieldName = col.ColumnName,
                            Width = col.ColumnName == "Lot No." || col.ColumnName == "품목명" ? 180 : 100,
                            Settings = SettingsType.Default
                        });
                }
            }

            Collections = dt;
        }

        public bool CanMouseDoubleClick(DataRowView dr)
        {
            return dr != null;
        }
        public void OnMouseDoubleClick(DataRowView dr)
        {

            string pm = (string)dr["검사번호"];
            string documentId = (string)dr["검사번호"];
            IDocument document = MainViewModel.FindDocument(documentId);

            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument("BAC60QUALITY001C", "분석결과 등록", new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel));
                document.DestroyOnClose = true;
                document.Id = documentId;
            }

            document.Show();
            SelectedItem = null;
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        public void OnShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM("29"));
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

            DocumentParamter pm = parameter as DocumentParamter;
            MainViewModel = (MainViewModel)pm.ParentViewmodel;
            MainViewModel.TabLoadingClose();
        }
    }
}
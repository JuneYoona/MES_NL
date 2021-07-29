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
    public class QualityResultVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDialogService PopupQualityResultIMRView { get { return GetService<IDialogService>("QualityResultIMRView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public object MainViewModel { get; set; }
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
            get
            {
                string qrType = string.Empty;
                DocumentParamter pm = Parameter as DocumentParamter;
                if (pm.Type == EntityMessageType.Added)
                    qrType = (string)pm.Item;
                return qrType;
            }
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
        public ICommand ShowIMRCmd { get; set; }
        #endregion

        public QualityResultVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;
            Columns = new ObservableCollection<Column>();
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDoubleClickCmd = new DelegateCommand<DataRowView>(OnMouseDoubleClick, CanMouseDoubleClick);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            ShowIMRCmd = new DelegateCommand(OnShowIMR, () => Collections != null && Collections.Rows.Count > 0);
        }

        public bool CanSearch() { return !string.IsNullOrEmpty(ItemCode); }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            DataTable dt = new QualityResultTable(ItemCode, StartDate, EndDate, QrType).Collections;

            Columns.Clear();
            if (dt != null)
            {
                Columns.Add(new Column { FieldName = "IsAttached", Settings = SettingsType.Image });
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName != "IsAttached")
                        Columns.Add(
                            new Column
                            {
                                FieldName = col.ColumnName,
                                Width = col.ColumnName == "차수" || col.ColumnName == "단위" ? 50 : 100,
                                Settings = SettingsType.Default
                            });
                }
            }
            Collections = dt;
            IsBusy = false;
        }

        public bool CanMouseDoubleClick(DataRowView dr)
        {
            return dr != null;
        }
        public void OnMouseDoubleClick(DataRowView dr)
        {
            DocumentParamter parameter = Parameter as DocumentParamter; // Menu paramter
            string viewName = string.Empty;
            string title = string.Empty;

            switch (QrType)
            {
                case "IQC":
                    viewName = "QualityRequestIQCView";
                    title = "수입검사등록";
                    break;
                case "LQC":
                    viewName = "QualityRequestLQCView";
                    title = "공정검사등록";
                    break;
                case "FQC":
                    viewName = "QualityRequestFQCView";
                    title = "최종검사등록";
                    break;
                case "OQC":
                    viewName = "QualityRequestOQCView";
                    title = "출하검사등록";
                    break;
                default:
                    break;
            }

            string[] pm = { (string)parameter.Item, (string)dr["검사요청번호"], dr["차수"].ToString() };
            string documentId = (string)dr["검사요청번호"] + dr["차수"].ToString();
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                ((MainViewModel)MainViewModel).TabLoadingOpen();
                document = DocumentManagerService.CreateDocument(viewName, new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = title;
            }
            document.Show();
            SelectedItem = null;
        }

        IDocument FindDocument(string documentId)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (documentId.Equals(doc.Id))
                    return doc;
            return null;
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            MainViewModel = pm.ParentViewmodel;

            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
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

        public void OnShowIMR()
        {
            var vmPopup = ViewModelSource.Create(() => new QualityResultIMRVM(Collections, QrType));
            PopupQualityResultIMRView.ShowDialog(
                dialogCommands: vmPopup.DialogCmds,
                title: "검사현황 관리도",
                viewModel: vmPopup
            );
        }
    }
}

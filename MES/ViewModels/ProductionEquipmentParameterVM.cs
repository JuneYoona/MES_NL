using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace MesAdmin.ViewModels
{
    public class ProductionEquipmentParameterVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
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
        public IEnumerable<CommonWorkAreaInfo> WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EditWaCode
        {
            get { return GetProperty(() => EditWaCode); }
            set { SetProperty(() => EditWaCode, value); }
        }
        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        #endregion

        #region Commands
        public ICommand<object> ToExcelCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<DataRowView> MouseDoubleClickCmd { get; set; }
        #endregion

        public ProductionEquipmentParameterVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "작업전", Value = "A" });
            Type.Add(new ItemInfo { Text = "작업중", Value = "B" });
            Type.Add(new ItemInfo { Text = "작업종료", Value = "C" });
            SelectedType = "C";

            WaCode = GlobalCommonWorkAreaInfo.Instance
                    .Where(u => u.BizAreaCode == DSUser.Instance.BizAreaCode);

            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;
            Columns = new ObservableCollection<Column>();
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDoubleClickCmd = new DelegateCommand<DataRowView>(OnMouseDoubleClick, CanMouseDoubleClick);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
        }

        public bool CanSearch() { return !string.IsNullOrEmpty(EditWaCode); }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            DataTable dt = new ProductionEquipmentParameter().GetList(StartDate, EndDate, EditWaCode, SelectedType);

            Columns.Clear();
            if (dt != null)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    Columns.Add(
                        new Column
                        {
                            FieldName = col.ColumnName,
                            Width = col.ColumnName == "단위" ? 50 : 100,
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
            if (SelectedItem == null) return;

            string[] pm = { (string)SelectedItem["제조번호"], SelectedType };
            string documentId = SelectedItem["제조번호"].ToString() + SelectedType;
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("ProductionEquipmentParameterValueView", new DocumentParamter(EntityMessageType.Changed, pm), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "설비파타미터 등록내역";
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
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
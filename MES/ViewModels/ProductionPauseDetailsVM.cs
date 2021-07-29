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
    public class ProductionPauseDetailsVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
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
        public IEnumerable<CommonMinor> BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
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
        public ObservableCollection<Column> Columns { get; private set; }
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
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        #endregion

        public ProductionPauseDetailsVM()
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.IsEnabled == true);
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = DSUser.Instance.BizAreaCode;

            StartDate = DateTime.Now.AddDays(-7);
            EndDate = DateTime.Now;

            Columns = new ObservableCollection<Column>();
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(EditWaCode));
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            DataTable dt = Commonsp.ProductionPauseDetails(StartDate, EndDate, EditBizAreaCode, EditWaCode);

            if (dt != null && Columns.Count == 0)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    Columns.Add(
                        new Column
                        {
                            FieldName = col.ColumnName,
                            Header = col.ColumnName.Split(new char[] { '@' })[0],
                            Width = int.Parse(col.ColumnName.Split(new char[] { '@' })[1]),
                            Settings = col.DataType == typeof(DateTime) ? SettingsType.DateTime : SettingsType.Default
                        });
                }
            }
            Collections = dt;
        }

        public void OnEditValueChanged()
        {
            WaCode = GlobalCommonWorkAreaInfo.Instance
                    .Where(u => string.IsNullOrEmpty(EditBizAreaCode) ? true : u.BizAreaCode == EditBizAreaCode);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}

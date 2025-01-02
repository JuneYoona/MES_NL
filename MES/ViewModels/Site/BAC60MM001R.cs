using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class BAC60MM001RVM : ViewModelBase
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
        #endregion

        public BAC60MM001RVM()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;

            Columns = new ObservableCollection<Column>();
            SearchCmd = new AsyncCommand(OnSearch);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Run(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            DataSet ds = Commonsp.BAC60MM001RS(StartDate, EndDate);
            DataTable dt = ds.Tables[0];
            DataTable dtConfig = ds.Tables[1];

            if (dt != null && Columns.Count == 0)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    string config = dtConfig.Rows[0][col.ColumnName].ToString();
                    Columns.Add(
                        new Column
                        {
                            FieldName = col.ColumnName,
                            Header = config.Split(new char[] { '@' })[0],
                            Width = int.Parse(config.Split(new char[] { '@' })[1]),
                            Settings = config.Contains("시간") ? SettingsType.DateTime : SettingsType.Default
                        });
                }
            }

            Collections = dt;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            Task.Run(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
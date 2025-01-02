using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class BAC60STOCKDETAIL001VM : ExportViewModelBase
    {
        #region Public Properties
        public ObservableCollection<string> Type { get; set; }
        public DataTable StockList
        {
            get { return GetProperty(() => StockList); }
            set { SetProperty(() => StockList, value); }
        }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        #endregion

        public BAC60STOCKDETAIL001VM()
        {
            Type = new ObservableCollection<string>();
            Type.Add("선검증 미진행");
            Type.Add("선검증 진행 중");
            Type.Add("선검증 완료");

            SearchCmd = new AsyncCommand(OnSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }

        public void SearchCore()
        {
            if (string.IsNullOrEmpty(SelectedType))
                StockList = Commonsp.BAC60STOCKDETAIL001();
            else if (SelectedType == "선검증 미진행")
            {
                var rows = Commonsp.BAC60STOCKDETAIL001().AsEnumerable().Where(u => u.Field<string>("QrNo") == null); ;
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else if (SelectedType == "선검증 진행 중")
            {
                var rows = Commonsp.BAC60STOCKDETAIL001().AsEnumerable().Where(u => u.Field<string>("QrNo") != null && string.IsNullOrEmpty(u.Field<string>("Result")));
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else if (SelectedType == "선검증 완료")
            {
                var rows = Commonsp.BAC60STOCKDETAIL001().AsEnumerable().Where(u => u.Field<string>("QrNo") != null && !string.IsNullOrEmpty(u.Field<string>("Result")));
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else { }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
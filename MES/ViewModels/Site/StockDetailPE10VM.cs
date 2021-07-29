using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Data;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class StockDetailPE10VM : ExportViewModelBase
    {
        #region Public Properties
        StockDetailPE10 StockDetail { get; set; }
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

        public StockDetailPE10VM()
        {
            StockDetail = new StockDetailPE10();

            Type = new ObservableCollection<string>();
            Type.Add("제품재고");
            Type.Add("선행검사대기품");
            Type.Add("선행검사출하품");
            Type.Add("선행검사완료품");
            SelectedType = "제품재고";

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
            if (SelectedType == "제품재고")
                StockList = StockDetail.GetStockDetail();
            else if (SelectedType == "선행검사대기품")
            {
                var rows = StockDetail.GetStockDetail().AsEnumerable().Where(u => u.Field<string>("QrNo") == null); ;
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else if (SelectedType == "선행검사출하품")
            {
                var rows = StockDetail.GetStockDetail().AsEnumerable().Where(u => u.Field<string>("QrNo") != null && u.Field<string>("Result") == null);
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else if (SelectedType == "선행검사완료품")
            {
                var rows = StockDetail.GetStockDetail().AsEnumerable().Where(u => u.Field<string>("QrNo") != null && u.Field<string>("Result") != null);
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else { }
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

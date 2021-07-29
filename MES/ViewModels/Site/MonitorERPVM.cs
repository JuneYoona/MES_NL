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
    public class MonitorERPVM : ViewModelBase
    {
        #region Public Properties
        public DataTable InterfaceList
        {
            get { return GetProperty(() => InterfaceList); }
            set { SetProperty(() => InterfaceList, value); }
        }
        public DataTable StockList
        {
            get { return GetProperty(() => StockList); }
            set { SetProperty(() => StockList, value); }
        }
        MonitorERP MonitorERP { get; set; }
        public ObservableCollection<string> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        public bool IsBusyA
        {
            get { return GetProperty(() => IsBusyA); }
            set { SetProperty(() => IsBusyA, value); }
        }
        public bool IsBusyB
        {
            get { return GetProperty(() => IsBusyB); }
            set { SetProperty(() => IsBusyB, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchStockCmd { get; set; }
        public AsyncCommand SearchInterfaceCmd { get; set; }
        #endregion

        public MonitorERPVM()
        {
            SearchInterfaceCmd = new AsyncCommand(OnSearchInterface);
            SearchStockCmd = new AsyncCommand(OnSearchStock);

            MonitorERP = new MonitorERP();
            Type = new ObservableCollection<string>();
            Type.Add("전체");
            Type.Add("차이분");
            SelectedType = "전체";
        }

        public Task OnSearchInterface()
        {
            IsBusyA = true;
            return Task.Factory.StartNew(SearchInterfaceCore);
        }

        public void SearchInterfaceCore()
        {
            InterfaceList = MonitorERP.GetInterface();
            IsBusyA = false;
        }

        public Task OnSearchStock()
        {
            IsBusyB = true;
            return Task.Factory.StartNew(SearchStockCore);
        }

        public void SearchStockCore()
        {
            if (SelectedType == "차이분")
            {
                var rows = MonitorERP.GetInventoryVariances().AsEnumerable().Where(u => u.Field<decimal>("Variances") != 0);
                StockList = rows.Any() ? rows.CopyToDataTable() : StockList.Clone();
            }
            else
                StockList = MonitorERP.GetInventoryVariances();

            IsBusyB = false;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            Task.Factory.StartNew(SearchInterfaceCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
            
            Task.Factory.StartNew(SearchStockCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}

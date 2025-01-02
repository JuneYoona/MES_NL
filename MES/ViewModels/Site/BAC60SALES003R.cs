﻿using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60SALES003RVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
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
        public DataTable OutputRecords
        {
            get { return GetProperty(() => OutputRecords); }
            set { SetProperty(() => OutputRecords, value); }
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
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        #endregion

        public BAC60SALES003RVM()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            SearchCmd = new AsyncCommand(OnSearch);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
        }

        public void OnMouseDown()
        {
            if (SelectedItem != null)
                OutputRecords = Commonsp.BAC60SALES003RS(SelectedItem["LotNo"].ToString());
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            //Collections = new ProductionInputRecordList(StartDate, EndDate, bizAreaCode: "BAC60", waCode: "WE60", lotNo: LotNo);
            Collections = Commonsp.BAC60SALES003RHS(StartDate, EndDate, LotNo);
            OutputRecords = null;
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
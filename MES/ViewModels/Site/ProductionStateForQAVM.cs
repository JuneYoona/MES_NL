using System;
using System.Windows;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using MesAdmin.Reports;
using System.Threading.Tasks;
using System.Collections.Generic;
using DevExpress.XtraReports.UI;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Core;
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    public class ProductionStateForQAVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public ProductionInputRecordList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ProductionInputRecord SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ProductionOutputRecordList OutputRecords
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

        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        #endregion

        public ProductionStateForQAVM()
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "선행검사출하품", Value = "SkyBlue" });
            Type.Add(new ItemInfo { Text = "선행검사완료품", Value = "Green" });

            SearchCmd = new AsyncCommand(OnSearch);
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
            PrintCmd = new DelegateCommand(OnPrint, CanPrint);
        }

        public void OnMouseDown()
        {
            if (SelectedItem != null)
                OutputRecords = new ProductionOutputRecordList(SelectedItem.OrderNo, SelectedItem.Seq);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new ProductionInputRecordList(StartDate, EndDate, bizAreaCode: EditBizAreaCode, waCode: EditWaCode, lotNo: LotNo, color: SelectedType);
            OutputRecords = null;
            IsBusy = false;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            SelectedItem.State =
                SelectedItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
        }

        public bool CanPrint()
        {
            return SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.Remark4);
        }
        public void OnPrint()
        {
            try
            {
                DXSplashScreen.Show<SplashScreenView>(System.Windows.WindowStartupLocation.CenterOwner, new SplashScreenOwner(Application.Current.MainWindow));

                LotHistory report = new LotHistory();
                report.Parameters["LotNo"].Value = SelectedItem.Remark4;
                Window wnd = PrintHelper.ShowPrintPreview(Application.Current.MainWindow, report);

                // 시간이 많이 걸리는 report loading panel 용
                //wnd.ContentRendered += ((o, e) => {
                //    DXSplashScreen.Show<SplashScreenView>(System.Windows.WindowStartupLocation.CenterOwner, new SplashScreenOwner(wnd));
                //});

                report.AfterPrint += ((o, e) =>
                {
                    if (DXSplashScreen.IsActive) DXSplashScreen.Close();
                });
            }
            catch { if (DXSplashScreen.IsActive) DXSplashScreen.Close(); }
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

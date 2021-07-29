using System;
using System.Linq;
using System.Threading.Tasks;
using MesAdmin.Common.CustomControl;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using MesAdmin.Models;
using System.Collections.Generic;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class ProductionEqpMonitorVM : ViewModelBase
    {
        #region Services
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public ObservableCollection<CommonWorkAreaInfo> WaCodeList
        {
            get { return GetProperty(() => WaCodeList); }
            set { SetProperty(() => WaCodeList, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public ObservableCollection<Equipment> EquipmentList
        {
            get { return GetProperty(() => EquipmentList); }
            set { SetProperty(() => EquipmentList, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand RefreshCmd { get; set; }
        #endregion

        public ProductionEqpMonitorVM()
        {
            WaCodeList = GlobalCommonWorkAreaInfo.Instance; // 공정정보
            WaCode = "WE10";
            RefreshCmd = new AsyncCommand(OnRefresh);
            SearchCmd = new AsyncCommand(OnSearch);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            while (DispatcherService == null) { System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1)); }
            DispatcherService.BeginInvoke(() =>
            {
                IEnumerable<CommonEquipment> commonEquipmentList = new CommonEquipmentList().Where(u => u.IsMonitor == true && u.IsEnabled == true && u.WaCode == WaCode);

                if (EquipmentList == null)
                    EquipmentList = new ObservableCollection<Equipment>();
                else
                    EquipmentList.Clear();

                CommonEquipment temp = null;
                bool isBreak = false;
                foreach (CommonEquipment commonEquipment in commonEquipmentList)
                {
                    // 공정별로 설비를 나누기위해 공정비교
                    if (temp != null)
                    {
                        if (temp.WaCode != commonEquipment.WaCode)
                            isBreak = true;
                        else isBreak = false;
                    }
                    temp = commonEquipment;

                    Equipment equipment = new Equipment
                    {
                        EqpCode = commonEquipment.EqpCode,
                        EqpName = commonEquipment.EqpName,
                        Status = commonEquipment.EqpState,
                        LeadTime = commonEquipment.LeadTime,
                        Pause = commonEquipment.PauseTime,
                        Break = isBreak,
                        EqpInList = new ProductionEqpInList(commonEquipment.EqpCode)
                    };
                    EquipmentList.Add(equipment);
                }
            });
            IsBusy = false;
        }

        public Task OnRefresh()
        {
            IsBusy = true;
            return Task.Factory.StartNew(RefreshCore);
        }
        public void RefreshCore()
        {
            while (DispatcherService == null) { System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1)); }
            DispatcherService.BeginInvoke(() =>
            {
                IEnumerable<CommonEquipment> commonEquipmentList = new CommonEquipmentList().Where(u => u.IsMonitor == true && u.IsEnabled == true && u.WaCode == WaCode);

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                foreach (Equipment equipment in EquipmentList)
                {
                    CommonEquipment eqp = commonEquipmentList.Where(u => u.EqpCode == equipment.EqpCode).FirstOrDefault();
                    equipment.EqpCode = eqp.EqpCode;
                    equipment.EqpName = eqp.EqpName;
                    equipment.LeadTime = eqp.LeadTime;
                    equipment.Pause = eqp.PauseTime;
                    equipment.Status = eqp.EqpState;
                    equipment.EqpInList = new ProductionEqpInList(eqp.EqpCode);
                }
            });
            IsBusy = false;
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

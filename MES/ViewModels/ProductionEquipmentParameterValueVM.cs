using System;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Input;
using System.Linq;

namespace MesAdmin.ViewModels
{
    public class ProductionEquipmentParameterValueVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
        public string Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public ProductionEquipmentParameterList Collection
        {
            get { return GetProperty(() => Collection); }
            set { SetProperty(() => Collection, value); }
        }
        public ProductionEquipmentParameter SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        #endregion

        #region Commands
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        #endregion

        public ProductionEquipmentParameterValueVM()
        {
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        public bool CanSave()
        {
            if (Collection == null) return false;
            return Collection.Where(o => o.State == EntityState.Modified).Count() > 0;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                Collection.Save();
                SearchCore();
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(ProductOrderNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collection = new ProductionEquipmentParameterList(ProductOrderNo, Seq);
            IsBusy = false;

            if (Collection.Count == 0)
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("파라미터내역 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

           // DataRowView item = pm.Item as DataRowView;
            string[] items = pm.Item as string[];

            ProductOrderNo = items[0];
            Seq = items[1];
            OnSearch();
        }
    }
}
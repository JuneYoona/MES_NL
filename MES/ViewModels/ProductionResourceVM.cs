using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class ProductionResourceVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonPartCodeList Collections
        {
            get { return GetValue<CommonPartCodeList>(); }
            set { SetValue(value, () => RaisePropertyChanged(nameof(BACCodeEnabled))); }
        }
        public ObservableCollection<CommonPartCode> SelectedItems { get; } = new ObservableCollection<CommonPartCode>();
        public CommonPartCode SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value, () => { WaCode = ""; PartGroupCode = ""; }); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string PartGroupCode
        {
            get { return GetProperty(() => PartGroupCode); }
            set { SetProperty(() => PartGroupCode, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool BACCodeEnabled { get { return Collections.Where(o => o.State == EntityState.Added).Count() == 0; } }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public ProductionResourceVM()
        {
            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand<object>(OnDelete, (object obj) => SelectedItems.Count > 0);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SearchCmd = new AsyncCommand(OnSearch);

            // Minor 정보
            var t = Task.Factory.StartNew(() => GlobalCommonMinor.Instance).ContinueWith(task =>
            {
                BizAreaCodeList = task.Result.Where(u => u.MajorCode == "I0004");
            });
            t.Wait();

            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        public bool CanSave()
        {
            bool ret = true;
            // 필수 입력값 처리
            if (Collections == null) return false;
            foreach (CommonPartCode item in Collections.Where(u => u.State == EntityState.Added || u.State == EntityState.Modified))
            {
                if (string.IsNullOrEmpty(item.PartCode) || string.IsNullOrEmpty(item.PartName))
                {
                    ret = false;
                    break;
                }
            }
            //  추가, 수정, 삭제작업이 있을경우
            if (Collections.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() == 0)
                ret = false;

            return ret;
        }
        public void OnSave()
        {
            try
            {
                Collections.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                string message;
                if (ex is SqlException)
                {
                    SqlException sqlEx = ex as SqlException;
                    if (sqlEx.Number == 2627) message = "이미 등록이 되어있는 부품코드입니다!";
                    else message = ex.Message;
                }
                else message = ex.Message;

                MessageBoxService.ShowMessage(message, "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public void OnNew()
        {
            CommonPartCode item = new CommonPartCode
            {
                State = EntityState.Added,
                BizAreaCode = BizAreaCode,
                IsEnabled = true,
                UpdateDate = DateTime.Now,
            };

            Collections.Insert(0, item);
            SelectedItem = item;
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = null;
            Collections = new CommonPartCodeList(BizAreaCode, WaCode, PartGroupCode);
            Collections.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(BACCodeEnabled));
        }

        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
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
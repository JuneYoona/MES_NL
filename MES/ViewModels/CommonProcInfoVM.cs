using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace MesAdmin.ViewModels
{
    public class CommonProcInfoVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public CommonWorkAreaInfoList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public CommonWorkAreaInfo SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public CommonMinorList BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public CommonMinorList WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public CommonMinor EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public CommonProcInfoVM()
        {
            BizAreaCode = new CommonMinorList(majorCode: "I0004");
            WhCode = new CommonMinorList(majorCode: "I0011");

            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);

            EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode);
        }

        public bool CanSave()
        {
            bool ret = true;
            // 필수 입력값 처리
            if (Collections == null) return false;
            foreach (CommonWorkAreaInfo item in Collections.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.BizAreaCode) || string.IsNullOrEmpty(item.WaCode))
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
                Collections.InitializeList();
            }
            catch (Exception ex)
            {
                string message;

                if (ex is SqlException)
                {
                    SqlException sqlEx = ex as SqlException;
                    message = sqlEx.Number == 547 ? "공정이 이미 사용중입니다!" : sqlEx.Message;
                }
                else message = ex.Message;

                MessageBoxService.ShowMessage(message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizAreaCode = EditBizAreaCode == null ? "" : EditBizAreaCode.MinorCode;
            Collections = new CommonWorkAreaInfoList(bizAreaCode);

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonWorkAreaInfo.Instance = null;
            IsBusy = false;
        }

        public void OnNew()
        {
            CommonWorkAreaInfo proc = new CommonWorkAreaInfo
            {
                UpdateDate = DateTime.Now,
                State = EntityState.Added,
                IsEnabled = true
            };
            Collections.Insert(0, proc);
            SelectedItem = proc;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Added)
                Collections.Remove(SelectedItem);
            else
                SelectedItem.State =
                    SelectedItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
        }

        public void OnCellValueChanged()
        {
            if(SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
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
};
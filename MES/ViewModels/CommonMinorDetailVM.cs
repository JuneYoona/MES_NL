using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace MesAdmin.ViewModels
{
    public class CommonMinorDetailVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public CommonMinorList MinorList
        {
            get { return GetProperty(() => MinorList); }
            set { SetProperty(() => MinorList, value); }
        }
        public CommonMinor Minor
        {
            get { return GetProperty(() => Minor); }
            set { SetProperty(() => Minor, value); }
        }
        public CommonMinorDetail MinorDetail
        {
            get { return GetProperty(() => MinorDetail); }
            set { SetProperty(() => MinorDetail, value); }
        }
        public ObservableCollection<CommonMinorDetail> SelectedMinorDetail
        {
            get { return GetProperty(() => SelectedMinorDetail); }
            set { SetProperty(() => SelectedMinorDetail, value); }
        }
        public CommonMinorDetailList MinorDetailList
        {
            get { return GetProperty(() => MinorDetailList); }
            set { SetProperty(() => MinorDetailList, value); }
        }
        public string MajorCode
        {
            get { return GetProperty(() => MajorCode); }
            set { SetProperty(() => MajorCode, value); }
        }
        public IEnumerable<CommonMajor> MajorCodeList
        {
            get { return GetProperty(() => MajorCodeList); }
            set { SetProperty(() => MajorCodeList, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand EditCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand SelectedItemChangedCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public CommonMinorDetailVM()
        {
            MajorCodeList = new CommonMajorList().Where(u => u.IsEnabled == true);

            NewCmd = new DelegateCommand(OnNew, CanNew);
            SelectedItemChangedCmd = new DelegateCommand(OnSelectedItemChanged);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);

            SelectedMinorDetail = new ObservableCollection<CommonMinorDetail>();
            SelectedMinorDetail.CollectionChanged += SelectedMinorDetails_CollectionChanged;
        }

        private void SelectedMinorDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeleteCmd.RaiseCanExecuteChanged();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(MajorCode);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            MinorList = new CommonMinorList(majorCode: MajorCode);
            IsBusy = false;
        }

        public bool CanNew()
        {
            return Minor != null;
        }
        public void OnNew()
        {
            CommonMinorDetail detail = new CommonMinorDetail
            {
                MajorCode = MajorCode,
                MinorCode = Minor.MinorCode,
                UpdateDate = DateTime.Now,
                State = EntityState.Added,
                IsEnabled = true
            };
            MinorDetailList.Insert(0, detail);
        }

        public void OnSelectedItemChanged()
        {
            if (Minor != null)
                MinorDetailList = new CommonMinorDetailList(MajorCode, Minor.MinorCode);
        }

        public void OnDelete()
        {
            SelectedMinorDetail.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    MinorDetailList.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public bool CanSave()
        {
            bool ret = true;
            if (MinorDetailList == null) return false;
            // 필수 입력값 처리
            foreach (CommonMinorDetail item in MinorDetailList.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.DetailCode) || string.IsNullOrEmpty(item.DetailName))
                {
                    ret = false;
                    break;
                }
            }
            //  추가, 삭제, 수정작업이 있을경우
            if (MinorDetailList.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() == 0)
                ret = false;

            return ret;
        }
        public void OnSave()
        {
            try
            {
                MinorDetailList.Save();
                OnSelectedItemChanged();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        public void OnCellValueChanged()
        {
            if (MinorDetail.State == EntityState.Unchanged)
                MinorDetail.State = EntityState.Modified;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}

using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;

namespace MesAdmin.ViewModels
{
    public class CommonMinorVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public CommonMajorList CollectionsMajor
        {
            get { return GetProperty(() => CollectionsMajor); }
            set { SetProperty(() => CollectionsMajor, value); }
        }
        public CommonMajor SelectedMajor
        {
            get { return GetProperty(() => SelectedMajor); }
            set { SetProperty(() => SelectedMajor, value); }
        }
        public CommonMinorList CollectionsMinor
        {
            get { return GetProperty(() => CollectionsMinor); }
            set { SetProperty(() => CollectionsMinor, value); }
        }
        public CommonMinor SelectedMinor
        {
            get { return GetProperty(() => SelectedMinor); }
            set { SetProperty(() => SelectedMinor, value); }
        }
        public CommonMinorList SelectedItems { get; } = new CommonMinorList();
        #endregion

        #region Commands
        public ICommand EditCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand SelectedItemChangedCmd { get; set; }
        public ICommand RefreshCmd { get; set; }
        #endregion

        public CommonMinorVM()
        {
            NewCmd = new DelegateCommand(OnNew);
            SelectedItemChangedCmd = new DelegateCommand(OnSelectedItemChanged);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            RefreshCmd = new DelegateCommand(OnRefresh);
            OnRefresh();
        }

        public void OnRefresh()
        {
            CollectionsMajor = new CommonMajorList();
        }

        public void OnNew()
        {
            CommonMinor minor = new CommonMinor
            {
                MajorCode = SelectedMajor.MajorCode, 
                UpdateDate = DateTime.Now, 
                State = EntityState.Added, 
                IsEnabled = true 
            };
            CollectionsMinor.Insert(0, minor);
            SelectedMinor = minor;
        }

        public void OnSelectedItemChanged()
        {
            if (SelectedMajor != null)
                CollectionsMinor = new CommonMinorList(SelectedMajor.MajorCode);
        }

        public void OnDelete()
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    CollectionsMinor.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public bool CanSave()
        {
            bool ret = true;
            // 필수 입력값 처리
            foreach (CommonMinor item in CollectionsMinor.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.MinorCode) || string.IsNullOrEmpty(item.MinorName))
                {
                    ret = false;
                    break;
                }
            }
            //  추가, 삭제, 수정작업이 있을경우
            if (CollectionsMinor.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() == 0)
                ret = false;

            return ret;
        }
        public void OnSave()
        {
            try
            {
                CollectionsMinor.Save();
                OnSelectedItemChanged();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error);
            }
        }

        public void OnCellValueChanged()
        {
            if (SelectedMinor.State == EntityState.Unchanged)
                SelectedMinor.State = EntityState.Modified;
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

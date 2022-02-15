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
    public class CommonMajorVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public CommonMajorList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public CommonMajor SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public string MajorCode
        {
            get { return GetProperty(() => MajorCode); }
            set { SetProperty(() => MajorCode, value); }
        }
        public string MajorName
        {
            get { return GetProperty(() => MajorName); }
            set { SetProperty(() => MajorName, value); }
        }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand SearchCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        #endregion

        public CommonMajorVM()
        {
            Collections = new CommonMajorList();

            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            SearchCmd = new DelegateCommand(OnSearch);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
        }

        public void OnNew()
        {
            CommonMajor major = new CommonMajor 
            { 
                UpdateDate = DateTime.Now, 
                State = EntityState.Added, 
                IsEnabled = true 
            };
            Collections.Insert(0, major);
            SelectedItem = major;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Added)
                Collections.Remove(SelectedItem);
            else
                SelectedItem.State = 
                    SelectedItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted ;
        }

        public bool CanSave()
        {
            bool ret = true;
            // 필수 입력값 처리
            foreach(CommonMajor item in Collections.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.MajorCode) || string.IsNullOrEmpty(item.MajorName))
                {
                    ret = false;
                    break;
                }
            }
            //  추가 또는 삭제작업이 있을경우
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
                MessageBoxService.ShowMessage(ex.Message);
            }
        }

        public void OnSearch()
        {
            Collections.InitializeList();
            Collections = new CommonMajorList
            (
                Collections
                    .Where(p =>
                        string.IsNullOrEmpty(MajorCode) ? true : p.MajorCode.ToUpper().Contains(MajorCode.ToUpper()))
                    .Where(p =>
                        string.IsNullOrEmpty(MajorName) ? true : p.MajorName.ToUpper().Contains(MajorName.ToUpper()))
            );
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
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

using System;
using System.Data;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using DevExpress.Xpf.Editors;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    class NetRoleVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public NetRoles Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public NetRole SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public NetMenus Menus
        {
            get { return GetProperty(() => Menus); }
            set { SetProperty(() => Menus, value); }
        }
        public EntityMessageType Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        #endregion

        #region Commands
        public ICommand MenuSaveCmd { get; set; }
        public ICommand RoleSaveCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand SelectedItemChangedCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand RefreshCmd { get; set; }
        #endregion

        public NetRoleVM()
        {
            Collections = NetRoles.Select();

            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SelectedItemChangedCmd = new DelegateCommand(OnSelectedItemChanged);
            MenuSaveCmd = new DelegateCommand(OnMenuSave);
            RoleSaveCmd = new DelegateCommand(OnRoleSave, CanRoleSave);
            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            RefreshCmd = new DelegateCommand(OnRefresh);
        }

        public void OnRefresh()
        {
            Collections = NetRoles.Select();
        }

        public void OnNew()
        {
            Collections.Add(new NetRole()
            { 
                State = EntityState.Added
            });
        }

        public void OnDelete() 
        {
            if (SelectedItem == null)
                return;
            if (SelectedItem.RoleId == Guid.Empty)
            {
                Collections.Remove(SelectedItem);
            }
            else
            {
                if (SelectedItem.State == EntityState.Deleted)
                    SelectedItem.State = EntityState.Unchanged;
                else
                    SelectedItem.State = EntityState.Deleted;
            }
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        public void OnMenuSave()
        {
            // save menus in role
            NetMenus checkedMnu = new NetMenus(Menus.Where(r => r.IsChecked == true && r.PMenuId != Guid.Empty));
            string errMsg = NetRoles.InsertMenus(SelectedItem.RoleId, checkedMnu);
            if (errMsg != "")
                MessageBoxService.ShowMessage(errMsg, "Error", MessageButton.OK, MessageIcon.Error);
        }

        public bool CanRoleSave()
        {
            return true;
        }
        public void OnRoleSave()
        {
            NetRoles.Insert(Collections.Where(r => r.State == EntityState.Added));
            NetRoles.Delete(Collections.Where(r => r.State == EntityState.Deleted));
            OnMenuSave();
            OnRefresh();
        }

        public void OnSelectedItemChanged()
        {
            if (SelectedItem == null)
                SelectedItem = Collections.FirstOrDefault();

            // copy role menu to Menus property
            Menus = new NetMenus
            (
                SelectedItem.Menus.Select(r => new NetMenu() 
                {
                    PMenuId = r.PMenuId,
                    MenuId = r.MenuId,
                    MenuName = r.MenuName,
                    IsChecked = r.IsChecked
                })
            );
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

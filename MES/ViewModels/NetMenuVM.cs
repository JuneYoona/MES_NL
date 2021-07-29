using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using DevExpress.Xpf.Editors;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    class NetMenuVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public NetMenus Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public IEnumerable<NetMenu> CbCollections
        {
            get { return GetProperty(() => CbCollections); }
            set { SetProperty(() => CbCollections, value); }
        }
        public NetMenu SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public EntityMessageType Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        #endregion

        #region Commands
        public ICommand SaveCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand ChangeStatusCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        #endregion

        public NetMenuVM()
        {
            SaveCmd = new DelegateCommand<object>(OnSave);
            NewCmd = new DelegateCommand(OnNew);
            ChangeStatusCmd = new DelegateCommand(OnChangeStatus);
            DeleteCmd = new DelegateCommand(OnDelete);

            Collections = (new NetMenus()).GetAllMenus();
            CbCollections = Collections.Where(u => string.IsNullOrEmpty(u.CommandParameter));
        }

        public void OnSave(object pm)
        {
            MessageResult result =
                MessageBoxService.ShowMessage(
                                "저장하시겠습니까?"
                                , "메뉴 수정"
                                , MessageButton.YesNo
                                , MessageIcon.Question);
            if (result == MessageResult.No)
                return;

            // 변경된 내용을 source로 update, UpdateSourceTrigger=Explicit
            var values = (List<object>)pm;
            BindingExpression be = null;
            foreach (var value in values)
            {
                if (value.GetType() == typeof(TextEdit) || value.GetType() == typeof(SpinEdit))
                {
                    TextEdit te = value as TextEdit;
                    be = te.GetBindingExpression(TextEdit.TextProperty);
                }
                else if (value.GetType() == typeof(ComboBoxEdit))
                {
                    ComboBoxEdit cb = value as ComboBoxEdit;
                    be = cb.GetBindingExpression(ComboBoxEdit.EditValueProperty);
                }
                be.UpdateSource();
            }

            if (Status == EntityMessageType.Changed)
                Collections.Update(SelectedItem);
            else
                Collections.Insert(SelectedItem);

            Collections = (new NetMenus()).GetAllMenus();
            SelectedItem = null;
            CbCollections = Collections.Where(u => string.IsNullOrEmpty(u.CommandParameter));
            Status = EntityMessageType.Changed;
        }

        public void OnNew()
        {
            MessageResult result =
                MessageBoxService.ShowMessage(
                                "입력모드로 전환하시겠습니까?"
                                , "Confirm"
                                , MessageButton.YesNo
                                , MessageIcon.Question);
            if (result == MessageResult.No)
                return;

            SelectedItem = new NetMenu();
            Status = EntityMessageType.Added;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            MessageResult result =
                MessageBoxService.ShowMessage(
                                "삭제 하시겠습니까?"
                                , "메뉴 삭제"
                                , MessageButton.YesNo
                                , MessageIcon.Question);
            if (result == MessageResult.No)
                return;

            Collections.Delete(SelectedItem);
            Collections = (new NetMenus()).GetAllMenus();
        }

        public void OnChangeStatus()
        {
            Status = EntityMessageType.Changed;
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

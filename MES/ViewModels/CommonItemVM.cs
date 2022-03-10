using System;
using System.Data;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace MesAdmin.ViewModels
{
    public class CommonItemVM : ViewModelBase
    {
        #region Services
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonItemList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public CommonItem SelectedItem
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
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
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
        public ICommand SyncErpCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditCmd { get; set; }
        #endregion

        public CommonItemVM()
        {
            Messenger.Default.Register<EntityMessage<CommonItem>>(this, OnMessage);

            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            NewCmd = new DelegateCommand(OnNew);
            EditCmd = new DelegateCommand(OnEdit);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            DeleteCmd = new DelegateCommand(OnDelete);
            SearchCmd = new AsyncCommand(OnSearch);
            SyncErpCmd = new DelegateCommand(OnSyncErp);
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(u => u.State == EntityState.Deleted).Count() > 0;
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
                    if (sqlEx.Number == 547)
                        message = "이미 품목코드가 사용중입니다. 삭제하실 수 없습니다.";
                    else
                        message = ex.Message;
                }
                else
                {
                    message = ex.Message;
                }
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
        }

        public void OnNew()
        {
            IDocument document = DocumentManagerService.CreateDocument("CommonItemNewView", new DocumentParamter(EntityMessageType.Added), this);
            document.DestroyOnClose = true;
            document.Title = "품목정보 추가";
            document.Show();
        }

        public void OnEdit()
        {
            if (SelectedItem == null) return;
            IDocument document = FindDocument(SelectedItem.ItemCode);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("CommonItemNewView", new DocumentParamter(SelectedItem), this);
                document.DestroyOnClose = true;
                document.Id = SelectedItem.ItemCode;
                document.Title = "품목정보 수정";
            }
            document.Show();
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

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }

        public void SearchCore()
        {
            string itemCode = ItemCode;
            string itemName = ItemName;

            Collections = new CommonItemList
            (
                new CommonItemList(BizAreaCode)
                            .Where(p =>
                                string.IsNullOrEmpty(itemCode) ? true : p.ItemCode.ToUpper().Contains(itemCode.ToUpper()))
                            .Where(p =>
                                string.IsNullOrEmpty(itemName) ? true : p.ItemName.ToUpper().Contains(itemName.ToUpper()))

            );

            IsBusy = false;
        }

        void OnMessage(EntityMessage<CommonItem> message)
        {
            switch (message.MessageType)
            {
                case EntityMessageType.Added:
                    Collections.Add(message.Entity);
                    SelectedItem = message.Entity;
                    break;
                case EntityMessageType.Changed:
                    CommonItem found = Collections.FirstOrDefault(x => x.ItemCode == message.Entity.ItemCode);
                    int idx = Collections.IndexOf(found);
                    if (idx != -1)
                    {
                        Collections.Remove(found);
                        Collections.Insert(idx, message.Entity);
                        SelectedItem = message.Entity;
                    }
                    break;
                case EntityMessageType.Deleted:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        IDocument FindDocument(string itemCode)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (itemCode.Equals(doc.Id))
                    return doc;
            return null;
        }

        public void OnSyncErp()
        {
            Collections.SyncErp();
            MessageBoxService.ShowMessage("품목정보가 동기화 되었습니다.", "Information", MessageButton.OK, MessageIcon.Information);
            OnSearch();
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
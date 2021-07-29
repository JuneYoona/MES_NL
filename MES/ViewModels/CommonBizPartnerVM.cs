using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Data;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class CommonBizPartnerVM : ViewModelBase
    {
        #region Services
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public CommonBizPartnerList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public CommonBizPartner SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public string BizName
        {
            get { return GetProperty(() => BizName); }
            set { SetProperty(() => BizName, value); }
        }
        public CommonMinorList BizTypeCollections
        {
            get { return GetProperty(() => BizTypeCollections); }
            set { SetProperty(() => BizTypeCollections, value); }
        }
        public string BizType
        {
            get { return GetProperty(() => BizType); }
            set { SetProperty(() => BizType, value); }
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
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand SyncErpCmd { get; set; }
        #endregion

        public CommonBizPartnerVM()
        {
            Messenger.Default.Register<EntityMessage<CommonBizPartner>>(this, OnMessage);

            BizTypeCollections = new CommonMinorList("B9005");
            EditCmd = new DelegateCommand(OnEdit);
            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch);
            SyncErpCmd = new DelegateCommand(OnSyncErp);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizName = BizName;
            string bizType = BizType;

            Collections = new CommonBizPartnerList(bizName: bizName, bizType: bizType);
            IsBusy = false;
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(r => r.State == MesAdmin.Common.Common.EntityState.Deleted).Count() > 0;
        }
        public void OnSave()
        {
            Collections.Save();
            OnSearch();
        }

        public void OnNew()
        {
            IDocument document = DocumentManagerService.CreateDocument("CommonBizPartnerNewView", new DocumentParamter(EntityMessageType.Added), this);
            document.DestroyOnClose = true;
            document.Title = "거래처 추가";
            document.Show();
        }

        public void OnEdit()
        {
            if (SelectedItem == null) return;
            IDocument document = FindDocument(SelectedItem.BizCode);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("CommonBizPartnerNewView", new DocumentParamter(SelectedItem.BizCode), this);
                document.DestroyOnClose = true;
                document.Id = SelectedItem.BizCode;
                document.Title = "거래처 수정";
            }
            document.Show();
        }

        public void OnDelete()
        {
            if (SelectedItem == null)
                return;

            SelectedItem.State = SelectedItem.State == MesAdmin.Common.Common.EntityState.Deleted ?
                MesAdmin.Common.Common.EntityState.Unchanged :
                MesAdmin.Common.Common.EntityState.Deleted;
        }

        void OnMessage(EntityMessage<CommonBizPartner> message)
        {
            switch (message.MessageType)
            {
                case EntityMessageType.Added:
                    Collections.Add(message.Entity);
                    SelectedItem = message.Entity;
                    break;
                case EntityMessageType.Changed:
                    CommonBizPartner found = Collections.FirstOrDefault(x => x.BizCode == message.Entity.BizCode);
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

        IDocument FindDocument(string bizCode)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (bizCode.Equals(doc.Id))
                    return doc;
            return null;
        }

        public void OnSyncErp()
        {
            Collections.SyncErp();
            MessageBoxService.ShowMessage("고객정보가 동기화 되었습니다.", "Information", MessageButton.OK, MessageIcon.Information);
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

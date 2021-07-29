using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MesAdmin.ViewModels
{
    public class NetUserVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public NetUsers Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public NetUser SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public string UserId
        {
            get { return GetProperty(() => UserId); }
            set { SetProperty(() => UserId, value); }
        }
        public string UserName
        {
            get { return GetProperty(() => UserName); }
            set { SetProperty(() => UserName, value); }
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
        #endregion

        public NetUserVM()
        {
            Messenger.Default.Register<EntityMessage<NetUser>>(this, OnMessage);

            EditCmd = new DelegateCommand(OnEdit);
            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = NetUsers.Select();
            Collections = new NetUsers
            (
                Collections
                    .Where(p => 
                        string.IsNullOrEmpty(UserId) ? true : p.UserName.ToUpper().Contains(UserId.ToUpper()))
                    .Where(p =>
                        string.IsNullOrEmpty(UserName) ? true : p.Profile.KorName.ToUpper().Contains(UserName.ToUpper()))
            );

            IsBusy = false;
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(r => r.State == MesAdmin.Common.Common.EntityState.Deleted).Count() > 0;
        }
        public void OnSave()
        {
            NetUsers.Delete(Collections.Where(r => r.State == MesAdmin.Common.Common.EntityState.Deleted));
            Collections = NetUsers.Select();
        }

        public void OnNew()
        {
            IDocument document = DocumentManagerService.CreateDocument("NetUserNewView", new DocumentParamter(EntityMessageType.Added), this);
            document.DestroyOnClose = true;
            document.Title = "사용자 추가";
            document.Show();
        }

        public void OnEdit()
        {
            IDocument document = FindDocument(SelectedItem.UserId);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("NetUserNewView", new DocumentParamter(SelectedItem.UserName), this);
                document.DestroyOnClose = true;
                document.Id = SelectedItem.UserId;
                document.Title = "사용자 수정";
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

        void OnMessage(EntityMessage<NetUser> message)
        {
            switch (message.MessageType)
            {
                case EntityMessageType.Added:
                    Collections.Add(message.Entity);
                    SelectedItem = message.Entity;
                    break;
                case EntityMessageType.Changed:
                    NetUser found = Collections.FirstOrDefault(x => x.UserId == message.Entity.UserId);
                    int idx = Collections.IndexOf(found);
                    Collections.Remove(found);
                    Collections.Insert(idx, message.Entity);
                    SelectedItem = message.Entity;
                    break;
                case EntityMessageType.Deleted:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        IDocument FindDocument(Guid userId)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (userId.Equals(doc.Id))
                    return doc;
            return null;
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

    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<NetRole> tt = value as ObservableCollection<NetRole>;
            string res = string.Empty;

            foreach (NetRole item in tt)
            {
                if (tt.First() != item)
                    res += ", ";
                res += item.RoleName;
            }

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



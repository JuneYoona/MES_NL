using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Input;
using System.Collections.Specialized;

namespace MesAdmin.ViewModels
{
    public class QualityElementRequestListVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public QualityElementHeaderList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<QualityElementHeader> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public QualityElementHeader SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public DateTime StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public DateTime EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        #endregion

        public QualityElementRequestListVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;

            AddCmd = new DelegateCommand(Add);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick, () => SelectedItem != null);

            SelectedItems = new ObservableCollection<QualityElementHeader>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public void Add()
        {
            DocumentParamter parameter = Parameter as DocumentParamter; // Menu paramter

            string[] pm = { (string)parameter.Item };
            string documentId = Guid.NewGuid().ToString();
            IDocument document = MainViewModel.FindDocument(documentId);
            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument("QualityRequestElementView", "소자검사등록", new DocumentParamter(EntityMessageType.Added, pm, "BAC60", MainViewModel));
                document.DestroyOnClose = true;
                document.Id = documentId;
            }

            document.Show();
            SelectedItem = null;
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public bool CanSave()
        {
            return Collections != null && Collections.Where(u => u.State == EntityState.Deleted).Count() > 0;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SaveCore).ContinueWith(task => IsBusy = false);
        }
        public void SaveCore()
        {
            try
            {
                Collections.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = new QualityElementHeaderList(StartDate, EndDate);
        }

        public void OnMouseDoubleClick()
        {
            DocumentParamter parameter = Parameter as DocumentParamter; // Menu paramter
           
            string[] pm = { (string)parameter.Item, SelectedItem.QrNo };
            string documentId = SelectedItem.QrNo;
            IDocument document = MainViewModel.FindDocument(documentId);
            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument("QualityRequestElementView", "소자검사등록", new DocumentParamter(EntityMessageType.Changed, pm, "BAC60", MainViewModel));
                document.DestroyOnClose = true;
                document.Id = documentId;
            }

            document.Show();
            SelectedItem = null;
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            Task.Run(SearchCore).ContinueWith(task => MainViewModel.TabLoadingClose());
        }
    }
}

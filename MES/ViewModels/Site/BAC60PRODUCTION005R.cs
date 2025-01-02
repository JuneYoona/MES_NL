using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60PRODUCTION005RVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public Z_BAC60_REPACKING_LIST Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public Z_BAC60_REPACKING SelectedItem
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
        public string LotNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        #endregion

        public BAC60PRODUCTION005RVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            SearchCmd = new AsyncCommand(OnSearch, () => !IsBusy);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick, () => SelectedItem != null);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Run(new Action(SearchCore)).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = new Z_BAC60_REPACKING_LIST(StartDate, EndDate, LotNo);
        }

        public void OnMouseDoubleClick()
        {
            string documentId = "ID" + SelectedItem.OrderNo;
            IDocument document = MainViewModel.FindDocument(documentId);
            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument("BAC60PRODUCTION005C", "재소분요청 등록", new DocumentParamter(EntityMessageType.Changed, SelectedItem.OrderNo, MainViewModel));
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
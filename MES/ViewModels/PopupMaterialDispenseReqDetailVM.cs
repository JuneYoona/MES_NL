using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MesAdmin.ViewModels
{
    public class PopupMaterialDispenseReqDetailVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string MDNo
        {
            get { return GetProperty(() => MDNo); }
            set { SetProperty(() => MDNo, value); }
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
        public string PostFlag
        {
            get { return GetProperty(() => PostFlag); }
            set { SetProperty(() => PostFlag, value); }
        }
        public IEnumerable<MaterialDispenseDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public MaterialDispenseDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public MaterialDispenseDetail ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
        }
        public ObservableCollection<CodeName> PostFlagCollections { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        #endregion

        public PopupMaterialDispenseReqDetailVM()
        {
            PostFlagCollections = GlobalCommonPackingFlag.Instance;
            PostFlag = "N";
            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(OnConfirm),
                Id = MessageBoxResult.OK,
            };
            CancelUICmd = new UICommand()
            {
                Caption = "취소",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() { ConfirmUICmd, CancelUICmd };

            // command
            SearchCmd = new AsyncCommand(OnSearch);
            ConfirmCmd = new DelegateCommand(OnConfirm);

            OnSearch();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string postFlag = PostFlag;

            Collections = new MaterialDispenseDetailList(startDate: StartDate, endDate: EndDate);
            Collections = Collections
                            .Where(u => string.IsNullOrEmpty(postFlag) ? true : u.PostFlag == postFlag);

            IsBusy = false;
        }

        public void OnConfirm()
        {
            ConfirmItem = SelectedItem;
            CurrentWindowService.Close();
        }
    }
}

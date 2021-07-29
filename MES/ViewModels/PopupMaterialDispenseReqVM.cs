using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Data;

namespace MesAdmin.ViewModels
{
    public class PopupMaterialDispenseReqVM : ViewModelBase
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
        public DataTable Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public DataRowView SelectedHeader
        {
            get { return GetProperty(() => SelectedHeader); }
            set { SetProperty(() => SelectedHeader, value); }
        }
        public DataRowView ConfirmHeader
        {
            get { return GetProperty(() => ConfirmHeader); }
            set { SetProperty(() => ConfirmHeader, value); }
        }
        public MaterialDispenseDetailList Detail
        {
            get { return GetProperty(() => Detail); }
            set { SetProperty(() => Detail, value); }
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
        public ICommand MouseDownCmd { get; set; }
        #endregion

        public PopupMaterialDispenseReqVM()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(() => ConfirmHeader = SelectedHeader),
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
            MouseDownCmd = new DelegateCommand(OnMouseDown);

            OnSearch();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Header = new MaterialDispenseHeaderTable(startDate: StartDate, endDate: EndDate).Collections;
            //Header = Header.Where(u => u.PostFlag == "Y")
            //               .Where(p => string.IsNullOrEmpty(MDNo) ? true : p.MDNo == MDNo);
            Detail = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            if (SelectedHeader != null)
                Detail = new MaterialDispenseDetailList(mdNo: (string)SelectedHeader["MDNo"]);
        }

        public void OnConfirm()
        {
            ConfirmHeader = SelectedHeader;
            CurrentWindowService.Close();
        }
    }
}

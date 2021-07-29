using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace MesAdmin.ViewModels
{
    public class PopupQualityRequestVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public IEnumerable<QualityRequest> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public QualityRequest SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public QualityRequest ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
        }
        public ObservableCollection<CodeName> QrTypeCollections { get; set; }
        public ObservableCollection<CodeName> TranCollections { get; set; }
        public string SelectedTran
        {
            get { return GetProperty(() => SelectedTran); }
            set { SetProperty(() => SelectedTran, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public string SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
        }
        public string SelectedQrType
        {
            get { return GetProperty(() => SelectedQrType); }
            set { SetProperty(() => SelectedQrType, value); }
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
        public IEnumerable<CommonMinor> BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        #endregion

        public PopupQualityRequestVM() : this("", "") { }
        public PopupQualityRequestVM(string qrType) : this(qrType, ""){ }
        public PopupQualityRequestVM(string qrType, string bizAreaCode)
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");

            QrTypeCollections = GlobalCommonQrType.Instance;
            TranCollections = GlobalCommonTransfer.Instance;
            SelectedTran = "0";
            SelectedQrType = qrType;
            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;
            EditBizAreaCode = bizAreaCode;

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(() => ConfirmItem = SelectedItem),
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
            DateTime startDate = StartDate;
            DateTime endDate = EndDate;
            string qrNo = QrNo;
            string qrType = SelectedQrType;
            string lotno = LotNo;
            bool transferFlag = Convert.ToBoolean(int.Parse(SelectedTran));

            Collections = new QualityRequestList(startDate: startDate, endDate: endDate, qrNo: qrNo, qrType: qrType, lotNo: lotno, bizAreaCode: EditBizAreaCode);
            Collections = Collections.Where(u => u.TransferFlag == transferFlag);
            IsBusy = false;
        }

        public void OnConfirm()
        {
            ConfirmItem = SelectedItem;
            CurrentWindowService.Close();
        }
    }
}

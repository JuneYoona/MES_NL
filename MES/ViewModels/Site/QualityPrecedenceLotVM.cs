using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Diagnostics;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.IO;
using DevExpress.Xpf.Editors;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using System.Collections.Generic;

namespace MesAdmin.ViewModels
{
    public class QualityPrecedenceLotVM : ViewModelBase
    {
        #region Services
        IDialogService PopupStockView { get { return GetService<IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set
            {
                SetProperty(() => IsNew, value);
                RaisePropertyChanged(() => CanEditReg);
            }
        }
        public QualityPrecedenceLot Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public WorkerList RegList
        {
            get { return GetProperty(() => RegList); }
            set { SetProperty(() => RegList, value); }
        }
        public WorkerList InspectorList
        {
            get { return GetProperty(() => InspectorList); }
            set { SetProperty(() => InspectorList, value); }
        }
        public double Opacity
        {
            get { return GetProperty(() => Opacity); }
            set { SetProperty(() => Opacity, value); }
        }
        public bool CanEditReg
        {
            get { return Header == null || string.IsNullOrEmpty(Header.Result); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public QualityPrecedenceLotVM()
        {
            Opacity = 1;
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(DocumentNo) && IsNew);
            DeleteCmd = new DelegateCommand(OnDelete, () => !string.IsNullOrEmpty(DocumentNo) && !IsNew);
            ShowDialogCmd = new DelegateCommand<string>(OnShowDialog);
            NewCmd = new DelegateCommand(() => { Header = new QualityPrecedenceLot(); DocumentNo = ""; IsNew = true; });

            // 업체정보가져오기
            Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList)
                .ContinueWith(task => { BizPartnerList = task.Result; });
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return GlobalCommonBizPartner.Instance.Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public bool CanSave()
        {
            return Header != null && Header.ReqDate != null && Header.LotNo != "" && Header.Qty != null && Header.LotNoWE10 != "" && Header.RegId != null;
        }
        public Task OnSave()
        {
            Opacity = 0.55;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                Header.Save();
                DocumentNo = Header.QrNo;
                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            
            // send mesaage to parent view
            Messenger.Default.Send<string>("Refresh");
            Opacity = 1;
        }

        public void OnDelete()
        {
            Opacity = 0.55;

            try
            {
                Header.Delete();
                var task = OnSearch();
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }

            // send mesaage to parent view
            Messenger.Default.Send<string>("Refresh");
            Opacity = 1;
        }

        public Task OnSearch()
        {
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Header = new QualityPrecedenceLot(DocumentNo);

            if (string.IsNullOrEmpty(Header.QrNo))
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("등록정보가 없습니다!"
                    , "Information"
                    , MessageButton.OK
                    , MessageIcon.Information));

                IsNew = true;
            }
            else IsNew = false;
        }

        public void OnShowDialog(string pm)
        {
            if (pm == "DocumentNo") // 수불현황조회
            {
                //var vmMove = ViewModelSource.Create(() => new PopupStockMoveVM("OI")
                //{
                //    MoveType = MoveType
                //}); // // Dialog 수불구분과 수불유형 정의(예외출고 내역만)

                //PopupStockMoveView.ShowDialog(
                //    dialogCommands: vmMove.DialogCmds,
                //    title: "수불현황 정보",
                //    viewModel: vmMove
                //);

                //if (vmMove.ConfirmHeader != null && vmMove.CollectionsDetail != null)
                //{
                //    DocumentNo = vmMove.ConfirmHeader.DocumentNo;
                //    OnSearch();
                //}
            }
            else // 재고조회
            {
                var vmItem = ViewModelSource.Create(() => new PopupStockVM(new CommonMinor { MinorCode = "PE10", MinorName = "완제품창고" }, "")); // 재고를 조회할 창고전달
                PopupStockView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "재고선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null)
                {
                    Header.ItemCode = vmItem.ConfirmItem.ItemCode;
                    Header.ItemName = vmItem.ConfirmItem.ItemName;
                    Header.LotNo = vmItem.ConfirmItem.LotNo;
                    Header.GetLotNoWE10();
                    Header.BasicUnit = vmItem.ConfirmItem.BasicUnit;
                }
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            // 등록/검사자 세팅
            RegList = new WorkerList("QNReg");
            InspectorList = new WorkerList("QNRes");

            DocumentParamter pm = parameter as DocumentParamter;
            
            if (pm.Type == EntityMessageType.Added)
            {
                Header = new QualityPrecedenceLot();
                IsNew = true;
            }
            else
            {
                DocumentNo = pm.Item as string;
                OnSearch();
            }

            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}

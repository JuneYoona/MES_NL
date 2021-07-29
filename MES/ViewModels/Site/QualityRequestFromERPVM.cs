﻿using System;
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
using MesAdmin.Reports;

namespace MesAdmin.ViewModels
{
    public class QualityRequestFromERPVM : ViewModelBase
    {
        #region Services
        IDialogService PopupQualityRequestView { get { return GetService<IDialogService>("PopupQualityRequestView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IOpenFileDialogService OpenFileDialogService { get { return GetService<IOpenFileDialogService>(); } }
        ISaveFileDialogService SaveFileDialogService { get { return this.GetService<ISaveFileDialogService>(); } }
        #endregion

        #region Public Properties
        public QualityRequest Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value, UpdateCanEdit); }
        }
        public QualityResultFromERPList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<QualityInspectItem> InspectItem
        {
            get { return GetProperty(() => InspectItem); }
            set { SetProperty(() => InspectItem, value); }
        }
        public CommonBizPartnerList BizPartnerList { get; set; }
        public CommonBizPartner SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
        }
        public WorkerList WorkerCollections
        {
            get { return GetProperty(() => WorkerCollections); }
            set { SetProperty(() => WorkerCollections, value); }
        }
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public int Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
        }
        public DateTime InputDate
        {
            get { return GetProperty(() => InputDate); }
            set { SetProperty(() => InputDate, value); }
        }
        public string QrType
        {
            get
            {
                string qrType = string.Empty;
                DocumentParamter pm = Parameter as DocumentParamter;
                if (pm.Type == EntityMessageType.Added)
                    qrType = (string)pm.Item;
                else
                {
                    string[] item = pm.Item as string[];
                    qrType = item[0];
                }
                return qrType;
            }
        }
        public string BizAreaCode
        {
            get
            {
                string bizAreaCode = string.Empty;
                DocumentParamter pm = Parameter as DocumentParamter;
                return (string)pm.BizAreaCode;
            }
        }
        public string WorkerId
        {
            get { return GetProperty(() => WorkerId); }
            set { SetProperty(() => WorkerId, value); }
        }
        public FileInformList AddedFiles
        {
            get { return GetProperty(() => AddedFiles); }
            set { SetProperty(() => AddedFiles, value); }
        }
        public FileInformList DeletedFiles { get; set; }
        public FileInform AddedFile
        {
            get { return GetProperty(() => AddedFile); }
            set { SetProperty(() => AddedFile, value); }
        }
        public QualityResult SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public bool CanEdit
        {
            get { return GetProperty(() => CanEdit); }
            set { SetProperty(() => CanEdit, value); }
        }
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand DelCmd { get; set; }
        public ICommand ReInspectCmd { get; set; }
        public ICommand ConfirmCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        public ICommand AddFileCmd { get; set; }
        public ICommand DeleteFileCmd { get; set; }
        public ICommand PrintAnalysisLetterCmd { get; set; }
        public ICommand<FileInform> DownloadCmd { get; set; }
        public AsyncCommand<object> SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        #endregion

        public QualityRequestFromERPVM()
        {
            IsNew = true;
            Collections = new QualityResultFromERPList();
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            SaveCmd = new AsyncCommand<object>(OnSave, CanSave);
            DelCmd = new DelegateCommand(OnDel, CanDel);
            ConfirmCmd = new DelegateCommand(OnConfirm, CanConfirm);
            CancelCmd = new DelegateCommand(OnCancel, CanCancel);
            ReInspectCmd = new DelegateCommand(OnReInspect, CanReInspect);
            AddFileCmd = new DelegateCommand(OnAddFile, CanAddFile);
            DeleteFileCmd = new DelegateCommand(OnDeleteFile, CanDeleteFile);
            DownloadCmd = new DelegateCommand<FileInform>(OnDownload);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            PrintAnalysisLetterCmd = new DelegateCommand(OnPrintAnalysisLetter, CanPrintAnalysisLetter);

            AddedFiles = new FileInformList();
            DeletedFiles = new FileInformList();
        }

        public void UpdateCanEdit()
        {
            CanEdit = Header == null ? true : !Header.TransferFlag && Header.LastOrder == Header.ResultOrder;
        }

        public bool CanDeleteFile()
        {
            return CanEdit && AddedFile != null;
        }
        public void OnDeleteFile()
        {
            DeletedFiles.Add(AddedFile);
            AddedFiles.Remove(AddedFile);
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;
            if (SelectedItem.State == Common.Common.EntityState.Unchanged)
                SelectedItem.State = Common.Common.EntityState.Modified;
        }

        public void OnDownload(FileInform pm)
        {
            try
            {
                FileInform fi = AddedFiles.Where(u => u.Id == pm.Id).FirstOrDefault();
                byte[] objData = fi.Contents;
                string path = Path.Combine(Path.GetTempPath(), fi.FileName);
                File.WriteAllBytes(path, objData);

                var process = new Process();
                process.StartInfo = new ProcessStartInfo(path);
                process.EnableRaisingEvents = true;

                process.Exited += delegate
                {
                    if (File.Exists(path))
                        File.Delete(path);
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public bool CanAddFile()
        {
            return CanEdit;
        }
        public void OnAddFile()
        {
            bool dialogResult = OpenFileDialogService.ShowDialog();
            if (dialogResult)
            {
                IFileInfo file = OpenFileDialogService.Files.First();
                FileStream objFileStream = file.OpenRead();
                int intLength = Convert.ToInt32(objFileStream.Length);
                byte[] objData;
                objData = new byte[intLength];
                objFileStream.Read(objData, 0, intLength);

                AddedFiles.Add(new FileInform
                {
                    State = Common.Common.EntityState.Added,
                    Id = Guid.NewGuid(),
                    FileSize = Convert.ToInt32(file.Length),
                    FileName = file.Name,
                    Contents = objData,
                    ItemIcon = IconManager.FindIconForFilename(file.Name, false)
                });
            }
        }

        public bool CanConfirm()
        {
            if (Header == null) return false;
            return Header.Status && !Header.TransferFlag && Header.Result == "Pass" && !IsNew;
        }
        public void OnConfirm()
        {
            try
            {
                Header.TransferStock();
                Header = new QualityRequest(Header.QrNo, Header.LastOrder);
                Collections = new QualityResultFromERPList(Header.QrNo, Header.ResultOrder);
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public bool CanCancel()
        {
            if (Header == null) return false;
            return Header.TransferFlag && Header.LastOrder == Header.ResultOrder;
        }
        public void OnCancel()
        {
            try
            {
                Header.TransferStockCancel();
                Header = new QualityRequest(Header.QrNo, Header.LastOrder);
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public bool CanSave(object pm)
        {
            if (Header == null) return false;
            return CanEdit && Header.InspectDate != null && !string.IsNullOrEmpty(Header.InspectorId);
        }
        public Task OnSave(object pm)
        {
            // 검사결과값에 따라 재검사 버튼 활성화가 변경되기때문에 검사결과는 마지막에 source로 update, UpdateSourceTrigger=Explicit
            ComboBoxEdit cb = pm as ComboBoxEdit;
            cb.GetBindingExpression(ComboBoxEdit.EditValueProperty).UpdateSource();

            IsBusy = true;
            SelectedItem = null; // for hiding editor
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            //try
            //{
            //    string inspectorName = GetInspectorName(Header.InspectorId);
            //    Collections.ToList().ForEach(u =>
            //    {
            //        u.InspectDate = (DateTime)Header.InspectDate;
            //        u.InspectorId = Header.InspectorId;
            //        u.InspectorName = inspectorName;
            //        u.Result = Header.Result;
            //        u.Remark = Header.Memo;
            //    });
            //    Collections.Save();
            //    Collections = new QualityResultFromERPList(Header.QrNo, Header.ResultOrder);

            //    // file 처리
            //    AddedFiles.ToList().ForEach(u => { u.DocumentNo = Header.QrNo; u.Seq = Header.ResultOrder; });
            //    AddedFiles.Save();
            //    DeletedFiles.Delete();

            //    // header 정보변경
            //    Header = new QualityRequest(Header.QrNo, Header.ResultOrder);
            //    // file 정보변경
            //    BindingAddedFiles();

            //    // send mesaage to parent view
            //    Messenger.Default.Send<string>("Refresh");
            //    IsNew = false;
            //}
            //catch (Exception ex)
            //{
            //    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
            //                                        , "Information"
            //                                        , MessageButton.OK
            //                                        , MessageIcon.Information));
            //}
            //IsBusy = false;
        }

        public bool CanDel()
        {
            return CanEdit && Collections.Count() > 0 && !Header.TransferFlag && Header.ResultOrder == Header.LastOrder && !IsNew;
        }
        public void OnDel()
        {
            Collections.ToList().ForEach(item =>
                    item.State = item.State == Common.Common.EntityState.Deleted ?
                    Common.Common.EntityState.Unchanged :
                    Common.Common.EntityState.Deleted
            );
        }

        public void OnShowDialog()
        {
            var vmQr = ViewModelSource.Create(() => new PopupQualityRequestVM(QrType, BizAreaCode));
            PopupQualityRequestView.ShowDialog(
                dialogCommands: vmQr.DialogCmds,
                title: "검사 현황",
                viewModel: vmQr
            );

            if (vmQr.ConfirmItem != null)
            {
                // header
                Header = new QualityRequest(vmQr.ConfirmItem.QrNo, vmQr.ConfirmItem.ResultOrder);
                Header.Result = "Pass"; // default pass

                //1. 진행중인 품목계정을 가져와서 검사항목을 세팅한다.
                Collections.Clear();
                if (!Header.Status)
                {
                    InspectItem = new QualityInspectItemList(vmQr.ConfirmItem.QrType, vmQr.ConfirmItem.ItemCode);
                    foreach (var item in InspectItem)
                    {
                        Collections.Add(new QualityResult
                        {
                            State = Common.Common.EntityState.Added,
                            QrNo = Header.QrNo,
                            Order = Header.ResultOrder,
                            QrType = item.QrType,
                            ItemCode = item.ItemCode,
                            InspectName = item.InspectName,
                            InspectSpec = item.InspectSpec,
                            DownRate = item.DownRate,
                            UpRate = item.UpRate,
                            Unit = item.Unit,
                            Equipment = item.Equipment,
                            InspectValue = "",
                            Memo = "",
                            Editor = item.Editor
                        });
                    }
                    IsNew = true;
                }
                else //2. 완료항목은 검사값을 가져와서 세팅한다.
                {
                    Collections = new QualityResultFromERPList(Header.QrNo, Header.ResultOrder);
                    BindingAddedFiles();
                    IsNew = false;
                }
            }
        }

        // 파일사이즈가 클경우 freeze 방지를 위해 async binding
        private async void BindingAddedFiles()
        {
            var task = Task<FileInformList>.Factory.StartNew(LoadingFiles);
            await task;

            if (task.IsCompleted)
            {
                AddedFiles = task.Result;
            }
        }
        private FileInformList LoadingFiles()
        {
            try
            {
                FileInformList fi = new FileInformList(Header.QrNo, Header.ResultOrder);
                return fi;
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Warning"
                                                    , MessageButton.OK
                                                    , MessageIcon.Warning));
                return null;
            }
        }

        public bool CanReInspect()
        {
            if (Header == null) return false;
            return !IsNew && Header.Status && Header.Result == "Fail" && Header.ResultOrder == Header.LastOrder;
        }
        public void OnReInspect()
        {
            Header.ResultOrder = ++Header.LastOrder;

            // header reset
            Header.InspectDate = null;
            Header.InspectorId = null;
            Header.Result = null;
            Header.Memo = null;
            // detail reset
            Collections.Clear();
            InspectItem = new QualityInspectItemList(Header.QrType, Header.ItemCode);
            foreach (var item in InspectItem)
            {
                Collections.Add(new QualityResult
                {
                    State = Common.Common.EntityState.Added,
                    QrNo = Header.QrNo,
                    Order = Header.ResultOrder,
                    QrType = item.QrType,
                    ItemCode = item.ItemCode,
                    InspectName = item.InspectName,
                    InspectSpec = item.InspectSpec,
                    DownRate = item.DownRate,
                    UpRate = item.UpRate,
                    Unit = item.Unit,
                    Equipment = item.Equipment,
                    InspectValue = "",
                    Memo1 = "",
                    Memo2 = "",
                    Editor = item.Editor
                });
            }

            AddedFiles.Clear();
            IsNew = true;
        }

        public void OnNew()
        {
            IsNew = true;
            Header = null;
            Collections.Clear();
            AddedFiles.Clear();
            DeletedFiles.Clear();
        }

        public bool CanPrintAnalysisLetter()
        {
            if (Header == null) return false;
            return Header.Status == true;
        }
        public void OnPrintAnalysisLetter()
        {
            DevExpress.XtraReports.UI.XtraReport report = null;

            if (Header.ItemCode.Substring(2, 2) == "RS") report = new COA_IQC02(); // Recycle 수입 검사 성적서(MERS17)
            else report = new COA_IQC01();

            report.Parameters["QrNo"].Value = Header.QrNo;
            report.Parameters["Order"].Value = Header.ResultOrder;
            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            // 품질정보에 따라 검사자 세팅
            WorkerCollections = new WorkerList(QrType, BizAreaCode);

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            BindingCollection(pm);
        }

        // 시간이 많이 걸리는 작업
        private async void BindingCollection(DocumentParamter pm)
        {
            var task = Task.Factory.StartNew(() =>
            {
                string[] items = pm.Item as string[];
                Header = new QualityRequestFromERP(items[1], int.Parse(items[2])).GetQualityRequestHeader();

                if (!Header.Status)
                {
                    InspectItem = new QualityInspectItemList(QrType, Header.ItemCode);
                    foreach (var item in InspectItem)
                    {
                        Collections.Add(new QualityResult
                        {
                            State = Common.Common.EntityState.Added,
                            QrNo = Header.QrNo,
                            Order = Header.ResultOrder,
                            QrType = item.QrType,
                            ItemCode = item.ItemCode,
                            InspectName = item.InspectName,
                            InspectSpec = item.InspectSpec,
                            DownRate = item.DownRate,
                            UpRate = item.UpRate,
                            Unit = item.Unit,
                            Equipment = item.Equipment,
                            InspectValue = "",
                            Memo1 = "",
                            Memo2 = "",
                            Editor = item.Editor
                        });
                    }
                    IsNew = true;
                }
                else //2. 완료항목은 검사값을 가져와서 세팅한다.
                {
                    Collections = new QualityResultFromERPList(Header.QrNo, Header.ResultOrder);
                    BindingAddedFiles();
                    IsNew = false;
                }
            });
            await task;

            if (task.IsCompleted) ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }

        NetUsers users = null;
        protected string GetInspectorName(string inspectorId)
        {
            if (users == null)
                users = NetUsers.Select();
            var user = users.Where(u => u.UserName == inspectorId);
            return user.FirstOrDefault().Profile.KorName;
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "InspectValue") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            try
            {
                decimal down;
                decimal up;
                decimal value;

                bool resDown = decimal.TryParse(grid.GetCellValue(pm.e.RowHandle, "DownRate").ToString(), out down);
                bool resUp = decimal.TryParse(grid.GetCellValue(pm.e.RowHandle, "UpRate").ToString(), out up);
                bool resValue = decimal.TryParse(pm.e.Value.ToString(), out value);

                if (resDown && resUp && resValue)
                {
                    if (value >= down && value <= up)
                        grid.SetCellValue(pm.e.RowHandle, "Memo2", "Pass");
                    else
                        grid.SetCellValue(pm.e.RowHandle, "Memo2", "Fail");
                }
            }
            catch
            {
                grid.SetCellValue(pm.e.RowHandle, "Memo2", "");
            }
        }
    }
}
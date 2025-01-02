using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60QUALITY001CVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IOpenFileDialogService OpenTextDialogService { get { return GetService<IOpenFileDialogService>("OpenText"); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public Z_QUALITY_INSPECTION_BAC60 Header
        {
            get { return GetValue<Z_QUALITY_INSPECTION_BAC60>(); }
            set { SetValue(value); }
        }
        public ObservableCollection<QualityInspectItem> InspectItem
        {
            get { return GetProperty(() => InspectItem); }
            set { SetProperty(() => InspectItem, value); }
        }
        public IEnumerable<CodeName> QrType { get; set; }
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
        public bool Inspecting
        {
            get { return GetProperty(() => Inspecting); }
            set { SetProperty(() => Inspecting, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand DeleteCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand ImportTextCmd { get; set; }
        #endregion

        public BAC60QUALITY001CVM()
        {
            QrType = GlobalCommonQrType.Instance;

            SaveCmd = new AsyncCommand(OnSave, CanSave);
            DeleteCmd = new AsyncCommand(OnDelete, () => !IsNew);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            ImportTextCmd = new DelegateCommand(OnImportText, () => IsNew && Header != null && !string.IsNullOrEmpty(Header.QrType) && !string.IsNullOrEmpty(Header.ItemCode));
        }

        public void OnImportText()
        {
            // 품목별 검사항목 상/하한 가져오기
            InspectItem = new QualityInspectItemList(Header.QrType, Header.ItemCode);

            bool dialogResult = OpenTextDialogService.ShowDialog();
            if (dialogResult)
            {
                Inspecting = true;
                Task.Run(() =>
                { 
                    try
                    {
                        string filePath = OpenTextDialogService.Files?.FirstOrDefault().GetFullName();
                        string[] lines = System.IO.File.ReadAllLines(filePath);

                        // 파일명 추가
                        Header.FileName = OpenTextDialogService.Files?.FirstOrDefault().Name;

                        // Peak# line number 찾기
                        int dataNum = 0;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            // Sample Name 추출
                            if (lines[i].Contains("Sample Name"))
                            {
                                Header.SampleName = lines[i].Split('\t')[1];
                            }

                            if (lines[i].Contains("Peak#"))
                            {
                                dataNum = ++i;
                                break;
                            }
                        }

                        ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT> inspectData = new ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT>();
                        string[] dataValue;

                        // 검사결과 라인 reading
                        for (int i = dataNum; i < lines.Length; i++)
                        {
                            if (lines[i] == "") break;

                            dataValue = lines[i].Split(new char[] { '\t' });

                            inspectData.Add(new Z_QUALITY_INSPECTION_BAC60_RESULT
                            {
                                Peak = Convert.ToDouble(dataValue[0]),
                                R_Time = Convert.ToDouble(dataValue[1]),
                                Area = Convert.ToDouble(dataValue[4]),
                                InspectName = dataValue[10],
                            });
                        }

                        // Conc. 재계산을 위해 Area Summary
                        double sumArea = inspectData.Sum(o => o.Area);

                        // 합불판정을 위해 품목스펙 조인
                        var joinQuery =
                            from A in inspectData
                            join B in InspectItem on A.InspectName.ToUpper() equals B.InspectName.ToUpper() into leftJoin
                            from C in leftJoin.DefaultIfEmpty()
                            select new
                            {
                                InspectName = A.InspectName,
                                InspectSpec = C?.InspectSpec,
                                Peak = A.Peak,
                                R_Time = A.R_Time,
                                Area = A.Area,
                                Conc = Math.Round(A.Area / sumArea * 100, 4),
                                UpRate = C?.UpRate,
                                DownRate = C?.DownRate,
                            };


                        // 스펙으로 합불 판정
                        ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT> result = new ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT>();
                        string pof = "";

                        foreach (var item in joinQuery)
                        {
                            pof = "Pass";

                            bool retD = double.TryParse(item.DownRate, out double downRate);
                            bool retU = double.TryParse(item.UpRate, out double upRate);

                            if (retD && downRate > item.Conc) pof = "Fail";
                            if (retU && upRate < item.Conc) pof = "Fail";
                            if (!retD && !retU) pof = "";

                            result.Add(new Z_QUALITY_INSPECTION_BAC60_RESULT
                            {
                                State = EntityState.Added,
                                InspectName = item.InspectName,
                                InspectSpec = item.InspectSpec,
                                Peak = item.Peak,
                                R_Time = item.R_Time,
                                Area = item.Area,
                                Conc = item.Conc,
                                DownRate = item.DownRate,
                                UpRate = item.UpRate,
                                Result = pof,
                            });
                        }

                        if (result.Where(o => o.Result != "Pass").Count() > 0) Header.Result = "Fail";
                        else Header.Result = "Pass";

                        // 최종결과 binding
                        Header.InspectData = result;
                    }
                    catch (Exception ex)
                    {
                        DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error));
                    }
                }).ContinueWith(t => Inspecting = false);
            }
        }

        public bool CanSave()
        {
            if (Header == null || Header.InspectData == null) return false;

            return Header != null
                && !string.IsNullOrEmpty(Header.QrType)
                && !string.IsNullOrEmpty(Header.ItemCode)
                && Header.InspectDate != null
                && !string.IsNullOrEmpty(Header.Result)
                && Header.InspectData.Count > 0;
        }

        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(() =>
            {
                try
                {
                    string orderNo = Header.Save(IsNew);
                    Header = new Z_QUALITY_INSPECTION_BAC60(orderNo);

                    // send mesaage to parent view
                    Messenger.Default.Send("Refresh");
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error));
                }
            }).ContinueWith(t => { IsBusy = false; IsNew = false; });
        }

        public Task OnDelete()
        {
            IsBusy = true;
            return Task.Run(() =>
            {
                try
                {
                    Header.Delete();
                    OnNew();

                    // send mesaage to parent view
                    Messenger.Default.Send("Refresh");
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error));
                }
            }).ContinueWith(t => IsBusy = false);
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new Z_QUALITY_INSPECTION_BAC60();
        }

        public void OnShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM());
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                Header.ItemCode = vmItem.ConfirmItem.ItemCode;
                Header.ItemName = vmItem.ConfirmItem.ItemName;
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
           
            Task.Run(() =>
            {
                Header = new Z_QUALITY_INSPECTION_BAC60(pm.Item?.ToString());
            }).ContinueWith(t => MainViewModel.TabLoadingClose());


            IsNew = pm.Item == null ? true : false;
        }
    }
}
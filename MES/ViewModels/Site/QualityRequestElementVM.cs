using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using DevExpress.Data;
using System.Collections.Generic;
using DevExpress.Spreadsheet;
using System.Reflection;
using DevExpress.Mvvm.POCO;

namespace MesAdmin.ViewModels
{
    public class QualityRequestElementVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        ISaveFileDialogService SaveFileDialogService { get { return this.GetService<ISaveFileDialogService>(); } }
        IDialogService PopupStockView { get { return GetService<IDialogService>(); } }
        #endregion

        #region Public Properties
        public QualityElementHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public QualityElementIVLList QualityElementIVLList
        {
            get { return GetProperty(() => QualityElementIVLList); }
            set { SetProperty(() => QualityElementIVLList, value, () => RaisePropertyChanged(() => LumEdit)); }
        }
        public QualityElementLTList QualityElementLTList
        {
            get { return GetProperty(() => QualityElementLTList); }
            set { SetProperty(() => QualityElementLTList, value, () => RaisePropertyChanged(() => LTEdit)); }
        }
        public ObservableCollection<RawResult> QualityElementLTResult
        {
            get { return GetProperty(() => QualityElementLTResult); }
            set { SetProperty(() => QualityElementLTResult, value); }
        }
        public ObservableCollection<RawResult> QualityElementIVLResult
        {
            get { return GetProperty(() => QualityElementIVLResult); }
            set { SetProperty(() => QualityElementIVLResult, value); }
        }
        public WorkerList WorkerCollections
        {
            get { return GetProperty(() => WorkerCollections); }
            set { SetProperty(() => WorkerCollections, value); }
        }
        public decimal? LT
        {
            get { return GetProperty(() => LT); }
            set { SetProperty(() => LT, value); }
        }
        public decimal? LumRef
        {
            get { return GetProperty(() => LumRef); }
            set { SetProperty(() => LumRef, value); }
        }
        public decimal? LumLot
        {
            get { return GetProperty(() => LumLot); }
            set { SetProperty(() => LumLot, value); }
        }
        public bool IsBusyLT
        {
            get { return GetProperty(() => IsBusyLT); }
            set { SetProperty(() => IsBusyLT, value); }
        }
        public bool IsBusyIVL
        {
            get { return GetProperty(() => IsBusyIVL); }
            set { SetProperty(() => IsBusyIVL, value); }
        }
        public bool LumEdit
        {
            get { return QualityElementIVLList == null || QualityElementIVLList.Count == 0; }
        }
        public bool LTEdit
        {
            get { return QualityElementLTList == null || QualityElementLTList.Count == 0; }
        }
        public double Opacity
        {
            get { return GetProperty(() => Opacity); }
            set { SetProperty(() => Opacity, value); }
        }
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        #endregion

        #region Commands
        public ICommand PastingIVLFromClipboardCmd { get; set; }
        public ICommand PastingLTFromClipboardCmd { get; set; }
        public ICommand DelRawLTItemsCmd { get; set; }
        public ICommand DelRawIVLItemsCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand ExportExcelCmd { get; set; }
        #endregion

        public QualityRequestElementVM()
        {
            Opacity = 1;
            SaveCmd = new AsyncCommand(OnSave, () => Header.ItemCode != null && Header.LotNo != null && Header.InspectDate != null && Header.InspectorId != null);

            PastingIVLFromClipboardCmd = new DelegateCommand<PastingFromClipboardEvent>(OnPastingIVLFromClipboard);
            PastingLTFromClipboardCmd = new DelegateCommand<PastingFromClipboardEvent>(OnPastingLTFromClipboard);
            DelRawLTItemsCmd = new DelegateCommand(OnDelRawLTItems, () => QualityElementLTList != null && QualityElementLTList.Count > 0);
            DelRawIVLItemsCmd = new DelegateCommand(OnDelRawIVLItems, () => QualityElementIVLList != null && QualityElementIVLList.Count > 0);
            ExportExcelCmd = new DelegateCommand(OnExportExcel, () => true);
            ShowDialogCmd = new DelegateCommand(ShowDialog);
            NewCmd = new DelegateCommand(OnNew);
        }

        public void InitializeBinding(string qrNo)
        {
            Header = new QualityElementHeader(qrNo);

            QualityElementIVLList = new QualityElementIVLList(qrNo);
            LumRef = QualityElementIVLList.Count > 0 ? QualityElementIVLList.First().LumRef : (decimal?)null;
            LumLot = QualityElementIVLList.Count > 0 ? QualityElementIVLList.First().LumLot : (decimal?)null;
            BindingQualityElementIVLResult();

            QualityElementLTList = new QualityElementLTList(qrNo);
            LT = QualityElementLTList.Count > 0 ? QualityElementLTList.First().Const : (decimal?)null;
            BindingQualityElementLTResult();
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
                if (IsNew)
                {
                    foreach (var item in QualityElementIVLList) item.QrNo = Header.QrNo;
                    foreach (var item in QualityElementLTList)item.QrNo = Header.QrNo;
                }

                QualityElementIVLList.Save(QualityElementIVLResult);
                QualityElementLTList.Save(QualityElementLTResult);

                InitializeBinding(Header.QrNo);
                IsNew = false;
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

        public void OnPastingIVLFromClipboard(PastingFromClipboardEvent pm)
        {
            if (LumRef == null || LumLot == null)
            {
                MessageBoxService.ShowMessage("요구휘도를 입력하세요!", "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsBusyIVL = true;

            TableView view = pm.sender as TableView;
            PastingFromClipboardEventArgs e = pm.e as PastingFromClipboardEventArgs;
            e.Handled = true; // prevent pasting

            QualityElementIVLList = QualityElementIVLList ?? new QualityElementIVLList();

            if (QualityElementIVLList.Count == 0)
            {
                view.Grid.CurrentColumn = view.Grid.Columns[0]; // focus 위치
                view.PasteMode = DevExpress.Export.PasteMode.Append;
                view.OnPaste();

                foreach (var item in QualityElementIVLList)
                {
                    item.State = EntityState.Added;
                    item.QrNo = Header.QrNo;
                    item.LumRef = Convert.ToDecimal(LumRef);
                    item.LumLot = Convert.ToDecimal(LumLot);
                }
            }
            else
            {
                view.AllowEditing = true;
                view.PasteMode = DevExpress.Export.PasteMode.Update;
                view.OnPaste();
                view.AllowEditing = false;

                foreach (var item in QualityElementIVLList)
                {
                    if (item.State != EntityState.Added) item.State = EntityState.Modified;
                }
            }

            BindingQualityElementIVLResult("Pasting");
            RaisePropertyChanged(() => LumEdit);
            IsBusyIVL = false;
        }

        // 결과값(예측값) 산출
        private void BindingQualityElementIVLResult(string op = "")
        {
            try
            {
                List<QualityElementIVL> temp = new List<QualityElementIVL>();
                double?[] refL = new double?[2];
                double?[] l = new double?[2];

                for (int i = 0; i < QualityElementIVLList.Count; i++)
                {
                    if (QualityElementIVLList[i].RefL > Convert.ToDecimal(LumRef))
                    {
                        refL[0] = QualityElementIVLList[i - 1].RefL == null ? (double?)null : Convert.ToDouble(QualityElementIVLList[i - 1].RefL);
                        refL[1] = QualityElementIVLList[i].RefL == null ? (double?)null : Convert.ToDouble(QualityElementIVLList[i].RefL);

                        l[0] = QualityElementIVLList[i - 1].L == null ? (double?)null : Convert.ToDouble(QualityElementIVLList[i - 1].L);
                        l[1] = QualityElementIVLList[i].L == null ? (double?)null : Convert.ToDouble(QualityElementIVLList[i].L);

                        temp.Add(QualityElementIVLList[i - 1]);
                        temp.Add(QualityElementIVLList[i]);
                        break;
                    }
                }

                if (temp.Count != 2)
                {
                    QualityElementIVLResult = null;
                    return;
                }

                // List<double> refL = temp.Select(o => Convert.ToDouble(o.RefL ?? 0)).ToList();
                // List<double> l = temp.Select(o => Convert.ToDouble(o.L ?? 0)).ToList();
               
                double lumRef = Convert.ToDouble(LumRef);
                double lumLot = Convert.ToDouble(LumLot);

                string _refVResult;
                string _refCurrentResult;
                string _refJResult;
                string _refCdAResult;
                string _refLmWResult;
                string _refEQEResult;
                string _refLResult;
                string _refCIExResult;
                string _refCIEyResult;

                string _VResult;
                string _CurrentResult;
                string _JResult;
                string _CdAResult;
                string _LmWResult;
                string _EQEResult;
                string _LResult;
                string _CIExResult;
                string _CIEyResult;

                if (op == "Pasting")
                {
                    _refVResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefV ?? 0)).ToArray(), lumRef).ToString();
                    _refCurrentResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefCurrent ?? 0)).ToArray(), lumRef).ToString();
                    _refJResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefJ ?? 0)).ToArray(), lumRef).ToString();
                    _refCdAResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefCdA ?? 0)).ToArray(), lumRef).ToString();
                    _refLmWResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefLmW ?? 0)).ToArray(), lumRef).ToString();
                    _refEQEResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefEQE ?? 0)).ToArray(), lumRef).ToString();
                    _refLResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefL ?? 0)).ToArray(), lumRef).ToString();
                    _refCIExResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefCIEx ?? 0)).ToArray(), lumRef).ToString();
                    _refCIEyResult = ForecastFromCOM(refL, temp.Select(o => Convert.ToDouble(o.RefCIEy ?? 0)).ToArray(), lumRef).ToString();

                    _VResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.V ?? 0)).ToArray(), lumLot).ToString();
                    _CurrentResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.Current ?? 0)).ToArray(), lumLot).ToString();
                    _JResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.J ?? 0)).ToArray(), lumLot).ToString();
                    _CdAResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.CdA ?? 0)).ToArray(), lumLot).ToString();
                    _LmWResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.LmW ?? 0)).ToArray(), lumLot).ToString();
                    _EQEResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.EQE ?? 0)).ToArray(), lumLot).ToString();
                    _LResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.L ?? 0)).ToArray(), lumLot).ToString();
                    _CIExResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.CIEx ?? 0)).ToArray(), lumLot).ToString();
                    _CIEyResult = ForecastFromCOM(l, temp.Select(o => Convert.ToDouble(o.CIEy ?? 0)).ToArray(), lumLot).ToString();
                }
                else
                {
                    _refVResult = QualityElementIVLList.Result.RefV.ToString();
                    _refCurrentResult = QualityElementIVLList.Result.RefCurrent.ToString();
                    _refJResult = QualityElementIVLList.Result.RefJ.ToString();
                    _refCdAResult = QualityElementIVLList.Result.RefCdA.ToString();
                    _refLmWResult = QualityElementIVLList.Result.RefLmW.ToString();
                    _refEQEResult = QualityElementIVLList.Result.RefEQE.ToString();
                    _refLResult = QualityElementIVLList.Result.RefL.ToString();
                    _refCIExResult = QualityElementIVLList.Result.RefCIEx.ToString();
                    _refCIEyResult = QualityElementIVLList.Result.RefCIEy.ToString();

                    _VResult = QualityElementIVLList.Result.V.ToString();
                    _CurrentResult = QualityElementIVLList.Result.Current.ToString();
                    _JResult = QualityElementIVLList.Result.J.ToString();
                    _CdAResult = QualityElementIVLList.Result.CdA.ToString();
                    _LmWResult = QualityElementIVLList.Result.LmW.ToString();
                    _EQEResult = QualityElementIVLList.Result.EQE.ToString();
                    _LResult = QualityElementIVLList.Result.L.ToString();
                    _CIExResult = QualityElementIVLList.Result.CIEx.ToString();
                    _CIEyResult = QualityElementIVLList.Result.CIEy.ToString();
                }

                QualityElementIVLResult = new ObservableCollection<RawResult>
                {
                    new RawResult() { FieldName = "RefV",DisplayResult = _refVResult == "" ? "" : Math.Round(Convert.ToDouble(_refVResult), 2).ToString(), Result = _refVResult },
                    new RawResult() { FieldName = "RefCurrent", DisplayResult = _refCurrentResult == "" ? "" : Math.Round(Convert.ToDouble(_refCurrentResult), 3).ToString(), Result = _refCurrentResult },
                    new RawResult() { FieldName = "RefJ", DisplayResult = _refJResult == "" ? "" : Math.Round(Convert.ToDouble(_refJResult), 2).ToString(), Result = _refJResult },
                    new RawResult() { FieldName = "RefCdA", DisplayResult = _refCdAResult == "" ? "" : Math.Round(Convert.ToDouble(_refCdAResult), 2).ToString(), Result = _refCdAResult },
                    new RawResult() { FieldName = "RefLmW", DisplayResult = _refLmWResult == "" ? "" : Math.Round(Convert.ToDouble(_refLmWResult), 2).ToString(), Result = _refLmWResult },
                    new RawResult() { FieldName = "RefEQE", DisplayResult = _refEQEResult == "" ? "" : Math.Round(Convert.ToDouble(_refEQEResult), 2).ToString(), Result = _refEQEResult },
                    new RawResult() { FieldName = "RefL", DisplayResult = _refLResult == "" ? "" : Math.Round(Convert.ToDouble(_refLResult), 2).ToString(), Result = _refLResult  },
                    new RawResult() { FieldName = "RefCIEx", DisplayResult = _refCIExResult == "" ? "" : Math.Round(Convert.ToDouble(_refCIExResult), 3).ToString(), Result = _refCIExResult },
                    new RawResult() { FieldName = "RefCIEy", DisplayResult = _refCIEyResult == "" ? "" : Math.Round(Convert.ToDouble(_refCIEyResult), 3).ToString(), Result = _refCIEyResult },

                    new RawResult() { FieldName = "V",DisplayResult = _VResult == "" ? "" : Math.Round(Convert.ToDouble(_VResult), 2).ToString(), Result = _VResult },
                    new RawResult() { FieldName = "Current", DisplayResult = _CurrentResult == "" ? "" : Math.Round(Convert.ToDouble(_CurrentResult), 3).ToString(), Result = _CurrentResult },
                    new RawResult() { FieldName = "J", DisplayResult = _JResult == "" ? "" : Math.Round(Convert.ToDouble(_JResult), 2).ToString(), Result = _JResult },
                    new RawResult() { FieldName = "CdA", DisplayResult = _CdAResult == "" ? "" : Math.Round(Convert.ToDouble(_CdAResult), 2).ToString(), Result = _CdAResult },
                    new RawResult() { FieldName = "LmW", DisplayResult = _LmWResult == "" ? "" : Math.Round(Convert.ToDouble(_LmWResult), 2).ToString(), Result = _LmWResult },
                    new RawResult() { FieldName = "EQE", DisplayResult = _EQEResult == "" ? "" : Math.Round(Convert.ToDouble(_EQEResult), 2).ToString(), Result = _EQEResult },
                    new RawResult() { FieldName = "L", DisplayResult = _LResult == "" ? "" : Math.Round(Convert.ToDouble(_LResult), 2).ToString(), Result = _LResult  },
                    new RawResult() { FieldName = "CIEx", DisplayResult = _CIExResult == "" ? "" : Math.Round(Convert.ToDouble(_CIExResult), 3).ToString(), Result = _CIExResult },
                    new RawResult() { FieldName = "CIEy", DisplayResult = _CIEyResult == "" ? "" : Math.Round(Convert.ToDouble(_CIEyResult), 3).ToString(), Result = _CIEyResult },
                };
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        // Excel FORECAST 함수 구현, You can add references to the Office COM libraries from your C# project and call the WorksheetFunction.Forecast
        private double Forecast(List<double> xValues, List<double> yValues, double forecastPoint)
        {
            var xAverage = xValues.Average();
            var yAverage = yValues.Average();

            var bounds = yValues
                .Select((y, i) => new { Value = y, Index = i })
                .Aggregate(new { Top = 0.0, Bottom = 0.0 }, (acc, cur) =>
                    new
                    {
                        Top = acc.Top + (xValues[cur.Index] - xAverage) * (yValues[cur.Index] - yAverage),
                        Bottom = acc.Bottom + Math.Pow(xValues[cur.Index] - xAverage, 2.0)
                    });

            var level = bounds.Top / bounds.Bottom;

            return double.IsNaN(level) ? 0 : (yAverage - level * xAverage) + level * forecastPoint;
        }

        Microsoft.Office.Interop.Excel.Application xl;
        Microsoft.Office.Interop.Excel.WorksheetFunction wsf;
        private double? ForecastFromCOM(double?[] xValues, double[] yValues, double forecastPoint)
        {
            double[] xValues2 = new double[2];

            for (int i = 0; i < xValues.Length; i++)
            {
                if (xValues[i] == null)
                {
                    // Ref L, L(계산식 기준값임)값이 없을경우 return null
                    return null;
                }
                else
                {
                    xValues2[i] = (double)xValues[i];
                }
            }

            if(xl == null) xl = new Microsoft.Office.Interop.Excel.Application();
            if (wsf == null) wsf = xl.WorksheetFunction;
          
            return wsf.Forecast(forecastPoint, yValues, xValues2);
        }

        public void OnPastingLTFromClipboard(PastingFromClipboardEvent pm)
        {
            if (LT == null)
            {
                MessageBoxService.ShowMessage("LT를 입력하세요!", "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsBusyLT = true;

            TableView view = pm.sender as TableView;
            PastingFromClipboardEventArgs e = pm.e as PastingFromClipboardEventArgs;
            e.Handled = true; // prevent pasting
            
            QualityElementLTList = QualityElementLTList ?? new QualityElementLTList();

            if (QualityElementLTList.Count == 0)
            {
                view.Grid.CurrentColumn = view.Grid.Columns[0]; // focus 위치
                view.PasteMode = DevExpress.Export.PasteMode.Append;
                view.OnPaste();

                foreach (var item in QualityElementLTList)
                {
                    item.State = EntityState.Added;
                    item.QrNo = Header.QrNo;
                    item.Const = Convert.ToDecimal(LT);
                }
            }
            else
            {
                view.AllowEditing = true;
                view.PasteMode = DevExpress.Export.PasteMode.Update;
                view.OnPaste();
                view.AllowEditing = false;

                foreach (var item in QualityElementLTList)
                {
                    if (item.State != EntityState.Added) item.State = EntityState.Modified;
                }
            }

            BindingQualityElementLTResult();
            RaisePropertyChanged(() => LTEdit);
            IsBusyLT = false;
        }

        private void BindingQualityElementLTResult()
        {
            // 결과값 산출
            QualityElementLT temp = new QualityElementLT();

            foreach (var item in QualityElementLTList)
            {
                if (item.RefRef <= LT)
                    if (temp.RefHP == null)
                    {
                        temp.RefHP = Math.Ceiling(item.RefHP ?? 0);
                        temp.RefRef = Math.Ceiling(LT ?? 0);
                    }

                if (item.Ref <= LT)
                    if (temp.HP == null)
                    {
                        temp.HP = Math.Ceiling(item.HP ?? 0);
                        temp.Ref = Math.Ceiling(LT ?? 0);
                    }
            }

            QualityElementLTResult = new ObservableCollection<RawResult>
            {
                new RawResult() { Type = SummaryItemType.Custom, FieldName = "RefHP", Result = temp.RefHP.ToString() },
                new RawResult() { Type = SummaryItemType.Custom, FieldName = "RefRef", Result = temp.RefRef.ToString() },
                new RawResult() { Type = SummaryItemType.Custom, FieldName = "HP", Result = temp.HP.ToString() },
                new RawResult() { Type = SummaryItemType.Custom, FieldName = "Ref", Result = temp.Ref.ToString() },
            };
        }

        public void OnDelRawLTItems()
        {
            if (QualityElementLTList != null)
            {
                if (QualityElementLTList.FirstOrDefault().State == EntityState.Added)
                {
                    QualityElementLTList = null;
                    QualityElementLTResult = null;
                }
                else
                {
                    QualityElementLTList.ToList().ForEach(item =>
                    item.State = item.State == EntityState.Deleted ?
                    EntityState.Unchanged :
                    EntityState.Deleted);
                }
            }
        }

        public void OnDelRawIVLItems()
        {
            if (QualityElementIVLList != null)
            {
                if (QualityElementIVLList.FirstOrDefault().State == EntityState.Added)
                {
                    QualityElementIVLList = null;
                    QualityElementIVLResult = null;
                }
                else
                {
                    QualityElementIVLList.ToList().ForEach(item =>
                    item.State = item.State == EntityState.Deleted ?
                    EntityState.Unchanged :
                    EntityState.Deleted);
                }
            }
        }

        public void OnExportExcel()
        {
            Workbook workbook = new Workbook();
            workbook.LoadDocument(Assembly.GetExecutingAssembly().GetManifestResourceStream("MesAdmin.Reports.Element.xlsx"));

            workbook.BeginUpdate();
            try
            {
                // Write LT
                Worksheet worksheet = workbook.Worksheets["LT"];
                worksheet[11, 3].Value = Header.LotNo; // 로트번호

                for (int i = 0; i < QualityElementLTList.Count; i++)
                {
                    worksheet[i + 13, 1].Value = QualityElementLTList[i].RefHP;
                    worksheet[i + 13, 2].Value = QualityElementLTList[i].RefRef;
                    worksheet[i + 13, 3].Value = QualityElementLTList[i].HP;
                    worksheet[i + 13, 4].Value = QualityElementLTList[i].Ref;
                }

                // Device Structure
                if (Header.GetDeviceStructure() != null && Header.GetDeviceStructure().Rows.Count != 0)
                {
                    for (int i = 0; i < Header.GetDeviceStructure().Rows.Count; i++)
                    {
                        string ref01 = Header.GetDeviceStructure().Rows[i][0].ToString();
                        string[] ref01Array = ref01.Split(new char[] { '\\' });

                        for (int j = 0; j < ref01Array.Length; j++)
                        {
                            Borders cb = worksheet[i + 11, j + 12].Borders;
                            cb.SetAllBorders(System.Drawing.Color.Black, BorderLineStyle.Thin);

                            worksheet[i + 11, j + 12].SetValueFromText(ref01Array[j]);
                        }
                    }
                }

                // Write IVL
                worksheet = workbook.Worksheets["JV"];
                worksheet[11, 11].Value = Header.LotNo; // 로트번호
                for (int i = 0; i < QualityElementIVLList.Count; i++)
                {
                    worksheet[i + 13, 1].Value = QualityElementIVLList[i].RefV;
                    worksheet[i + 13, 2].Value = QualityElementIVLList[i].RefCurrent;
                    worksheet[i + 13, 3].Value = QualityElementIVLList[i].RefJ;
                    worksheet[i + 13, 4].Value = QualityElementIVLList[i].RefCdA;
                    worksheet[i + 13, 5].Value = QualityElementIVLList[i].RefLmW;
                    worksheet[i + 13, 6].Value = QualityElementIVLList[i].RefEQE;
                    worksheet[i + 13, 7].Value = QualityElementIVLList[i].RefL;
                    worksheet[i + 13, 8].Value = QualityElementIVLList[i].RefCIEx;
                    worksheet[i + 13, 9].Value = QualityElementIVLList[i].RefCIEy;

                    worksheet[i + 13, 11].Value = QualityElementIVLList[i].V;
                    worksheet[i + 13, 12].Value = QualityElementIVLList[i].Current;
                    worksheet[i + 13, 13].Value = QualityElementIVLList[i].J;
                    worksheet[i + 13, 14].Value = QualityElementIVLList[i].CdA;
                    worksheet[i + 13, 15].Value = QualityElementIVLList[i].LmW;
                    worksheet[i + 13, 16].Value = QualityElementIVLList[i].EQE;
                    worksheet[i + 13, 17].Value = QualityElementIVLList[i].L;
                    worksheet[i + 13, 18].Value = QualityElementIVLList[i].CIEx;
                    worksheet[i + 13, 19].Value = QualityElementIVLList[i].CIEy;
                }

                // Write Summary
                worksheet = workbook.Worksheets["Summary"];
                worksheet[9, 1].Value = Header.LotNo; // 로트번호

                decimal temp;
                if (QualityElementIVLResult != null)
                {
                    worksheet[8, 2].Value = decimal.TryParse(QualityElementIVLResult[0].Result, out temp) ? temp : worksheet[8, 2].Value;
                    worksheet[9, 2].Value = decimal.TryParse(QualityElementIVLResult[9].Result, out temp) ? temp : worksheet[9, 2].Value;

                    worksheet[8, 3].Value = decimal.TryParse(QualityElementIVLResult[2].Result, out temp) ? temp : worksheet[8, 3].Value;
                    worksheet[9, 3].Value = decimal.TryParse(QualityElementIVLResult[11].Result, out temp) ? temp : worksheet[9, 3].Value;

                    worksheet[8, 4].Value = decimal.TryParse(QualityElementIVLResult[3].Result, out temp) ? temp : worksheet[8, 4].Value;
                    worksheet[9, 4].Value = decimal.TryParse(QualityElementIVLResult[12].Result, out temp) ? temp : worksheet[9, 4].Value;
                    
                    if(QualityElementIVLResult[8].Result != "" && Convert.ToDecimal(QualityElementIVLResult[8].Result) != 0)
                        worksheet[8, 5].Value = Convert.ToDecimal(QualityElementIVLResult[3].Result) / Convert.ToDecimal(QualityElementIVLResult[8].Result);
                    if(QualityElementIVLResult[17].Result != "" && Convert.ToDecimal(QualityElementIVLResult[17].Result) != 0)
                        worksheet[9, 5].Value = Convert.ToDecimal(QualityElementIVLResult[12].Result) / Convert.ToDecimal(QualityElementIVLResult[17].Result);

                    worksheet[8, 6].Value = decimal.TryParse(QualityElementIVLResult[4].Result, out temp) ? temp : worksheet[8, 6].Value;
                    worksheet[9, 6].Value = decimal.TryParse(QualityElementIVLResult[13].Result, out temp) ? temp : worksheet[9, 6].Value;

                    worksheet[8, 7].Value = decimal.TryParse(QualityElementIVLResult[7].Result, out temp) ? temp : worksheet[8, 7].Value;
                    worksheet[9, 7].Value = decimal.TryParse(QualityElementIVLResult[16].Result, out temp) ? temp : worksheet[9, 7].Value;

                    worksheet[8, 8].Value = decimal.TryParse(QualityElementIVLResult[8].Result, out temp) ? temp : worksheet[8, 8].Value;
                    worksheet[9, 8].Value = decimal.TryParse(QualityElementIVLResult[17].Result, out temp) ? temp : worksheet[9, 8].Value;
                    
                    worksheet[8, 9].Value = Header.IVLRefMax;
                    worksheet[9, 9].Value = Header.IVLMax;

                    worksheet[8, 10].Value = decimal.TryParse(QualityElementIVLResult[6].Result, out temp) ? temp : worksheet[8, 10].Value;
                    worksheet[9, 10].Value = decimal.TryParse(QualityElementIVLResult[15].Result, out temp) ? temp : worksheet[9, 10].Value;

                    worksheet[8, 11].Value = decimal.TryParse(QualityElementIVLResult[5].Result, out temp) ? temp : worksheet[8, 11].Value;
                    worksheet[9, 11].Value = decimal.TryParse(QualityElementIVLResult[14].Result, out temp) ? temp : worksheet[9, 11].Value;
                }

                if (QualityElementLTResult != null)
                {
                    worksheet[8, 12].Value = decimal.TryParse(QualityElementLTResult[0].Result, out temp) ? temp : worksheet[8, 12].Value;
                    worksheet[9, 12].Value = decimal.TryParse(QualityElementLTResult[2].Result, out temp) ? temp : worksheet[9, 12].Value;
                }
            }
            finally
            {
                workbook.EndUpdate();
            }

            SaveFileDialogService.DefaultFileName = Header.LotNo + "_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            if (SaveFileDialogService.ShowDialog())
            {
                try
                {
                    workbook.SaveDocument(SaveFileDialogService.GetFullFileName());
                    System.Diagnostics.Process.Start(SaveFileDialogService.GetFullFileName());
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            }
        }

        public void ShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupStockVM("PE10")); // 재고를 조회할 창고전달
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
                Header.Qty = vmItem.ConfirmItem.Qty;
            }
        }

        public void OnNew()
        {
            InitializeBinding("");
            IsNew = true;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            // 품질정보에 따라 검사자 세팅
            WorkerCollections = new WorkerList("FQC", "BAC60");

            DocumentParamter pm = parameter as DocumentParamter;

            if (pm.Type != EntityMessageType.Added)
            {
                IsNew = false;
                string[] items = pm.Item as string[];
                InitializeBinding(items[1]);
            }
            else
            {
                InitializeBinding("");
                IsNew = true;
            }
          
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }

    public class RawResult
    {
        public SummaryItemType Type { get; set; }
        public string FieldName { get; set; }
        public string Result { get; set; }
        public string DisplayResult { get;set; }
    }
}

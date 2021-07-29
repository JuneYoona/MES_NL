using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class LotTracingVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public List<TreeElementDisplay> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public TreeElementDisplay SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public bool Forward
        {
            get { return GetProperty(() => Forward); }
            set { SetProperty(() => Forward, value); }
        }
        public bool Reverse
        {
            get { return GetProperty(() => Reverse); }
            set { SetProperty(() => Reverse, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
        }
        public string ItemSpec
        {
            get { return GetProperty(() => ItemSpec); }
            set { SetProperty(() => ItemSpec, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public DateTime? DocumentDate
        {
            get { return GetProperty(() => DocumentDate); }
            set { SetProperty(() => DocumentDate, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public decimal? Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool DetailBusy
        {
            get { return GetProperty(() => DetailBusy); }
            set { SetProperty(() => DetailBusy, value); }
        }
        public string Direction { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand SelectedItemChangedCmd { get; set; }
        #endregion

        public LotTracingVM()
        {
            Forward = true;

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            SelectedItemChangedCmd = new AsyncCommand(OnSelectedItemChanged);
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(LotNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            SelectedItem = null;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string lotNo = LotNo;
            Details = null;

            int count = 0;
            string lotno;
            string itemCode;
            string parentNameTL;
            string cProductOrderNo;
            string productOrderNo;
            int lvl;
            List<TreeElementDisplay> list = new List<TreeElementDisplay>();

            try
            {
                DataTable dt;

                if (Forward) // 정전개
                {
                    dt = new LotTracing(lotNo).Collections;
                    Direction = "Foward";
                }
                else
                {
                    dt = new LotTracing(lotNo).RCollections;
                    Direction = "Reverse ";
                }

                foreach (DataRow r in dt.AsEnumerable())
                {
                    itemCode = r.Field<string>("ItemCode");
                    lotno = r.Field<string>("LotNo");
                    productOrderNo = r.Field<string>("PProductOrderNo");
                    cProductOrderNo = r.Field<string>("CProductOrderNo");
                    lvl = r.Field<int>("lvl");
                    parentNameTL = null;

                    foreach (TreeElementDisplay treeElementDisplay in list)
                    {
                        if (treeElementDisplay.CProductOrderNo == productOrderNo)
                        {
                            parentNameTL = treeElementDisplay.KeyFieldName;
                            break;
                        }
                    }

                    list.Add(new TreeElementDisplay()
                    {
                        ItemCode = itemCode,
                        LotNo = lotno,
                        ProductOrderNo = productOrderNo,
                        CProductOrderNo = cProductOrderNo,
                        KeyFieldName = Convert.ToString(count++),
                        ParentFieldName = parentNameTL,
                        Lvl = lvl
                    });
                }
                Collections = list;
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }

            ItemCode = null;
            ItemName = null;
            ItemSpec = null;
            BasicUnit = null;
            DocumentDate = null;
            WaCode = null;
            Qty = null;
            Details = null;

            IsBusy = false;
        }

        public Task OnSelectedItemChanged()
        {
            DetailBusy = true;
            return Task.Factory.StartNew(SelectedItemChangedCore).ContinueWith(task => DetailBusy = false);
        }
        public void SelectedItemChangedCore()
        {
            if (SelectedItem != null)
            {
                DataTable detail = new LotTracing
                {
                    ProductOrderNo = SelectedItem.CProductOrderNo,
                    ItemCode = SelectedItem.ItemCode,
                    LotNo = SelectedItem.LotNo,
                }.Details;
                
                // 출하로트는 CProductOrderNo 또는ProductOrderNo 번호를 강제로 출하로트로 등록
                if (SelectedItem.LotNo == SelectedItem.CProductOrderNo || SelectedItem.LotNo == SelectedItem.ProductOrderNo)
                {
                    var rows = detail.AsEnumerable().Where(u => u.Field<string>("DCFlag") == "C");
                    Details = rows.Any() ? rows.CopyToDataTable() : null;

                    DataRow dr = rows.FirstOrDefault();
                    ItemCode = dr == null ? null : (string)dr["ItemCode"];
                    ItemName = dr == null ? null : (string)dr["ItemName"];
                    ItemSpec = dr == null ? null : (string)dr["ItemSpec"];
                    BasicUnit = dr == null ? null : (string)dr["BasicUnit"];
                    DocumentDate = null;
                    WaCode = null;
                    Qty = null;
                }
                else
                {
                    var rows = detail.AsEnumerable().Where(u => u.Field<string>("DCFlag") == "D").OrderBy(u => u.Field<string>("DocumentNo"));
                    Details = rows.Any() ? rows.CopyToDataTable() : null;

                    DataRow dr = rows.FirstOrDefault();
                    ItemCode = dr == null ? null : (string)dr["ItemCode"];
                    ItemName = dr == null ? null : (string)dr["ItemName"];
                    ItemSpec = dr == null ? null : (string)dr["ItemSpec"];
                    BasicUnit = dr == null ? null : (string)dr["BasicUnit"];
                    DocumentDate = dr == null ? null : (DateTime?)dr["DocumentDate"];
                    WaCode = dr == null ? null : (string)dr["WaCode"];
                    Qty = dr == null ? null : (decimal?)dr["Qty"];
                }
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }

    public class TreeElementDisplay
    {
        public string ProductOrderNo { get; set; }
        public string CProductOrderNo { get; set; }
        public string ItemCode { get; set; }
        public string LotNo { get; set; }
        public string KeyFieldName { get; set; }
        public string ParentFieldName { get; set; }
        public int Lvl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using DevExpress.Mvvm;

namespace MesAdmin.Models
{
    public class ProductionEqpIn : ViewModelBase
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string OrderNo
        {
            get { return GetProperty(() => OrderNo); }
            set { SetProperty(() => OrderNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
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
        public decimal InQty
        {
            get { return GetProperty(() => InQty); }
            set { SetProperty(() => InQty, value); }
        }
        public decimal ProductQty
        {
            get { return GetProperty(() => ProductQty); }
            set { SetProperty(() => ProductQty, value); }
        }
        public decimal CancelQty
        {
            get { return GetProperty(() => CancelQty); }
            set { SetProperty(() => CancelQty, value); }
        }
        public decimal RemainQty
        {
            get { return GetProperty(() => RemainQty); }
            set { SetProperty(() => RemainQty, value); }
        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
    }

    public class ProductionEqpInList : ObservableCollection<ProductionEqpIn>
    {
        private string eqpCode;

        public ProductionEqpInList(string eqpCode)
        {
            this.eqpCode = eqpCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit FROM production_EqpIn A (NOLOCK) ";
            str += "INNER JOIN common_Item B (NOLOCK) ON A.ItemCode = B.ItemCode ";
            str += "WHERE A.RemainQty > 0 AND EqpCode = '" + eqpCode + "'";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionEqpIn
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        OrderNo = (string)u["OrderNo"],
                        Seq = (int)u["Seq"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        EqpCode = (string)u["EqpCode"],
                        LotNo = (string)u["LotNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        InQty = (decimal)u["InQty"],
                        ProductQty = (decimal)u["ProductQty"],
                        CancelQty = (decimal)u["CancelQty"],
                        RemainQty = (decimal)u["RemainQty"]
                    }
                )
            );
        }
    }
}

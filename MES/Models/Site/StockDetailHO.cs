using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;

namespace MesAdmin.Models
{
    public class StockDetailHO : ViewModelBase, IEquatable<StockDetailHO>
    {
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
        public DateTime FinishDate
        {
            get { return GetProperty(() => FinishDate); }
            set { SetProperty(() => FinishDate, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
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
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public string ItemSpec
        {
            get { return GetProperty(() => ItemSpec); }
            set { SetProperty(() => ItemSpec, value); }
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
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public string Remark5
        {
            get { return GetProperty(() => Remark5); }
            set { SetProperty(() => Remark5, value); }
        }
        public string QrState
        {
            get { return GetProperty(() => QrState); }
            set { SetProperty(() => QrState, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }

        }
        public decimal QrQty
        {
            get { return GetProperty(() => QrQty); }
            set { SetProperty(() => QrQty, value); }

        }
        public decimal BadQty
        {
            get { return GetProperty(() => BadQty); }
            set { SetProperty(() => BadQty, value); }

        }
        public decimal PickingQty
        {
            get { return GetProperty(() => PickingQty); }
            set { SetProperty(() => PickingQty, value); }

        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
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
        public decimal EqpQty
        {
            get { return GetProperty(() => EqpQty); }
            set { SetProperty(() => EqpQty, value); }

        }

        public bool Equals(StockDetailHO other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return BizAreaCode.Equals(other.BizAreaCode) && ItemCode.Equals(other.ItemCode) && WhCode.Equals(other.WhCode) && WaCode.Equals(other.WaCode) && LotNo.Equals(other.LotNo);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode()
        {
            int hashBizAreaCode = BizAreaCode == null ? 0 : BizAreaCode.GetHashCode();
            int hashItemCode = ItemCode == null ? 0 : ItemCode.GetHashCode();
            int hashWhCode = WhCode == null ? 0 : WhCode.GetHashCode();
            int hashWaCode = WaCode == null ? 0 : WaCode.GetHashCode();
            int hashLotNo = LotNo == null ? 0 : LotNo.GetHashCode();
            ////Calculate the hash code for the pk.
            return hashBizAreaCode ^ hashItemCode ^ hashWhCode ^ hashWaCode ^ hashLotNo;
        }
    }

    public class StockDetailHOList : ObservableCollection<StockDetailHO>
    {
        public StockDetailHOList()
        {
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM view_StockDetailHO");
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockDetailHO
                    {
                        ProductOrderNo = (string)u["ProductOrderNo"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        WaCode = (string)u["WaCode"],
                        LotNo = (string)u["LotNo"],
                        Remark5 = u["Remark5"].ToString(),
                        QrState = u["QrState"].ToString(),
                        Qty = (decimal)u["Qty"],
                        QrQty = (decimal)u["QrQty"],
                        BasicUnit = (string)u["BasicUnit"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                        FinishDate = (DateTime)u["FinishDate"],
                    }
                )
            );
        }
    }
}

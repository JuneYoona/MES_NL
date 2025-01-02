using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;
using System.Data.SqlClient;

namespace MesAdmin.Models
{
    public class StockDetail : ViewModelBase, IEquatable<StockDetail>
    {
        public EntityState State
        {
            get { return GetProperty(() => State); }
            set { SetProperty(() => State, value); }
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
        public decimal BalanceQty
        {
            get { return GetProperty(() => BalanceQty); }
            set { SetProperty(() => BalanceQty, value); }
        }
        public int Bottle
        {
            get { return GetProperty(() => Bottle); }
            set { SetProperty(() => Bottle, value); }
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
        public string BottleLotNo
        {
            get { return GetProperty(() => BottleLotNo); }
            set { SetProperty(() => BottleLotNo, value); }

        }
        public string Remark4
        {
            get { return GetProperty(() => Remark4); }
            set { SetProperty(() => Remark4, value); }

        }
        public string Remark5
        {
            get { return GetProperty(() => Remark5); }
            set { SetProperty(() => Remark5, value); }

        }
        public string Remark6
        {
            get { return GetProperty(() => Remark6); }
            set { SetProperty(() => Remark6, value); }

        }
        public string Remark7
        {
            get { return GetProperty(() => Remark7); }
            set { SetProperty(() => Remark7, value); }

        }
        public string WE10LotNo
        {
            get { return GetProperty(() => WE10LotNo); }
            set { SetProperty(() => WE10LotNo, value); }

        }
        public string ProcureType
        {
            get { return GetProperty(() => ProcureType); }
            set { SetProperty(() => ProcureType, value); }

        }
        public string StockNo
        {
            get { return GetProperty(() => StockNo); }
            set { SetProperty(() => StockNo, value); }

        }
        public string Container
        {
            get { return GetProperty(() => Container); }
            set { SetProperty(() => Container, value); }

        }
        public DateTime? ExpDate
        {
            get { return GetProperty(() => ExpDate); }
            set { SetProperty(() => ExpDate, value); }

        }
        public DateTime? ProductDate
        {
            get { return GetProperty(() => ProductDate); }
            set { SetProperty(() => ProductDate, value); }
        }
        public string FontColor { get; set; }

        public bool Equals(StockDetail other)
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

        public void Save(Database db, DbTransaction trans)
        {
            DbCommand dbCom = db.GetStoredProcCommand("usps_stock_detail_Update");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, BizAreaCode);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
            db.AddInParameter(dbCom, "@WhCode", DbType.String, WhCode);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, WaCode);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
            db.AddInParameter(dbCom, "@Remark1", DbType.String, Remark4);
            db.AddInParameter(dbCom, "@Remark2", DbType.String, Remark5);
            db.AddInParameter(dbCom, "@Qty", DbType.Decimal, Qty);
            db.AddInParameter(dbCom, "@BasicUnit", DbType.String, BasicUnit);
            db.AddInParameter(dbCom, "@ExpDate", DbType.Date, ExpDate);
            db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
            db.ExecuteNonQuery(dbCom, trans);
        }
    }

    public class StockDetailList : ObservableCollection<StockDetail>
    {
        private string bizAreaCode;
        private string whCode;
        private string itemAccount;

        public StockDetailList(IEnumerable<StockDetail> items) : base(items) { }
        public StockDetailList(string bizAreaCode = "", string whCode = "", string itemAccount = "")
        {
            this.bizAreaCode = bizAreaCode;
            this.whCode = whCode;
            this.itemAccount = itemAccount;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM view_StockDetailEx WHERE Qty + EqpQty + QrQty + BadQty > 0";
            if (!string.IsNullOrEmpty(bizAreaCode))
                str += " And BizAreaCode = '" + bizAreaCode + "'";
            if (!string.IsNullOrEmpty(whCode))
                str += " And WhCode = '" + whCode + "'";
            if (!string.IsNullOrEmpty(itemAccount))
                str += " And ItemAccount = '" + itemAccount + "'";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockDetail
                    {
                        State = EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        BalanceQty = (decimal)u["Qty"] - (decimal)u["PickingQty"],
                        EqpQty = (decimal)u["EqpQty"],
                        QrQty = (decimal)u["QrQty"],
                        BadQty = (decimal)u["BadQty"],
                        PickingQty = (decimal)u["PickingQty"],
                        BasicUnit = (string)u["BasicUnit"],
                        Remark4 = u["Remark4"].ToString(),
                        Remark5 = u["Remark5"].ToString(),
                        Remark6 = u["PIG"].ToString(),
                        Remark7 = u["TSC"].ToString(),
                        WE10LotNo = u["WE10LotNo"].ToString(),
                        ExpDate = u["ExpDate"] == DBNull.Value ? null : (DateTime?)u["ExpDate"],
                        ProductDate = u["ProductDate"] == DBNull.Value ? null : (DateTime?)u["ProductDate"],
                        StockNo = u["StockNo"].ToString(),
                        Container = u["PartCode"].ToString(),
                    }
                )
            );
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    foreach (var item in Items.Where(o => o.State != EntityState.Unchanged)) item.Save(db, trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    string message = ex is SqlException sqlEx ? sqlEx.Errors[0].Message : ex.Message;
                    throw new Exception(message);
                }
            }
        }
    }

    public class StockDailyList : ObservableCollection<StockDetail>
    {
        private DateTime date;

        public StockDailyList() { }
        public StockDailyList(IEnumerable<StockDetail> items) : base(items) { }
        public StockDailyList(DateTime date)
        {
            this.date = date;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_GetStock_Daily(@Date)");
            db.AddInParameter(dbCom, "@Date", DbType.String, date.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockDetail
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        QrQty = (decimal)u["QrQty"],
                        BadQty = (decimal)u["BadQty"],
                        BasicUnit = (string)u["BasicUnit"]
                    }
                )
            );
        }
    }

    public class StockDailtyListItem : StockDailyList
    {
        private DateTime date;

        public StockDailtyListItem(IEnumerable<StockDetail> items) : base(items) { }
        public StockDailtyListItem(DateTime date)
        {
            this.date = date;
            this.InitializeItemList();
        }

        public void InitializeItemList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_GetStock_Daily(@Date)");
            db.AddInParameter(dbCom, "@Date", DbType.String, date.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            var temp = ds.Tables[0].AsEnumerable()
                        .GroupBy(row => new
                        {
                            BizAreaCode = row.Field<string>("BizAreaCode"),
                            ItemCode = row.Field<string>("ItemCode"),
                            ItemAccount = row.Field<string>("ItemAccount"),
                            ItemName = row.Field<string>("ItemName"),
                            ItemSpec = row.Field<string>("ItemSpec"),
                            WhCode = row.Field<string>("WhCode"),
                            BasicUnit = row.Field<string>("BasicUnit"),

                        })
                       .Select(g => new
                       {
                           BizAreaCode = g.Key.BizAreaCode,
                           ItemCode = g.Key.ItemCode,
                           ItemAccount = g.Key.ItemAccount,
                           ItemName = g.Key.ItemName,
                           ItemSpec = g.Key.ItemSpec,
                           WhCode = g.Key.WhCode,
                           BasicUnit = g.Key.BasicUnit,
                           Qty = g.Sum(x => x.Field<decimal>("Qty")),
                           QrQty = g.Sum(x => x.Field<decimal>("QrQty")),
                           BadQty = g.Sum(x => x.Field<decimal>("BadQty")),
                       });

            foreach (var item in temp)
            {
                base.Add(
                    new StockDetail
                    {
                        BizAreaCode = item.BizAreaCode,
                        ItemCode = item.ItemCode,
                        ItemAccount = item.ItemAccount,
                        ItemName = item.ItemName,
                        ItemSpec = item.ItemSpec,
                        WhCode = item.WhCode,
                        Qty = item.Qty,
                        QrQty = item.QrQty,
                        BadQty = item.BadQty,
                        BasicUnit = item.BasicUnit
                    }
                );
            }

        }
    }

    public class StockItemList : ObservableCollection<StockDetail>
    {
        private string bizAreaCode;
        private string whCode;
        private string itemAccount;

        public StockItemList(IEnumerable<StockDetail> items) : base(items) { }
        public StockItemList(string bizAreaCode = "", string whCode = "", string itemAccount = "")
        {
            this.bizAreaCode = bizAreaCode;
            this.whCode = whCode;
            this.itemAccount = itemAccount;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_GetStock_Item");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@WhCode", DbType.String, whCode);
            db.AddInParameter(dbCom, "@ItemAccount", DbType.String, itemAccount);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockDetail
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        WhCode = (string)u["WhCode"],
                        Qty = (decimal)u["Qty"],
                        QrQty = (decimal)u["QrQty"],
                        BadQty = (decimal)u["BadQty"],
                        EqpQty = (decimal)u["EqpQty"],
                        PickingQty = (decimal)u["PickingQty"],
                        BasicUnit = (string)u["BasicUnit"],
                        FontColor = (string)u["FontColor"]
                    }
                )
            );
        }
    }
}

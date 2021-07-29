using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Models
{
    public class PurcharseWarehousing : StateBusinessObject
    {
        public string GrNo
        {
            get { return GetProperty(() => GrNo); }
            set { SetProperty(() => GrNo, value); }
        }
        public int GrSeq
        {
            get { return GetProperty(() => GrSeq); }
            set { SetProperty(() => GrSeq, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public DateTime InputDate
        {
            get { return GetProperty(() => InputDate); }
            set { SetProperty(() => InputDate, value); }
        }
        public string PoNo
        {
            get { return GetProperty(() => PoNo); }
            set { SetProperty(() => PoNo, value); }
        }
        public int PoSeq
        {
            get { return GetProperty(() => PoSeq); }
            set { SetProperty(() => PoSeq, value); }
        }
        public string PmNo
        {
            get { return GetProperty(() => PmNo); }
            set { SetProperty(() => PmNo, value); }
        }
        public int? PmSeq
        {
            get { return GetProperty(() => PmSeq); }
            set { SetProperty(() => PmSeq, value); }
        }
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public decimal QrQty
        {
            get { return GetProperty(() => QrQty); }
            set { SetProperty(() => QrQty, value); }
        }
        public string QrResultNo
        {
            get { return GetProperty(() => QrResultNo); }
            set { SetProperty(() => QrResultNo, value); }
        }
        public DateTime? QrResultDate
        {
            get { return GetProperty(() => QrResultDate); }
            set { SetProperty(() => QrResultDate, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public bool QrFlag
        {
            get { return GetProperty(() => QrFlag); }
            set { SetProperty(() => QrFlag, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
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

    public class PurcharseWarehousingList : ObservableCollection<PurcharseWarehousing>
    {
        private string grNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public PurcharseWarehousingList() { }
        public PurcharseWarehousingList(IEnumerable<PurcharseWarehousing> items) : base(items) { }
        public PurcharseWarehousingList(string grNo="", DateTime? startDate=null, DateTime? endDate=null)
        {
            this.grNo = grNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT A.*, B.ItemName, B.ItemSpec FROM purcharse_Warehousing A (NOLOCK) INNER JOIN common_Item B (NOLOCK) ON A.ItemCode=B.ItemCode ";
            sql += "WHERE GrNo = GrNo ";
            if (!string.IsNullOrEmpty(grNo))
                sql += "And GrNo = '" + grNo + "' ";
            if (startDate != null && endDate != null)
                sql += "And InputDate BETWEEN '"+ startDate.ToString().Substring(0, 10) +"' AND '" + endDate.ToString().Substring(0, 10) + "' ";
            sql += "ORDER BY InputDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new PurcharseWarehousing
                    {
                        GrNo = (string)u["GrNo"],
                        GrSeq = (int)u["GrSeq"],
                        BizCode = (string)u["BizCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        BasicUnit = (string)u["BasicUnit"],
                        InputDate = (DateTime)u["InputDate"],
                        PoNo = (string)u["PoNo"],
                        PoSeq = (int)u["PoSeq"],
                        PmNo = (string)u["PmNo"],
                        PmSeq = (int?)u["PmSeq"],
                        QrNo = (string)u["QrNo"],
                        QrQty = (decimal)u["QrQty"],
                        QrResultNo = u["QrResultNo"].ToString(),
                        QrResultDate = u["QrResultDate"] == DBNull.Value ? null : (DateTime?)u["QrResultDate"],
                        WhCode = (string)u["WhCode"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        Memo = u["Memo"].ToString(),
                        QrFlag = (bool)u["QrFlag"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public string Save()
        {
            string grNo;

            IEnumerable<PurcharseWarehousing> items = this.Items;
            grNo = Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));

            return grNo;
        }

        public string Insert(IEnumerable<PurcharseWarehousing> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand("SELECT dbo.fn_GetWarehousingNo('GrNo')");
                    string grNo = (string)db.ExecuteScalar(dbCom, trans); // 입고번호

                    dbCom = db.GetSqlStringCommand("SELECT dbo.fn_GetWarehousingNo('PmNo')");
                    string pmNo = (string)db.ExecuteScalar(dbCom, trans); // 재고처리번호
                    int idx = 0; // 재고처리순번

                    string qrNo;
                    foreach (PurcharseWarehousing item in items.Where(u => u.QrFlag == true))
                    {
                        dbCom = db.GetSqlStringCommand("SELECT dbo.fn_GetWarehousingNo('QrNo')"); // 검사요청번호
                        qrNo = (string)db.ExecuteScalar(dbCom, trans);

                        dbCom = db.GetStoredProcCommand("usp_purcharse_Warehousing");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@GrNo", DbType.String, grNo);
                        db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                        db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                        db.AddInParameter(dbCom, "@InputDate", DbType.Date, item.InputDate);
                        db.AddInParameter(dbCom, "@PoNo", DbType.String, item.PoNo);
                        db.AddInParameter(dbCom, "@PoSeq", DbType.Int16, item.PoSeq);
                        db.AddInParameter(dbCom, "@PmNo", DbType.String, pmNo);
                        db.AddInParameter(dbCom, "@PmSeq", DbType.Int16, ++idx);
                        db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
                        db.AddInParameter(dbCom, "@QrQty", DbType.Decimal, item.QrQty);
                        db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                        db.AddInParameter(dbCom, "@QrFlag", DbType.String, item.QrFlag);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.ExecuteNonQuery(dbCom, trans);
                    }

                    // 수입검사 미품목 처리
                    dbCom = db.GetSqlStringCommand("SELECT dbo.fn_GetWarehousingNo('PmNo')");
                    pmNo = (string)db.ExecuteScalar(dbCom, trans); // 재고처리번호
                    idx = 0;
                    foreach (PurcharseWarehousing item in items.Where(u => u.QrFlag == false))
                    {
                        dbCom = db.GetStoredProcCommand("usp_purcharse_Warehousing");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@GrNo", DbType.String, grNo);
                        db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                        db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                        db.AddInParameter(dbCom, "@InputDate", DbType.String, item.InputDate.ToShortDateString());
                        db.AddInParameter(dbCom, "@PoNo", DbType.String, item.PoNo);
                        db.AddInParameter(dbCom, "@PoSeq", DbType.Int16, item.PoSeq);
                        db.AddInParameter(dbCom, "@PmNo", DbType.String, pmNo);
                        db.AddInParameter(dbCom, "@PmSeq", DbType.Int16, ++idx);
                        db.AddInParameter(dbCom, "@QrNo", DbType.String, "");
                        db.AddInParameter(dbCom, "@QrQty", DbType.Decimal, 0);
                        db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                        db.AddInParameter(dbCom, "@QrFlag", DbType.Boolean, item.QrFlag);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    
                    trans.Commit();
                    return grNo;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete(IEnumerable<PurcharseWarehousing> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                string str;
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (PurcharseWarehousing detail in items)
                    {
                        str = string.Format("DELETE purcharse_Warehousing WHERE GrNo = '{0}' AND GrSeq = {1}", detail.GrNo, detail.GrSeq);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }

    public class PurcharseWarehousingHeaderList : ObservableCollection<PurcharseWarehousing>
    {
        private string grNo;
        private DateTime startDate;
        private DateTime endDate;

        public PurcharseWarehousingHeaderList() { }
        public PurcharseWarehousingHeaderList(IEnumerable<PurcharseWarehousing> items) : base(items) { }
        public PurcharseWarehousingHeaderList(DateTime startDate, DateTime endDate, string grNo = "")
        {
            this.grNo = grNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT GrNo, BizCode, InputDate, Memo, UpdateId FROM purcharse_Warehousing ";
            sql += "WHERE InputDate BETWEEN '" + startDate.ToShortDateString() + "' AND '" + endDate.ToShortDateString() + "' ";
            if (!string.IsNullOrEmpty(grNo))
                sql += "And GrNo = '" + grNo + "' ";
            sql += "GROUP BY GrNo, BizCode, InputDate, Memo, UpdateId ORDER BY InputDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new PurcharseWarehousing
                    {
                        GrNo = (string)u["GrNo"],
                        BizCode = (string)u["BizCode"],
                        InputDate = (DateTime)u["InputDate"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"]
                    }
                )
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace MesAdmin.Models
{
    public class MaterialDispenseDetailSub : StateBusinessObject
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string MDNo
        {
            get { return GetProperty(() => MDNo); }
            set { SetProperty(() => MDNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public int SubSeq
        {
            get { return GetProperty(() => SubSeq); }
            set { SetProperty(() => SubSeq, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal DspQty
        {
            get { return GetProperty(() => DspQty); }
            set { SetProperty(() => DspQty, value); }
        }
        public string OutWhCode
        {
            get { return GetProperty(() => OutWhCode); }
            set { SetProperty(() => OutWhCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string InWhCode
        {
            get { return GetProperty(() => InWhCode); }
            set { SetProperty(() => InWhCode, value); }
        }
        public string STNo
        {
            get { return GetProperty(() => STNo); }
            set { SetProperty(() => STNo, value); }
        }
        public int STSeq
        {
            get { return GetProperty(() => STSeq); }
            set { SetProperty(() => STSeq, value); }
        }
        public DateTime DspDate
        {
            get { return GetProperty(() => DspDate); }
            set { SetProperty(() => DspDate, value); }
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
        public string TSC
        {
            get { return GetProperty(() => TSC); }
            set { SetProperty(() => TSC, value); }
        }
        public string PIG
        {
            get { return GetProperty(() => PIG); }
            set { SetProperty(() => PIG, value); }
        }
        public DateTime? ExpDate
        {
            get { return GetProperty(() => ExpDate); }
            set { SetProperty(() => ExpDate, value); }
        }
    }

    public class MaterialDispenseDetailSubList : ObservableCollection<MaterialDispenseDetailSub>
    {
        private string mdNo;
        private int seq;
        private DateTime? startDate;
        private DateTime? endDate;

        public MaterialDispenseDetailSubList() { }
        public MaterialDispenseDetailSubList(IEnumerable<MaterialDispenseDetailSub> items) : base(items) { }
        public MaterialDispenseDetailSubList(string mdNo = "", int seq = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            this.mdNo = mdNo;
            this.seq = seq;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "";
            if (!string.IsNullOrEmpty(mdNo))
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit FROM material_Dispense_DetailSub (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN material_Dispense_Detail (NOLOCK) C ON A.MDNo = C.MDNo AND A.Seq = C.Seq "
                    + "WHERE A.MDNo = @MDNo AND A.Seq = @Seq";
            else
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit FROM material_Dispense_DetailSub (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN material_Dispense_Detail (NOLOCK) C ON A.MDNo = C.MDNo AND A.Seq = C.Seq "
                    + "WHERE A.DspDate BETWEEN @StartDate AND @EndDate "
                    + "ORDER BY A.DspDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@MDNo", DbType.String, mdNo);
            db.AddInParameter(dbCom, "@Seq", DbType.String, seq);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate == null ? "" : startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate == null ? "" : endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new MaterialDispenseDetailSub
                    {
                        MDNo = (string)u["MDNo"],
                        Seq = (int)u["Seq"],
                        SubSeq = (int)u["SubSeq"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        LotNo = (string)u["LotNo"],
                        DspQty = (decimal)u["DspQty"],
                        DspDate = (DateTime)u["DspDate"],
                        OutWhCode = (string)u["OutWhCode"],
                        InWhCode = (string)u["InWhCode"],
                        //STNo = (string)u["STNo"],
                        //STSeq = (int)u["STSeq"],
                        Memo = u["Memo"].ToString(),
                        TSC = u["TSC"].ToString(),
                        PIG = u["PIG"].ToString(),
                        ExpDate = u["ExpDate"] == DBNull.Value ? (DateTime?)null : (DateTime)u["ExpDate"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<MaterialDispenseDetailSub> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == EntityState.Added), db, trans, dbCom);
                    Delete(items.Where(u => u.State == EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public void Insert(IEnumerable<MaterialDispenseDetailSub> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (MaterialDispenseDetailSub item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_material_Dispense_DetailSub");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@MDNo", DbType.String, item.MDNo);
                db.AddInParameter(dbCom, "@Seq", DbType.Int16, item.Seq);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                db.AddInParameter(dbCom, "@DspDate", DbType.Date, item.DspDate);
                db.AddInParameter(dbCom, "@DspQty", DbType.Decimal, item.DspQty);
                db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                db.AddInParameter(dbCom, "@OutWhCode", DbType.String, item.OutWhCode);
                db.AddInParameter(dbCom, "@InWhCode", DbType.String, item.InWhCode);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@TSC", DbType.String, item.TSC);
                db.AddInParameter(dbCom, "@PIG", DbType.String, item.PIG);
                db.AddInParameter(dbCom, "@ExpDate", DbType.Date, item.ExpDate);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<MaterialDispenseDetailSub> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;

            foreach (MaterialDispenseDetailSub item in items)
            {
                str = string.Format("DELETE material_Dispense_DetailSub WHERE MDNo = '{0}' AND Seq = {1} AND SubSeq = {2}", item.MDNo, item.Seq, item.SubSeq);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}

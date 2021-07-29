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
    public class MaterialDispenseDetail : StateBusinessObject
    {
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
        public decimal ReqQty
        {
            get { return GetProperty(() => ReqQty); }
            set { SetProperty(() => ReqQty, value); }
        }
        public string InWhCode
        {
            get { return GetProperty(() => InWhCode); }
            set { SetProperty(() => InWhCode, value); }
        }
        public string PostFlag
        {
            get { return GetProperty(() => PostFlag); }
            set { SetProperty(() => PostFlag, value); }
        }
        public string CloseFlag
        {
            get { return GetProperty(() => CloseFlag); }
            set { SetProperty(() => CloseFlag, value); }
        }
        public DateTime ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        public DateTime DlvyDate
        {
            get { return GetProperty(() => DlvyDate); }
            set { SetProperty(() => DlvyDate, value); }
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

        public MaterialDispenseDetail() { }
        public MaterialDispenseDetail(string mdNo, int seq)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT A.*, B.ItemName FROM material_Dispense_Detail (NOLOCK) A "
                        + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode WHERE MDNo = @MDNo AND Seq = @Seq";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@MDNo", DbType.String, mdNo);
            db.AddInParameter(dbCom, "@Seq", DbType.Int16, seq);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                MDNo = (string)u["MDNo"];
                Seq = (int)u["Seq"];
                ReqQty = (decimal)u["ReqQty"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                InWhCode = (string)u["InWhCode"];
                PostFlag = (string)u["PostFlag"];
                CloseFlag = (string)u["CloseFlag"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }
    }

    public class MaterialDispenseDetailList : ObservableCollection<MaterialDispenseDetail>
    {
        private string mdNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public MaterialDispenseDetailList() { }
        public MaterialDispenseDetailList(IEnumerable<MaterialDispenseDetail> items) : base(items) { }
        public MaterialDispenseDetailList(string mdNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.mdNo = mdNo;
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
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit, C.ReqDate, C.DlvyDate FROM material_Dispense_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN material_Dispense_Header (NOLOCK) C ON A.MDNo = C.MDNo "
                    + "WHERE A.MDNo = @MDNo";
            else
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit, C.ReqDate, C.DlvyDate FROM material_Dispense_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN material_Dispense_Header (NOLOCK) C ON A.MDNo = C.MDNo "
                    + "WHERE C.ReqDate BETWEEN @StartDate AND @EndDate "
                    + "ORDER BY C.ReqDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@MDNo", DbType.String, mdNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate == null ? "" : startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate == null ? "" : endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new MaterialDispenseDetail
                    {
                        MDNo = (string)u["MDNo"],
                        Seq = (int)u["Seq"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        ReqQty = (decimal)u["ReqQty"],
                        ReqDate = (DateTime)u["ReqDate"],
                        DlvyDate = (DateTime)u["DlvyDate"],
                        InWhCode = (string)u["InWhCode"],
                        PostFlag = (string)u["PostFlag"],
                        CloseFlag = (string)u["CloseFlag"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<MaterialDispenseDetail> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added), db, trans, dbCom);
                    Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<MaterialDispenseDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (MaterialDispenseDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_material_Dispense_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@MDNo", DbType.String, item.MDNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@ReqQty", DbType.Decimal, item.ReqQty);
                db.AddInParameter(dbCom, "@InWhCode", DbType.String, item.InWhCode);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }

            // 자동메일 발송
            if(items.FirstOrDefault() != null)
            { 
                dbCom = db.GetStoredProcCommand("usp_material_Dispense_MailService");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@MDNo", DbType.String, items.FirstOrDefault().MDNo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<MaterialDispenseDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (MaterialDispenseDetail item in items)
            {
                str = string.Format("DELETE material_Dispense_Detail WHERE MDNo = '{0}' AND Seq = {1}", item.MDNo, item.Seq);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}

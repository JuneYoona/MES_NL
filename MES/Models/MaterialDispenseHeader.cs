using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;
using System.Linq;

namespace MesAdmin.Models
{
    public class MaterialDispenseHeader : ViewModelBase
    {
        public string MDNo
        {
            get { return GetProperty(() => MDNo); }
            set { SetProperty(() => MDNo, value); }
        }
        public DateTime? ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }   
        public DateTime? DlvyDate
        {
            get { return GetProperty(() => DlvyDate); }
            set { SetProperty(() => DlvyDate, value); }
        }
        public string InWhCode
        {
            get { return GetProperty(() => InWhCode); }
            set { SetProperty(() => InWhCode, value); }
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

        public MaterialDispenseHeader() { }
        public MaterialDispenseHeader(string mdNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM material_Dispense_Header (NOLOCK) WHERE MDNo = @MDNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@MDNo", DbType.String, mdNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                MDNo = (string)u["MDNo"];
                ReqDate = (DateTime)u["ReqDate"];
                DlvyDate = (DateTime)u["DlvyDate"];
                InWhCode = (string)u["InWhCode"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                dbCom = db.GetSqlStringCommand(string.Format("DELETE material_Dispense_Header WHERE MDNo = '{0}'", MDNo));
                db.ExecuteNonQuery(dbCom);
            }
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_material_Dispense_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@DlvyDate", DbType.Date, DlvyDate);
                    db.AddInParameter(dbCom, "@InWhCode", DbType.String, InWhCode);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@OutMDNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    MDNo = db.GetParameterValue(dbCom, "@OutMDNo").ToString();
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

    public class MaterialDispenseHeaderTable
    {
        private DateTime startDate;
        private DateTime endDate;
        public DataTable Collections { get; set; }

        public MaterialDispenseHeaderTable(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            string sql;

            sql = "SELECT * FROM material_Dispense_Header (NOLOCK) WHERE ReqDate BETWEEN @StartDate AND @EndDate";
            dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables.Count > 0)
                Collections = ds.Tables[0];
        }
    }
}

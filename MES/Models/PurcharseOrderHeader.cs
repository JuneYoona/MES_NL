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
    public class PurcharseOrderHeader : ViewModelBase
    {
        public string PoNo
        {
            get { return GetProperty(() => PoNo); }
            set { SetProperty(() => PoNo, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public DateTime? PoDate
        {
            get { return GetProperty(() => PoDate); }
            set { SetProperty(() => PoDate, value); }
        }
        public string RcptFlag
        {
            get { return GetProperty(() => RcptFlag); }
            set { SetProperty(() => RcptFlag, value); }

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

        public void Delete(string poNo)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                dbCom = db.GetSqlStringCommand(string.Format("DELETE purcharse_Order_Header WHERE PoNo = '{0}'", poNo));
                db.ExecuteNonQuery(dbCom);
            }
        }

        public string Save(PurcharseOrderHeader header)
        {
            string poNo = string.Empty;
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_purcharse_Order_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@PoNo", DbType.String, header.PoNo);
                    db.AddInParameter(dbCom, "@BizCode", DbType.String, header.BizCode);
                    db.AddInParameter(dbCom, "@PoDate", DbType.Date, header.PoDate);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, header.Memo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    poNo = (string)db.ExecuteScalar(dbCom, trans);
                    trans.Commit();
                    return poNo;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }

    public class PurcharseOrderHeaderList : ObservableCollection<PurcharseOrderHeader>
    {
        private DateTime startDate;
        private DateTime endDate;

        public PurcharseOrderHeaderList() { }
        public PurcharseOrderHeaderList(IEnumerable<PurcharseOrderHeader> items) : base(items) { }
        public PurcharseOrderHeaderList(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT * FROM purcharse_Order_Header (NOLOCK) ";
            sql += "WHERE PoDate BETWEEN '" + startDate.ToShortDateString() + "' AND '" + endDate.ToShortDateString() + "' ";
            sql += "ORDER BY PoDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new PurcharseOrderHeader
                    {
                        PoNo = (string)u["PoNo"],
                        BizCode = (string)u["BizCode"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        PoDate = (DateTime)u["PoDate"],
                        RcptFlag = (string)u["RcptFlag"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}

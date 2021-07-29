using System;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;

namespace MesAdmin.Models
{
    public class StockClose : ViewModelBase
    {
        public DateTime? ClosedDate
        {
            get { return GetProperty(() => ClosedDate); }
            set { SetProperty(() => ClosedDate, value); }
        }
        public DateTime ClosingDate
        {
            get { return GetProperty(() => ClosingDate); }
            set { SetProperty(() => ClosingDate, value); }
        }

        public StockClose()
        {
            Initialize();
        }

        public void Initialize()
        {
            ClosedDate = GetMaxDate();
            if (ClosedDate != null)
                ClosingDate = ClosedDate.Value.AddMonths(1);
            else
                ClosingDate = DateTime.Now;
        }

        public DateTime? GetMaxDate()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT TOP 1 CAST(MAX([Month])+'01' AS date) FROM stock_Monthly");
            DataSet ds = db.ExecuteDataSet(dbCom);

            DateTime? maxDate = ds.Tables[0].Rows[0][0] == DBNull.Value ? null : (DateTime?)ds.Tables[0].Rows[0][0];
            
            return maxDate;
        }

        public void Cancel()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    str = string.Format("DELETE stock_Monthly WHERE [Month] = '{0}' ", string.Format("{0:yyyyMM}", ClosedDate));
                    dbCom = db.GetSqlStringCommand(str);
                    db.ExecuteNonQuery(dbCom, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Close()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_CreateStock_Monthly");
                    db.AddInParameter(dbCom, "@Month", DbType.String, string.Format("{0:yyyyMM}", ClosingDate));
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.ExecuteNonQuery(dbCom, trans);
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
}

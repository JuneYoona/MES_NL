using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;

namespace MesAdmin.Models
{
    public class Commonsp
    {
        public static DataRow GetItemSpec(string itemCode, string type)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT UpRate, DownRate FROM quality_InspectItem WHERE ItemCode = @ItemCode AND InspectName = @Type";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@Type", DbType.String, type);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0].Rows[0];
        }

        public static DataTable BAC60INDICATORR005S(string syyyymm, string eyyyymm, string itemCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60INDICATORR005S");
            db.AddInParameter(dbCom, "@SYYYYMM", DbType.String, syyyymm);
            db.AddInParameter(dbCom, "@EYYYYMM", DbType.String, eyyyymm);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0];
        }

        public static DataTable BAC60INDICATORR006S(string syyyymm, string eyyyymm, string waCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60INDICATORR006S");
            db.AddInParameter(dbCom, "@SYYYYMM", DbType.String, syyyymm);
            db.AddInParameter(dbCom, "@EYYYYMM", DbType.String, eyyyymm);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0];
        }

        public static DataTable ProductionPauseDetails(DateTime startDate, DateTime endDate, string bizAreaCode, string waCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_production_PauseList");
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate);
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public static void C_CLOSE_STATUS(string yyyymm)
        {
            try
            {
                Database db = ProviderFactory.Instance;
                string sql = "IF(NOT EXISTS(SELECT * FROM ERPSERVER.DSNL.dbo.C_CLOSE_STATUS WHERE YYYYMM = @YYYYMM)) BEGIN RAISERROR('ERP 마감 전월입니다!', 16, 1) END";

                DbCommand dbCom = db.GetSqlStringCommand(sql);
                db.AddInParameter(dbCom, "@YYYYMM", DbType.String, yyyymm);
                db.ExecuteNonQuery(dbCom);
            }
            catch
            {
                throw;
            }
        }
    }
}
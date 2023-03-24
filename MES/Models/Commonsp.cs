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

        public static DataTable GetInpectItem(string itemCode, string gate)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_Quality_InspectItem");
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@Gate", DbType.String, gate);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
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

        public static DataTable SIBAC60SD001RS(DateTime startDate, DateTime endDate, string lotNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("SIBAC60SD001RS");
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate);
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public static string GetLotCountWE30(string pItemCode, string itemCode, string lotNo, string waCode)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_getLotCountWE30");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@PItemCode", DbType.String, pItemCode);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
                    db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
                    db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
                    db.AddOutParameter(dbCom, "@LotCnt", DbType.String, 50);
                    db.ExecuteNonQuery(dbCom, trans);
                    string lotCnt = db.GetParameterValue(dbCom, "@LotCnt").ToString();
                    trans.Commit();

                    return lotCnt;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static DataTable BAC60PRODUCTION001B(DateTime date, string itemCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION001B");
            db.AddInParameter(dbCom, "@Date", DbType.Date, date);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0];
        }

        public static void BAC60PRODUCTION003C(DateTime sourceDate, DateTime targetDate)
        {
            try
            {
                Database db = ProviderFactory.Instance;
                DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION003C");
                db.AddInParameter(dbCom, "@SourceDate", DbType.Date, sourceDate);
                db.AddInParameter(dbCom, "@TargetDate", DbType.Date, targetDate);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom);
            }
            catch
            {
                throw;
            }
        }

        public static DataTable BAC60PRODUCTION004RS(string lotNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM Z_BAC60BOTTLEWEIGHT WHERE LotNo = @LotNo");
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0];
        }

        public static DataTable BAC60SALES002RS(string dnNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60SALES002RS");
            db.AddInParameter(dbCom, "@DnNo", DbType.String, dnNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0];
        }
    }
}
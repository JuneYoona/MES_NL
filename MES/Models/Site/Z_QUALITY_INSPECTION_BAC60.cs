using DevExpress.Mvvm;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class Z_QUALITY_INSPECTION_BAC60_RESULT : StateBusinessObject
    {
        public string InspectName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string InspectSpec
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public double Peak
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public double R_Time
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public double Area
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public double Conc
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }
        public string DownRate
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string UpRate
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Result
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Memo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
    }

    public class Z_QUALITY_INSPECTION_BAC60 : ViewModelBase
    {
        public string OrderNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string QrType
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string SampleName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Memo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string FileName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string InspectorId
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Result
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT> InspectData
        {
            get { return GetValue<ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT>>(); }
            set { SetValue(value); }
        }
        public DateTime? InspectDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }

        public Z_QUALITY_INSPECTION_BAC60() { }
        public Z_QUALITY_INSPECTION_BAC60(string orderNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60QUALITY001RS");
            db.AddInParameter(dbCom, "@OrderNo", DbType.String, orderNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                OrderNo = (string)u["OrderNo"];
                QrType = (string)u["QrType"];
                SampleName = u["SampleName"].ToString();
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                Memo = u["Memo"].ToString();
                FileName = u["FileName"].ToString();
                InspectorId = u["InspectorId"].ToString();
                InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"];
                Result = u["Result"].ToString();
            });

            InspectData = new ObservableCollection<Z_QUALITY_INSPECTION_BAC60_RESULT>();
            ds.Tables[1].AsEnumerable().ToList().ForEach(u =>
            {
                InspectData.Add(new Z_QUALITY_INSPECTION_BAC60_RESULT
                {
                    InspectName = (string)u["InspectName"],
                    InspectSpec = (string)u["InspectSpec"],
                    Peak = (double)u["Peak"],
                    R_Time = (double)u["R_Time"],
                    Area = (double)u["Area"],
                    Conc = (double)u["Conc"],
                    DownRate = (string)u["DownRate"],
                    UpRate = (string)u["UpRate"],
                    Result = (string)u["Result"],
                    Memo = u["Memo"].ToString(),
                });
            });
        }

        public string Save(bool isNew)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    string orderNo = string.Empty;
                    if (isNew)
                    {
                        XDocument doc = new XDocument
                        (
                            new XElement("Inspect", from item in InspectData select new XElement("InspectData",
                                    new XAttribute("InspectName", item.InspectName ?? ""),
                                    new XAttribute("InspectSpec", item.InspectSpec ?? ""),
                                    new XAttribute("Peak", item.Peak),
                                    new XAttribute("R_Time", item.R_Time),
                                    new XAttribute("Area", item.Area),
                                    new XAttribute("Conc", item.Conc),
                                    new XAttribute("DownRate", item.DownRate ?? ""),
                                    new XAttribute("UpRate", item.UpRate ?? ""),
                                    new XAttribute("Result", item.Result ?? ""),
                                    new XAttribute("Memo", item.Memo ?? "")
                                )
                            )
                        );

                        dbCom = db.GetStoredProcCommand("BAC60QUALITY001CS");
                        db.AddInParameter(dbCom, "@QrType", DbType.String, QrType);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                        db.AddInParameter(dbCom, "@SampleName", DbType.String, SampleName);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                        db.AddInParameter(dbCom, "@FileName", DbType.String, FileName);
                        db.AddInParameter(dbCom, "@InspectData", DbType.Xml, doc.ToString());
                        db.AddInParameter(dbCom, "@InspectDate", DbType.Date, InspectDate);
                        db.AddInParameter(dbCom, "@InspectorId", DbType.String, InspectorId);
                        db.AddInParameter(dbCom, "@Result", DbType.String, Result);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddOutParameter(dbCom, "@RetOrderNo", DbType.String, 50);
                        db.ExecuteNonQuery(dbCom, trans);

                        orderNo = db.GetParameterValue(dbCom, "@RetOrderNo").ToString();
                    }
                    else
                    {
                        dbCom = db.GetSqlStringCommand("UPDATE Z_QUALITY_INSPECTION_BAC60 SET Memo = @Memo WHERE OrderNo = @OrderNo");
                        db.AddInParameter(dbCom, "@OrderNo", DbType.String, OrderNo);
                        db.AddInParameter(dbCom, "Memo", DbType.String, Memo);
                        db.ExecuteNonQuery(dbCom, trans);

                        orderNo = OrderNo;
                    }

                    trans.Commit();

                    return orderNo;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand("DELETE Z_QUALITY_INSPECTION_BAC60 WHERE OrderNo = @OrderNo");
                    db.AddInParameter(dbCom, "@OrderNo", DbType.String, OrderNo);
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

    public class Z_QUALITY_INSPECTION_BAC60_LIST : ObservableCollection<Z_QUALITY_INSPECTION_BAC60>
    {
        private DateTime startDate;
        private DateTime endDate;
        private string lotNo;

        public Z_QUALITY_INSPECTION_BAC60_LIST() { }
        public Z_QUALITY_INSPECTION_BAC60_LIST(DateTime startDate, DateTime endDate, string lotNo = "")
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.lotNo = lotNo;
            InitializeList();
        }

        public void InitializeList()
        {
            Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC20PRODUCTION010RS");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                Add(
                    new Z_QUALITY_INSPECTION_BAC60
                    {
                        OrderNo = (string)u["OrderNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        //LotNo = (string)u["LotNo"],
                    }
                )
            );
        }
    }
}
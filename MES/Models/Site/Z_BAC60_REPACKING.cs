using DevExpress.Mvvm;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MesAdmin.Models
{
    public class Z_BAC60_REPACKING_DETAIL : ViewModelBase
    {
        public EntityState State
        {
            get { return GetProperty(() => State); }
            set { SetProperty(() => State, value); }
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
        public string ItemSpec
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string LotNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public int? Qty
        {
            get { return GetValue<int?>(); }
            set { SetValue(value); }
        }
        public string BasicUnit
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark1
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string LabelNo
        {
            get { return GetValue<string> (); }
            set { SetValue(value); }
        }
    }

    public class Z_BAC60_REPACKING : ViewModelBase
    {
        public string OrderNo
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
        public string ItemSpec
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string BasicUnit
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string LotNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime ReqDate
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }
        public int Qty
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public string Remark1
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string OutInspName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime? OutInspDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public string InspectorId
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string InspectorName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime? InspectDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public string InsertName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime InsertDate
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }
        public DateTime? UpdateDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public ObservableCollection<Z_BAC60_REPACKING_DETAIL> Detail
        {
            get { return GetValue<ObservableCollection<Z_BAC60_REPACKING_DETAIL>>(); }
            set { SetValue(value); }
        }
        
        public Z_BAC60_REPACKING() { }

        public Z_BAC60_REPACKING(string orderNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION005US");
            db.AddInParameter(dbCom, "@OrderNo", DbType.String, orderNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u => {
                OrderNo = (string)u["OrderNo"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                ItemSpec = u["ItemSpec"].ToString();
                LotNo = (string)u["LotNo"];
                ReqDate = (DateTime)u["ReqDate"];
                Qty =(int)u["Qty"];
                Remark1 = u["Remark1"].ToString();
                InspectorId = u["InspectorId"].ToString();
                InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"];
            });

            Detail = new ObservableCollection<Z_BAC60_REPACKING_DETAIL>();
            ds.Tables[1].AsEnumerable().ToList().ForEach(u => {
                Detail.Add(new Z_BAC60_REPACKING_DETAIL
                {
                    State = EntityState.Unchanged,
                    ItemCode = (string)u["ItemCode"],
                    ItemName = (string)u["ItemName"],
                    ItemSpec = (string)u["ItemSpec"],
                    LotNo = (string)u["LotNo"],
                    Qty = (int)u["Qty"],
                    BasicUnit = (string)u["BasicUnit"],
                    Remark1 = (string)u["Remark1"],
                    LabelNo = u["LabelNo"].ToString()
                });
            });
        }

        public string Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    int i = 0;
                    int lossQty = Qty - Detail.Where(o => o.State != EntityState.Deleted).Sum(o => o.Qty.Value);
                    
                    XDocument doc = new XDocument
                    (
                        new XElement("Details", from item in Detail.Where(o => o.State != EntityState.Deleted)
                            select new XElement
                            ("Detail",
                               new XAttribute("ItemCode", item.ItemCode ?? ""),
                               new XAttribute("ItemName", item.ItemName ?? ""),
                               new XAttribute("ItemSpec", item.ItemSpec ?? ""),
                               new XAttribute("LotNo", item.LotNo ?? ""),
                               new XAttribute("Qty", item.Qty),
                               new XAttribute("BasicUnit", item.BasicUnit ?? ""),
                               new XAttribute("Remark1", item.Remark1 ?? ""),
                               new XAttribute("LabelStartNo", "@LabelStartNo"),
                               new XAttribute("LabelNo", ++i)
                            )
                        )
                    );

                    dbCom = db.GetStoredProcCommand("BAC60PRODUCTION005CS");
                    db.AddInParameter(dbCom, "@OrderNo", DbType.String, OrderNo);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                    db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                    db.AddInParameter(dbCom, "@Qty", DbType.Int32, Qty);
                    db.AddInParameter(dbCom, "@LossQty", DbType.Int32, lossQty);
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@Remark1", DbType.String, Remark1);
                    db.AddInParameter(dbCom, "@InspectorId", DbType.String, InspectorId);
                    db.AddInParameter(dbCom, "@InspectDate", DbType.Date, InspectDate);
                    db.AddInParameter(dbCom, "@Detail", DbType.Xml, doc.Root.Elements("Detail").Count() != 0 ? doc.ToString() : null);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@Ret", DbType.String, 50);
                    db.ExecuteNonQuery(dbCom, trans);
                    trans.Commit();

                    return db.GetParameterValue(dbCom, "@Ret").ToString();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Save_PrintHistory()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("BAC60PRODUCTION005HS");
                    db.AddInParameter(dbCom, "@OrderNo", DbType.String, OrderNo);
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

    public class Z_BAC60_REPACKING_LIST : ObservableCollection<Z_BAC60_REPACKING>
    {
        private DateTime startDate;
        private DateTime endDate;
        private string lotNo;

        public Z_BAC60_REPACKING_LIST() { }
        public Z_BAC60_REPACKING_LIST(DateTime startDate, DateTime endDate, string lotNo = "")
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
            DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION005RS");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                Add(
                    new Z_BAC60_REPACKING
                    {
                        OrderNo = (string)u["OrderNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = u["ItemSpec"].ToString(),
                        LotNo = (string)u["LotNo"],
                        BasicUnit = u["BasicUnit"].ToString(),
                        ReqDate = (DateTime)u["ReqDate"],
                        Qty = (int)u["Qty"],
                        Remark1 = u["Remark1"].ToString(),
                        OutInspName = u["OutInspName"].ToString(),
                        OutInspDate = u["OutInspDate"] == DBNull.Value ? null : (DateTime?)u["OutInspDate"],
                        InspectorName = u["InspectorName"].ToString(),
                        InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"],
                        InsertName = u["InsertName"].ToString(),
                        InsertDate = (DateTime)u["InsertDate"],
                        UpdateDate = u["UpdateDate"] == DBNull.Value ? null : (DateTime?)u["UpdateDate"],
                    }
                )
            );
        }
    }
}
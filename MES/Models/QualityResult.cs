using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Xml.Linq;

namespace MesAdmin.Models
{
    public class QualityResult : StateBusinessObject
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public int Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
        }
        public string QrType
        {
            get { return GetProperty(() => QrType); }
            set { SetProperty(() => QrType, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string InspectName
        {
            get { return GetProperty(() => InspectName); }
            set { SetProperty(() => InspectName, value); }
        }
        public string InspectSpec
        {
            get { return GetProperty(() => InspectSpec); }
            set { SetProperty(() => InspectSpec, value); }
        }
        public string DownRate
        {
            get { return GetProperty(() => DownRate); }
            set { SetProperty(() => DownRate, value); }
        }
        public string UpRate
        {
            get { return GetProperty(() => UpRate); }
            set { SetProperty(() => UpRate, value); }
        }
        public string InspectValue
        {
            get { return GetProperty(() => InspectValue); }
            set { SetProperty(() => InspectValue, value); }
        }
        public string InspectValue_BP
        {
            get { return GetProperty(() => InspectValue_BP); }
            set { SetProperty(() => InspectValue_BP, value); }
        }
        public DateTime InspectDate
        {
            get { return GetProperty(() => InspectDate); }
            set { SetProperty(() => InspectDate, value); }
        }
        public string InspectorId
        {
            get { return GetProperty(() => InspectorId); }
            set { SetProperty(() => InspectorId, value); }
        }
        public string InspectorName
        {
            get { return GetProperty(() => InspectorName); }
            set { SetProperty(() => InspectorName, value); }
        }
        public string Result
        {
            get { return GetProperty(() => Result); }
            set { SetProperty(() => Result, value); }
        }
        public string Remark
        {
            get { return GetProperty(() => Remark); }
            set { SetProperty(() => Remark, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public string Memo1
        {
            get { return GetProperty(() => Memo1); }
            set { SetProperty(() => Memo1, value); }
        }
        public string Memo2
        {
            get { return GetProperty(() => Memo2); }
            set { SetProperty(() => Memo2, value); }
        }
        public string Editor
        {
            get { return GetProperty(() => Editor); }
            set { SetProperty(() => Editor, value); }
        }
        public string Unit
        {
            get { return GetProperty(() => Unit); }
            set { SetProperty(() => Unit, value); }
        }
        public string Equipment
        {
            get { return GetProperty(() => Equipment); }
            set { SetProperty(() => Equipment, value); }
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

    public class QualityResultList : ObservableCollection<QualityResult>
    {
        private string qrNo;
        private int order;

        public QualityResultList() { }
        public QualityResultList(IEnumerable<QualityResult> items) : base(items) { }
        public QualityResultList(string qrNo, int order)
        {
            this.qrNo = qrNo;
            this.order = order;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_QualityResultXml");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            db.AddInParameter(dbCom, "@Order", DbType.Int16, order);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityResult
                    {
                        State = EntityState.Unchanged,
                        QrNo = (string)u["QrNo"],
                        Order = (int)u["Order"],
                        QrType = (string)u["QrType"],
                        ItemCode = (string)u["ItemCode"],
                        InspectName = (string)u["InspectName"],
                        InspectSpec = (string)u["InspectSpec"],
                        DownRate = (string)u["DownRate"],
                        UpRate = (string)u["UpRate"],
                        InspectValue = (string)u["InspectValue"],
                        Unit = u["Unit"].ToString(),
                        Equipment = u["Equipment"].ToString(),
                        Memo = u["Memo"].ToString(),
                        Memo1 = u["Memo1"].ToString(),
                        Memo2 = u["Memo2"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<QualityResult> items = this.Items;
            Insert(items.Where(u => u.State == EntityState.Added));
            Update(items.Where(u => u.State == EntityState.Modified));
            Delete(items.Where(u => u.State == EntityState.Deleted));
        }

        public void Insert(IEnumerable<QualityResult> items)
        {
            if (items == null || items.Count() == 0) return;

            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    XDocument doc = new XDocument(new XElement("Root",
                                new XElement("Inspect", from item in items
                                select new XElement("InspectData"
                                , new XAttribute("InspectName", item.InspectName)
                                , new XAttribute("InspectValue", item.InspectValue ?? "")
                                , new XAttribute("InspectSpec", item.InspectSpec)
                                , new XAttribute("DownRate", item.DownRate)
                                , new XAttribute("UpRate", item.UpRate)
                                , new XAttribute("Unit", item.Unit)
                                , new XAttribute("Equipment", item.Equipment)
                                , new XAttribute("Memo1", item.Memo1 ?? "")
                                , new XAttribute("Memo2", item.Memo2 ?? "")
                                )
                            )
                        )
                    );

                    string sql = "INSERT INTO quality_Result(QrNo, [Order], QrType, ItemCode, InspectData, InspectDate, InspectorId, InspectorName, Result, Memo, InsertId, UpdateId, UpdateDate) "
                        + "VALUES(@QrNo, @Order, @QrType, @ItemCode, @InspectData, @InspectDate, @InspectorId, @InspectorName, @Result, @Memo, @InsertId, @InsertId, getdate())";
                    dbCom = db.GetSqlStringCommand(sql);

                    var result = Items.FirstOrDefault();
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, result.QrNo);
                    db.AddInParameter(dbCom, "@Order", DbType.Int16, result.Order);
                    db.AddInParameter(dbCom, "@QrType", DbType.String, result.QrType);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, result.ItemCode);
                    db.AddInParameter(dbCom, "@InspectData", DbType.Xml, doc.ToString());
                    db.AddInParameter(dbCom, "@InspectDate", DbType.Date, result.InspectDate);
                    db.AddInParameter(dbCom, "@InspectorId", DbType.String, result.InspectorId);
                    db.AddInParameter(dbCom, "@InspectorName", DbType.String, result.InspectorName);
                    db.AddInParameter(dbCom, "@Result", DbType.String, result.Result);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, result.Remark);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.ExecuteNonQuery(dbCom, trans);

                    sql = "UPDATE quality_Request SET Status = 1, UpdateId = '{1}', UpdateDate = getdate() WHERE QrNo = '{0}' ";
                    sql += "UPDATE purcharse_Warehousing SET QrResultDate = '{2}' WHERE QrNo = '{0}'";
                    sql = string.Format(sql, result.QrNo, DSUser.Instance.UserID, result.InspectDate.ToShortDateString());
                    dbCom = db.GetSqlStringCommand(sql);
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

        public void Update(IEnumerable<QualityResult> items)
        {
            if (items == null || items.Count() == 0) return;

            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    XDocument doc = new XDocument(new XElement("Root",
                                new XElement("Inspect", from item in Items
                                select new XElement("InspectData"
                                , new XAttribute("InspectName", item.InspectName)
                                , new XAttribute("InspectValue", item.InspectValue ?? "")
                                , new XAttribute("InspectSpec", item.InspectSpec)
                                , new XAttribute("DownRate", item.DownRate)
                                , new XAttribute("UpRate", item.UpRate)
                                , new XAttribute("Unit", item.Unit)
                                , new XAttribute("Equipment", item.Equipment)
                                , new XAttribute("Memo", item.Memo ?? "")
                                , new XAttribute("Memo1", item.Memo1 ?? "")
                                , new XAttribute("Memo2", item.Memo2 ?? "")
                                )
                            )
                        )
                    );
                    string sql = "UPDATE quality_Result SET InspectDate = @InspectDate, InspectData = @InspectData, InspectorId = @InspectorId, InspectorName = @InspectorName, Result = @Result, Memo = @Memo"
                            + ",  UpdateId = @UpdateId, UpdateDate = getdate() WHERE QrNo = @QrNo AND [Order] = @Order ";
                    dbCom = db.GetSqlStringCommand(sql);

                    var result = Items.FirstOrDefault();
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, result.QrNo);
                    db.AddInParameter(dbCom, "@Order", DbType.Int16, result.Order);
                    db.AddInParameter(dbCom, "@InspectData", DbType.Xml, doc.ToString());
                    db.AddInParameter(dbCom, "@InspectDate", DbType.Date, result.InspectDate);
                    db.AddInParameter(dbCom, "@InspectorId", DbType.String, result.InspectorId);
                    db.AddInParameter(dbCom, "@InspectorName", DbType.String, result.InspectorName);
                    db.AddInParameter(dbCom, "@Result", DbType.String, result.Result);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, result.Remark);
                    db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                    db.ExecuteNonQuery(dbCom, trans);

                    sql = "UPDATE quality_Request SET UpdateId = '{1}', UpdateDate = getdate() WHERE QrNo = '{0}' ";
                    sql += "UPDATE purcharse_Warehousing SET QrResultDate = '{2}' WHERE QrNo = '{0}'";
                    sql = string.Format(sql, result.QrNo, DSUser.Instance.UserID, result.InspectDate.ToShortDateString());
                    dbCom = db.GetSqlStringCommand(sql);
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

        public void Delete(IEnumerable<QualityResult> items)
        {
            if (items == null || items.Count() == 0) return;

            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    if (items != null && items.Count() > 0)
                    {
                        sql = string.Format("DELETE quality_Result WHERE QrNo = '{0}' AND [Order] = {1}", items.FirstOrDefault().QrNo, items.FirstOrDefault().Order);
                        dbCom = db.GetSqlStringCommand(sql);
                        db.ExecuteNonQuery(dbCom, trans);
                        trans.Commit();
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }

    public class QualityResultTable
    {
        private string itemCode;
        private string qrType;
        private string uspName;
        private DateTime startDate;
        private DateTime endDate;
        public DataTable Collections { get; set; }

        public QualityResultTable(string itemCode, DateTime startDate, DateTime endDate, string qrType, string uspName = "usp_QualityResult")
        {
            this.itemCode = itemCode;
            this.qrType = qrType;
            this.uspName = uspName;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand(uspName);
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@QrType", DbType.String, qrType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables.Count > 0)
                Collections = ds.Tables[0];
        }
    }
}

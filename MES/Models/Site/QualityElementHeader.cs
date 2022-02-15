using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Data.SqlClient;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class QualityElementHeader : StateBusinessObject
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
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
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public DateTime? InspectDate
        {
            get { return GetProperty(() => InspectDate); }
            set { SetProperty(() => InspectDate, value); }
        }
        public DateTime? FinishDate
        {
            get { return GetProperty(() => FinishDate); }
            set { SetProperty(() => FinishDate, value); }
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
        public decimal? IVLRefMax
        {
            get { return GetProperty(() => IVLRefMax); }
            set { SetProperty(() => IVLRefMax, value); }
        }
        public decimal? IVLMax
        {
            get { return GetProperty(() => IVLMax); }
            set { SetProperty(() => IVLMax, value); }
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
        public DateTime? UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
        public bool IVL
        {
            get { return GetProperty(() => IVL); }
            set { SetProperty(() => IVL, value); }
        }
        public bool LT
        {
            get { return GetProperty(() => LT); }
            set { SetProperty(() => LT, value); }
        }

        public QualityElementHeader() { }
        public QualityElementHeader(string qrNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_qualityElement_Header");
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u => {
                QrNo = (string)u["QrNo"];
                ProductOrderNo = (string)u["ProductOrderNo"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                LotNo = (string)u["LotNo"];
                Qty = (decimal)u["Qty"];
                InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"];
                InspectorId = u["InspectorId"] == DBNull.Value ? null : (string)u["InspectorId"];
                IVLRefMax = u["IVLRefMax"] == DBNull.Value ? null : (decimal?)u["IVLRefMax"];
                IVLMax = u["IVLMax"] == DBNull.Value ? null : (decimal?)u["IVLMax"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
                IVL = (bool)u["IVL"];
                LT = (bool)u["LT"];
            });
        }
        
        protected string GetInspectorName(string inspectorId)
        {
            if (string.IsNullOrEmpty(inspectorId)) return "";
            NetUsers users = NetUsers.Select();
            var user = users.Where(u => u.UserName == inspectorId);
            return user == null ? null : user.FirstOrDefault().Profile.KorName;
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
                    dbCom = db.GetStoredProcCommand("usps_qualityElemenHeader_Update");
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, QrNo);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                    db.AddInParameter(dbCom, "@ItemName", DbType.String, ItemName);
                    db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                    db.AddInParameter(dbCom, "@Qty", DbType.Decimal, Qty);
                    db.AddInParameter(dbCom, "@InspectDate", DbType.Date, InspectDate);
                    db.AddInParameter(dbCom, "@InspectorId", DbType.String, InspectorId);
                    db.AddInParameter(dbCom, "@InspectorName", DbType.String, GetInspectorName(InspectorId));
                    db.AddInParameter(dbCom, "@IVLRefMax", DbType.Decimal, IVLRefMax);
                    db.AddInParameter(dbCom, "@IVLMax", DbType.Decimal, IVLMax);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@QrNoOut", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    QrNo = db.GetParameterValue(dbCom, "@QrNoOut").ToString();
                    trans.Commit();
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public DataTable GetRequestList(DateTime startDate, DateTime endDate)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_qualityElementRequest_List");
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public DataTable GetDeviceStructure()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT Ref01 FROM common_Minor_Detail WHERE MajorCode = 'QR001D' AND MinorCode = @ItemCode");
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }

    public class QualityElementHeaderList : ObservableCollection<QualityElementHeader>
    {
        private DateTime startDate;
        private DateTime endDate;

        public QualityElementHeaderList() { }
        public QualityElementHeaderList(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();

            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_qualityElementRequest_List");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityElementHeader
                    {
                        State = EntityState.Unchanged,
                        QrNo = (string)u["QrNo"],
                        ProductOrderNo = (string)u["ProductOrderNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        BasicUnit = (string)u["BasicUnit"],
                        InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"],
                        FinishDate = u["FinishDate"] == DBNull.Value ? null : (DateTime?)u["FinishDate"],
                        InspectorId = u["InspectorId"] == DBNull.Value ? null : (string)u["InspectorId"],
                        InspectorName = u["InspectorName"] == DBNull.Value ? null : (string)u["InspectorName"],
                        IVLRefMax = u["IVLRefMax"] == DBNull.Value ? null : (decimal?)u["IVLRefMax"],
                        IVLMax = u["IVLMax"] == DBNull.Value ? null : (decimal?)u["IVLMax"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                        IVL = (bool)u["IVL"],
                        LT = (bool)u["LT"],
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<QualityElementHeader> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    //Insert(items.Where(u => u.State == EntityState.Added), db, trans, dbCom);
                    Delete(items.Where(u => u.State == EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<QualityElementHeader> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (QualityElementHeader item in items)
            {
                //dbCom = db.GetStoredProcCommand("usp_sales_Order_Detail");
                //dbCom.CommandType = CommandType.StoredProcedure;
                //db.AddInParameter(dbCom, "@SoNo", DbType.String, item.SoNo);
                //db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                //db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                //db.AddInParameter(dbCom, "@ExchangeRate", DbType.Decimal, item.ExchangeRate);
                //db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                //db.AddInParameter(dbCom, "@ShipTo", DbType.String, item.ShipTo);
                //db.AddInParameter(dbCom, "@ReqDlvyDate", DbType.Date, item.ReqDlvyDate);
                //db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                //db.AddInParameter(dbCom, "@UnitPrice", DbType.Decimal, item.UnitPrice);
                //db.AddInParameter(dbCom, "@VATRate", DbType.Decimal, item.VATRate);
                //db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                //db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                //db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<QualityElementHeader> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (QualityElementHeader item in items)
            {
                str = string.Format("DELETE quality_Element_Header WHERE QrNo = '{0}'", item.QrNo);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}

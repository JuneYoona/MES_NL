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
    public class QualityInspectItem : StateBusinessObject
    {
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public int? Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
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
        public string Editor
        {
            get { return GetProperty(() => Editor); }
            set { SetProperty(() => Editor, value); }
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

    public class QualityInspectItemList : ObservableCollection<QualityInspectItem>
    {
        private string qrType;
        private string itemCode;

        public QualityInspectItemList() { }
        public QualityInspectItemList(IEnumerable<QualityInspectItem> items) : base(items) { }
        public QualityInspectItemList(string qrType, string itemCode)
        {
            this.qrType = qrType;
            this.itemCode = itemCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT * FROM quality_InspectItem WHERE QrType='" + qrType + "' AND ItemCode='" + itemCode + "' ORDER BY [Order]";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityInspectItem
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        QrType = (string)u["QrType"],
                        ItemCode = (string)u["ItemCode"],
                        InspectName = (string)u["InspectName"],
                        InspectSpec = u["InspectSpec"].ToString(),
                        DownRate = u["DownRate"].ToString(),
                        UpRate = u["UpRate"].ToString(),
                        Unit = u["Unit"].ToString(),
                        Equipment = u["Equipment"].ToString(),
                        Order = (int)u["Order"],
                        Memo1 = u["Memo1"].ToString(),
                        Memo2 = u["Memo2"].ToString(),
                        Editor = (string)u["Editor"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<QualityInspectItem> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));
        }

        public void Insert(IEnumerable<QualityInspectItem> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (QualityInspectItem item in items)
                    {
                        sql = "INSERT INTO quality_InspectItem(QrType, ItemCode, [Order], InspectName, InspectSpec, DownRate, UpRate, Unit, Equipment, Editor, Memo1, Memo2, InsertId, InsertDate, UpdateId, UpdateDate) VALUES "
                            + "(@QrType, @ItemCode, @Order, @InspectName, @InspectSpec, @DownRate, @UpRate, @Unit, @Equipment, @Editor, @Memo1, @Memo2, @InsertId, getdate(), @InsertId, getdate())";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@QrType", DbType.String, item.QrType);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                        db.AddInParameter(dbCom, "@InspectName", DbType.String, item.InspectName);
                        db.AddInParameter(dbCom, "@InspectSpec", DbType.String, item.InspectSpec);
                        db.AddInParameter(dbCom, "@DownRate", DbType.String, item.DownRate);
                        db.AddInParameter(dbCom, "@UpRate", DbType.String, item.UpRate);
                        db.AddInParameter(dbCom, "@Unit", DbType.String, item.Unit);
                        db.AddInParameter(dbCom, "@Equipment", DbType.String, item.Equipment);
                        db.AddInParameter(dbCom, "@Editor", DbType.String, item.Editor);
                        db.AddInParameter(dbCom, "@Memo1", DbType.String, item.Memo1);
                        db.AddInParameter(dbCom, "@Memo2", DbType.String, item.Memo2);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Update(IEnumerable<QualityInspectItem> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (QualityInspectItem item in items)
                    {
                        sql = "UPDATE quality_InspectItem SET [Order] = @Order "
                            + ", InspectSpec = @InspectSpec "
                            + ", DownRate = @DownRate "
                            + ", UpRate = @UpRate "
                            + ", Unit = @Unit "
                            + ", Equipment = @Equipment "
                            + ", Editor = @Editor "
                            + ", Memo1 = @Memo1 "
                            + ", Memo2 = @Memo2 "
                            + ", UpdateId = @UpdateId "
                            + ", UpdateDate = getdate() "
                            + "WHERE QrType = @QrType AND ItemCode = @ItemCode AND InspectName = @InspectName";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@QrType", DbType.String, item.QrType);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@InspectName", DbType.String, item.InspectName);
                        db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                        db.AddInParameter(dbCom, "@InspectSpec", DbType.String, item.InspectSpec);
                        db.AddInParameter(dbCom, "@DownRate", DbType.String, item.DownRate);
                        db.AddInParameter(dbCom, "@UpRate", DbType.String, item.UpRate);
                        db.AddInParameter(dbCom, "@Unit", DbType.String, item.Unit);
                        db.AddInParameter(dbCom, "@Equipment", DbType.String, item.Equipment);
                        db.AddInParameter(dbCom, "@Editor", DbType.String, item.Editor);
                        db.AddInParameter(dbCom, "@Memo1", DbType.String, item.Memo1);
                        db.AddInParameter(dbCom, "@Memo2", DbType.String, item.Memo2);
                        db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete(IEnumerable<QualityInspectItem> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (QualityInspectItem item in items)
                    {
                        sql = "DELETE quality_InspectItem WHERE QrType = @QrType AND ItemCode = @ItemCode AND InspectName = @InspectName";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@QrType", DbType.String, item.QrType);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@InspectName", DbType.String, item.InspectName);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class CommonBillOfMaterial : StateBusinessObject
    {
        public string PItemCode
        {
            get { return GetProperty(() => PItemCode); }
            set { SetProperty(() => PItemCode, value); }
        }
        public string CItemCode
        {
            get { return GetProperty(() => CItemCode); }
            set { SetProperty(() => CItemCode, value); }
        }
        public int CItemSeq
        {
            get { return GetProperty(() => CItemSeq); }
            set { SetProperty(() => CItemSeq, value); }
        }
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
        }
        public string PUnit
        {
            get { return GetProperty(() => PUnit); }
            set { SetProperty(() => PUnit, value); }
        }
        public Decimal PPerQty
        {
            get { return GetProperty(() => PPerQty); }
            set { SetProperty(() => PPerQty, value); }
        }
        public string CUnit
        {
            get { return GetProperty(() => CUnit); }
            set { SetProperty(() => CUnit, value); }
        }
        public Decimal CPerQty
        {
            get { return GetProperty(() => CPerQty); }
            set { SetProperty(() => CPerQty, value); }
        }
        public DateTime? StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public DateTime? EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
        }
        public int RecursionLevel
        {
            get { return GetProperty(() => RecursionLevel); }
            set { SetProperty(() => RecursionLevel, value); }
        }
        public string KeyFieldName
        {
            get { return GetProperty(() => KeyFieldName); }
            set { SetProperty(() => KeyFieldName, value); }
        }
        public string ParentFieldName
        {
            get { return GetProperty(() => ParentFieldName); }
            set { SetProperty(() => ParentFieldName, value); }
        }
    }

    public class CommonBillOfMaterialList : ObservableCollection<CommonBillOfMaterial>
    {
        private string itemCode;
        private DateTime checkDate;

        public CommonBillOfMaterialList() { }
        public CommonBillOfMaterialList(string itemCode, DateTime checkDate)
        {
            this.itemCode = itemCode;
            this.checkDate = checkDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();

            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_GetBillOfMaterial");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@StartItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@CheckDate", DbType.Date, checkDate);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonBillOfMaterial
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        PItemCode = (string)u["PItemCode"],
                        CItemSeq = int.Parse(u["CItemSeq"].ToString()),
                        CItemCode = (string)u["CItemCode"],
                        ItemName = (string)u["ItemName"],
                        PPerQty = (Decimal)u["PPerQty"],
                        PUnit = (string)u["PUnit"],
                        CPerQty = (Decimal)u["CPerQty"],
                        CUnit = (string)u["CUnit"],
                        StartDate = string.IsNullOrEmpty(u["StartDate"].ToString()) ? null : (DateTime?)u["StartDate"],
                        EndDate = string.IsNullOrEmpty(u["EndDate"].ToString()) ? null : (DateTime?)u["EndDate"],
                        RecursionLevel = (int)u["RecursionLevel"],
                        KeyFieldName = (string)u["CItemCode"] + (string)u["PItemCode"] + u["RecursionLevel"].ToString(),
                        ParentFieldName = (string)u["PItemCode"] + (string)u["PPItemCode"] + ((int)u["RecursionLevel"] - 1).ToString(),
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonBillOfMaterial> items = this.Items;
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added), db, trans);
                    Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified), db, trans);
                    Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted), db, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<CommonBillOfMaterial> items, Database db, DbTransaction trans)
        {
            DbCommand dbCom = null;

            foreach (CommonBillOfMaterial item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_common_BillOfMaterial");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@PItemCode", DbType.String, item.PItemCode);
                db.AddInParameter(dbCom, "@CItemCode", DbType.String, item.CItemCode);
                db.AddInParameter(dbCom, "@StartDate", DbType.Date, item.StartDate);
                db.AddInParameter(dbCom, "@EndDate", DbType.Date, item.EndDate);
                db.AddInParameter(dbCom, "@PPerQty", DbType.Decimal, item.PPerQty);
                db.AddInParameter(dbCom, "@PUnit", DbType.String, item.PUnit);
                db.AddInParameter(dbCom, "@CPerQty", DbType.Decimal, item.CPerQty);
                db.AddInParameter(dbCom, "@CUnit", DbType.String, item.CUnit);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<CommonBillOfMaterial> items, Database db, DbTransaction trans)
        {
            string str;
            DbCommand dbCom = null;

            foreach (CommonBillOfMaterial item in items)
            {
                str = "UPDATE common_BillOfMaterial SET StartDate = @StartDate, EndDate = @EndDate, PPerQty = @PPerQty, CPerQty = @CPerQty, UpdateId = @UpdateId, UpdateDate = getdate() ";
                str += "WHERE PItemCode = @PItemCode AND CItemCode = @CItemCode AND CItemSeq = @CItemSeq";
                dbCom = db.GetSqlStringCommand(str);
                db.AddInParameter(dbCom, "@PItemCode", DbType.String, item.PItemCode);
                db.AddInParameter(dbCom, "@CItemCode", DbType.String, item.CItemCode);
                db.AddInParameter(dbCom, "@CItemSeq", DbType.Int16, item.CItemSeq);
                db.AddInParameter(dbCom, "@StartDate", DbType.Date, item.StartDate);
                db.AddInParameter(dbCom, "@EndDate", DbType.Date, item.EndDate);
                db.AddInParameter(dbCom, "@PPerQty", DbType.Decimal, item.PPerQty);
                db.AddInParameter(dbCom, "@CPerQty", DbType.Decimal, item.CPerQty);
                db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<CommonBillOfMaterial> items, Database db, DbTransaction trans)
        {
            string str;
            DbCommand dbCom = null;

            foreach (CommonBillOfMaterial item in items)
            {
                str = string.Format("DELETE common_BillOfMaterial WHERE PItemCode = @PItemCode AND CItemCode = @CItemCode AND CItemSeq = @CItemSeq");
                dbCom = db.GetSqlStringCommand(str);
                db.AddInParameter(dbCom, "@PItemCode", DbType.String, item.PItemCode);
                db.AddInParameter(dbCom, "@CItemCode", DbType.String, item.CItemCode);
                db.AddInParameter(dbCom, "@CItemSeq", DbType.Int16, item.CItemSeq);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void SyncErp()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbCommand dbCom = db.GetSqlStringCommand("EXEC USP_IF_ERP2MES_BOM_KO656");
                db.ExecuteNonQuery(dbCom);
            }
        }
    }
}

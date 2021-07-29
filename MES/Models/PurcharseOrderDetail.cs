using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace MesAdmin.Models
{
    public class PurcharseOrderDetail : StateBusinessObject
    {
        public bool IsChecked
        {
            get { return GetProperty(() => IsChecked); }
            set { SetProperty(() => IsChecked, value); }
        }
        public string PoNo
        {
            get { return GetProperty(() => PoNo); }
            set { SetProperty(() => PoNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public DateTime? DlvyDate
        {
            get { return GetProperty(() => DlvyDate); }
            set { SetProperty(() => DlvyDate, value); }
        }
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal PoQty
        {
            get { return GetProperty(() => PoQty); }
            set { SetProperty(() => PoQty, value); }
        }
        public string PoBasicUnit
        {
            get { return GetProperty(() => PoBasicUnit); }
            set { SetProperty(() => PoBasicUnit, value); }
        }
        public decimal GrQty
        {
            get { return GetProperty(() => GrQty); }
            set { SetProperty(() => GrQty, value); }
        }
        public decimal RemainQty
        {
            get { return GetProperty(() => RemainQty); }
            set { SetProperty(() => RemainQty, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public DateTime? PoDate
        {
            get { return GetProperty(() => PoDate); }
            set { SetProperty(() => PoDate, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string CloseFlag
        {
            get { return GetProperty(() => CloseFlag); }
            set { SetProperty(() => CloseFlag, value); }
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
        public bool IQCFlag
        {
            get { return GetProperty(() => IQCFlag); }
            set { SetProperty(() => IQCFlag, value); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
    }

    public class PurcharseOrderDetailList : ObservableCollection<PurcharseOrderDetail>
    {
        private string poNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public PurcharseOrderDetailList() { }
        public PurcharseOrderDetailList(IEnumerable<PurcharseOrderDetail> items) : base(items) { }
        public PurcharseOrderDetailList(string poNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.poNo = poNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();

            string sql;
            sql = "SELECT * FROM view_purcharse_Order_Detail WHERE PoNo !='' ";
            if(!string.IsNullOrEmpty(poNo))
                sql += "AND PoNo = '" + poNo + "' ";
            if (startDate != null && endDate != null)
                sql += "AND PoDate BETWEEN '" + startDate.ToString().Substring(0, 10) + "' AND '" + endDate.ToString().Substring(0, 10) + "' ";
            sql += "ORDER BY PoDate DESC";

            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new PurcharseOrderDetail
                    {
                        PoNo = (string)u["PoNo"],
                        Seq = (int)u["Seq"],
                        DlvyDate = (DateTime)u["DlvyDate"],
                        PoQty = (decimal)u["PoQty"],
                        PoBasicUnit = (string)u["PoBasicUnit"],
                        GrQty = (decimal)u["GrQty"],
                        RemainQty = (decimal)u["RemainQty"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        WhCode = (string)u["WhCode"],
                        CloseFlag = (string)u["CloseFlag"],
                        Memo = u["Memo"].ToString(),
                        PoDate = (DateTime)u["PoDate"],
                        BizCode = (string)u["BizCode"],
                        IQCFlag = (bool)u["IQCFlag"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<PurcharseOrderDetail> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));
        }

        public void Insert(IEnumerable<PurcharseOrderDetail> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (PurcharseOrderDetail detail in items)
                    {
                        dbCom = db.GetStoredProcCommand("usp_purcharse_Order_Detail");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@PoNo", DbType.String, detail.PoNo);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, detail.ItemCode);
                        db.AddInParameter(dbCom, "@DlvyDate", DbType.Date, detail.DlvyDate);
                        db.AddInParameter(dbCom, "@PoQty", DbType.Decimal, detail.PoQty);
                        db.AddInParameter(dbCom, "@PoBasicUnit", DbType.String, detail.PoBasicUnit);
                        db.AddInParameter(dbCom, "@WhCode", DbType.String, detail.WhCode);
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

        public void Delete(IEnumerable<PurcharseOrderDetail> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                string str;
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (PurcharseOrderDetail detail in items)
                    {
                        str = string.Format("DELETE purcharse_Order_Detail WHERE PoNo = '{0}' AND Seq = {1}", detail.PoNo, detail.Seq);
                        dbCom = db.GetSqlStringCommand(str);
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

        public void Close()
        {
            IEnumerable<PurcharseOrderDetail> items = this.Items.Where(u => u.IsChecked == true);
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                string str;
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (PurcharseOrderDetail detail in items)
                    {
                        str = string.Format("UPDATE purcharse_Order_Detail SET CloseFlag = 'Y' WHERE PoNo = '{0}' AND Seq = {1}", detail.PoNo, detail.Seq);
                        dbCom = db.GetSqlStringCommand(str);
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MesAdmin.Models
{
    public class StockMovementDetail : StateBusinessObject
    {
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public DateTime DocumentDate
        {
            get { return GetProperty(() => DocumentDate); }
            set { SetProperty(() => DocumentDate, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string TransType
        {
            get { return GetProperty(() => TransType); }
            set { SetProperty(() => TransType, value); }
        }
        public string MoveType
        {
            get { return GetProperty(() => MoveType); }
            set { SetProperty(() => MoveType, value); }
        }
        public string DCFlag
        {
            get { return GetProperty(() => DCFlag); }
            set { SetProperty(() => DCFlag, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public decimal? Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string TransLotNo
        {
            get { return GetProperty(() => TransLotNo); }
            set { SetProperty(() => TransLotNo, value); }
        }
        public string TransWhCode
        {
            get { return GetProperty(() => TransWhCode); }
            set { SetProperty(() => TransWhCode, value); }
        }
        public string TransWhName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string TransWaCode
        {
            get { return GetProperty(() => TransWaCode); }
            set { SetProperty(() => TransWaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string TransItemCode
        {
            get { return GetProperty(() => TransItemCode); }
            set { SetProperty(() => TransItemCode, value); }
        }
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
        public int? DnSeq
        {
            get { return GetProperty(() => DnSeq); }
            set { SetProperty(() => DnSeq, value); }
        }
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public int? SoSeq
        {
            get { return GetProperty(() => SoSeq); }
            set { SetProperty(() => SoSeq, value); }
        }
        public string DnLotNo
        {
            get { return GetProperty(() => DnLotNo); }
            set { SetProperty(() => DnLotNo, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string WhName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
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
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public string StockType
        {
            get { return GetProperty(() => StockType); }
            set { SetProperty(() => StockType, value); }
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
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
    }

    public class StockMovementDetailList : ObservableCollection<StockMovementDetail>
    {
        private object thisLock = new object();
        private string documentNo;
        private DateTime? startDate;
        private DateTime? endDate;
        private string itemCode;
        private string itemAccount;
        private string transType;
        private string moveType;
        private string bizAreaCode;
        private string lotNo;

        public StockMovementDetailList() {}
        public StockMovementDetailList(IEnumerable<StockMovementDetail> items) : base(items) {}
        public StockMovementDetailList(string documentNo = "", DateTime? startDate = null, DateTime? endDate = null,
            string itemCode = "", string itemAccount = "", string transType = "", string moveType = "", string bizAreaCode = "", string lotNo = "")
        {
            this.documentNo = documentNo;
            this.startDate = startDate;
            this.endDate = endDate;
            this.itemCode = itemCode;
            this.itemAccount = itemAccount;
            this.transType = transType;
            this.moveType = moveType;
            this.bizAreaCode = bizAreaCode;
            this.lotNo = lotNo;
            InitializeList();
        }

        public string Save()
        {
            string documentNo = string.Empty;
            IEnumerable<StockMovementDetail> items = this.Items;
            documentNo = Insert(items.Where(u => u.State == EntityState.Added));
            Delete(items.Where(u => u.State == EntityState.Deleted));

            return documentNo;
        }

        public string Insert(IEnumerable<StockMovementDetail> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    string createdDocNo = string.Empty;
                    foreach (StockMovementDetail detail in items)
                    {
                        dbCom = db.GetStoredProcCommand("usp_stock_Movement_Create");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@DocumentNo", DbType.String, createdDocNo);
                        db.AddInParameter(dbCom, "@DocumentDate", DbType.Date, detail.DocumentDate);
                        db.AddInParameter(dbCom, "@TransType", DbType.String, detail.TransType);
                        db.AddInParameter(dbCom, "@MoveType", DbType.String, detail.MoveType);
                        db.AddInParameter(dbCom, "@DCFlg", DbType.String, detail.DCFlag);
                        db.AddInParameter(dbCom, "@StockType", DbType.String, detail.StockType);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, detail.ItemCode);
                        db.AddInParameter(dbCom, "@WhCode", DbType.String, detail.WhCode);
                        db.AddInParameter(dbCom, "@WaCode", DbType.String, detail.WaCode);
                        db.AddInParameter(dbCom, "@LotNo", DbType.String, detail.LotNo);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, detail.Qty);
                        db.AddInParameter(dbCom, "@BasicUnit", DbType.String, detail.BasicUnit);
                        db.AddInParameter(dbCom, "@TransLotNo", DbType.String, detail.TransLotNo);
                        db.AddInParameter(dbCom, "@TransWhCode", DbType.String, detail.TransWhCode);
                        db.AddInParameter(dbCom, "@TransWaCode", DbType.String, detail.TransWaCode);
                        db.AddInParameter(dbCom, "@TransItemCode", DbType.String, detail.TransItemCode);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, detail.Memo);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddOutParameter(dbCom, "@CreatedDocNo", DbType.String, 20);
                        db.ExecuteNonQuery(dbCom, trans);
                        createdDocNo = db.GetParameterValue(dbCom, "@CreatedDocNo").ToString();
                    }
                    trans.Commit();
                    return createdDocNo;
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public void Delete(IEnumerable<StockMovementDetail> items) 
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
                    foreach (StockMovementDetail detail in items)
                    {
                        str = string.Format("UPDATE stock_Movement_Detail SET DelFlag = 'Y' WHERE DocumentNo='{0}' AND Seq = {1}", detail.DocumentNo, detail.Seq);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch(SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public void InitializeList()
        {
            base.Clear();

            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_stock_Movement_Detail_List");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@DocumentNo", DbType.String, documentNo);
            db.AddInParameter(dbCom, "@TransType", DbType.String, transType);
            db.AddInParameter(dbCom, "@MoveType", DbType.String, moveType);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ItemAccount", DbType.String, itemAccount);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add
                (
                    new StockMovementDetail
                    {
                        DocumentNo = (string)u["DocumentNo"],
                        DocumentDate = (DateTime)u["DocumentDate"],
                        Seq = (int)u["Seq"],
                        DCFlag = (string)u["DCFlag"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        ItemAccount = (string)u["ItemAccount"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        StockType = (string)u["StockType"],
                        WhCode = (string)u["WhCode"],
                        WhName = u["WhName"].ToString(),
                        WaCode = (string)u["WaCode"],
                        BasicUnit = (string)u["BasicUnit"],
                        TransType = (string)u["TransType"],
                        MoveType = (string)u["MoveType"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        BizCode = (string)u["BizCode"],
                        DnNo = u["DnNo"].ToString(),
                        DnSeq = DBNull.Value == u["DnSeq"] ? null : (int?)u["DnSeq"],
                        SoNo = u["SoNo"].ToString(),
                        SoSeq = DBNull.Value == u["SoSeq"] ? null : (int?)u["SoSeq"],
                        TransItemCode = u["TransItemCode"].ToString(),
                        TransLotNo = u["TransLotNo"].ToString(),
                        TransWhCode = u["TransWhCode"].ToString(),
                        TransWhName = u["TransWhName"].ToString(),
                        TransWaCode = u["TransWaCode"].ToString(),
                        ProductOrderNo = u["ProductOrderNo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
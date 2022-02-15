using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Reflection;
using System.Threading.Tasks;

namespace MesAdmin.Models
{
    public class CommonBizPartner : StateBusinessObject
    {
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string BizType
        {
            get { return GetProperty(() => BizType); }
            set { SetProperty(() => BizType, value); }
        }
        public string BizRegNo
        {
            get { return GetProperty(() => BizRegNo); }
            set { SetProperty(() => BizRegNo, value); }
        }
        public string BizFullName
        {
            get { return GetProperty(() => BizFullName); }
            set { SetProperty(() => BizFullName, value); }
        }
        public string BizName
        {
            get { return GetProperty(() => BizName); }
            set { SetProperty(() => BizName, value); }
        }
        public string PresidentName
        {
            get { return GetProperty(() => PresidentName); }
            set { SetProperty(() => PresidentName, value); }
        }
        public string AccountNo
        {
            get { return GetProperty(() => AccountNo); }
            set { SetProperty(() => AccountNo, value); }
        }
        public string AccountHolder
        {
            get { return GetProperty(() => AccountHolder); }
            set { SetProperty(() => AccountHolder, value); }
        }
        public string ZipCode
        {
            get { return GetProperty(() => ZipCode); }
            set { SetProperty(() => ZipCode, value); }
        }
        public string Addr1
        {
            get { return GetProperty(() => Addr1); }
            set { SetProperty(() => Addr1, value); }
        }
        public string Addr2
        {
            get { return GetProperty(() => Addr2); }
            set { SetProperty(() => Addr2, value); }
        }
        public string IndType
        {
            get { return GetProperty(() => IndType); }
            set { SetProperty(() => IndType, value); }
        }
        public string IndClass
        {
            get { return GetProperty(() => IndClass); }
            set { SetProperty(() => IndClass, value); }
        }
        public string TelNo1
        {
            get { return GetProperty(() => TelNo1); }
            set { SetProperty(() => TelNo1, value); }
        }
        public string TelNo2
        {
            get { return GetProperty(() => TelNo2); }
            set { SetProperty(() => TelNo2, value); }
        }
        public string FaxNo
        {
            get { return GetProperty(() => FaxNo); }
            set { SetProperty(() => FaxNo, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public string VATFlag
        {
            get { return GetProperty(() => VATFlag); }
            set { SetProperty(() => VATFlag, value); }
        }
        public string Currency
        {
            get { return GetProperty(() => Currency); }
            set { SetProperty(() => Currency, value); }
        }
        public decimal VATRate
        {
            get { return GetProperty(() => VATRate); }
            set { SetProperty(() => VATRate, value); }
        }
        public string UpdateId { get; set; }
        public DateTime UpdateDate { get; set; }

        public CommonBizPartner ShallowCopy()
        {
            return (CommonBizPartner)this.MemberwiseClone();
        }

        public CommonBizPartner DeepCloneReflection()
        {
            var newItem = new CommonBizPartner();
             PropertyInfo[] properties = typeof(CommonBizPartner).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                try
                {
                    property.SetValue(newItem, property.GetValue(this, null), null);
                }
                catch { }
            }
            return newItem;
        }
    }

    public class CommonBizPartnerList : ObservableCollection<CommonBizPartner>
    {
        private object thisLock = new object();
        private string bizCode;
        private string bizName;
        private string bizType;

        public CommonBizPartnerList(IEnumerable<CommonBizPartner> items) : base(items) { }
        public CommonBizPartnerList(string bizCode = "", string bizName = "", string bizType = "")
        {
            this.bizCode = bizCode;
            this.bizName = bizName;
            this.bizType = bizType;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM common_BizPartner WHERE BizCode !='' ";
            if (!string.IsNullOrEmpty(bizCode))
                str += "And BizCode = '" + bizCode + "'";
            if (!string.IsNullOrEmpty(bizType))
                str += "And bizType = '" + bizType + "'";
            if (!string.IsNullOrEmpty(bizName))
                str += "And bizName LIKE '%" + bizName + "%'";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add
                (
                    new CommonBizPartner
                    {
                        BizCode = (string)u["BizCode"],
                        BizType = (string)u["BizType"],
                        BizRegNo = (string)u["BizRegNo"],
                        BizFullName = (string)u["BizFullName"],
                        BizName = (string)u["BizName"],
                        PresidentName = (string)u["PresidentName"],
                        AccountNo = (string)u["AccountNo"],
                        AccountHolder = (string)u["AccountHolder"],
                        ZipCode = (string)u["ZipCode"],
                        Addr1 = (string)u["Addr1"],
                        Addr2 = (string)u["Addr2"],
                        IndType = (string)u["IndType"],
                        IndClass = (string)u["IndClass"],
                        TelNo1 = (string)u["TelNo1"],
                        TelNo2 = (string)u["TelNo2"],
                        FaxNo = (string)u["FaxNo"],
                        IsEnabled = (bool)u["IsEnabled"],
                        VATFlag = u["VATFlag"].ToString(),
                        Currency = u["Currency"].ToString(),
                        VATRate = u["VATRate"] == DBNull.Value ? 0 : (decimal)u["VATRate"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonBizPartner> items = this.Items;
            Insert(items.Where(u => u.State == EntityState.Added));
            Update(items.Where(u => u.State == EntityState.Modified));
            Delete(items.Where(u => u.State == EntityState.Deleted));

            // Global 고객정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonBizPartner.Instance = null;
        }

        public void Insert(IEnumerable<CommonBizPartner> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                string bizCode; 
                try
                {
                    foreach (CommonBizPartner item in items)
                    {
                        // Code 생성
                        dbCom = db.GetSqlStringCommand("SELECT dbo.fn_GetBizPartnerCode(@BizType)");
                        db.AddInParameter(dbCom, "@BizType", DbType.String, item.BizType);
                        bizCode = (string)db.ExecuteScalar(dbCom);

                        str = "INSERT INTO common_BizPartner(BizCode"
                            + ", BizType"
                            + ", BizRegNo"
                            + ", BizFullName"
                            + ", BizName"
                            + ", PresidentName"
                            + ", ZipCode"
                            + ", Addr1"
                            + ", Addr2"
                            + ", IndType"
                            + ", IndClass"
                            + ", TelNo1"
                            + ", TelNo2"
                            + ", FaxNo"
                            + ", IsEnabled"
                            + ", VATFlag"
                            + ", Currency"
                            + ", VATRate"
                            + ", InsertId"
                            + ", UpdateId"
                            + ", UpdateDate"
                            + ", AccountNo"
                            + ", AccountHolder) VALUES "
                            + "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', {17}, '{18}', '{18}', getdate(), '{19}', '{20}')";
                        str = string.Format(str, bizCode
                            , item.BizType
                            , item.BizRegNo
                            , item.BizFullName
                            , item.BizName
                            , item.PresidentName
                            , item.ZipCode
                            , item.Addr1
                            , item.Addr2
                            , item.IndType
                            , item.IndClass
                            , item.TelNo1
                            , item.TelNo2
                            , item.FaxNo
                            , item.IsEnabled
                            , item.VATFlag
                            , item.Currency
                            , item.VATRate
                            , DSUser.Instance.UserID
                            , item.AccountNo
                            , item.AccountHolder);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                        item.BizCode = bizCode;
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

        public void Update(IEnumerable<CommonBizPartner> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (CommonBizPartner item in items)
                    {
                        str = "UPDATE common_BizPartner SET BizType = '{1}', BizRegNo = '{2}', BizFullName = '{3}', BizName = '{4}', PresidentName = '{5}', ZipCode = '{6}', Addr1 = '{7}', Addr2 = '{8}'"
                            + ", IndType = '{9}', IndClass = '{10}', TelNo1 = '{11}', TelNo2 = '{12}', FaxNo = '{13}', IsEnabled = '{14}', UpdateId = '{15}', UpdateDate = getdate() "
                            + ", AccountNo = '{16}', AccountHolder = '{17}', VATFlag = '{18}', Currency = '{19}', VATRate = {20} WHERE BizCode='{0}'";
                        str = string.Format(str
                            , item.BizCode
                            , item.BizType
                            , item.BizRegNo
                            , item.BizFullName
                            , item.BizName
                            , item.PresidentName
                            , item.ZipCode
                            , item.Addr1
                            , item.Addr2
                            , item.IndType
                            , item.IndClass
                            , item.TelNo1
                            , item.TelNo2
                            , item.FaxNo
                            , item.IsEnabled
                            , DSUser.Instance.UserID
                            , item.AccountNo
                            , item.AccountHolder
                            , item.VATFlag
                            , item.Currency
                            , item.VATRate);
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

        public void Delete(IEnumerable<CommonBizPartner> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (CommonBizPartner item in items)
                    {
                        str = string.Format("DELETE common_BizPartner WHERE BizCode = '{0}' ", item.BizCode);
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

        public void SyncErp()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbCommand dbCom = db.GetSqlStringCommand("EXEC usp_IF_ERP2MES_B_BIZ_PARTNER_KO656");
                db.ExecuteNonQuery(dbCom);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Models
{
    public class CommonWorkAreaInfo : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string WaName
        {
            get { return GetProperty(() => WaName); }
            set { SetProperty(() => WaName, value); }
        }
        public int Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
        }
        public string Explain
        {
            get { return GetProperty(() => Explain); }
            set { SetProperty(() => Explain, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public string WorkOrderFlag
        {
            get { return GetProperty(() => WorkOrderFlag); }
            set { SetProperty(() => WorkOrderFlag, value); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
    }

    public class CommonWorkAreaInfoList : ObservableCollection<CommonWorkAreaInfo>
    {
        private string bizAreaCode;

        public CommonWorkAreaInfoList(string bizAreaCode = "")
        {
            this.bizAreaCode = bizAreaCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM common_WorkAreaInfo WHERE WaCode != '' ";
            if (!string.IsNullOrEmpty(bizAreaCode))
                str += " And BizAreaCode = '" + bizAreaCode + "' ";
            str += "Order By BizAreaCode, [Order]";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonWorkAreaInfo
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        WaCode = (string)u["WaCode"],
                        WhCode = (string)u["WhCode"],
                        WaName = (string)u["WaName"],
                        Order = (int)u["Order"],
                        Explain = u["Explain"].ToString(),
                        IsEnabled = (bool)u["IsEnabled"],
                        UpdateId = (string)u["UpdateId"],
                        WorkOrderFlag = u["WorkOrderFlag"].ToString(),
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonWorkAreaInfo> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonWorkAreaInfo.Instance = null;
        }

        public void Insert(IEnumerable<CommonWorkAreaInfo> items)
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
                    foreach (CommonWorkAreaInfo item in items)
                    {
                        str = "INSERT INTO common_WorkAreaInfo(BizAreaCode, WaCode, WaName, WhCode, [Order], Explain, IsEnabled, InsertId, UpdateId, UpdateDate) VALUES "
                            + "('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', '{7}', getdate())";
                        str = string.Format(str
                            , item.BizAreaCode
                            , item.WaCode
                            , item.WaName
                            , item.WhCode
                            , item.Order
                            , item.Explain
                            , item.IsEnabled
                            , DSUser.Instance.UserID);
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
        public void Update(IEnumerable<CommonWorkAreaInfo> items)
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
                    foreach (CommonWorkAreaInfo item in items)
                    {
                        str = "UPDATE common_WorkAreaInfo SET WaName = '{1}', [Order] = {2}, Explain = '{3}', IsEnabled = '{4}', WhCode = '{5}', UpdateId = '{6}', UpdateDate = getdate() WHERE WaCode='{0}'";
                        str = string.Format(str
                            , item.WaCode
                            , item.WaName
                            , item.Order
                            , item.Explain
                            , item.IsEnabled
                            , item.WhCode
                            , DSUser.Instance.UserID);
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
        public void Delete(IEnumerable<CommonWorkAreaInfo> items)
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
                    foreach (CommonWorkAreaInfo item in items)
                    {
                        str = string.Format("DELETE common_WorkAreaInfo WHERE WaCode = '{0}' ", item.WaCode);
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

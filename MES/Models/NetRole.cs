using System;
using System.Collections.Generic;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Web.Security;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class NetRole : StateBusinessObject
    {
        public Guid RoleId
        {
            get { return GetProperty(() => RoleId); }
            set { SetProperty(() => RoleId, value); }
        }
        public string RoleName
        {
            get { return GetProperty(() => RoleName); }
            set { SetProperty(() => RoleName, value); }
        }
        public string Description
        {
            get { return GetProperty(() => Description); }
            set { SetProperty(() => Description, value); }
        }
        public ObservableCollection<NetMenu> Menus
        {
            get { return GetProperty(() => Menus) ?? new ObservableCollection<NetMenu>(); }
            set { SetProperty(() => Menus, value); }
        }
    }

    public class NetRoles : ObservableCollection<NetRole>
    {
        public NetRoles(IEnumerable<NetRole> items) : base(items) { }

        public static NetRoles Select()
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            DbCommand dbCom = db.GetStoredProcCommand("USP_GetRolesAndMenus");
            DataSet ds = db.ExecuteDataSet(dbCom);

            IEnumerable<NetRole> items = ds.Tables[0].AsEnumerable().Select(u => new NetRole
            {
                State = EntityState.Unchanged,
                RoleId = (Guid)u["RoleId"],
                RoleName = (string)u["RoleName"],
                Description = u["Description"].ToString(),
                Menus = new NetMenus
                (
                    GetNetMenus((Guid)u["RoleId"])
                                .AsEnumerable()
                                .Select(r => new NetMenu()
                                {
                                    PMenuId = r["PMenuId"] == DBNull.Value ? Guid.Empty : (Guid)r["PMenuId"],
                                    MenuId = (Guid)r["MenuId"],
                                    MenuName = (string)r["MenuName"],
                                    Sequence = (int)r["Sequence"],
                                    IsChecked = (bool)r["IsChecked"]
                                })
                )
            });

            NetRoles res = new NetRoles(items);
            return res;
        }

        public static string InsertMenus(Guid roleId, NetMenus menus)
        {
            string err = string.Empty;
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    str = string.Format("DELETE aspnet_RolesInMenus WHERE RoleId = '{0}'", roleId);
                    dbCom = db.GetSqlStringCommand(str);
                    db.ExecuteNonQuery(dbCom, trans);
                    
                    foreach (NetMenu item in menus)
                    {
                        str = string.Format("INSERT INTO aspnet_RolesInMenus VALUES ('{0}','{1}')", roleId, item.MenuId);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    err = ex.Message;
                }
            }

            return err;
        }

        public static DataTable GetNetMenus(Guid roleId)
        {
            DataTable dt = new DataTable();
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            DbCommand dbCom = db.GetStoredProcCommand("USP_GetRolesAndMenus");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@RoleId", DbType.Guid, roleId);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[1];
        }

        public static void Insert(IEnumerable<NetRole> roles)
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (NetRole item in roles)
                    {
                        str = string.Format("INSERT INTO aspnet_Roles VALUES ('{0}', '{1}', '{2}', '{3}', '{4}') "
                            , "EF52B6D6-6DF0-4463-875A-A9D1475E49D2"
                            , Guid.NewGuid()
                            , item.RoleName
                            , item.RoleName.ToLower()
                            , item.Description);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }
        }

        public static void Delete(IEnumerable<NetRole> roles)
        {
            foreach (NetRole item in roles)
            {
                Roles.DeleteRole(item.RoleName);
            }
        }
    }
}

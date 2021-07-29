using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Web.Security;
using System.Web.Profile;
using System.ComponentModel.DataAnnotations;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class NetUser : StateBusinessObject
    {
        public Guid UserId
        {
            get { return GetProperty(() => UserId); }
            set { SetProperty(() => UserId, value); }
        }
        [Required(ErrorMessage = "사용자 ID를 입력하세요!")]
        public string UserName
        {
            get { return GetProperty(() => UserName); }
            set { SetProperty(() => UserName, value); }
        }
        [Required(ErrorMessage = "Password를 입력하세요!")]
        [MinLength(4, ErrorMessage = "Password는 4자리 이상이어야 합니다.")]
        public string Password
        {
            get { return GetProperty(() => Password); }
            set { SetProperty(() => Password, value); }
        }
        public string Email
        {
            get { return GetProperty(() => Email); }
            set { SetProperty(() => Email, value); }
        }
        public bool IsApproved
        {
            get { return GetProperty(() => IsApproved); }
            set { SetProperty(() => IsApproved, value); }
        }
        public ObservableCollection<NetRole> Roles
        {
            get { return GetProperty(() => Roles); }
            set { SetProperty(() => Roles, value); }
        }
        public NetProfile Profile
        {
            get { return GetProperty(() => Profile); }
            set { SetProperty(() => Profile, value); }
        }
    }

    public class NetUsers : ObservableCollection<NetUser>
    {
        public NetUsers(IEnumerable<NetUser> items) : base(items) { }

        public static NetUsers Select(string userName = "")
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            DbCommand dbCom = db.GetStoredProcCommand("USP_GetUsersAndRoles");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@UserName", DbType.String, userName);
            DataSet ds = db.ExecuteDataSet(dbCom);

            IEnumerable<NetUser> items = ds.Tables[0].AsEnumerable().Select(u => new NetUser 
            {
                UserId = (Guid)u["UserId"],
                UserName = (string)u["UserName"],
                Password = (string)u["Password"],
                Email = u["Email"].ToString(),
                IsApproved = (bool)u["IsApproved"],
                Profile = GetProfile((string)u["UserName"]),
                Roles = new NetRoles
                (
                    ds.Tables[1].AsEnumerable()
                                .Where(r => (Guid)r["UserId"] == (Guid)u["UserId"])
                                .Select(r => new NetRole()
                                {
                                    RoleId = (Guid)r["RoleId"],
                                    RoleName = (string)r["RoleName"],
                                    Description = (string)r["Description"],
                                    State = MesAdmin.Common.Common.EntityState.Unchanged
                                })
                )
            });

            NetUsers res = new NetUsers(items);
            return res;
        }

        public static NetProfile GetProfile(string userName)
        {
            ProfileBase profile = ProfileBase.Create(userName);
            NetProfile res = new NetProfile
            {
                Profile = profile,
                KorName = (string)profile["KorName"],
                Department = (string)profile["Department"],
                WorkParts = (string)profile["WorkParts"],
            };
            return res;
        }

        public static string AddRoles(Guid userId, IEnumerable<NetRole> netRoles)
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
                    foreach (NetRole item in netRoles)
                    {
                        str = string.Format("INSERT INTO aspnet_UsersInRoles VALUES ('{0}', '{1}') ", userId, item.RoleId);
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

        public static void DelteRoles(Guid userId, IEnumerable<NetRole> netRoles)
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
                    foreach (NetRole item in netRoles)
                    {
                        str = string.Format("DELETE aspnet_UsersInRoles WHERE UserId = '{0}' AND RoleId = '{1}'", userId, item.RoleId);
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

        public static void Delete(IEnumerable<NetUser> users)
        {
            foreach (NetUser user in users)
            { 
                // membsership 삭제
                Membership.DeleteUser(user.UserName);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MesAdmin.Models
{
    public class NetMenu : CheckableBusinessObject
    {
        public Guid PMenuId
        {
            get { return GetProperty(() => PMenuId); }
            set { SetProperty(() => PMenuId, value); }
        }
        public Guid MenuId
        {
            get { return GetProperty(() => MenuId); }
            set { SetProperty(() => MenuId, value); }
        }
        public int Sequence
        {
            get { return GetProperty(() => Sequence); }
            set { SetProperty(() => Sequence, value); }
        }
        public string MenuName
        {
            get { return GetProperty(() => MenuName); }
            set { SetProperty(() => MenuName, value); }
        }
        public string MenuNameFull
        {
            get { return GetProperty(() => MenuNameFull); }
            set { SetProperty(() => MenuNameFull, value); }
        }
        public string CommandParameter
        {
            get { return GetProperty(() => CommandParameter); }
            set { SetProperty(() => CommandParameter, value); }
        }
        public List<NetMenu> Submenu { get; set; }
        public BitmapImage ImageSource { get; set; }
        public string ImageSourceUri { get; set; }
        public ICommand Command { get; set; }
        public bool IsExpanded { get; set; }
    }

    public class NetMenus : ObservableCollection<NetMenu>
    {
        public NetMenus(IEnumerable<NetMenu> items) : base(items) { }
        public NetMenus() { }

        public IList<NetMenu> GetUserMenus(string userName = "")
        {
            base.Clear();
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            DbCommand dbCom = db.GetStoredProcCommand("usp_GetMenus");
            db.AddInParameter(dbCom, "@UserName", DbType.String, userName);
            DataSet ds = db.ExecuteDataSet(dbCom);

            IEnumerable<NetMenu> roots = ds.Tables[0].AsEnumerable()
                .Where(u => u["PMenuId"] == DBNull.Value).Select(o =>
                    new NetMenu
                    {
                        MenuName = (string)o["MenuName"],
                        PMenuId = Guid.Empty,
                        MenuId = (Guid)o["MenuId"],
                        Sequence = (int)o["Sequence"],
                        ImageSourceUri = o["Svg"].ToString(),
                    });

            foreach (NetMenu item in roots)
            {
                base.Add(item);
                AddNodesRecursively(item, ds.Tables[0].AsEnumerable());
            }

            return base.Items;
        }

        public NetMenus GetAllMenus()
        {
            base.Clear();
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            DbCommand dbCom = db.GetStoredProcCommand("usp_GetMenus");
            db.AddInParameter(dbCom, "@UserName", DbType.String, "");
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(o =>
                base.Add(
                    new NetMenu
                    {
                        PMenuId = o["PMenuId"] == DBNull.Value ? Guid.Empty : (Guid)o["PMenuId"],
                        MenuName = (string)o["MenuName"],
                        MenuNameFull = (string)o["MenuNameFull"],
                        MenuId = (Guid)o["MenuId"],
                        Sequence = (int)o["Sequence"],
                        CommandParameter = o["CommandParameter"].ToString()
                    }
                )
            );

            return new NetMenus(base.Items);
        }

        public void AddNodesRecursively(NetMenu root, IEnumerable<DataRow> datas)
        {
            Guid id = root.MenuId;
            var current = datas.Where(o => (o["PMenuId"] == DBNull.Value ? Guid.Empty : (Guid)o["PMenuId"]) == id);
            Properties.Settings.Default.ExpandedMenus = Properties.Settings.Default.ExpandedMenus ?? new System.Collections.Specialized.StringCollection();

            foreach (var item in current)
            {
                NetMenu child = new NetMenu
                {
                    PMenuId = (Guid)item["PMenuId"],
                    MenuId = (Guid)item["MenuId"],
                    MenuName = (string)item["MenuName"],
                    Command = MainViewModel.ShowDocumentCmd,
                    CommandParameter = (string)item["CommandParameter"],
                    IsExpanded = Properties.Settings.Default.ExpandedMenus.Contains(item["MenuId"].ToString().ToUpper()) ? true : false,
                };

                if (root.Submenu == null)
                    root.Submenu = new List<NetMenu>();
                root.Submenu.Add(child);
                AddNodesRecursively(child, datas);
            }
        }

        public void Update(NetMenu pm)
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            string sql = string.Empty;
            if (pm.PMenuId == Guid.Empty)
            {
                sql = string.Format("UPDATE aspnet_Menus SET MenuName = '{0}', Sequence = {1}, CommandParameter = '{2}' WHERE MenuId='{3}'"
                    , pm.MenuName, pm.Sequence, pm.CommandParameter, pm.MenuId);
            }
            else
            {
                sql = string.Format("UPDATE aspnet_Menus SET MenuName = '{0}', Sequence = {1}, PMenuId = '{2}', CommandParameter = '{3}' WHERE MenuId='{4}'"
                    , pm.MenuName, pm.Sequence, pm.PMenuId, pm.CommandParameter, pm.MenuId);
            }
            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.ExecuteNonQuery(dbCom);
        }

        public void Insert(NetMenu pm)
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            string sql = string.Empty;
            if (pm.PMenuId == Guid.Empty)
            {
                sql = string.Format("INSERT INTO aspnet_Menus(Sequence, PMenuId, MenuId, MenuName, CommandParameter) VALUES ({0}, null, '{1}', '{2}', '{3}')"
                    , pm.Sequence, Guid.NewGuid(), pm.MenuName, pm.CommandParameter);
            }
            else
            {
                sql = string.Format("INSERT INTO aspnet_Menus(Sequence, PMenuId, MenuId, MenuName, CommandParameter) VALUES ({0}, '{1}', '{2}', '{3}', '{4}')"
                    , pm.Sequence, pm.PMenuId, Guid.NewGuid(), pm.MenuName, pm.CommandParameter);
            }
            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.ExecuteNonQuery(dbCom);
        }

        public void Delete(NetMenu pm)
        {
            Database db = new DatabaseProviderFactory().Create(DBInfo.Instance.AuthName);
            string sql = string.Format("DELETE aspnet_Menus WHERE MenuId = '{0}'", pm.MenuId);
            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.ExecuteNonQuery(dbCom);
        }
    }
}

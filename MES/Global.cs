using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Common;
using System.Windows.Input;
using DevExpress.Xpf.Grid;

namespace MesAdmin
{
    public class DSUser
    {
        private static DSUser m_instance;
        private string userID;
        private string userName;
        private string position;
        private List<string> roleName;
        private string bizAreaCode;

        private DSUser() { }

        public static DSUser Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new DSUser();
                }
                return m_instance;
            }
        }
        public string UserID { get { return userID; } set { userID = value; } }
        public string UserName { get { return userName; } set { userName = value; } }
        public string Position { get { return position; } set { position = value; } }
        public List<string> RoleName { get { return roleName; } set { roleName = value; } }
        public string BizAreaCode { get { return bizAreaCode; } set { bizAreaCode = value; } }
    }

    public class DBInfo
    { 
        private static DBInfo m_instance;
        private string name;
        private string authName = "Authentication";

        private DBInfo() { }

        public static DBInfo Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new DBInfo();
                return m_instance;
            }
        }

        public string Name { get { return name; } set { name = value; } }
        public string AuthName { get { return authName; } set { authName = value; } }
    }

    public class ProviderFactory
    {
        private static Database instance;
        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        instance = new DatabaseProviderFactory().Create(DBInfo.Instance.Name);
                    }
                    catch { }
                }
                return instance;
            }
            set { instance = value; }
        }
    }
}

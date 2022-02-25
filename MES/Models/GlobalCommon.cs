using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;

namespace MesAdmin.Models
{
    public class GlobalCommonMinor
    {
        private static CommonMinorList m_instance;

        public static CommonMinorList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CommonMinorList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonWorkAreaInfo
    {
        private static CommonWorkAreaInfoList m_instance;

        public static CommonWorkAreaInfoList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CommonWorkAreaInfoList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonEquipment
    {
        private static CommonEquipmentList m_instance;

        public static CommonEquipmentList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CommonEquipmentList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonBizPartner
    {
        private static CommonBizPartnerList m_instance;

        public static CommonBizPartnerList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CommonBizPartnerList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonSoTypeList
    {
        private static SalesOrderTypeConfigList m_instance;

        public static SalesOrderTypeConfigList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new SalesOrderTypeConfigList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonItem
    {
        private static CommonItemList m_instance;

        public static CommonItemList Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new CommonItemList();
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonQrType
    {
        private static ObservableCollection<CodeName> m_instance;

        public static ObservableCollection<CodeName> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ObservableCollection<CodeName>
                    {
                        new CodeName { Code = "IQC", Name = "수입검사" },
                        new CodeName { Code = "LQC", Name = "공정검사" },
                        new CodeName { Code = "FQC", Name = "최종검사" },
                        new CodeName { Code = "OQC", Name = "포장검사" }
                    };
                }
                return m_instance;
            }
        }
    }

    public class GlobalCommonTransfer
    {
        private static ObservableCollection<CodeName> m_instance;

        public static ObservableCollection<CodeName> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ObservableCollection<CodeName>
                    {
                        new CodeName { Code = "0", Name = "미확정" },
                        new CodeName { Code = "1", Name = "확정" }
                    };
                }
                return m_instance;
            }
        }
    }

    public class GlobalCommonIncude
    {
        private static ObservableCollection<CodeName> m_instance;

        public static ObservableCollection<CodeName> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ObservableCollection<CodeName>
                    {
                        new CodeName { Code = "N", Name = "별도" },
                        new CodeName { Code = "Y", Name = "포함" }
                    };
                }
                return m_instance;
            }
        }
    }

    public class GlobalCommonCloseFlag
    {
        private static ObservableCollection<CodeName> m_instance;

        public static ObservableCollection<CodeName> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ObservableCollection<CodeName>
                    {
                        new CodeName { Code = "Y", Name = "마감" },
                        new CodeName { Code = "N", Name = "미마감" }
                    };
                }
                return m_instance;
            }
        }
    }

    public class GlobalCommonPackingFlag
    {
        private static ObservableCollection<CodeName> m_instance;

        public static ObservableCollection<CodeName> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new ObservableCollection<CodeName>
                    {
                        new CodeName { Code = "Y", Name = "완료" },
                        new CodeName { Code = "N", Name = "대기" }
                    };
                }
                return m_instance;
            }
        }
    }

    public class GlobalCommonLayout
    {
        private static DataTable m_instance;

        public static DataTable Instance
        {
            get
            {
                if (m_instance == null)
                {
                    Database db = ProviderFactory.Instance;
                    string sql = "SELECT ViewName, Layout FROM common_gridLayout (NOLOCK) WHERE UserId = @UserID";

                    DbCommand dbCom = db.GetSqlStringCommand(sql);
                    db.AddInParameter(dbCom, "@UserID", DbType.String, DSUser.Instance.UserID);
                    DataSet ds = db.ExecuteDataSet(dbCom);
                    m_instance = ds.Tables[0];
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class GlobalCommonDockLayout
    {
        private static DataTable m_instance;

        public static DataTable Instance
        {
            get
            {
                if (m_instance == null)
                {
                    Database db = ProviderFactory.Instance;
                    string sql = "SELECT ViewName, Layout FROM common_dockLayout (NOLOCK) WHERE UserId = @UserID";

                    DbCommand dbCom = db.GetSqlStringCommand(sql);
                    db.AddInParameter(dbCom, "@UserID", DbType.String, DSUser.Instance.UserID);
                    DataSet ds = db.ExecuteDataSet(dbCom);
                    m_instance = ds.Tables[0];
                }
                return m_instance;
            }
            set { m_instance = value; }
        }
    }

    public class CodeName
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class ItemInfo
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}

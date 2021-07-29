using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesAdmin.Common.Common
{
    public class DocumentParamter
    {
        public EntityMessageType Type { get; set; }
        public object Item { get; set; }
        public string BizAreaCode { get; set; }
        public object ParentViewmodel { get; set; }

        public DocumentParamter(EntityMessageType type, object item, string bizAreaCode, object parentViewmodel)
        {
            Type = type;
            Item = item;
            BizAreaCode = bizAreaCode;
            ParentViewmodel = parentViewmodel;
        }
        public DocumentParamter(EntityMessageType type, object item) : this(type, item, ""){ }
        public DocumentParamter(EntityMessageType type, object item, string bizAreaCode) : this(type, item, bizAreaCode, null) { }
        public DocumentParamter(EntityMessageType type, object item, object parentViewmodel) : this(type, item, "", parentViewmodel) { }
        public DocumentParamter(EntityMessageType type) : this(type, null) { }
        public DocumentParamter(object item): this(EntityMessageType.Changed, item){ }
    }
}

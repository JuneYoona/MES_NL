using MesAdmin.Common.Common;

namespace MesAdmin.Common.Common
{
    public abstract class StateBusinessObject : ValidationBase
    {
        public EntityState State
        {
            get { return GetProperty(() => State); }
            set { SetProperty(() => State, value); }
        }
    }
}

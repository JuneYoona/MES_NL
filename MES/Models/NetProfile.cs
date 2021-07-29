using DevExpress.Mvvm;
using System.Web.Profile;

namespace MesAdmin.Models
{
    public class NetProfile : BindableBase
    {
        public ProfileBase Profile { get; set; }
        public string KorName
        {
            get { return GetProperty(() => KorName); }
            set { SetProperty(() => KorName, value); }
        }
        public string Department
        {
            get { return GetProperty(() => Department); }
            set { SetProperty(() => Department, value); }
        }
        public string WorkParts
        {
            get { return GetProperty(() => WorkParts); }
            set { SetProperty(() => WorkParts, value); }
        }

        public void Save(string userName = "")
        {
            if (userName != string.Empty)
                Profile = ProfileBase.Create(userName);

            Profile.SetPropertyValue("KorName", KorName);
            Profile.SetPropertyValue("Department", Department);
            Profile.SetPropertyValue("WorkParts", WorkParts);
            Profile.Save();
        }
    }
}

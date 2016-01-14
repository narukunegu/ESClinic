using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using ESClinic.Framework;
using ESClinic.Resources;
using Screen = Caliburn.Micro.Screen;

namespace ESClinic.ViewModels
{
    [Export(typeof(HomeViewModel))]
    public class HomeViewModel : Screen
    {
            
        private readonly User _user;

        public string ClinicName { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicPhone { get; set; }
        public BitmapImage LogoImage { get; set; }
        
        [ImportingConstructor]
        public HomeViewModel()
        {
            _user = App.Current.User;                             

            var clinic = App.Current.EsClinicContext.ClinicInfoes.FirstOrDefault();
            if (clinic == null)
            {
                if (_user.ManagementAccessible) MessageBox.Show(InfoMessages.WRN_FILL_CLINIC_INFO);
            }
            else
            {
                ClinicName = clinic.Name;
                ClinicAddress = clinic.Address;
                ClinicPhone = clinic.Phone;
                NotifyOfPropertyChange(() => ClinicName);
                NotifyOfPropertyChange(() => ClinicAddress);
                NotifyOfPropertyChange(() => ClinicPhone);
                if (clinic.Logo != null)
                {
                    LogoImage = ImageDataConverter.BytesToBitmapImage(clinic.Logo);
                    NotifyOfPropertyChange(() => LogoImage);
                }
            }
        }
              
    }
}

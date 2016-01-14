using System;
using System.Data;
using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ESClinic.Framework;
using Microsoft.Win32;

namespace ESClinic.ViewModels
{
    public class ClinicInfoManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        private bool _isLogoChanged;
        private readonly ClinicInfo _info;

        public ClinicInfoManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            LogoImg = new BitmapImage(new Uri("/ESClinic;component/Resources/Placeholder.jpg", UriKind.Relative));          
            NotifyOfPropertyChange(() => LogoImg);

            var info = _esClinicContext.ClinicInfoes.FirstOrDefault();
            if (info != null)
            {
                _info = info;
                ClinicName = info.Name;
                ClinicAddress = info.Address;
                ClinicPhone = info.Phone;
                if (info.Logo != null)
                {
                    LogoImg = ImageDataConverter.BytesToBitmapImage(info.Logo);
                    NotifyOfPropertyChange(() => LogoImg);
                }
            }
            else
            {
                _info = new ClinicInfo();
                _esClinicContext.ClinicInfoes.Add(_info);
                _esClinicContext.SaveChanges();
            }
        }

        private string _clinicName;

        public string ClinicName
        {
            get
            {
                return _clinicName;
            }
            set
            {
                _clinicName = value;
                NotifyOfPropertyChange(() => ClinicName);
            }
        }

        private string _clinicAddress;

        public string ClinicAddress
        {
            get
            {
                return _clinicAddress;
            }
            set
            {
                _clinicAddress = value;
                NotifyOfPropertyChange(() => ClinicAddress);
            }
        }

        private string _clinicPhone;

        public string ClinicPhone
        {
            get
            {
                return _clinicPhone;
            }
            set
            {
                _clinicPhone = value;
                NotifyOfPropertyChange(() => ClinicPhone);
            }
        }

        public BitmapImage LogoImg { get; set; }

        public void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = "JPEG Files: (*.JPG;*.JPEG;*.JPE)|*.JPG;*.JPEG;*.JPE"
            };
            if (dialog.ShowDialog() != true) return;
            _isLogoChanged = true;
            LogoImg = new BitmapImage();
            LogoImg.BeginInit();               
            LogoImg.UriSource = new Uri(dialog.FileName);
            LogoImg.EndInit();
            NotifyOfPropertyChange(() => LogoImg);
        }

        public void Save()
        {
            _info.Name = ClinicName;
            _info.Address = ClinicAddress;
            _info.Phone = ClinicPhone;
            if (_isLogoChanged)
                _info.Logo = ImageDataConverter.BitmapImageToBytes(LogoImg);
            _esClinicContext.Entry(_info).State = EntityState.Modified;
            _esClinicContext.SaveChanges();
        }
    }
}
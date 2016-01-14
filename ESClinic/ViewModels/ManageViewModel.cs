using System.Windows;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class ManageViewModel : Conductor<IScreen>
    {
        private bool _isProductSubCollapsed;
        private Visibility _productSubVisibility;

        public Visibility ProductSubVisibility
        {
            get { return _productSubVisibility; }
            set
            {
                _productSubVisibility = value;
                NotifyOfPropertyChange(() => ProductSubVisibility);
            }
        }

        private bool _isOptionsSubCollapsed;
        private Visibility _optionsSubVisibility;

        public Visibility OptionsSubVisibility
        {
            get
            {
                return _optionsSubVisibility;
            }
            set
            {
                _optionsSubVisibility = value;
                NotifyOfPropertyChange(() => OptionsSubVisibility);
            }
        }

        private bool _isInfoSubCollapsed;
        private Visibility _infoSubVisibility;

        public Visibility InfoSubVisibility
        {
            get
            {
                return _infoSubVisibility;
            }
            set
            {
                _infoSubVisibility = value;
                NotifyOfPropertyChange(() => InfoSubVisibility);
            }
        }

        public ManageViewModel()
        {
            ProductSubVisibility = Visibility.Collapsed;
            _isProductSubCollapsed = true;
            OptionsSubVisibility = Visibility.Collapsed;
            _isOptionsSubCollapsed = true;
            InfoSubVisibility = Visibility.Collapsed;
            _isInfoSubCollapsed = true;
        }

        public void ProductMenu()
        {
            ProductSubVisibility = _isProductSubCollapsed ? Visibility.Visible : Visibility.Collapsed;
            _isProductSubCollapsed = !_isProductSubCollapsed;
        }

        public void OptionsMenu()
        {
            OptionsSubVisibility = _isOptionsSubCollapsed ? Visibility.Visible : Visibility.Collapsed;
            _isOptionsSubCollapsed = !_isOptionsSubCollapsed;
        }

        public void InfoMenu()
        {
            InfoSubVisibility = _isInfoSubCollapsed ? Visibility.Visible : Visibility.Collapsed;
            _isInfoSubCollapsed = !_isInfoSubCollapsed;
        }

        public void InventoryManage()
        {
            ActivateItem(new ProductManageViewModel());
        }

        public void ImportManage()
        {
            ActivateItem(new ImportProductsViewModel());
        }

        public void ServiceManage()
        {
            ActivateItem(new ServiceManageViewModel());
        }

        public void SurchargeManage()
        {
            ActivateItem(new SurchargeManageViewModel());    
        }

        public void EndoscopyManage()
        {
            ActivateItem(new EndoscopyManageViewModel());
        }

        public void ClinicInfoManage()
        {
            ActivateItem(new ClinicInfoManageViewModel());
        }

        public void UserInfoManage()
        {
            ActivateItem(new UserInfoManageViewModel());
        }
    }
}

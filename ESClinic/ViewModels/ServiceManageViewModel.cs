using System.Linq;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class ServiceManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        public BindableCollection<ServiceType> ServiceTypes { get; set; }

        public ServiceManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            ServiceTypes = new BindableCollection<ServiceType>();

            var serviceTypes = _esClinicContext.ServiceTypes.ToList();

            foreach (var service in serviceTypes)
            {
                ServiceTypes.Add(service);
            }
        }
        public string Name { get; set; }
        public int Price { get; set; }

        public void AddService()
        {
            var servicetype = new ServiceType(){Name = Name, Price = Price};

            _esClinicContext.ServiceTypes.Add(servicetype);
            _esClinicContext.SaveChanges();

            ServiceTypes.Add(servicetype);
        }
    }
}
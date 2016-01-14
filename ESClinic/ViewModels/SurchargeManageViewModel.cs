using System.Linq;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class SurchargeManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        public BindableCollection<Surcharge> Surcharges { get; set; }

        public SurchargeManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            Surcharges = new BindableCollection<Surcharge>();

            var surcharges = _esClinicContext.Surcharges.ToList();

            foreach (var surcharge in surcharges)
            {
                Surcharges.Add(surcharge);
            }
        }

        public string Name { get; set; }
        public int Price { get; set; }

        public void AddSurcharge()
        {
            var surcharge = new Surcharge() { Name = Name, Price = Price };

            _esClinicContext.Surcharges.Add(surcharge);
            _esClinicContext.SaveChanges();

            Surcharges.Add(surcharge);
        }
    }
}
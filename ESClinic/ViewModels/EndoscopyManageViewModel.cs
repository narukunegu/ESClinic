using System.Linq;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class EndoscopyManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        public BindableCollection<EndoscopyType> EsTypes { get; set; }

        public EndoscopyManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            EsTypes = new BindableCollection<EndoscopyType>();

            var esTypes = _esClinicContext.EndoscopyTypes.ToList();

            foreach (var esType in esTypes)
            {
                EsTypes.Add(esType);
            }
        }

        public string Name { get; set; }
        public string Pattern { get; set; }

        public void AddEsType()
        {
            var esType = new EndoscopyType() { Name = Name, Pattern = Pattern};

            _esClinicContext.EndoscopyTypes.Add(esType);
            _esClinicContext.SaveChanges();

            EsTypes.Add(esType);
        }
    }
}
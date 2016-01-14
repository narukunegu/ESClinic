using System.Data;
using System.Data.Objects;
using System.Linq;
using Caliburn.Micro;
using EFAutoRefresh;
using ESClinic.Resources;

namespace ESClinic.ViewModels
{
    public class PaymentsViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        private readonly IWindowManager _windowManager;
        public AutoRefreshWrapper<Payor> Payors { get; set; }

        public PaymentsViewModel(IWindowManager windowManager)
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
                Payors = new AutoRefreshWrapper<Payor>(_esClinicContext.Payors, "Session", _esClinicContext, RefreshMode.StoreWins);
                _windowManager = windowManager;
            }

            

            Services = new BindableCollection<Service>();
            Drugs = new BindableCollection<Drug>();
            Surcharges = new BindableCollection<Surcharge>();

            var surchanges = _esClinicContext.Surcharges.ToList();
            foreach (var surchange in surchanges)
            {
                Surcharges.Add(surchange);
            }
        }

        public Payor SelectedPayor { get; set; }

        private Session _currentSession;

        public BindableCollection<Service> Services { get; set; }

        public BindableCollection<Drug> Drugs { get; set; }

        public BindableCollection<Surcharge> Surcharges { get; set; }

        private int? _totalCost;

        public int? TotalCost
        {
            get
            {
                return _totalCost;
            }
            set
            {
                _totalCost = value;
                NotifyOfPropertyChange(() => TotalCost);
            }
        }

        public int Fare { get; set; }

        public int Change { get; set; }

        public void LoadSessionInfo()
        {
            if (SelectedPayor == null) return;

            _currentSession = _esClinicContext.Sessions.Find(SelectedPayor.SessionId);

            TotalCost = _currentSession.TotalCost;

            var services =
                _esClinicContext.Services.Include("ServiceType").ToList().Where(s => s.SessionId == SelectedPayor.SessionId);

            Services.Clear();
            foreach (var service in services)
            {
                Services.Add(service);
            }

            var drugs =
                _esClinicContext.Drugs.Include("Product").ToList().Where(d => d.SessionId == SelectedPayor.SessionId);

            Drugs.Clear();
            foreach (var drug in drugs)
            {
                Drugs.Add(drug);
            }
        }

        public void Print()
        {
            if (SelectedPayor == null) return;
            _windowManager.ShowWindow(new PrescriptionViewModel(_currentSession));   
        }

        public void Finished()
        {
            if (_currentSession == null) return;

            foreach (var drug in Drugs)
            {
                var product = drug.Product;
                //product.Quantity -= drug.Quantity;
                _esClinicContext.Entry(product).State = EntityState.Modified;
            }
            _currentSession.Fare = Fare;
            _currentSession.Change = Change;
            _currentSession.Stage = ClinicStage.Finished;
            _esClinicContext.Entry(_currentSession).State = EntityState.Modified;
            _esClinicContext.Entry(SelectedPayor).State = EntityState.Deleted;
            _esClinicContext.SaveChanges();
        }
    }
}
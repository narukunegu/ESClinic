using System.Data;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public sealed class PatientViewModel : Screen
    {
        private Patient _patient;
        
        private readonly bool _isNewMode;

        private IEventAggregator _events;

        public PatientViewModel(IEventAggregator events)
        {
            DisplayName = "Bệnh Nhân Mới";
            
            _isNewMode = true;

            _events = events;
        }

        public PatientViewModel(Patient patient)
        {
            DisplayName = "Sửa Thông Tin Bệnh Nhân";
          
            _patient = patient;
            Name = patient.Name;
            BirthDay = patient.Birthday;
            Phone = patient.Phone;
            Address = patient.Address;
            Sex = patient.Sex;
            _isNewMode = false;
        }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private string _birthday;

        public string BirthDay
        {
            get
            {
                return _birthday;
            }
            set
            {
                _birthday = value;
                NotifyOfPropertyChange(() => BirthDay);
            }
        }

        private string _phone;

        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                _phone = value;
                NotifyOfPropertyChange(() => Phone);
            }
        }
      
        public string Sex { get; set; }

        private string _address;

        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
                NotifyOfPropertyChange(() => Address);
            }
        }

        public void Male()
        {
            Sex = "Nam";
        }

        public void Female()
        {
            Sex = "Nữ";
        }

        public void Ok()
        {
            if (_patient == null) _patient = new Patient();
            _patient.Name = Name;
            _patient.Sex = Sex;
            _patient.Birthday = BirthDay;
            _patient.Address = Address;
            _patient.Phone = Phone;

            App.Current.EsClinicContext.Entry(_patient).State = _patient.PatientId == 0
                ? EntityState.Added
                : EntityState.Modified;
            App.Current.EsClinicContext.SaveChanges();

            if (_isNewMode)
            {
                var visitor = new Visitor() { PatientId = _patient.PatientId };
                App.Current.EsClinicContext.Visitors.Add(visitor);
                App.Current.EsClinicContext.SaveChanges();
                _events.Publish(_patient, action => { Task.Factory.StartNew(action); });   
            }            
            TryClose();
        }
    }
}
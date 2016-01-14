using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using ESClinic.Framework;
using ESClinic.Resources;
using FeserWard.Controls;

namespace ESClinic.ViewModels
{
    public class ExamineViewModel : Screen, IHandle<EndoscopyRecord>, IHandle<Patient>
    {
        private readonly EsClinicEntities _esClinicContext;

        public BindableCollection<Visitor> Visitors { get; set; }

        private readonly User _user;

        public IIntelliboxResultsProvider PatientResultsProvider { get; set; }

        public ExamineViewModel(IWindowManager windowManager)
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
                
                _user = App.Current.User;
                _windowManager = windowManager;
            }           

            PatientResultsProvider = new PatientResultsProvider(_esClinicContext);
            ProductResultsProvider = new ProductResultsProvider(_esClinicContext);

            Sessions = new BindableCollection<Session>();
            EsRecords = new BindableCollection<EndoscopyRecord>();
            Services = new BindableCollection<Service>();
            Drugs = new BindableCollection<Drug>();
            Visitors = new BindableCollection<Visitor>();           
            RefreshVisitors();

            _events = new EventAggregator();
            _events.Subscribe(this);

            CanNewSession = false;
            _drugSaved = true;

            NotifyOfPropertyChange(() => CanSaveSession);
            NotifyOfPropertyChange(() => CanSaveDrugs);
        }

        #region Bệnh nhân

        private readonly IWindowManager _windowManager;

        public void NewPatient()
        {
            _windowManager.ShowDialog(new PatientViewModel(_events));
        }

        private void RefreshVisitors()
        {
            var visitors = _esClinicContext.Visitors.Include("Patient").ToList();
            Visitors.Clear();
            foreach (var visitor in visitors)
            {
                Visitors.Add(visitor);
            }
        }

        public void Handle(Patient patient)
        {
            RefreshVisitors();
        }

        private readonly IEventAggregator _events;

        private Visitor _currentVisitor;

        public Visitor SelectedVisitor { get; set; }

        public int SelectedPatient { get; set; }

        public void OkPatient()
        {
            if (SelectedPatient == 0)
                return;
            try
            {
                var visitor = new Visitor() { PatientId = SelectedPatient };
                _esClinicContext.Visitors.Add(visitor);
                _esClinicContext.SaveChanges();
                visitor.Patient = _esClinicContext.Patients.First(p => p.PatientId == visitor.PatientId);
                Visitors.Add(visitor);
                Visitors.Refresh();
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể bệnh nhân!");
            }
        }

        public BindableCollection<Session> Sessions { get; set; }

        public void LoadSessions(Visitor visitor)
        {
            if (visitor == null) return;

            _currentVisitor = visitor;          

            var sessions =
                _esClinicContext.Sessions.Where(p => p.PatientId == _currentVisitor.PatientId).ToList();

            Sessions.Clear();
            foreach (var session in sessions)
            {
                if (session.Stage == ClinicStage.OnExamining) session.Stage = ClinicStage.Paused;
                Sessions.Add(session);
            }

            CanNewSession = true;
            _isSessionCreated = false;

            PatientInfo = visitor.Patient.Name + " - " +
                          visitor.Patient.Sex;
        }

        public void ContextMenuLoadSessions()
        {
            LoadSessions(SelectedVisitor);
        }

        private bool _isSessionCreated;

        public Session SelectedSession { get; set; }

        private Session _currentSession;

        private bool _canNewSession;

        public bool CanNewSession
        {
            get { return _canNewSession; }
            set
            {
                _canNewSession = value;
                NotifyOfPropertyChange(() => CanNewSession);
            }
        }

        public void NewSession()
        {
            if (_isSessionCreated)
            {
                if (MessageBox.Show("Đã tồn tại phiên khám mới.\n\nBạn có muốn tiếp tục tạo mới?", "Nhắc nhở", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }
            _isSessionCreated = true;
            _currentSession = new Session() { Date = DateTime.Now, PatientId = _currentVisitor.PatientId, Stage = ClinicStage.OnExamining };
           
            if (_user.IsDoctor)
            {
                _currentSession.DoctorId = _user.UserId;
                _currentSession.DoctorName = _user.Name;
            }

            Sessions.Add(_currentSession);
            _esClinicContext.Sessions.Add(_currentSession);

            _currentVisitor.Stage = ClinicStage.OnExamining;
            _esClinicContext.Entry(_currentVisitor).State = EntityState.Modified;
            _esClinicContext.SaveChanges();

            RefreshVisitors();

            SelectedSession = _currentSession;
            NotifyOfPropertyChange(() => SelectedSession);
            NotifyOfPropertyChange(() => CanAddDrug);
            NotifyOfPropertyChange(() => CanSaveSession);
            NotifyOfPropertyChange(() => CanSaveDrugs);
            NotifyOfPropertyChange(() => CanNewEsRecord);

            ReExamDate = DateTime.Today;
            NotifyOfPropertyChange(() => ReExamDate);

            var serviceTypeList = _esClinicContext.ServiceTypes.ToList();

            Services.Clear();
            foreach (var service in serviceTypeList.Select(serviceType => new Service()
            {
                IsChecked = false,
                SessionId = _currentSession.SessionId,
                ServiceTypeId = serviceType.ServiceTypeId,
                ServiceType = serviceType
            }))
            {
                Services.Add(service);
                _esClinicContext.Services.Add(service);
            }
            _esClinicContext.SaveChanges();
        }

        private string _patientInfo;

        public string PatientInfo
        {
            get { return _patientInfo; }
            set
            {
                _patientInfo = value;
                NotifyOfPropertyChange(() => PatientInfo);
            }
        }

        public void LoadSessionInfo()
        {
            if (SelectedSession == null) return;

            NotifyOfPropertyChange(() => CanSaveSession);
            NotifyOfPropertyChange(() => CanNewEsRecord);
            NotifyOfPropertyChange(() => CanChangeService);
            NotifyOfPropertyChange(() => CanAddDrug);
            NotifyOfPropertyChange(() => CanSaveDrugs);

            Symtoms = SelectedSession.Symtoms;
            Diagnose = SelectedSession.Diagnose;
            if (SelectedSession.ReExamDate != null) ReExamDate = SelectedSession.ReExamDate.Value;
            SessionNote = SelectedSession.Note;

            var esRecords =
                _esClinicContext.EndoscopyRecords.Include("Type").ToList().Where(e => e.SessionId == SelectedSession.SessionId);

            EsRecords.Clear();
            foreach (var esRecord in esRecords)
            {
                EsRecords.Add(esRecord);
            }

            var services =
                _esClinicContext.Services.Include("ServiceType").ToList().Where(s => s.SessionId == SelectedSession.SessionId);

            Services.Clear();
            foreach (var service in services)
            {
                Services.Add(service);
            }

            var drugs =
                _esClinicContext.Drugs.Include("Product").ToList().Where(d => d.SessionId == SelectedSession.SessionId);

            Drugs.Clear();
            foreach (var drug in drugs)
            {
                Drugs.Add(drug);
            }
        }

        public void ContinueSession()
        {
            if (SelectedSession == null) return;
            if (SelectedSession.Stage != ClinicStage.Paused) return;

            _currentSession = SelectedSession;
        }

        public void RemoveVisitor(Visitor visitor)
        {
            if (visitor == null) return;

            _esClinicContext.Visitors.Remove(visitor);
            _esClinicContext.SaveChanges();
            RefreshVisitors();

            _currentVisitor = null;
            Sessions.Clear();
            EsRecords.Clear();
            Services.Clear();
            Drugs.Clear();
        }

        public void ContextMenuRemoveVisitor()
        {
            RemoveVisitor(SelectedVisitor);
        }

        public void NextVisitor(Visitor visitor)
        {
            if (_currentSession == null)
            {
                RemoveVisitor(visitor);               
                return;
            }

            if (!_drugSaved)
            {
                var result = MessageBox.Show("Lưu lại toa thuốc hiện tại?", "Nhắc lưu", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes) SaveDrugs();
                else if (result == MessageBoxResult.Cancel) return;
            }

            _currentSession.TotalCost = 0;
            _currentSession.ServiceCost = 0;
            var services = _esClinicContext.Services.Include("ServiceType").ToList().Where(s => s.SessionId == _currentSession.SessionId);
            foreach (var service in services)
            {
                if (service.IsChecked) _currentSession.ServiceCost += service.ServiceType.Price;
            }

            var surchanges = _esClinicContext.Surcharges.ToList();
            foreach (var surcharge in surchanges)
            {
                TotalCost += surcharge.Price;
            }

            _currentSession.TotalCost = TotalCost + _currentSession.ServiceCost;

            _currentSession.Stage = ClinicStage.WaitToPay;

            var newPayor = new Payor
            {
                SessionId = _currentSession.SessionId,
                Name = _currentVisitor.Patient.Name,
                Session = _currentSession
            };
            _esClinicContext.Payors.Add(newPayor);
            _esClinicContext.Entry(_currentVisitor).State = EntityState.Deleted;
            _esClinicContext.Entry(_currentSession).State = EntityState.Modified;
            _esClinicContext.SaveChanges();

            TotalCost = 0;
            RefreshVisitors();
            _currentVisitor = null;
            _currentSession = null;
            Sessions.Clear();
            EsRecords.Clear();
            Services.Clear();
            Drugs.Clear();
        }

        public void ContextMenuNextVisitor()
        {
            NextVisitor(SelectedVisitor);
        }

        public void ContextMenuEditPatient()
        {
            if (SelectedVisitor == null) return;
            _windowManager.ShowDialog(new PatientViewModel(SelectedVisitor.Patient));
        }

        #endregion Bệnh nhân

        #region Khám Bệnh

        private string _symtoms;

        public string Symtoms
        {
            get { return _symtoms; }
            set
            {
                _symtoms = value;
                NotifyOfPropertyChange(() => Symtoms);
            }
        }

        private string _diagnose;

        public string Diagnose
        {
            get { return _diagnose; }
            set
            {
                _diagnose = value;
                NotifyOfPropertyChange(() => Diagnose);
            }
        }

        public bool IsReExam { get; set; }

        private DateTime _reExamDate;

        public DateTime ReExamDate
        {
            get
            {
                return _reExamDate;
            }
            set
            {
                _reExamDate = value;
                NotifyOfPropertyChange(() => ReExamDate);
            }
        }

        private string _sessionNote;

        public string SessionNote
        {
            get
            {
                return _sessionNote;
            }
            set
            {
                _sessionNote = value;
                NotifyOfPropertyChange(() => SessionNote);
            }
        }

        public bool CanSaveSession
        {
            get
            {
                return _currentSession != null
                       && _currentVisitor.Stage == ClinicStage.OnExamining
                       && SelectedSession == _currentSession;
            }
        }

        public void SaveSession()
        {
            _currentSession.Symtoms = Symtoms;
            _currentSession.Diagnose = Diagnose;
            Debug.Print(Diagnose);
            if (IsReExam)
                _currentSession.ReExamDate = ReExamDate;
            _currentSession.Note = SessionNote;

            _esClinicContext.Entry(_currentSession).State = EntityState.Modified;
            _esClinicContext.SaveChanges();

            Sessions.Refresh();
        }

        #endregion Khám Bệnh

        #region Nội Soi

        public BindableCollection<EndoscopyRecord> EsRecords { get; set; }

        public bool CanNewEsRecord
        {
            get
            {
                return _currentSession != null
                       && _currentVisitor.Stage == ClinicStage.OnExamining
                       && SelectedSession == _currentSession;
            }
        }

        public void NewEsRecord()
        {
            _windowManager.ShowWindow(new EndoscopyViewModel(_events, _currentSession.SessionId));
        }

        public void Handle(EndoscopyRecord esRecord)
        {
            EsRecords.Add(esRecord);
        }

        public void ViewEsRecord(EndoscopyRecord record)
        {
            _windowManager.ShowDialog(new EndoscopyRecordViewModel(record));
        }

        public void PrintEsRecord(EndoscopyRecord record)
        {
            _windowManager.ShowWindow(new EndoscopyResultsViewModel(record, _currentVisitor.Patient, SelectedSession));
        }

        #endregion Nội Soi

        #region Thủ thuật

        public BindableCollection<Service> Services { get; set; }

        public bool CanChangeService
        {
            get
            {
                return _currentSession != null
                       && _currentVisitor.Stage == ClinicStage.OnExamining
                       && SelectedSession == _currentSession;
            }
        }

        #endregion Thủ thuật

        #region Toa thuốc

        #region Property

        public IIntelliboxResultsProvider ProductResultsProvider { get; set; }

        public int SelectedProduct { get; set; }

        private string _drugType;

        public string DrugType
        {
            get { return _drugType; }
            set
            {
                _drugType = value;
                NotifyOfPropertyChange(() => DrugType);
            }
        }

        private int _days;

        public int Days
        {
            get
            {
                return _days;
            }
            set
            {
                _days = value;
                NotifyOfPropertyChange(() => Days);
            }
        }

        private double _morning;

        public double Morning
        {
            get
            {
                return _morning;
            }
            set
            {
                _morning = value;
                NotifyOfPropertyChange(() => Morning);
            }
        }

        private double _noon;

        public double Noon
        {
            get
            {
                return _noon;
            }
            set
            {
                _noon = value;
                NotifyOfPropertyChange(() => Noon);
            }
        }

        private double _afternoon;

        public double Afternoon
        {
            get { return _afternoon; }
            set
            {
                _afternoon = value;
                NotifyOfPropertyChange(() => Afternoon);
            }
        }

        private double _night;

        public double Night
        {
            get { return _night; }
            set
            {
                _night = value;
                NotifyOfPropertyChange(() => Night);
            }
        }

        private double _quantity;

        public double Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                NotifyOfPropertyChange(() => Quantity);
            }
        }

        private string _drugNote;

        public string DrugNote
        {
            get { return _drugNote; }
            set
            {
                _drugNote = value;
                NotifyOfPropertyChange(() => DrugNote);
            }
        }

        private int _price;

        public int Price
        {
            get { return _price; }
            set
            {
                _price = value;
                NotifyOfPropertyChange(() => Price);
            }
        }

        private int _cost;

        public int Cost
        {
            get { return _cost; }
            set
            {
                _cost = value;
                NotifyOfPropertyChange(() => Cost);
            }
        }

        private int _totalCost;

        public int TotalCost
        {
            get { return _totalCost; }
            set
            {
                _totalCost = value;
                NotifyOfPropertyChange(() => TotalCost);
            }
        }

        public BindableCollection<Drug> Drugs { get; set; }

        private bool _drugSaved;

        #endregion Property

        public bool CanAddDrug
        {
            get { return _currentSession != null && Quantity != 0 && SelectedProduct != 0; }
        }

        public void AddDrug(Intellibox control)
        {           
            var product = _esClinicContext.Products.ToList().FirstOrDefault(p => p.ProductId == SelectedProduct);

            Cost = (int)(Quantity*product.Price);

            var drug = new Drug
            {
                Days = Days,
                Morning = Morning,
                Noon = Noon,
                Afternoon = Afternoon,
                Night = Night,
                Quantity = Quantity,
                Note = DrugNote,
                Cost = Cost,
                SessionId = _currentSession.SessionId,
                ProductId = SelectedProduct,
                Product = product
            };

            Drugs.Add(drug);

            Morning = 0;
            Noon = 0;
            Afternoon = 0;
            Night = 0;
            Quantity = 0;
            DrugNote = "";

            _drugSaved = false;
            NotifyOfPropertyChange(() => CanSaveDrugs);
            TotalCost += drug.Cost;

            control.Focus();
        }

        public void DeleteDrug(Drug drug)
        {
            TotalCost -= drug.Cost;
            Drugs.Remove(drug);
        }

        public bool CanSaveDrugs
        {
            get
            {
                return _currentSession != null
                       && _currentVisitor.Stage == ClinicStage.OnExamining
                       && SelectedSession == _currentSession
                       && !_drugSaved;
            }
        }

        public void SaveDrugs()
        {
            SaveSession();
            var drugs = _esClinicContext.Drugs.ToList().Where(d => d.SessionId == _currentSession.SessionId);
            foreach (var drug in drugs)
            {
                _esClinicContext.Entry(drug).State = EntityState.Deleted;
            }
            _esClinicContext.SaveChanges();

            foreach (var drug in Drugs)
            {
                _esClinicContext.Drugs.Add(drug);
            }
            _esClinicContext.SaveChanges();
            _drugSaved = true;
            NotifyOfPropertyChange(() => CanSaveDrugs);
        }

        //Unfin
        public void CopyDrugs()
        {
            if (SelectedSession == null) return;
            if (SelectedSession == _currentSession) return;

            var drugs = _esClinicContext.Drugs.ToList().Where(d => d.SessionId == SelectedSession.SessionId);
            Drugs.Clear();
            foreach (var drug in drugs)
            {
                var newDrug = new Drug();
            }
        }

        public void ProductSelectCompleted()
        {
            if (SelectedProduct == 0) return;

            var product = _esClinicContext.Products.First(p => p.ProductId == SelectedProduct);
            DrugType = product.Type;
            Price = product.Price;
        }

        public void IntelboxEnterPressed(KeyEventArgs keyEvents)
        {
            if (keyEvents.Key == Key.Enter)
            {
                ProductSelectCompleted();
            }
        }

        public void NumerisValueChanged()
        {
            Quantity = Days * (Morning + Noon + Afternoon + Night);

            if (Morning != 0)
                DrugNote = "Sáng uống " + Morning + " " + DrugType + "; ";
            if (Noon != 0)
                DrugNote += "Trưa uống " + Noon + " " + DrugType + "; ";
            if (Afternoon != 0)
                DrugNote += "Chiều uống " + Afternoon + " " + DrugType + "; ";
            if (Night != 0)
                DrugNote += "Tối uống " + Night + " " + DrugType + "; ";

            NotifyOfPropertyChange(() => CanAddDrug);
        }

        public void QuantityValueChanged()
        {
            Cost = (int)(Quantity * Price);
            NotifyOfPropertyChange(() => CanAddDrug);
        }

        #endregion Toa thuốc
    }
}
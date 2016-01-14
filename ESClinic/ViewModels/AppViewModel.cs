using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : Conductor<IScreen>
    {
        private readonly IWindowManager _windowManager;
        private readonly User _user;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;          

            ActivateItem(new HomeViewModel());

            _user = App.Current.User;           
            Username = App.Current.User.Name;

            NotifyOfPropertyChange(() => CanExamine);
            NotifyOfPropertyChange(() => CanPaid);
            NotifyOfPropertyChange(() => CanManage);
        }              

        public string Username { get; set; }

        public bool CanExamine { get { return _user.ExaminationAccessible; } }


        public bool CanPaid { get { return _user.PaymentAccessible; } }


        public bool CanManage { get { return _user.ManagementAccessible; } }

        public void Home()
        {
            ActivateItem(new HomeViewModel());
        }

        public void Examine()
        {
            ActivateItem(new ExamineViewModel(_windowManager));
        }

        public void Paid()
        {
            ActivateItem(new PaymentsViewModel(_windowManager));
        }

        public void Manage()
        {
            ActivateItem(new ManageViewModel());
        }
    }
}

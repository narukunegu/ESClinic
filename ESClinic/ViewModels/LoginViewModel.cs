using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using ESClinic.Framework;

namespace ESClinic.ViewModels
{
    [Export(typeof(LoginViewModel))]
    public class LoginViewModel : Screen
    {
        private readonly IEventAggregator _events;
        private readonly EsClinicEntities _esClinicContext;

        public string Username { get; set; }

        [ImportingConstructor]
        public LoginViewModel(IEventAggregator events)
        {
            _events = events;

            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;

            }
        }

        public void Login(object obj)
        {
            var pwBox = obj as PasswordBox;

            if (Username == "") return;

            var q = (from user in _esClinicContext.Users
                where user.Username == Username
                select user).ToList().First();

            if (q == null) return;
            if (pwBox == null || pwBox.Password != q.Password) return;
            App.Current.User = q;
            _events.PublishOnUIThread(new LoginEvent());
        }

        public void EnterToLogin(object obj, KeyEventArgs keyEvents)
        {
            if (keyEvents.Key == Key.Enter)
            {
                Login(obj);
            }
        }
    }
}

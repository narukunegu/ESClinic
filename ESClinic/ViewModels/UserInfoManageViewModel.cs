using System.Linq;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class UserInfoManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        public BindableCollection<User> Users { get; set; }

        public UserInfoManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            Users = new BindableCollection<User>();

            var users = _esClinicContext.Users.ToList();

            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        public string Name { get; set; }

        public void AddUser()
        {
            var newuser = new User(){Name = Name};
            _esClinicContext.Users.Add(newuser);
            _esClinicContext.SaveChanges();
            Users.Add(newuser);
        }

        public void Save()
        {
            _esClinicContext.SaveChanges();
        }
    }
}
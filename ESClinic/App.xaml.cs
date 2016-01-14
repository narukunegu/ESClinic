using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ESClinic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public EsClinicEntities EsClinicContext { get; private set; }        

        public User User { get; set; }

        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            try
            {
                EsClinicContext = new EsClinicEntities();
                ((IObjectContextAdapter)EsClinicContext).ObjectContext.Connection.Open();

                
                var admin = new User()
                {
                    Name = "Admin",
                    Username = "admin",
                    Password = "1234",
                    ExaminationAccessible = true,
                    ManagementAccessible = true,
                    PaymentAccessible = true
                };

                var users = EsClinicContext.Users.Count();
                if (users == 0)
                {
                    EsClinicContext.Users.Add(admin);
                    EsClinicContext.SaveChanges();
                }            

                EventManager.RegisterClassHandler(typeof(TextBox),
                                       UIElement.GotFocusEvent,
                                       new RoutedEventHandler(TextBox_GotFocus)
                                      );
                base.OnStartup(eventArgs);
            }
            catch (EntityException ex)
            {
                MessageBox.Show(
                    string.Format("{0}\n\n{1}\n\nPlease make sure that you can logon to the SQL Server \"{2}\" or edit the file \"ESClinic.exe.config\".",
                    ex.Message, ex.InnerException != null ? ex.InnerException.Message : null, EsClinicContext.Database.Connection.DataSource),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        public new static App Current
        {
            get { return Application.Current as App; }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null) textBox.SelectAll();
        }
    }
}
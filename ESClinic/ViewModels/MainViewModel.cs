using System.ComponentModel.Composition;
using Caliburn.Micro;
using ESClinic.Framework;

namespace ESClinic.ViewModels
{
    [Export(typeof(MainViewModel))]
    public sealed class MainViewModel : Conductor<IScreen>, IHandle<LoginEvent>
    {
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public MainViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            DisplayName = "ESClinic Lite Beta";

            ActivateItem(new LoginViewModel(events));

            _events = events;
            _windowManager = windowManager;
            _events.Subscribe(this);
        }      

        public void Handle(LoginEvent e)
        {
            ActivateItem(new AppViewModel(_windowManager, _events));
        }
    }
}

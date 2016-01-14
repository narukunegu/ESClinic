namespace ESClinic.Framework
{
    public class HomeEvent
    {
        public const string CanExamine = "CanExamine";
        public const string CanPaid = "CanPaid";
        public const string CanManage = "CanManage";

        public string Message { get; private set; }

        public HomeEvent(string message)
        {
            Message = message;
        }

    }
}

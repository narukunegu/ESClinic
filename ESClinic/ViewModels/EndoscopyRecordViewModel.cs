using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ESClinic.Framework;

namespace ESClinic.ViewModels
{
    public sealed class EndoscopyRecordViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        public BindableCollection<BitmapImage> EsImages { get; set; }
        public string ResultTitle { get; set; }
        public string Result { get; set; }

        public EndoscopyRecordViewModel(EndoscopyRecord esRecord)
        {
            DisplayName = "Xem kết quả nội soi";
            if (App.Current != null)
                _esClinicContext = App.Current.EsClinicContext;

            EsImages = new BindableCollection<BitmapImage>();
            var esImages =
                _esClinicContext.EndoscopyPhotoes.ToList().Where(e => e.EndoscopyRecordId == esRecord.EndoscopyRecordId);
            foreach (var image in esImages)
            {
                EsImages.Add(ImageDataConverter.BytesToBitmapImage(image.Photo));
            }

            var type = _esClinicContext.EndoscopyTypes.ToList()
                .FirstOrDefault(e => e.EndoScopyTypeId == esRecord.EndoscopyTypeId);
            if (type != null)
                ResultTitle = "Mục nội soi: " + type.Name;
            NotifyOfPropertyChange(()=>ResultTitle);

            Result = esRecord.Result;
            NotifyOfPropertyChange(() => Result);
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ESClinic.Framework;
using Touchless.Vision.Camera;
using Touchless.Vision.Contracts;
using Action = System.Action;
using Frame = Touchless.Vision.Contracts.Frame;

namespace ESClinic.ViewModels
{
    public sealed class EndoscopyViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        private readonly IEventAggregator _events;
        private readonly int _sessionId;
        public EndoscopyViewModel(IEventAggregator events, int sessionId)
        {
            DisplayName = "Nội soi";
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }
            _events = events;
            _sessionId = sessionId;
          
            Photos = new BindableCollection<TmpPhoto>();

            IsEnabledScopy = true;
            NotifyOfPropertyChange(() => IsEnabledScopy);

            EsTypes = new BindableCollection<EndoscopyType>();
            var esTypes = _esClinicContext.EndoscopyTypes.ToList();
            
            foreach (var esType in esTypes)
            {
                EsTypes.Add(esType);
            }

            Cameras = new BindableCollection<Camera>();
            foreach (var camera in CameraService.AvailableCameras)
            {
                Cameras.Add(camera);
            }

            _selectedCamera = Cameras.FirstOrDefault();
        }

        public Image ImgVideo;
       
        public BindableCollection<Camera> Cameras { get; set; }
        private Camera _selectedCamera;

        public void SelectCamera(Camera camera)
        {
            ThrashOldCamera();
            _selectedCamera = camera;
            CameraHelper(_selectedCamera, 720, 480, 50);
            StartPreview();
        }

        public void LoadControl(Image imgVideo)
        {
            ImgVideo = imgVideo;
                      
            CameraHelper(_selectedCamera, 720, 480, 50);
            StartPreview();
        }

        public void StartPreview()
        {
            if (_frameSource == null) return;
            _frameSource.StartFrameCapture();
        }

        public void StopPreview()
        {
            ThrashOldCamera();
        }

        public void VideoSetting()
        {
            _frameSource.Camera.ShowPropertiesDialog();
        }

        public void ThrashOldCamera()
        {
            if (_frameSource == null) return;
            _frameSource.NewFrame -= OnImageCaptured;
            _frameSource.Camera.Dispose();
            _frameSource = null;
        }

        public bool IsEnabledScopy { get; set; }
        public bool IsSelectedResult { get; set; }
        public void TabControl()
        {
            if (!IsSelectedResult) return;

            ThrashOldCamera();
            IsEnabledScopy = false;
            NotifyOfPropertyChange(() => IsEnabledScopy);
        }

        public BindableCollection<TmpPhoto> Photos { get; set; }  
        public void Capture()
        {
            
            Photos.Add(new TmpPhoto(){IsChecked = false, Photo = (BitmapSource)ImgVideo.Source});
        }

        public void SelectAllPhotos()
        {
            foreach (var tmpPhoto in Photos)
            {
                tmpPhoto.IsChecked = true;
            }    
        }

        public void SelectPhotos()
        {
            var list = Photos.ToList().Where(p => p.IsChecked == false);
            foreach (var photo in list)
            {
                Photos.Remove(photo);
            }
        }

        public BindableCollection<EndoscopyType> EsTypes { get; set; }
        public EndoscopyType SelectedEsType { get; set; }
        
        private string _esTypePattern;
        public string EsTypePattern
        {
            get
            {
                return _esTypePattern;
            }
            set
            {
                _esTypePattern = value;
                NotifyOfPropertyChange(() => EsTypePattern);
            }
        }

        private string _esResult;
        public void EsTypeChanged()
        {
            EsTypePattern = SelectedEsType.Pattern;
        }

        public void SaveEsResult()
        {
            _esResult = EsTypePattern;
        }

        public void Finished()
        {
            try
            {               
                SaveEsResult();
                var esRecord = new EndoscopyRecord()
                {
                    EndoscopyTypeId = SelectedEsType.EndoScopyTypeId,
                    SessionId = _sessionId,
                    Result = _esResult
                };                
                _esClinicContext.EndoscopyRecords.Add(esRecord);
                _esClinicContext.SaveChanges();

                SelectPhotos();
                if (Photos.Count > 6)
                {
                    MessageBox.Show("Số hình nội soi nhiều hơn 6 tấm!");
                    return;
                }
                foreach (var esPhoto in Photos.Select(photo => new EndoscopyPhoto()
                {
                    EndoscopyRecordId = esRecord.EndoscopyRecordId,
                    Photo = ImageDataConverter.BitmapSourceToBytes(photo.Photo)
                }))
                {
                    _esClinicContext.EndoscopyPhotoes.Add(esPhoto);
                }
                _esClinicContext.SaveChanges();

                _events.Publish(esRecord, action => { Task.Factory.StartNew(action); });                              
            }
            catch (Exception)
            {
                MessageBox.Show("Dữ liệu không đầy đủ!\n\nVui lòng kiểm lại thông tin mục nội soi.");
            }
            finally
            {
                TryClose();                      
            }
        }

#region CameraHelper

        private CameraFrameSource _frameSource;
       
        public void CameraHelper(Camera camera, int width, int height, int fps)
        {
            try
            {
                _frameSource = new CameraFrameSource(camera)
                {
                    Camera =
                    {
                        CaptureWidth = width,
                        CaptureHeight = height,
                        Fps = fps
                    }
                };
                _frameSource.NewFrame += OnImageCaptured;                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }       

        public void OnImageCaptured(IFrameSource frameSource,
            Frame frame, double fps)
        {          
            ImgVideo.Dispatcher.BeginInvoke(
                (Action) (() => ImgVideo.Source = ImageDataConverter.BitmapToBitmapSource(frame.Image)));
        }               
              
#endregion
        
    }
}
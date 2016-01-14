using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps.Packaging;
using Caliburn.Micro;
using CodeReason.Reports;
using ESClinic.Framework;

namespace ESClinic.ViewModels
{
    public sealed class EndoscopyResultsViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        private readonly Patient _patient;
        private readonly Session _session;
        private readonly EndoscopyRecord _esRecord;
        private readonly ClinicInfo _clinic;
        private int _count;
        private readonly int _nPhoto;

        public bool IsBusyHidden { get; set; }

        public EndoscopyResultsViewModel(EndoscopyRecord esRecord, Patient patient, Session session)
        {
            DisplayName = "Kết quả nội soi";
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }
            _esRecord = esRecord;
            _patient = patient;
            _session = session;

            _esClinicContext.Entry(_esRecord).Collection("Photos").Load();
            _nPhoto = _esRecord.EndoscopyPhotoes.Count;
            
            IsBusyHidden = true;
            NotifyOfPropertyChange(() => IsBusyHidden);

            
            _clinic = _esClinicContext.ClinicInfoes.FirstOrDefault();
        }

        public void DocViewerLoaded(DocumentViewer documentViewer)
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.ImageProcessing += reportDocument_ImageProcessing;
                reportDocument.ImageError += reportDocument_ImageError;

                StreamReader reader =
                    new StreamReader(new FileStream(@"Templates\EndoscopyResults.xaml", FileMode.Open, FileAccess.Read));
                reportDocument.XamlData = reader.ReadToEnd();
                reportDocument.XamlImagePath = Path.Combine(Environment.CurrentDirectory, @"Templates\");
                reader.Close();

                ReportData data = new ReportData();

                //Clinic info
                if (_clinic != null)
                {
                    data.ReportDocumentValues.Add("ClinicName", _clinic.Name.ToUpper());
                    data.ReportDocumentValues.Add("ClinicAddress", _clinic.Address);
                    data.ReportDocumentValues.Add("ClinicPhone", _clinic.Phone);
                }

                //Patient info
                data.ReportDocumentValues.Add("PatientName", _patient.Name.ToUpper());
                data.ReportDocumentValues.Add("BirthDay", "Ngày sinh:  " + _patient.Birthday);
                data.ReportDocumentValues.Add("Address", _patient.Address);
                data.ReportDocumentValues.Add("Sex", "Giới tính:  " + _patient.Sex);
                data.ReportDocumentValues.Add("Diagnose", _session.Diagnose);
                data.ReportDocumentValues.Add("Type", _esRecord.EndoscopyType.Name);

                //Endoscopy results
                data.ReportDocumentValues.Add("Result", _esRecord.Result);

                //Note
                string tmp = "Ngày " + string.Format("{0:dd}", _session.Date) + " tháng " +
                             string.Format("{0:MM}", _session.Date)
                             + " năm " + String.Format("{0:yyyy}", _session.Date);
                data.ReportDocumentValues.Add("Date", tmp);
                data.ReportDocumentValues.Add("DoctorName", _session.DoctorName);

                XpsDocument xps = reportDocument.CreateXpsDocument(data);
                documentViewer.Document = xps.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsBusyHidden = true;
                NotifyOfPropertyChange(() => IsBusyHidden);
            }
        }

        private void reportDocument_ImageError(object sender, ImageErrorEventArgs e)
        {
            e.Handled = true;
        }

        private void reportDocument_ImageProcessing(object sender, ImageEventArgs e)
        {
            if (e.Image.Name == "ClinicLogo")
            {
                if (_clinic.Logo != null) e.Image.Source = ImageDataConverter.BytesToBitmapImage(_clinic.Logo);
            }
            else
            {               
                if (_count < _nPhoto)
                {
                    e.Image.Source = ImageDataConverter.BytesToBitmapImage(_esRecord.EndoscopyPhotoes.ElementAt(_count).Photo);
                }
                else
                {
                    e.Image.Visibility = Visibility.Collapsed;
                }
                _count++;
            }
        }
    }
}
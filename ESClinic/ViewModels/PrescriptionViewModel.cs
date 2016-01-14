using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Xps.Packaging;
using CodeReason.Reports;
using ESClinic.Framework;
using Screen = Caliburn.Micro.Screen;

namespace ESClinic.ViewModels
{
    public sealed class PrescriptionViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;
        private readonly Session _session;
        private readonly ClinicInfo _clinic;
        private readonly Patient _patient;

        public bool IsBusyHidden { get; set; }

        public PrescriptionViewModel(Session session)
        {
            DisplayName = "Toa thuốc";
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }
            _session = session;
            IsBusyHidden = true;
            NotifyOfPropertyChange(() => IsBusyHidden);

            _clinic = _esClinicContext.ClinicInfoes.FirstOrDefault();
            _patient = _esClinicContext.Patients.Find(session.PatientId);
        }

        public void DocViewerLoaded(DocumentViewer documentViewer)
        {
            try
            {
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.ImageProcessing += reportDocument_ImageProcessing;
                reportDocument.ImageError += reportDocument_ImageError;

                StreamReader reader =
                    new StreamReader(new FileStream(@"Templates\Prescription.xaml", FileMode.Open, FileAccess.Read));
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

                //Prescription
                DataTable table = new DataTable("Prescription");
                table.Columns.Add("Count", typeof (int));
                table.Columns.Add("BrandName", typeof (string));
                table.Columns.Add("Quantity");
                table.Columns.Add("Type", typeof (string));
                table.Columns.Add("DrugNote", typeof (string));

                var drugs =
                    _esClinicContext.Drugs.Include("Product").ToList().Where(d => d.SessionId == _session.SessionId);
                int count = 0;
                foreach (var drug in drugs)
                {
                    count++;
                    table.Rows.Add(count, drug.Product.BrandName, drug.Quantity, drug.Product.Type, drug.Note);
                }
                data.DataTables.Add(table);

                //Note
                data.ReportDocumentValues.Add("Note", _session.Note);
                data.ReportDocumentValues.Add("ReExamDate", string.Format("{0:dd/MM/yyyy}", _session.ReExamDate));
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

        void reportDocument_ImageError(object sender, ImageErrorEventArgs e)
        {
            e.Handled = true;
        }

        void reportDocument_ImageProcessing(object sender, ImageEventArgs e)
        {         
            if (_clinic.Logo != null)
            e.Image.Source = ImageDataConverter.BytesToBitmapImage(_clinic.Logo);
        }
    }
}
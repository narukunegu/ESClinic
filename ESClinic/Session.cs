//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ESClinic
{
    using System;
    using System.Collections.Generic;
    
    public partial class Session
    {
        public Session()
        {
            this.Drugs = new HashSet<Drug>();
            this.EndoscopyRecords = new HashSet<EndoscopyRecord>();
            this.Services = new HashSet<Service>();
        }
    
        public int SessionId { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Symtoms { get; set; }
        public string Diagnose { get; set; }
        public string Note { get; set; }
        public Nullable<System.DateTime> ReExamDate { get; set; }
        public Nullable<int> DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Stage { get; set; }
        public string DrugNote { get; set; }
        public Nullable<int> DrugCost { get; set; }
        public Nullable<int> ServiceCost { get; set; }
        public Nullable<int> TotalCost { get; set; }
        public Nullable<int> Fare { get; set; }
        public Nullable<int> Change { get; set; }
        public int PatientId { get; set; }
    
        public virtual ICollection<Drug> Drugs { get; set; }
        public virtual ICollection<EndoscopyRecord> EndoscopyRecords { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Payor Payor { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}

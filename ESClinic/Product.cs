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
    
    public partial class Product
    {
        public Product()
        {
            this.Drugs = new HashSet<Drug>();
        }
    
        public int ProductId { get; set; }
        public string BrandName { get; set; }
        public string GenericDrug { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public System.DateTime Exp { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    
        public virtual ICollection<Drug> Drugs { get; set; }
    }
}
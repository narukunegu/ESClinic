using System;
using System.Linq;
using Caliburn.Micro;

namespace ESClinic.ViewModels
{
    public class ProductManageViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;

        public BindableCollection<Product> Products { get; set; }

        public ProductManageViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            Products = new BindableCollection<Product>();

            var products = _esClinicContext.Products.ToList();

            foreach (var product in products)
            {
                Products.Add(product);
            }

            Exp = DateTime.Now;
            BrandName = "";
            GenericDrug = "";
        }

        public string BrandName { get; set; }
        public string GenericDrug { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }

        private DateTime _exp;

        public DateTime Exp
        {
            get
            {
                return _exp;
            }
            set
            {
                _exp = value;
                NotifyOfPropertyChange(() => Exp);
            }
        }

        public int Quantity { get; set; }
        public int Price { get; set; }

        public void AddProduct()
        {
            var product = new Product()
            {
                BrandName = BrandName,
                GenericDrug = GenericDrug,
                Quantity = Quantity,
                Price = Price,
                Type = Type,
                Country = Country,
                Exp = Exp
            };
           
            _esClinicContext.Products.Add(product);
            _esClinicContext.SaveChanges();
            Products.Add(product);
        }

        public void Save()
        {
            _esClinicContext.SaveChanges();
        }
    }
}
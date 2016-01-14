using System;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using ESClinic.Framework;
using FeserWard.Controls;

namespace ESClinic.ViewModels
{
    public class ImportProductsViewModel : Screen
    {
        private readonly EsClinicEntities _esClinicContext;

        public BindableCollection<Product> ImportProducts { get; set; }

        public IIntelliboxResultsProvider ProductResultsProvider { get; set; }

        public ImportProductsViewModel()
        {
            if (App.Current != null)
            {
                _esClinicContext = App.Current.EsClinicContext;
            }

            ImportProducts = new BindableCollection<Product>();
           
            Exp = DateTime.Today;

            ProductResultsProvider = new ProductResultsProvider(_esClinicContext);
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

        private int _price;

        public int Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                NotifyOfPropertyChange(() => Price);
            }
        }

        public int SelectedProductId { get; set; }
        private Product _selectedProduct;        

        public void ProductSelectCompleted()
        {            
            _selectedProduct = _esClinicContext.Products.First(p => p.ProductId == SelectedProductId);
            
            Exp = _selectedProduct.Exp;
            Price = _selectedProduct.Price;
        }

        public void IntelboxEnterPressed(KeyEventArgs keyEvents)
        {
            if (keyEvents.Key == Key.Enter || keyEvents.Key == Key.Tab)
            {
                ProductSelectCompleted();
            }
        }

        public void AddDrug(Intellibox control)
        {            
            if (SelectedProductId == 0) return;
            ImportProducts.Add(new Product()
            {
                ProductId = _selectedProduct.ProductId,
                BrandName = _selectedProduct.BrandName,
                Quantity = Quantity,
                Country = _selectedProduct.Country,
                Exp = Exp,
                GenericDrug = _selectedProduct.GenericDrug,
                Price = Price,
                Type = _selectedProduct.Type
            });
            control.Focus();
        }

        public void SaveDrugs()
        {
            foreach (var importProduct in ImportProducts)
            {             
                var p = _esClinicContext.Products.First(pp => pp.ProductId == importProduct.ProductId);
                p.Quantity += importProduct.Quantity;
                p.Price = Price;
                p.Exp = Exp;
            }
            _esClinicContext.SaveChanges();
            Stage = "Đã lưu";
        }

        public Product SelectedImportProduct { get; set; }

        public void DeleteImportDrug()
        {
            if (SelectedImportProduct == null) return;
            ImportProducts.Remove(SelectedImportProduct);
        }

        private string _stage;

        public string Stage
        {
            get
            {
                return _stage;
            }
            set
            {
                _stage = value;
                NotifyOfPropertyChange(() => Stage);
            }
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using FeserWard.Controls;

namespace ESClinic.Framework
{
    public class ProductResultsProvider : IIntelliboxResultsProvider
    {
        private readonly EsClinicEntities _esClinicContext;

        public ProductResultsProvider(EsClinicEntities esClinicContext)
        {
            _esClinicContext = esClinicContext;
        }

        public IEnumerable DoSearch(string searchTerm, int maxResults, object extraInfo)
        {
            var products = _esClinicContext.Products.ToList()
                .Where(
                    p =>
                        p.BrandName.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.GenericDrug.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

            return products;
        }
    }
}
using System;
using System.Collections;
using System.Linq;
using FeserWard.Controls;

namespace ESClinic.Framework
{
    public class PatientResultsProvider : IIntelliboxResultsProvider
    {
        private readonly EsClinicEntities _esClinicContext;

        public PatientResultsProvider(EsClinicEntities esClinicContext)
        {        
            _esClinicContext = esClinicContext;
        }

        public IEnumerable DoSearch(string searchTerm, int maxResults, object extraInfo)
        {
            var patients = _esClinicContext.Patients.ToList()
                .Where(p => p.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

            return patients;
        }
    }
}
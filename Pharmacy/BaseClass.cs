using Pharmacy.Models;

namespace Pharmacy
{
    public static class BaseClass
    {
        private static dbPharmaEntities _pharmaEntities;
        public static dbPharmaEntities dbEntities
        {
            get { return _pharmaEntities ?? (_pharmaEntities = new dbPharmaEntities()); }
        }
    }
}
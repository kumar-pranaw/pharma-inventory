using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class SalesLedgerViewModel
    {
        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public double? BalanceAmount { get; set; }
        public double? CreditAmount { get; set; }
        public double? DebitAmunt { get; set; }
        public string Particulars { get; set; }
        public string InvoiceId { get; set; }
        public int Invoice { get; set; }
        public int PaymentModeType { get; set; }
        public DateTime? Date { get; set; }
    }
}
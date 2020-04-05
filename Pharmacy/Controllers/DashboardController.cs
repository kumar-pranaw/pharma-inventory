using Pharmacy.Models;
using Pharmacy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Pharmacy.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult AddSupplier()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddSupplier(SupplierViewModel supplierViewModel)
        {
            Supplier addNewSupplier = new Supplier()
            {
                SupplierName = supplierViewModel.SupplierName,
                SupplierAddress = supplierViewModel.SupplierAddress,
                SupplierPhoneNo = supplierViewModel.SupplierPhoneNo,
                SupplierCINNumber = supplierViewModel.SupplierCINNumber,
                SupplierDLNumber = supplierViewModel.SupplierDLNumber,
                SupplierEmail = supplierViewModel.SupplierEmail,
                SupplierGSTNumber = supplierViewModel.SupplierGSTNumber,
                SupplierPanNumber = supplierViewModel.SupplierPanNumber
            };

            BaseClass.dbEntities.Suppliers.Add(addNewSupplier);
            BaseClass.dbEntities.SaveChanges();
            return RedirectToAction("ViewSuppliers");
        }

        public ActionResult ViewSuppliers()
        {
            var suppliersList = (from supplierlist in BaseClass.dbEntities.Suppliers
                                 select new SupplierViewModel
                                 {
                                     SupplierAddress = supplierlist.SupplierAddress,
                                     SupplierCINNumber = supplierlist.SupplierCINNumber,
                                     SupplierDLNumber = supplierlist.SupplierDLNumber,
                                     SupplierEmail = supplierlist.SupplierEmail,
                                     SupplierGSTNumber = supplierlist.SupplierGSTNumber,
                                     SupplierPanNumber = supplierlist.SupplierPanNumber,
                                     SupplierName = supplierlist.SupplierName,
                                     SupplierPhoneNo = supplierlist.SupplierPhoneNo,
                                     id = supplierlist.id
                                 }).ToList();
            return View(suppliersList);
        }

        public ActionResult AddProductPurchase(int? id)
        {
            var getSupplier = BaseClass.dbEntities.Suppliers.Where(x => x.id == id).Select(x => x.SupplierName).SingleOrDefault();
            ViewData["SupplierName"] = getSupplier;
            TempData["ID"] = id;
            return View();
        }

        [HttpPost]
        public JsonResult AddProductPurchase(List<AddProductViewModel> products)
        {
            long sumOfAmounts = 0;
            int supplierId = Convert.ToInt32(TempData["ID"]);
            if (products.Count > 1)
            {
                sumOfAmounts = products.Select(m => m.Amount).Sum();
            }
            else
            {
                sumOfAmounts = products.Select(m => m.Amount).FirstOrDefault();
            }

            var gstNumber = products.Select(m => m.GST).FirstOrDefault();
            var gstAmounts = (gstNumber / 100f) * sumOfAmounts;
            var sumofFinalAmounts = gstAmounts + sumOfAmounts;

            PurchaseInvoice invoice = new PurchaseInvoice
            {
                DateOfPurchase = DateTime.Now,
                supplierId = supplierId,
                TotalPurchaseAmount = Convert.ToDecimal(sumofFinalAmounts),
            };

            BaseClass.dbEntities.PurchaseInvoices.Add(invoice);
            BaseClass.dbEntities.SaveChanges();
            int invoiceId = invoice.ID;

            var getInvoice = BaseClass.dbEntities.PurchaseInvoices.Where(m => m.ID == invoiceId && m.supplierId == supplierId).FirstOrDefault();
            var getLedgerBySupplier = BaseClass.dbEntities.PurchaseLedgers.Where(m => m.SupplierId == supplierId).ToList();
            if (getLedgerBySupplier.Count == 0)
            {
                PurchaseLedger ledger = new PurchaseLedger
                {
                    Date = DateTime.Now,
                    SupplierId = supplierId,
                    CreditAmount = 0,
                    DebitAmunt = Convert.ToInt32(sumofFinalAmounts),
                    BalanceAmount = Convert.ToInt32(sumofFinalAmounts),
                    InvoiceId = invoiceId,
                    Particulars = "To Sales Invoice Number " + getInvoice.InvoiceId
                };
                BaseClass.dbEntities.PurchaseLedgers.Add(ledger);
                BaseClass.dbEntities.SaveChanges();
            }
            else
            {
                var getLastInvoiceAmount = BaseClass.dbEntities.PurchaseLedgers.Where(m => m.SupplierId == supplierId).OrderByDescending(x => x.Id).FirstOrDefault();

                PurchaseLedger ledger = new PurchaseLedger
                {
                    Date = DateTime.Now,
                    SupplierId = supplierId,
                    CreditAmount = 0,
                    DebitAmunt = Convert.ToInt32(sumofFinalAmounts),
                    BalanceAmount = getLastInvoiceAmount.BalanceAmount + Convert.ToInt32(sumofFinalAmounts),
                    InvoiceId = invoiceId,
                    Particulars = "To Sales Invoice Number " + getInvoice.InvoiceId
                };
                BaseClass.dbEntities.PurchaseLedgers.Add(ledger);
                BaseClass.dbEntities.SaveChanges();
            }




            if (products != null)
            {
                foreach (var item in products)
                {
                    Product product1 = new Product
                    {
                        ProductName = item.ProductName,
                        BatchNumber = item.BatchNumber,
                        ExpiryDate = item.ExpiryDate,
                        HSNNumber = item.HSNNumber,
                        CostPrice = item.CP,
                        SellingPrice = item.SP
                    };
                    BaseClass.dbEntities.Products.Add(product1);
                    BaseClass.dbEntities.SaveChanges();
                    int id = product1.Id;
                    purchase purchase1 = new purchase
                    {
                        InvoiceId = invoiceId,
                        DateOfPurchase = DateTime.UtcNow,
                        price = item.Amount.ToString(),
                        Pack = item.Pack,
                        Quantity = item.Quantity.ToString(),
                        GstPercent = item.GST.ToString(),
                        productId = id,
                        SupplierId = supplierId
                    };
                    BaseClass.dbEntities.purchases.Add(purchase1);
                    BaseClass.dbEntities.SaveChanges();
                }
            }

            return Json("Successfully Inserted");
        }

        public ActionResult ViewSuppliersDetails()
        {
            return View();
        }

        public ActionResult ViewAllProducts()
        {
            var listProducts = (from listProduct in BaseClass.dbEntities.Products
                                select new ListProductViewModel
                                {
                                    ProductName = listProduct.ProductName,
                                    HSNNumber = listProduct.HSNNumber,
                                    BatchNumber = listProduct.BatchNumber,
                                    CostPrice = listProduct.CostPrice,
                                    ExpiryDate = listProduct.ExpiryDate,
                                    SellingPrice = listProduct.SellingPrice
                                });

            return View(listProducts);
        }

        public ActionResult ManageSupplierLedger(int? id)
        {
            var invoiceList = (from list in BaseClass.dbEntities.PurchaseInvoices
                               where list.supplierId == id
                               select new InvoiceList
                               {
                                   Id = list.ID,
                                   InvoiceId = list.InvoiceId
                               }).ToList();
            ViewBag.listInvoice = new SelectList(invoiceList, "id", "InvoiceId");

            var paymentModes = (from payment in BaseClass.dbEntities.Payment_Mode
                                select new PaymentMode
                                {
                                    Id = payment.Id,
                                    PaymentModeType = payment.PaymentMode
                                }).ToList();
            ViewBag.paymentMode = new SelectList(paymentModes, "Id", "PaymentModeType");

            var getSupplier = BaseClass.dbEntities.Suppliers.Where(x => x.id == id).Select(x => x.SupplierName).SingleOrDefault();
            ViewData["SupplierName"] = getSupplier;
            TempData["ID"] = id;
            return View();
        }

        [HttpPost]
        public ActionResult ManageSupplierLedger(SupplierLedgerViewModel ledgerViewModel)
        {
            int supplierId = Convert.ToInt32(TempData["ID"]);
            var invoiceList = (from list in BaseClass.dbEntities.PurchaseInvoices
                               where list.supplierId == supplierId
                               select new InvoiceList
                               {
                                   Id = list.ID,
                                   InvoiceId = list.InvoiceId
                               }).ToList();

            ViewBag.listInvoice = new SelectList(invoiceList, "id", "InvoiceId");

            var paymentModes = (from payment in BaseClass.dbEntities.Payment_Mode
                                select new PaymentMode
                                {
                                    Id = payment.Id,
                                    PaymentModeType = payment.PaymentMode
                                }).ToList();
            ViewBag.paymentMode = new SelectList(paymentModes, "Id", "PaymentMode");
            var paymentMode = BaseClass.dbEntities.Payment_Mode.Where(x => x.Id == ledgerViewModel.PaymentModeType).FirstOrDefault();

            var invoiceId = Convert.ToInt32(ledgerViewModel.InvoiceId);
            var invoice = BaseClass.dbEntities.PurchaseInvoices.Where(m => m.ID == invoiceId).FirstOrDefault();

            var checkLedger = BaseClass.dbEntities.PurchaseLedgers.Where(m => m.SupplierId == supplierId).ToList();
            var getLastInvoiceAmount = BaseClass.dbEntities.PurchaseLedgers.Where(m => m.SupplierId == supplierId).OrderByDescending(x => x.Id).FirstOrDefault();
            ledgerViewModel.Particulars = "By " + paymentMode.PaymentMode + " For The Invoice " + invoice.InvoiceId;
            ledgerViewModel.BalanceAmount = getLastInvoiceAmount.BalanceAmount - ledgerViewModel.CreditAmount;


            PurchaseLedger ledger = new PurchaseLedger()
            {
                Date = DateTime.Now,
                SupplierId = supplierId,
                CreditAmount = ledgerViewModel.CreditAmount,
                DebitAmunt = ledgerViewModel.DebitAmunt,
                Particulars = ledgerViewModel.Particulars,
                BalanceAmount = ledgerViewModel.BalanceAmount,
                InvoiceId = Convert.ToInt32(ledgerViewModel.InvoiceId)
            };
            BaseClass.dbEntities.PurchaseLedgers.Add(ledger);
            BaseClass.dbEntities.SaveChanges();

            return RedirectToAction("ViewSuppliers");
        }

        public ActionResult ViewLedgerBySupplierId(int id, DateTime? startDate, DateTime? endDate)
        {
            TempData["supplierId"] = id;

            if (startDate != null && endDate != null)
            {
                var getLedgerByDateRange = BaseClass.dbEntities.getLedgerByDateRange(startDate, endDate, id).ToList();
                return View(getLedgerByDateRange);
            }
            else
            {
                var getLedgerDetails = BaseClass.dbEntities.getLedgerByDateRange(startDate, endDate, id).ToList();
                return View(getLedgerDetails);
            }


        }

        public ActionResult GetPurchaseBySupplierId(int id)
        {
            var allPurchasesFromSupplier = (from invoices in BaseClass.dbEntities.PurchaseInvoices
                                            where invoices.supplierId == id
                                            select new ViewPurchases
                                            {
                                                Id = invoices.ID,
                                                DateOfPurchase = invoices.DateOfPurchase,
                                                PurchaseAmount = invoices.TotalPurchaseAmount,
                                                InvoiceId = invoices.InvoiceId,
                                                SupplierId = invoices.supplierId
                                            }).ToList();

            return View(allPurchasesFromSupplier);
        }

        public ActionResult GetInvoiceDetailsBySupplierId(int? invoiceId, int? supplierId)
        {
            var getInvoiceDetails = BaseClass.dbEntities.PurchaseInvoices.Where(m => m.ID == invoiceId).SingleOrDefault();
            var supplierDetails = BaseClass.dbEntities.Suppliers.Where(m => m.id == supplierId).SingleOrDefault();


            TempData["SupplierName"] = supplierDetails.SupplierName;
            TempData["Phonenumber"] = supplierDetails.SupplierPhoneNo;
            TempData["Address"] = supplierDetails.SupplierAddress;
            TempData["DlNumber"] = supplierDetails.SupplierDLNumber;
            TempData["GSTIN"] = supplierDetails.SupplierGSTNumber;


            TempData["InvoiceId"] = getInvoiceDetails.InvoiceId;
            TempData["TotalAmount"] = getInvoiceDetails.TotalPurchaseAmount;
            TempData["DateOfPurchase"] = getInvoiceDetails.DateOfPurchase;

            var getInvoiceBySupplierIdAndInvoiceId = (from invoice in BaseClass.dbEntities.PurchaseInvoices
                                                      join purchase in BaseClass.dbEntities.purchases on invoice.ID equals purchase.InvoiceId
                                                      join product in BaseClass.dbEntities.Products on purchase.productId equals product.Id
                                                      where invoice.supplierId == supplierId && invoice.ID == invoiceId
                                                      select new ListOfInvoice
                                                      {
                                                          ProductName = product.ProductName,
                                                          GstPercent = purchase.GstPercent,
                                                          Costprice = product.CostPrice,
                                                          SellingPrice = product.SellingPrice,
                                                          ExpiryDate = product.ExpiryDate,
                                                          HSNNumber = product.HSNNumber,
                                                          Quantity = purchase.Quantity,
                                                          Pack = purchase.Pack,
                                                          Batch = product.BatchNumber,
                                                          TotalAmount = purchase.price
                                                      }).ToList();
            return View(getInvoiceBySupplierIdAndInvoiceId);
        }

        public ActionResult EditSupplier(int? id)
        {
            return View();
        }

        public ActionResult AddPaymentMethod()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPaymentMethod(PaymentMode payment)
        {
            Payment_Mode mode = new Payment_Mode
            {
                PaymentMode = payment.PaymentModeType
            };

            BaseClass.dbEntities.Payment_Mode.Add(mode);
            BaseClass.dbEntities.SaveChanges();
            return RedirectToAction("ViewSuppliers");
        }

        public ActionResult ViewAllPaymentMethods()
        {
            return View();
        }

        public ActionResult AddDistributor()
        {
            return View();
        }
    }
}
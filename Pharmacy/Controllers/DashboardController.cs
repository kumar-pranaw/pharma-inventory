using Pharmacy.Models;
using Pharmacy.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Pharmacy.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Home()
        {
            var totalSupplier = BaseClass.dbEntities.Suppliers.Count();
            var totalCustomer = BaseClass.dbEntities.Customers.Count();
            var totalSales = BaseClass.dbEntities.Sales.Count();
            var totalPurchases = BaseClass.dbEntities.purchases.Count();

            TempData["totalSupplier"] = totalSupplier;
            TempData["totalCustomer"] = totalCustomer;
            TempData["totalSales"] = totalSales;
            TempData["totalPurchases"] = totalPurchases;

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

        public ActionResult EditSupplier(int? id)
        {
            var getSupplier = BaseClass.dbEntities.Suppliers.Where(m => m.id == id).FirstOrDefault();
            return View(getSupplier);
        }

        [HttpPost]
        public ActionResult EditSupplier(int? id, Supplier supplier)
        {
            var getDetails = BaseClass.dbEntities.Suppliers.Where(m => m.id == id).SingleOrDefault();
            if (getDetails != null)
            {
                getDetails.SupplierName = supplier.SupplierName;
                getDetails.SupplierPanNumber = supplier.SupplierPanNumber;
                getDetails.SupplierPhoneNo = supplier.SupplierPhoneNo;
                getDetails.SupplierGSTNumber = supplier.SupplierGSTNumber;
                getDetails.SupplierEmail = supplier.SupplierEmail;
                getDetails.SupplierCINNumber = supplier.SupplierCINNumber;
                getDetails.SupplierDLNumber = supplier.SupplierDLNumber;
                getDetails.SupplierAddress = supplier.SupplierAddress;

                BaseClass.dbEntities.Entry(getDetails).State = EntityState.Modified;
                BaseClass.dbEntities.SaveChanges();
            }

            return RedirectToAction("ViewSuppliers");
        }

        public ActionResult DeleteSupplier(int? id)
        {
            var getSupplier = BaseClass.dbEntities.Suppliers.Find(id);
            if (getSupplier != null)
            {
                BaseClass.dbEntities.Suppliers.Remove(getSupplier);
                BaseClass.dbEntities.SaveChanges();
            }
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
        public JsonResult AddProductPurchase(string invoiceNumber, string invoiceDate, List<AddProductViewModel> products)
        {
            double? sumOfAmounts = 0;
            double? allgstAmounts = 0;

            double? totalIgstAmounts = 0;
            double? totalCgstAmounts = 0;
            double? totalUtgstAmounts = 0;

            int supplierId = Convert.ToInt32(TempData["ID"]);
            if (products.Count > 1)
            {
                sumOfAmounts = products.Select(m => Math.Round(m.Amount, 3)).Sum();
            }
            else
            {
                sumOfAmounts = products.Select(m => Math.Round(m.Amount, 3)).FirstOrDefault();
            }



            foreach (var item in products)
            {
                if (item.CGST != 0)
                {
                    allgstAmounts = (item.CGST / 100f) * item.Amount;
                    totalCgstAmounts = allgstAmounts + totalCgstAmounts;
                }
                if (item.UTGST != 0)
                {
                    allgstAmounts = (item.UTGST / 100f) * item.Amount;
                    totalUtgstAmounts = allgstAmounts + totalUtgstAmounts;
                }
                if (item.IGST != 0)
                {
                    allgstAmounts = (item.IGST / 100f) * item.Amount;
                    totalIgstAmounts = allgstAmounts + totalIgstAmounts;
                }

            }

            var sumOfAllGstAmounts = totalCgstAmounts + totalIgstAmounts + totalUtgstAmounts;
            var sumofFinalAmounts = sumOfAllGstAmounts + sumOfAmounts;

            string s = invoiceDate.Substring(3, 2) + "/" + invoiceDate.Substring(0, 2) + "/" + invoiceDate.Substring(6, 4);

            DateTime purchasedDate = DateTime.ParseExact(s, "dd/MM/yyyy",
                                                CultureInfo.InvariantCulture);

            PurchaseInvoice invoice = new PurchaseInvoice
            {
                DateOfPurchase = purchasedDate,
                InvoiceId = invoiceNumber,
                supplierId = supplierId,
                TotalPurchaseAmount = sumofFinalAmounts,
            };

            BaseClass.dbEntities.PurchaseInvoices.Add(invoice);
            BaseClass.dbEntities.SaveChanges();
            int invoiceId = invoice.ID;

            var getInvoice = BaseClass.dbEntities.PurchaseInvoices.Where(m => m.ID == invoiceId && m.supplierId == supplierId).FirstOrDefault();
            var getLedgerBySupplier = BaseClass.dbEntities.PurchaseLedgers.Where(m => m.SupplierId == supplierId).ToList();
            if (getLedgerBySupplier.Count() == 0)
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
                        MRP = item.MRP,
                        Rate = item.Rate,
                        SellingPrice = item.SellingPrice,
                        CGST = item.CGST,
                        IGST = item.IGST,
                        UTGST = item.UTGST
                    };
                    BaseClass.dbEntities.Products.Add(product1);
                    BaseClass.dbEntities.SaveChanges();
                    int id = product1.Id;
                    purchase purchase1 = new purchase
                    {
                        InvoiceId = invoiceId,
                        DateOfPurchase = DateTime.UtcNow,
                        price = item.Amount,
                        Pack = item.Pack,
                        Quantity = item.Quantity,
                        CGST = item.CGST,
                        IGST = item.IGST,
                        UTGST = item.UTGST,
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

            TempData["message"] = TempData["Delete"];
            var listProducts = (from listProduct in BaseClass.dbEntities.Products
                                select new ListProductViewModel
                                {
                                    Id = listProduct.Id,
                                    ProductName = listProduct.ProductName,
                                    HSNNumber = listProduct.HSNNumber,
                                    BatchNumber = listProduct.BatchNumber,
                                    SellingPrice = listProduct.MRP,
                                    ExpiryDate = listProduct.ExpiryDate,
                                    CostPrice = listProduct.Rate
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

        public ActionResult GetPurchaseBySupplierId(int id, DateTime? startDate, DateTime? endDate)
        {
            var getPurchaseBySupplier = BaseClass.dbEntities.getPurcaseByDateRangeAndSupplierId(startDate, endDate, id).ToList();
            return View(getPurchaseBySupplier);
        }

        public ActionResult DeletePurchaseAndInvoiceDetails(int? invoiceId)
        {
            var getPurchaseByInvoiceId = BaseClass.dbEntities.purchases.Where(x => x.InvoiceId == invoiceId).ToList();

            foreach (var items in getPurchaseByInvoiceId)
            {
                BaseClass.dbEntities.purchases.Remove(items);
                BaseClass.dbEntities.SaveChanges();
            }

            var getLedgerByInvoiceId = BaseClass.dbEntities.PurchaseLedgers.Where(x => x.InvoiceId == invoiceId).SingleOrDefault();
            BaseClass.dbEntities.PurchaseLedgers.Remove(getLedgerByInvoiceId);
            BaseClass.dbEntities.SaveChanges();

            var invoiceByInvoiceId = BaseClass.dbEntities.PurchaseInvoices.Find(invoiceId);
            BaseClass.dbEntities.PurchaseInvoices.Remove(invoiceByInvoiceId);
            BaseClass.dbEntities.SaveChanges();

            return RedirectToAction("ViewSuppliers");
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
                                                      where invoice.supplierId == supplierId && invoice.ID == getInvoiceDetails.ID
                                                      select new ListOfInvoice
                                                      {
                                                          ProductName = product.ProductName,
                                                          CGstPercent = purchase.CGST,
                                                          IGST = purchase.IGST,
                                                          UTGST = purchase.UTGST,
                                                          MRP = product.MRP,
                                                          Rate = product.Rate,
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
            var allPaymentMethods = BaseClass.dbEntities.Payment_Mode.ToList();
            return View(allPaymentMethods);
        }

        public ActionResult AddDistributor()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddDistributor(DistributorViewModel distributor)
        {
            Customer customer = new Customer
            {
                CustomerName = distributor.CustomerName,
                CustomerAddress = distributor.CustomerAddress,
                CustomerOfficePhoneNumber = distributor.CustomerOfficePhoneNumber,
                CustomerResidencePhoneNumber = distributor.CustomerResidencePhoneNumber,
                CINNumber = distributor.CINNumber,
                Email = distributor.CustomerEmail,
                DLnumber = distributor.DLnumber,
                GSTNumber = distributor.GSTNumber,
                PanNumber = distributor.PanNumber,
                TINNumber = distributor.TINNumber
            };
            BaseClass.dbEntities.Customers.Add(customer);
            BaseClass.dbEntities.SaveChanges();
            return RedirectToAction("ViewDistributors");
        }

        public ActionResult EditDistributor(int? id)
        {
            var getCustomer = BaseClass.dbEntities.Customers.Where(m => m.id == id).FirstOrDefault();
            return View(getCustomer);
        }

        [HttpPost]
        public ActionResult EditDistributor(int? id, Customer customer)
        {
            var getCustomer = BaseClass.dbEntities.Customers.Where(m => m.id == id).FirstOrDefault();
            if (getCustomer != null)
            {
                getCustomer.CustomerName = customer.CustomerName;
                getCustomer.CustomerAddress = customer.CustomerAddress;
                getCustomer.CustomerOfficePhoneNumber = customer.CustomerOfficePhoneNumber;
                getCustomer.CustomerResidencePhoneNumber = customer.CustomerResidencePhoneNumber;
                getCustomer.DLnumber = customer.DLnumber;
                getCustomer.Email = customer.Email;
                getCustomer.GSTNumber = customer.GSTNumber;
                getCustomer.PanNumber = customer.PanNumber;
                getCustomer.TINNumber = customer.TINNumber;
                getCustomer.CINNumber = customer.CINNumber;

                BaseClass.dbEntities.Entry(getCustomer).State = EntityState.Modified;
                BaseClass.dbEntities.SaveChanges();
            }
            return RedirectToAction("ViewDistributors");
        }

        public ActionResult ViewDistributors()
        {
            var distributorList = (from distributor in BaseClass.dbEntities.Customers
                                   select new DistributorViewModel
                                   {
                                       CustomerName = distributor.CustomerName,
                                       CINNumber = distributor.CINNumber,
                                       CustomerAddress = distributor.CustomerAddress,
                                       CustomerEmail = distributor.Email,
                                       CustomerOfficePhoneNumber = distributor.CustomerOfficePhoneNumber,
                                       CustomerResidencePhoneNumber = distributor.CustomerResidencePhoneNumber,
                                       DLnumber = distributor.DLnumber,
                                       GSTNumber = distributor.GSTNumber,
                                       PanNumber = distributor.PanNumber,
                                       TINNumber = distributor.TINNumber,
                                       Id = distributor.id
                                   }).ToList();
            return View(distributorList);
        }

        public ActionResult DeleteDistributors(int? id)
        {
            var getSupplier = BaseClass.dbEntities.Customers.Find(id);
            if (getSupplier != null)
            {
                BaseClass.dbEntities.Customers.Remove(getSupplier);
                BaseClass.dbEntities.SaveChanges();
            }
            return RedirectToAction("ViewDistributors");
        }

        public ActionResult AddSale(int id)
        {
            var getDistributors = BaseClass.dbEntities.Customers.Where(x => x.id == id).Select(x => x.CustomerName).SingleOrDefault();
            ViewData["DistributorName"] = getDistributors;
            TempData["ID"] = id;

            var productList = BaseClass.dbEntities.Products.ToList();
            ViewBag.listProducts = new SelectList(productList, "id", "ProductName");

            return View();
        }

        [HttpPost]
        public JsonResult AddSale(List<ProductSaleViewModel> productSale)
        {
            double? sumOfAmounts = 0;
            double? allgstAmounts = 0;

            double? totalIgstAmounts = 0;
            double? totalCgstAmounts = 0;
            double? totalUtgstAmounts = 0;

            double? originalAmount = 0;
            double? discountedAmount = 0;
            double? amountAfterDiscount = 0;


            int distributorId = Convert.ToInt32(TempData["ID"]);
            if (productSale.Count > 1)
            {
                sumOfAmounts = productSale.Select(m => m.Amount).Sum();
            }
            else
            {
                sumOfAmounts = productSale.Select(m => m.Amount).FirstOrDefault();
            }

            foreach (var item in productSale)
            {
                if (item.CGST != 0)
                {
                    allgstAmounts = (item.CGST / 100f) * item.Amount;
                    totalCgstAmounts = allgstAmounts + totalCgstAmounts;
                }
                if (item.UTGST != 0)
                {
                    allgstAmounts = (item.UTGST / 100f) * item.Amount;
                    totalUtgstAmounts = allgstAmounts + totalUtgstAmounts;
                }
                if (item.IGST != 0)
                {
                    allgstAmounts = (item.IGST / 100f) * item.Amount;
                    totalIgstAmounts = allgstAmounts + totalIgstAmounts;
                }

            }   

            // Finding Gst Based on Given GST Percent
            var sumOfAllGstAmounts = totalCgstAmounts + totalIgstAmounts + totalUtgstAmounts;
            originalAmount = sumOfAllGstAmounts + sumOfAmounts;

            // Discount Percentage
            if (productSale != null)
            {
                var discount = productSale.Find(x => x.Discount != 0);
                if (discount != null)
                {
                    var discountPer = (discount.Discount / 100f);
                    amountAfterDiscount = Math.Round(Convert.ToDouble(originalAmount - (discountPer * originalAmount)), 2);
                    discountedAmount = Math.Round(Convert.ToDouble(originalAmount - amountAfterDiscount), 2);
                }
            }

            SalesInvoice invoice = new SalesInvoice
            {
                DateOfPurchase = DateTime.Now,
                CustomerId = distributorId,
                TotalPurchaseAmount = originalAmount,
                TotalDiscount = amountAfterDiscount,
                DiscountedAmount = discountedAmount
            };

            BaseClass.dbEntities.SalesInvoices.Add(invoice);
            BaseClass.dbEntities.SaveChanges();
            int invoiceId = invoice.ID;

            var getInvoice = BaseClass.dbEntities.SalesInvoices.Where(m => m.ID == invoiceId && m.CustomerId == distributorId).FirstOrDefault();
            var getLedgerBySupplier = BaseClass.dbEntities.SalesLedgers.Where(m => m.CustomerId == distributorId).ToList();
            if (getLedgerBySupplier.Count() == 0)
            {
                SalesLedger ledger = new SalesLedger
                {
                    Date = DateTime.Now,
                    CustomerId = distributorId,
                    CreditAmount = 0,
                    DebitAmount = amountAfterDiscount == 0 ? originalAmount : amountAfterDiscount,
                    BalanceAmount = amountAfterDiscount == 0 ? originalAmount : amountAfterDiscount,
                    InvoiceId = invoiceId,
                    Particulars = "To Sales Invoice Number " + getInvoice.InvoiceId
                };
                BaseClass.dbEntities.SalesLedgers.Add(ledger);
                BaseClass.dbEntities.SaveChanges();
            }
            else
            {
                var getLastInvoiceAmount = BaseClass.dbEntities.SalesLedgers.Where(m => m.CustomerId == distributorId).OrderByDescending(x => x.Id).FirstOrDefault();

                SalesLedger ledger = new SalesLedger
                {
                    Date = DateTime.Now,
                    CustomerId = distributorId,
                    CreditAmount = 0,
                    DebitAmount = amountAfterDiscount == 0 ? originalAmount : amountAfterDiscount,
                    BalanceAmount = amountAfterDiscount == 0 ? getLastInvoiceAmount.BalanceAmount + originalAmount : getLastInvoiceAmount.BalanceAmount + amountAfterDiscount,
                    InvoiceId = invoiceId,
                    Particulars = "To Sales Invoice Number " + getInvoice.InvoiceId
                };
                BaseClass.dbEntities.SalesLedgers.Add(ledger);
                BaseClass.dbEntities.SaveChanges();
            }

            if (productSale != null)
            {
                foreach (var item in productSale)
                {
                    Sale sale = new Sale
                    {
                        InvoiceId = invoiceId,
                        price = Math.Round(Convert.ToDouble(item.Amount), 2),
                        pack = item.Pack,
                        quantity = item.Quantity,
                        CGST = item.CGST,
                        SGST = item.UTGST,
                        IGST = item.IGST,
                        discountpercentage = Convert.ToInt32(item.Discount),
                        CustomerId = distributorId,
                        productid = item.id,
                    };
                    BaseClass.dbEntities.Sales.Add(sale);
                    BaseClass.dbEntities.SaveChanges();
                }


            }

            return Json("Successfully Inserted");
        }

        [HttpPost]
        public ActionResult GetProductDetails(int productId)
        {
            AddProductViewModel getData;

            getData = (from aa in BaseClass.dbEntities.Products
                       where aa.Id == productId
                       select new AddProductViewModel
                       {
                           HSNNumber = aa.HSNNumber,
                           SellingPrice = aa.SellingPrice,
                           ExpiryDate = aa.ExpiryDate,
                           BatchNumber = aa.BatchNumber,
                           MRP = aa.MRP
                       }).SingleOrDefault();

            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSaleByDistributorId(int id, DateTime? startDate, DateTime? endDate)
        {
            var allsalesForCustomer = BaseClass.dbEntities.getSalesByDateRangeAndSupplierId(startDate, endDate, id).ToList();
            return View(allsalesForCustomer);
        }

        public ActionResult ViewLedgerByDistributorId(int id, DateTime? startDate, DateTime? endDate)
        {
            TempData["distributorId"] = id;

            if (startDate != null && endDate != null)
            {
                var getLedgerByDateRange = BaseClass.dbEntities.getSalesLedgerByDateRange(startDate, endDate, id).ToList();
                return View(getLedgerByDateRange);
            }
            else
            {
                var getLedgerDetails = BaseClass.dbEntities.getSalesLedgerByDateRange(startDate, endDate, id).ToList();
                return View(getLedgerDetails);
            }
        }

        public ActionResult GetInvoiceDetailsByDistributorId(int? invoiceId, int? customerId)
        {
            double? sumOfAmounts = 0;
            double? allgstAmounts = 0;

            double? totalIgstAmounts = 0;
            double? totalCgstAmounts = 0;
            double? totalUtgstAmounts = 0;
            var getInvoiceDetails = BaseClass.dbEntities.SalesInvoices.Where(m => m.ID == invoiceId).SingleOrDefault();
            var customerDetails = BaseClass.dbEntities.Customers.Where(m => m.id == customerId).SingleOrDefault();
            var purchaseDetails = BaseClass.dbEntities.Sales.Where(m => m.CustomerId == customerId && m.InvoiceId == invoiceId).ToList();

            if (purchaseDetails.Count() > 1)
            {
                sumOfAmounts = purchaseDetails.Select(m => m.price).Sum();
            }
            else
            {
                sumOfAmounts = purchaseDetails.Select(m => m.price).FirstOrDefault();
            }


            foreach (var item in purchaseDetails)
            {
                if (item.CGST != 0)
                {
                    allgstAmounts = (item.CGST / 100f) * item.price;
                    totalCgstAmounts = allgstAmounts + totalCgstAmounts;
                }
                if (item.SGST != 0)
                {
                    allgstAmounts = (item.SGST / 100f) * item.price;
                    totalUtgstAmounts = allgstAmounts + totalUtgstAmounts;
                }
                if (item.IGST != 0)
                {
                    allgstAmounts = (item.IGST / 100f) * item.price;
                    totalIgstAmounts = allgstAmounts + totalIgstAmounts;
                }

            }

            var sumOfAllGstAmounts = totalCgstAmounts + totalIgstAmounts + totalUtgstAmounts;
            var totalAmountAfterGst = sumOfAllGstAmounts + sumOfAmounts;

            //var gstAmounts = (12 / 100f) * sumOfAmounts;

            //gstAmounts = Math.Round(Convert.ToDouble(gstAmounts), 2);



            var convertAmount = ConvertToFigure.ConvertAmount(Convert.ToDouble(getInvoiceDetails.TotalPurchaseAmount));
            

            TempData["DistributorName"] = customerDetails.CustomerName;
            TempData["Phonenumber"] = customerDetails.CustomerOfficePhoneNumber;
            TempData["ResidenceNumber"] = customerDetails.CustomerResidencePhoneNumber;
            TempData["Address"] = customerDetails.CustomerAddress;
            TempData["DlNumber"] = customerDetails.DLnumber;
            TempData["GSTIN"] = customerDetails.GSTNumber;
            TempData["CIN"] = customerDetails.CINNumber;


            TempData["InvoiceId"] = getInvoiceDetails.InvoiceId;
            TempData["TotalAmount"] = getInvoiceDetails.TotalPurchaseAmount;
            TempData["DateOfPurchase"] = getInvoiceDetails.DateOfPurchase.Value.ToShortDateString();

            TempData["TotalConvertedAmount"] = convertAmount;
            TempData["AmountBeforeGst"] = sumOfAmounts;
            TempData["TotalAmount"] = getInvoiceDetails.TotalPurchaseAmount;
            TempData["CGstAmount"] = totalCgstAmounts;
            TempData["IGSTAmount"] = totalIgstAmounts;
            TempData["UTGSTAmount"] = totalUtgstAmounts;
            TempData["discount"] = getInvoiceDetails.DiscountedAmount;
            TempData["discountedAmount"] = getInvoiceDetails.TotalDiscount;
            TempData["totalAmountAfterGst"] = totalAmountAfterGst;

            var getInvoiceByDistributorId = (from invoice in BaseClass.dbEntities.SalesInvoices
                                             join sales in BaseClass.dbEntities.Sales on invoice.ID equals sales.InvoiceId
                                             join product in BaseClass.dbEntities.Products on sales.productid equals product.Id
                                             where invoice.CustomerId == customerId && invoice.ID == invoiceId
                                             select new ListOfInvoice
                                             {
                                                 ProductName = product.ProductName,
                                                 CGstPercent = sales.CGST,
                                                 UTGST = sales.SGST,
                                                 IGST = sales.IGST,
                                                 MRP = product.MRP,
                                                 Rate = product.Rate,
                                                 SellingPrice = product.SellingPrice,
                                                 ExpiryDate = product.ExpiryDate,
                                                 HSNNumber = product.HSNNumber,
                                                 Quantity = sales.quantity,
                                                 Pack = sales.pack,
                                                 Batch = product.BatchNumber,
                                                 TotalAmount = sales.price,
                                                 Discount = sales.discountpercentage
                                             }).ToList();

            return View(getInvoiceByDistributorId);
        }

        public ActionResult ManageDistributorLedger(int? id)
        {
            var invoiceList = (from list in BaseClass.dbEntities.SalesInvoices
                               where list.CustomerId == id
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

            var getCustomer = BaseClass.dbEntities.Customers.Where(x => x.id == id).Select(x => x.CustomerName).SingleOrDefault();
            ViewData["CustomerName"] = getCustomer;
            TempData["ID"] = id;
            return View();
        }

        [HttpPost]
        public ActionResult ManageDistributorLedger(SalesLedgerViewModel ledgerViewModel)
        {
            int customerId = Convert.ToInt32(TempData["ID"]);
            var invoiceList = (from list in BaseClass.dbEntities.PurchaseInvoices
                               where list.supplierId == customerId
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
            var invoice = BaseClass.dbEntities.SalesInvoices.Where(m => m.ID == invoiceId).FirstOrDefault();

            var checkLedger = BaseClass.dbEntities.SalesLedgers.Where(m => m.CustomerId == customerId).ToList();
            var getLastInvoiceAmount = BaseClass.dbEntities.SalesLedgers.Where(m => m.CustomerId == customerId).OrderByDescending(x => x.Id).FirstOrDefault();
            ledgerViewModel.Particulars = "By " + paymentMode.PaymentMode + " For The Invoice " + invoice.InvoiceId;
            ledgerViewModel.BalanceAmount = getLastInvoiceAmount.BalanceAmount - ledgerViewModel.CreditAmount;


            SalesLedger ledger = new SalesLedger()
            {
                Date = DateTime.Now,
                CustomerId = customerId,
                CreditAmount = ledgerViewModel.CreditAmount,
                DebitAmount = ledgerViewModel.DebitAmunt,
                Particulars = ledgerViewModel.Particulars,
                BalanceAmount = ledgerViewModel.BalanceAmount,
                InvoiceId = Convert.ToInt32(ledgerViewModel.InvoiceId)
            };
            BaseClass.dbEntities.SalesLedgers.Add(ledger);
            BaseClass.dbEntities.SaveChanges();

            return RedirectToAction("ViewDistributors");
        }

        public ActionResult ViewAllPurchases()
        {
            var allPurchases = BaseClass.dbEntities.getAllPurchases().ToList();
            return View(allPurchases);
        }

        public ActionResult ViewAllSales()
        {
            var allSales = BaseClass.dbEntities.getAllSales().ToList();
            return View(allSales);
        }

        public ActionResult ProductProfilePage()
        {
            return View();
        }

        public ActionResult AddProduct()
        {
            return View(new ProductViewModel());
        }

        [HttpPost]
        public ActionResult AddProduct(ProductViewModel addProduct)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                Product addNewProduct = new Product()
                {
                    ProductName = addProduct.ProductName,
                    HSNNumber = addProduct.HSNNumber,
                    BatchNumber = addProduct.BatchNumber,
                    ExpiryDate = addProduct.ExpiryDate,
                    MRP = addProduct.MRP,
                    Rate = addProduct.Rate,
                    SellingPrice = addProduct.SP,
                    CGST = addProduct.CGST,
                    IGST = addProduct.IGST,
                    UTGST = addProduct.UTGST
                };

                BaseClass.dbEntities.Products.Add(addNewProduct);
                BaseClass.dbEntities.SaveChanges();
                return RedirectToAction("ViewAllProducts");
            }

        }
        public ActionResult EditProduct(int id)
        {
            var getProductById = (from db in BaseClass.dbEntities.Products where db.Id == id select db).SingleOrDefault();
            return View(getProductById);
        }

        [HttpPost]
        public ActionResult EditProduct(int id, Product model)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {

                var getProductById = (from db in BaseClass.dbEntities.Products where db.Id == id select db).SingleOrDefault();

                if (getProductById != null)
                {
                    getProductById.ProductName = model.ProductName;
                    getProductById.HSNNumber = model.HSNNumber;
                    getProductById.BatchNumber = model.BatchNumber;
                    getProductById.SellingPrice = model.SellingPrice;
                    getProductById.MRP = model.MRP;
                    getProductById.Rate = model.Rate;
                    getProductById.ExpiryDate = model.ExpiryDate;
                    getProductById.CGST = model.CGST;
                    getProductById.IGST = model.IGST;
                    getProductById.UTGST = model.UTGST;

                    BaseClass.dbEntities.Entry(getProductById).State = EntityState.Modified;
                    BaseClass.dbEntities.SaveChanges();
                }
                return RedirectToAction("ViewAllProducts");
            }
        }

        public ActionResult DeleteProduct(int id)
        {

            var getPurchaseById = BaseClass.dbEntities.purchases.Where(x => x.productId == id).ToList();
            if (getPurchaseById.Count() > 0)
            {
                TempData["Delete"] = "You cant delete this product which have already made a purchase";
                return RedirectToAction("ViewAllProducts");
            }

            var getSales = BaseClass.dbEntities.Sales.Where(x => x.productid == id).ToList();
            if (getSales.Count() > 0)
            {
                TempData["Delete"] = "You cant delete this product which sales related to it";
                return RedirectToAction("ViewAllProducts");
            }

            var productById = BaseClass.dbEntities.Products.Find(id);
            BaseClass.dbEntities.Products.Remove(productById);
            BaseClass.dbEntities.SaveChanges();
            return RedirectToAction("ViewAllProducts");
        }

        public ActionResult DbBackup()
        {
            return View();
        }
    }
}
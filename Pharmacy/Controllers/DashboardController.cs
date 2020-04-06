using Pharmacy.Models;
using Pharmacy.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                        price = item.Amount,
                        Pack = item.Pack,
                        Quantity = item.Quantity,
                        GstPercent = item.GST,
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
                                                      where invoice.supplierId == supplierId && invoice.ID == getInvoiceDetails.ID
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
            int? sumOfAmounts = 0;
            int distributorId = Convert.ToInt32(TempData["ID"]);
            if (productSale.Count > 1)
            {
                sumOfAmounts = productSale.Select(m => m.Amount).Sum();
            }
            else
            {
                sumOfAmounts = productSale.Select(m => m.Amount).FirstOrDefault();
            }

            var gstNumber = productSale.Select(m => m.GST).FirstOrDefault();
            var gstAmounts = (gstNumber / 100f) * sumOfAmounts;
            var sumofFinalAmounts = gstAmounts + sumOfAmounts;

            SalesInvoice invoice = new SalesInvoice
            {
                DateOfPurchase = DateTime.Now,
                CustomerId = distributorId,
                TotalPurchaseAmount = Convert.ToDecimal(sumofFinalAmounts),
            };

            BaseClass.dbEntities.SalesInvoices.Add(invoice);
            BaseClass.dbEntities.SaveChanges();
            int invoiceId = invoice.ID;

            var getInvoice = BaseClass.dbEntities.SalesInvoices.Where(m => m.ID == invoiceId && m.CustomerId == distributorId).FirstOrDefault();
            var getLedgerBySupplier = BaseClass.dbEntities.SalesLedgers.Where(m => m.CustomerId == distributorId).ToList();
            if (getLedgerBySupplier.Count == 0)
            {
                SalesLedger ledger = new SalesLedger
                {
                    Date = DateTime.Now,
                    CustomerId = distributorId,
                    CreditAmount = 0,
                    DebitAmount = Convert.ToInt32(sumofFinalAmounts),
                    BalanceAmount = Convert.ToInt32(sumofFinalAmounts),
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
                    DebitAmount = Convert.ToInt32(sumofFinalAmounts),
                    BalanceAmount = getLastInvoiceAmount.BalanceAmount + Convert.ToInt32(sumofFinalAmounts),
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
                        price = item.Amount,
                        pack = item.Pack,
                        quantity = item.Quantity,
                        CGstPercent = item.GST,
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
                           SP = aa.SellingPrice,
                           ExpiryDate = aa.ExpiryDate,
                           BatchNumber = aa.BatchNumber
                       }).SingleOrDefault();

            return Json(getData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSaleByDistributorId(int id)
        {
            var allsalesForCustomer = (from invoices in BaseClass.dbEntities.SalesInvoices
                                       where invoices.CustomerId == id
                                       select new ViewSales
                                       {
                                           Id = invoices.ID,
                                           DateOfPurchase = invoices.DateOfPurchase,
                                           PurchaseAmount = invoices.TotalPurchaseAmount,
                                           InvoiceId = invoices.InvoiceId,
                                           CustomerId = invoices.CustomerId
                                       }).ToList();

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
            int? sumOfAmounts = 0;
            var getInvoiceDetails = BaseClass.dbEntities.SalesInvoices.Where(m => m.ID == invoiceId).SingleOrDefault();
            var customerDetails = BaseClass.dbEntities.Customers.Where(m => m.id == customerId).SingleOrDefault();
            var purchaseDetails = BaseClass.dbEntities.Sales.Where(m => m.CustomerId == customerId && m.InvoiceId == invoiceId).ToList();

            if (purchaseDetails.Count > 1)
            {
                sumOfAmounts = purchaseDetails.Select(m => m.price).Sum();
            }
            else
            {
                sumOfAmounts = purchaseDetails.Select(m => m.price).FirstOrDefault();
            }

            var convertAmount = ConvertToFigure.ConvertAmount(Convert.ToDouble(getInvoiceDetails.TotalPurchaseAmount));
            var gstAmounts = (12 / 100f) * sumOfAmounts;

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
            TempData["GstAmount"] = gstAmounts;

            var getInvoiceByDistributorId = (from invoice in BaseClass.dbEntities.SalesInvoices
                                             join sales in BaseClass.dbEntities.Sales on invoice.ID equals sales.InvoiceId
                                             join product in BaseClass.dbEntities.Products on sales.productid equals product.Id
                                             where invoice.CustomerId == customerId && invoice.ID == invoiceId
                                             select new ListOfInvoice
                                             {
                                                 ProductName = product.ProductName,
                                                 GstPercent = sales.CGstPercent,
                                                 Costprice = product.CostPrice,
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
    }
}
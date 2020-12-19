using System;
using System.Collections.Generic;
using ES.Data.Models.Products;


namespace ES.MsOffice.ExcelManager
{
    public static class ExcelExportManager
    {
        public static bool ExportProducts(string filePath, List<ProductModel> products)
        {
            using (var xlApp = new ExcelDataContent(filePath))
            {
                var xlWSh = xlApp.GetWorksheet();
                if (xlWSh == null) return false;
                xlWSh.Activate();
                var nextRow = 1;
                try
                {
                    foreach (var product in products)
                    {
                        xlWSh.Cells[nextRow, 1] = product.Code;
                        xlWSh.Cells[nextRow, 2] = product.Description;
                        xlWSh.Cells[nextRow, 3] = product.Mu;
                        xlWSh.Cells[nextRow, 4] = product.Note;
                        xlWSh.Cells[nextRow, 5] = product.CostPrice;
                        xlWSh.Cells[nextRow, 6] = product.DealerPrice;
                        xlWSh.Cells[nextRow, 7] = product.DealerDiscount;
                        xlWSh.Cells[nextRow, 8] = product.Price;
                        xlWSh.Cells[nextRow, 9] = product.Discount;
                        nextRow++;
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    xlApp.Dispose();
                }
            }
        }

       // public static string PrintPurchaseInvoice(int moveInvoiceId)
       // {
       //     using (var xlApp = new ExcelDataContent())
       //     {
                
       //             var invoice = DataInvoices.GetPurchaseInvoice(moveInvoiceId);
       //             var listInvoiceItems = DataInvoices.GetPurchaseInvoiceItems(moveInvoiceId);
       //             if (invoice == null || listInvoiceItems.Count == 0) return null;
       //             var filePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Files");
       //             filePath = Path.Combine(filePath, "InvoiceOrder.xlsx");
       //             var destFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Temp");
       //             destFilePath = Path.Combine(destFilePath, Guid.NewGuid().ToString("N") + Path.GetExtension(filePath));
       //             File.Copy(filePath, destFilePath, true);
       //             //var xlWorkbook = (Workbook) xlApp.Workbooks.Open(destFilePath);
       //             var xlWSh = xlApp.GetWorksheet("Invoice");
       //             if (xlWSh == null)
       //             {
       //                 return null;
       //             }
       //             xlWSh.Activate();
       //             try
       //             {
       //                 //Invoice
       //             xlWSh.Cells[1, 2] = (!string.IsNullOrEmpty(invoice.InvoiceNumber))
       //                 ? invoice.InvoiceNumber
       //                 : "PI" + invoice.Date.ToString("yy") + "-" + invoice.Id;
       //             //Invoice type
       //             var invoiceType = DataInvoices.GetInvoiceType(invoice.InvoiceTypeId);
       //             xlWSh.Cells[2, 2] = (invoiceType != null) ? invoiceType.Description : string.Empty;
       //             //Date
       //             xlWSh.Cells[3, 2] = invoice.Date;
       //             //Discount
       //             xlWSh.Cells[4, 2] = invoice.Discount;
       //             //Creator
       //             var creator = DataWorker.GetWorker(invoice.CreatorId);
       //             xlWSh.Cells[5, 2] = (creator != null) ? creator.FullName : string.Empty;
       //             //From
       //             var from = DataProvider.GetProvider(invoice.FromId);
       //             xlWSh.Cells[6, 2] = (from != null) ? from.Name : string.Empty;
       //             xlWSh.Cells[7, 2] = string.Empty;
       //             //To
       //             var toStock = DataStock.GetStore(invoice.ToId);
       //             xlWSh.Cells[8, 2] = (toStock != null) ? toStock.Description : string.Empty;
       //             xlWSh.Cells[9, 2] = (toStock != null) ? toStock.Address : string.Empty;
       //             //Conformer
       //             var conformer = DataWorker.GetWorker(invoice.ConformedById);
       //             xlWSh.Cells[10, 2] = (conformer != null) ? conformer.FullName : string.Empty;
       //             //Released
       //             var released = DataWorker.GetWorker(invoice.ReleasedById);
       //             xlWSh.Cells[11, 2] = (released != null) ? released.FullName : string.Empty;
       //             //Received
       //             var received = DataWorker.GetWorker(invoice.ReceivedById);
       //             xlWSh.Cells[12, 2] = (received != null) ? received.FullName : string.Empty;
       //             //Delivery date
       //             xlWSh.Cells[14, 2] = invoice.DeliveryDate;

       //             var wShItems =
       //                 xlWorkbook.Worksheets.Cast<Worksheet>()
       //                     .FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "InvoiceItems");
       //             if (wShItems == null) return null;
       //             wShItems.Activate();
       //             var ind = 13;
       //             foreach (var invoiceItem in listInvoiceItems)
       //             {
       //                 var item = invoiceItem;
       //                 if (item == null) return null;
       //                 wShItems.Cells[ind, 2] = item.Product.Code;
       //                 wShItems.Cells[ind, 3] = item.Product.Description;
       //                 wShItems.Cells[ind, 4] = item.Product.Mu;
       //                 wShItems.Cells[ind, 5] = item.Quantity;
       //                 wShItems.Cells[ind, 6] = item.Cost;
       //                 wShItems.Cells[ind, 7] = item.Quantity*item.Cost;
       //                 wShItems.Cells[ind, 8] = item.Description;
       //                 ind++;
       //             }
       //             var wShOrder =
       //                 xlWorkbook.Worksheets.Cast<Worksheet>()
       //                     .FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Order");
       //             if (wShOrder == null)
       //             {
       //                 return null;
       //             }
       //             wShOrder.Range["A12", "G150"].AutoFilter(Field: 2, Criteria1: "<>");
       //             wShOrder.Range["D1", "G2"].Clear();
       //             wShOrder.Activate();
       //             xlApp.Visible = true;
       //             xlApp.Dialogs[XlBuiltInDialog.xlDialogPrint].Show();
       //             xlWorkbook.Close(true);
       //             return destFilePath;
       //         }
       //         catch (Exception e)
       //         {
        //             //MessageManager.ShowMessage("Ֆայլի խմբագրումը հնարավոր չէ, խնդրում ենք փորձել մի փոքր ուշ կամ դիմել ադմինիստրատորին։" + e.Message, "Խմբագրում", MessageBoxButtons.OK, MessageBoxIcon.Error);
       //             return null;
       //         }
       //         finally
       //         {
       //             xlApp.Dispose();

       //         }
       //     }
       // }
       // public static string CreateMoveInvoiceOrder(int moveInvoiceId)
       // {
       //     var xlApp = new Application();
       //     try
       //     {
       //         var invoice = DataInvoices.GetMoveInvoice(moveInvoiceId);
       //         var listInvoiceItems = DataInvoices.GetMoveInvoiceItems(moveInvoiceId);
       //         if (invoice == null || listInvoiceItems.Count == 0) return null;
       //         var filePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Files");
       //         filePath = Path.Combine(filePath, "InvoiceOrder.xlsx");
       //         var destFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Temp");
       //         destFilePath = Path.Combine(destFilePath, Guid.NewGuid().ToString("N") + Path.GetExtension(filePath));
       //         File.Copy(filePath, destFilePath, true);
       //         var xlWorkbook = (Workbook)xlApp.Workbooks.Open(destFilePath);
       //         var wShInvoice = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Invoice");
       //         if (wShInvoice == null) { xlApp.Quit(); return null; }
       //         wShInvoice.Activate();
       //         //Invoice
       //         wShInvoice.Cells[1, 2] = (!string.IsNullOrEmpty(invoice.InvoiceNumber)) ? invoice.InvoiceNumber : "IO" + invoice.Date.ToString("yy") + "-" + invoice.Id;
       //         //Invoice type
       //         var invoiceType = DataInvoices.GetInvoiceType(invoice.InvoiceTypeId);
       //         wShInvoice.Cells[2, 2] = (invoiceType != null) ? invoiceType.Description : string.Empty;
       //         //Date
       //         wShInvoice.Cells[3, 2] = invoice.Date;
       //         //Discount
       //         wShInvoice.Cells[4, 2] = invoice.Discount;
       //         //Creator
       //         var creator = DataWorker.GetWorker(invoice.CreatorId);
       //         wShInvoice.Cells[5, 2] = (creator != null) ? creator.FullName : string.Empty;
       //         //From
       //         var fromStock = DataStock.GetStore(invoice.FromId);
       //         wShInvoice.Cells[6, 2] = (fromStock != null) ? fromStock.Description : string.Empty;
       //         wShInvoice.Cells[7, 2] = (fromStock != null) ? fromStock.Address : string.Empty;
       //         //To
       //         var toStock = DataStock.GetStore(invoice.ToId);
       //         wShInvoice.Cells[8, 2] = (toStock != null) ? toStock.Description : string.Empty;
       //         wShInvoice.Cells[9, 2] = (toStock != null) ? toStock.Address : string.Empty;
       //         //Conformer
       //         var conformer = DataWorker.GetWorker(invoice.ConformedById);
       //         wShInvoice.Cells[10, 2] = (conformer != null) ? conformer.FullName : string.Empty;
       //         //Released
       //         var released = DataWorker.GetWorker(invoice.ReleasedById);
       //         wShInvoice.Cells[11, 2] = (released != null) ? released.FullName : string.Empty;
       //         //Received
       //         var received = DataWorker.GetWorker(invoice.ReceivedById);
       //         wShInvoice.Cells[12, 2] = (received != null) ? received.FullName : string.Empty;
       //         //InvoiceBook
       //         wShInvoice.Cells[17, 2] = invoice.InvoiceBook;
       //         //IbPage
       //         wShInvoice.Cells[18, 2] = invoice.IbPage;
       //         //IbRow
       //         wShInvoice.Cells[19, 2] = invoice.IbRow;
       //         //DeliveryType
       //         var delivery = DataInvoices.GetDeliveryType(invoice.DeliveryTypeId);
       //         wShInvoice.Cells[13, 2] = (delivery != null) ? delivery.Description : string.Empty;
       //         //Delivery date
       //         wShInvoice.Cells[14, 2] = invoice.DeliveryDate;
       //         //Delivery address
       //         wShInvoice.Cells[15, 2] = invoice.DeliveryAddress;

       //         var wShItems = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "InvoiceItems");
       //         if (wShItems == null) return null;
       //         wShItems.Activate();
       //         var ind = 13;
       //         foreach (var invoiceItem in listInvoiceItems)
       //         {
       //             var item = invoiceItem;
       //             if (item == null) return null;
       //             wShItems.Cells[ind, 2] = item.Product.Code;
       //             wShItems.Cells[ind, 3] = item.Product.Description;
       //             wShItems.Cells[ind, 4] = item.Product.Mu;
       //             wShItems.Cells[ind, 5] = item.Quantity;
       //             wShItems.Cells[ind, 6] = item.Cost;
       //             wShItems.Cells[ind, 7] = item.Quantity * item.Cost;
       //             wShItems.Cells[ind, 8] = item.Description;
       //             ind++;
       //         }
       //         var wShOrder = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Order");
       //         if (wShOrder == null) { return null; }
       //         wShOrder.Range["A12", "G150"].AutoFilter(Field: 2, Criteria1: "<>");
       //         wShOrder.Range["D1", "G2"].Clear();
       //         wShOrder.Activate();
       //         xlApp.Visible = true;
       //         xlApp.Dialogs[XlBuiltInDialog.xlDialogPrint].Show();
       //         xlWorkbook.Close(true);
       //         return destFilePath;
       //     }
       //     catch (Exception e)
       //     {
       //         MessageBox.Show("Ֆայլի խմբագրումը հնարավոր չէ, խնդրում ենք փորձել մի փոքր ուշ կամ դիմել ադմինիստրատորին։" + e.Message, "Խմբագրում", MessageBoxButtons.OK, MessageBoxIcon.Error);
       //         return null;
       //     }
       //     finally
       //     {
       //         xlApp.Quit();
       //     }
       //}
       // public static string CreateSaleInvoiceOrder(int saleInvoiceId)
       // {
       //     var xlApp = new Application();
       //     try
       //     {
       //         var invoice = DataInvoices.GetSaleInvoice(saleInvoiceId);
       //         var listInvoiceItems = DataInvoices.GetSaleInvoiceItems(saleInvoiceId);
       //         if (invoice == null || listInvoiceItems.Count == 0) return null;
       //         var filePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Files");
       //         filePath = Path.Combine(filePath, "InvoiceOrder.xlsx");
       //         var destFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Temp");
       //         destFilePath = Path.Combine(destFilePath, Guid.NewGuid().ToString("N") + Path.GetExtension(filePath));
       //         File.Copy(filePath, destFilePath, true);
       //         var xlWorkbook = (Workbook)xlApp.Workbooks.Open(destFilePath);
       //         var wShInvoice = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Invoice");
       //         if (wShInvoice == null) { xlApp.Quit(); return null; }
       //         wShInvoice.Activate();
       //         //Invoice
       //         wShInvoice.Cells[1, 2] = (!string.IsNullOrEmpty(invoice.InvoiceNumber)) ? invoice.InvoiceNumber : "IO" + invoice.Date.ToString("yy") + "-" + invoice.Id;
       //         //Invoice type
       //         var invoiceType = DataInvoices.GetInvoiceType(invoice.InvoiceTypeId);
       //         wShInvoice.Cells[2, 2] = (invoiceType != null) ? invoiceType.Description : string.Empty;
       //         //Date
       //         wShInvoice.Cells[3, 2] = invoice.Date;
       //         //Discount
       //         wShInvoice.Cells[4, 2] = invoice.Discount;
       //         //Creator
       //         var creator = DataWorker.GetWorkerByUser(invoice.CreatorId);
       //         wShInvoice.Cells[5, 2] = (creator != null) ? creator.FullName : string.Empty;
       //         //From
       //         var fromStock = DataStock.GetStore(invoice.FromId);
       //         wShInvoice.Cells[6, 2] = (fromStock != null) ? fromStock.Description : string.Empty;
       //         wShInvoice.Cells[7, 2] = (fromStock != null) ? fromStock.Address : string.Empty;
       //         //To
       //         var customer = DataCustomer.GetCustomer(invoice.ToId);
       //         wShInvoice.Cells[8, 2] = (customer != null) ? customer.CompanyName : string.Empty;
       //         wShInvoice.Cells[9, 2] = (customer != null) ? customer.Address : string.Empty;
       //         //Conformer
       //         var conformer = DataWorker.GetWorker(invoice.ConformedById);
       //         wShInvoice.Cells[10, 2] = (conformer != null) ? conformer.FullName : string.Empty;
       //         //Released
       //         var released = DataWorker.GetWorker(invoice.ReleasedById);
       //         wShInvoice.Cells[11, 2] = (released != null) ? released.FullName : string.Empty;
       //         //Received
       //         var received = DataCustomer.GetCustomer(invoice.ReceivedById);
       //         wShInvoice.Cells[12, 2] = (received != null) ? received.HeadOfCompany : string.Empty;
       //         //Description
       //         wShInvoice.Cells[16, 2] = invoice.Description;
       //         //InvoiceBook
       //         wShInvoice.Cells[17, 2] = string.Empty; // invoice.InvoiceBook;
       //         //IbPage
       //         wShInvoice.Cells[18, 2] = string.Empty; // invoice.IbPage;
       //         //IbRow
       //         wShInvoice.Cells[19, 2] = String.Empty; //invoice.IbRow;
       //         //DeliveryType
       //         //var delivery = DataInvoices.GetDeliveryType(invoice.DeliveryTypeId);
       //         wShInvoice.Cells[13, 2] = String.Empty; // (delivery != null) ? delivery.Description : string.Empty;
       //         //Delivery date
       //         wShInvoice.Cells[14, 2] = invoice.DeliveryDate;
       //         //Delivery address
       //         wShInvoice.Cells[15, 2] = invoice.DeliveryAddress;

       //         var wShItems = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "InvoiceItems");
       //         if (wShItems == null) return null;
       //         wShItems.Activate();
       //         var ind = 13;
       //         foreach (var invoiceItem in listInvoiceItems)
       //         {
       //             var item = invoiceItem;
       //             if (item == null) return null;
       //             wShItems.Cells[ind, 2] = item.Product.Code;
       //             wShItems.Cells[ind, 3] = item.Product.Description;
       //             wShItems.Cells[ind, 4] = item.Product.Mu;
       //             wShItems.Cells[ind, 5] = item.Quantity;
       //             wShItems.Cells[ind, 6] = item.Price;
       //             wShItems.Cells[ind, 7] = item.Quantity * item.Price;
       //             wShItems.Cells[ind, 8] = item.Description;
       //             ind++;
       //         }
       //         var wShOrder = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Order");
       //         if (wShOrder == null) { return null; }
       //         wShOrder.Range["A12", "G150"].AutoFilter(Field: 2, Criteria1: "<>");
       //         wShOrder.Range["D1", "G2"].Clear();
       //         wShOrder.Activate();
       //         xlApp.Visible = true;
       //         xlApp.Dialogs[XlBuiltInDialog.xlDialogPrint].Show();
       //         xlWorkbook.Close(true);
       //         return destFilePath;
       //     }
       //     catch (Exception e)
       //     {
       //         MessageBox.Show("Ֆայլի խմբագրումը հնարավոր չէ, խնդրում ենք փորձել մի փոքր ուշ կամ դիմել ադմինիստրատորին։" + e.Message, "Խմբագրում", MessageBoxButtons.OK, MessageBoxIcon.Error);
       //         return null;
       //     }
       //     finally
       //     {
       //         xlApp.Quit();
       //     }
       // }
       // public static bool CreateProductionReport(DateTime fromDate, DateTime toDate, decimal purchaseSum, decimal saleSum, List<Releate.ProductionReport> listSum)
       // {
       //     var xlApp = new Application();
       //     try
       //     {
       //         var filePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Files");
       //         filePath = Path.Combine(filePath, "ProductionStockReport.xlsx");
       //         var destFilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Temp");
       //         destFilePath = Path.Combine(destFilePath, Guid.NewGuid().ToString("N") + Path.GetExtension(filePath));
       //         File.Copy(filePath, destFilePath, true);
       //         var xlWorkbook = (Workbook)xlApp.Workbooks.Open(destFilePath);
       //         var wShReport = xlWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(xlWorksheet => xlWorksheet.CodeName == "Report");
       //         if (wShReport == null) { xlApp.Quit(); return false; }
       //         wShReport.Activate();
       //         //Month
       //         wShReport.Cells[2, 2] = fromDate;
       //         wShReport.Cells[2, 3] = toDate;
       //         wShReport.Cells[4, 3] = purchaseSum;
       //         wShReport.Cells[5, 3] = saleSum;
       //         var ind = 2;
       //         foreach (var exItem in listSum)
       //         {
       //             var item = exItem;
       //             if (item == null) return false;
       //             wShReport.Cells[ind, 5] = item.Նախագիծ;
       //             wShReport.Cells[ind, 6] = item.Գումար;
       //             ind++;
       //         }
       //         wShReport.Range["A1", "F33"].AutoFilter(Field: 6, Criteria1: "<>");
       //         xlApp.Visible = true;
       //         xlApp.Dialogs[XlBuiltInDialog.xlDialogPrint].Show();
       //         xlWorkbook.Close(true);
       //         return true;
       //     }
       //     catch (Exception e)
       //     {
       //         MessageBox.Show("Ֆայլի խմբագրումը հնարավոր չէ, խնդրում ենք փորձել մի փոքր ուշ կամ դիմել ադմինիստրատորին։" + e.Message, "Խմբագրում", MessageBoxButtons.OK, MessageBoxIcon.Error);
       //         return false;
       //     }
       //     finally
       //     {
       //         xlApp.Quit();
       //     }
       // }
    }
}

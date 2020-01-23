using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ES.Business.Models;
using ES.Common.Managers;
using ES.Data.Models;
using Microsoft.Office.Interop.Excel;
using ProductOrderModel = ES.Data.Models.ProductOrderModel;

namespace ES.Business.ExcelManager
{
    public static class ExcelExportManager
    {
        private const string InvoiceBlankPath = @"\Blanks\Invoice.xlsx";
        private const string TempPath = @"\Temp\";

        public static bool ExportProducts(List<ProductOrderModel> productOrderItems)
        {
            if (productOrderItems == null) return false;
            var xlApp = new ExcelDataContent(false);

            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return false;
            xlWSh.Activate();

            var nextRow = 1;
            try
            {
                xlWSh.Cells[nextRow, 1] = "Կոդ";
                xlWSh.Cells[nextRow, 2] = "Անվանում";
                xlWSh.Cells[nextRow, 3] = "Չմ";
                xlWSh.Cells[nextRow, 4] = "Առկա քանակ";
                xlWSh.Cells[nextRow, 5] = "Գին";
                xlWSh.Cells[nextRow, 6] = "Գումար";
                xlWSh.Cells[nextRow, 7] = "Նշումներ";
                nextRow++;
                foreach (var item in productOrderItems)
                {
                    xlWSh.Cells[nextRow, 1] = item.Code;
                    xlWSh.Cells[nextRow, 2] = item.Description;
                    xlWSh.Cells[nextRow, 3] = item.Mu;
                    xlWSh.Cells[nextRow, 4] = item.ExistingQuantity;
                    xlWSh.Cells[nextRow, 5] = item.Product != null ? item.Product.Price : 0;
                    xlWSh.Cells[nextRow, 6] = item.Amount;
                    xlWSh.Cells[nextRow, 7] = item.Notes;
                    nextRow++;
                }
                xlApp.Show();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //xlApp.Dispose();
            }
        }
        public static bool ExportProducts(List<ProductModel> products)
        {
            var xlApp = new ExcelDataContent(false);

            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return false;
            xlWSh.Activate();
            xlApp.Show();
            var nextRow = 1;
            //Code
            xlWSh.Cells[nextRow, 1] = "Code";
            //Barcode
            xlWSh.Cells[nextRow, 2] = "Barcode";
            //HcdCs
            xlWSh.Cells[nextRow, 3] = "HcdCs";
            //Description
            xlWSh.Cells[nextRow, 4] = "Description";
            //Mu
            xlWSh.Cells[nextRow, 5] = "Mu";
            //Note
            xlWSh.Cells[nextRow, 6] = "Note";
            //Cost price
            xlWSh.Cells[nextRow, 7] = "Cost price";
            //Price
            xlWSh.Cells[nextRow, 8] = "Price";
            //Discount
            xlWSh.Cells[nextRow, 9] = "Discount";
            //DealerPrice
            xlWSh.Cells[nextRow, 10] = "Dealer Price";
            //Dealer Discount
            xlWSh.Cells[nextRow, 11] = "Dealer Discount";
            nextRow++;

            try
            {
                foreach (var product in products)
                {
                    Range cell;
                    
                    //Code
                    cell = xlWSh.Cells[nextRow, 1] ;
                    cell.NumberFormat = "@";
                    cell.Value2 = product.Code;
                    
                    //Barcode
                    cell = xlWSh.Cells[nextRow, 2];
                    cell.NumberFormat = "@";
                    cell.Value2 = product.Barcode;
                    
                    //HcdCs
                    cell = xlWSh.Cells[nextRow, 3];
                    cell.NumberFormat = "@";
                    cell.Value2 = product.HcdCs;
                     
                    //Description
                    xlWSh.Cells[nextRow, 4] = product.Description;
                    //Mu
                    xlWSh.Cells[nextRow, 5] = product.Mu;
                    //Note
                    xlWSh.Cells[nextRow, 6] = product.Note;
                    //Cost price
                    xlWSh.Cells[nextRow, 7] = product.CostPrice;
                    //Price
                    xlWSh.Cells[nextRow, 8] = product.Price;
                    //Discount
                    xlWSh.Cells[nextRow, 9] = product.Discount;
                    //Dealer Price
                    xlWSh.Cells[nextRow, 10] = product.DealerPrice;
                    //DealerDiscount
                    xlWSh.Cells[nextRow, 11] = product.DealerDiscount;

                    nextRow++;
                }
                return true;
            }
            catch (Exception)
            {
                return false; 
                xlApp.Dispose();
            }
            finally
            {
               
            }
        }

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

        public static bool ExportInvoice(InvoiceModel invoice, ObservableCollection<InvoiceItemsModel> invoiceItems,
            bool isCostPrice = false, bool isPrice = true, bool printInvoice = false)
        {
            var xlApp = new ExcelDataContent(Directory.GetCurrentDirectory() + InvoiceBlankPath);
            {
                var xlWSh = xlApp.GetWorksheet();
                if (xlWSh == null) return false;
                xlWSh.Cells[1, 3] = invoice.ProviderName;
                xlWSh.Cells[5, 3] = invoice.RecipientName;
                xlWSh.Activate();
                xlWSh.Cells[2, 7] = invoice.InvoiceNumber;
                xlWSh.Cells[6, 5] = invoice.CreateDate;
                var nextRowIndex = 13;
                var copiedRow = xlWSh.Rows[nextRowIndex];
                try
                {
                    foreach (var invoiceItem in invoiceItems)
                    {
                        xlWSh.Cells[nextRowIndex, 2] = nextRowIndex - 12;
                        xlWSh.Cells[nextRowIndex, 2] = invoiceItem.Code;
                        xlWSh.Cells[nextRowIndex, 3] = invoiceItem.Description;
                        xlWSh.Cells[nextRowIndex, 4] = invoiceItem.Mu;
                        xlWSh.Cells[nextRowIndex, 5] = invoiceItem.Quantity;
                        xlWSh.Cells[nextRowIndex, 6] = isPrice ? invoiceItem.Product.Price : invoiceItem.Price;
                        xlWSh.Cells[nextRowIndex, 7] = invoiceItem.Quantity *
                                                       (isPrice ? invoiceItem.Product.Price : invoiceItem.Price);
                        xlWSh.Cells[nextRowIndex, 8] = invoiceItem.Note;
                        var cellRange = xlWSh.Range[xlWSh.Cells[nextRowIndex, 1], xlWSh.Cells[nextRowIndex, 8]];
                        if (nextRowIndex % 2 == 1)
                        {
                            cellRange.Interior.Color = Color.Silver;
                        }
                        else
                        {
                            cellRange.Interior.Color = Color.White;
                        }
                        nextRowIndex++;
                        xlWSh.Rows[nextRowIndex].Insert(XlInsertShiftDirection.xlShiftDown, copiedRow);

                    }
                    var range = xlWSh.Range[xlWSh.Cells[13, 1], xlWSh.Cells[nextRowIndex - 1, 8]];
                    range.Borders.LineStyle = XlLineStyle.xlContinuous;
                    range.Borders.Weight = XlBorderWeight.xlHairline;
                    range.Borders.Color = Color.Black;
                    xlWSh.Rows[nextRowIndex].Delete(XlDeleteShiftDirection.xlShiftUp);
                    xlWSh.Cells[nextRowIndex + 1, 6] = invoiceItems.Sum(s => s.Quantity * (isPrice ? s.Product.Price : s.Price));
                    xlWSh.Cells[nextRowIndex + 4, 6] = invoice.Creator;
                    if (printInvoice) { xlApp.Print(true); xlApp.Dispose(); }
                    xlApp.Show();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {

                }
            }
        }

        public static bool ExportInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems,
         bool isPrice = true, bool isPrint = true)
        {
            var xlApp = new ExcelDataContent(Directory.GetCurrentDirectory() + InvoiceBlankPath);
            {
                var xlWSh = xlApp.GetWorksheet();
                if (xlWSh == null) return false;
                xlWSh.Cells[1, 3] = invoice.ProviderName;
                xlWSh.Cells[5, 3] = invoice.RecipientName;
                xlWSh.Activate();
                xlWSh.Cells[2, 7] = invoice.InvoiceNumber;
                xlWSh.Cells[6, 5] = invoice.CreateDate;
                var nextRowIndex = 13;
                var copiedRow = xlWSh.Rows[nextRowIndex];
                try
                {
                    foreach (var invoiceItem in invoiceItems)
                    {
                        xlWSh.Cells[nextRowIndex, 2] = nextRowIndex - 12;
                        xlWSh.Cells[nextRowIndex, 2] = invoiceItem.Code;
                        xlWSh.Cells[nextRowIndex, 3] = invoiceItem.Description;
                        xlWSh.Cells[nextRowIndex, 4] = invoiceItem.Mu;
                        xlWSh.Cells[nextRowIndex, 5] = invoiceItem.Quantity;
                        xlWSh.Cells[nextRowIndex, 6] = isPrice ? invoiceItem.Product.Price : invoiceItem.Price;
                        xlWSh.Cells[nextRowIndex, 7] = invoiceItem.Quantity *
                                                       (isPrice ? invoiceItem.Product.Price : invoiceItem.Price);
                        xlWSh.Cells[nextRowIndex, 8] = invoiceItem.Note;
                        var cellRange = xlWSh.Range[xlWSh.Cells[nextRowIndex, 1], xlWSh.Cells[nextRowIndex, 8]];
                        if (nextRowIndex % 2 == 1)
                        {
                            cellRange.Interior.Color = Color.Silver;
                        }
                        else
                        {
                            cellRange.Interior.Color = Color.White;
                        }
                        nextRowIndex++;
                        xlWSh.Rows[nextRowIndex].Insert(XlInsertShiftDirection.xlShiftDown, copiedRow);

                    }
                    var range = xlWSh.Range[xlWSh.Cells[13, 1], xlWSh.Cells[nextRowIndex - 1, 8]];
                    range.Borders.LineStyle = XlLineStyle.xlContinuous;
                    range.Borders.Weight = XlBorderWeight.xlHairline;
                    range.Borders.Color = Color.Black;
                    xlWSh.Rows[nextRowIndex].Delete(XlDeleteShiftDirection.xlShiftUp);
                    xlWSh.Cells[nextRowIndex + 1, 6] = invoiceItems.Sum(s => s.Quantity * (isPrice ? s.Product.Price : s.Price));
                    xlWSh.Cells[nextRowIndex + 4, 6] = invoice.Creator;
                    if (isPrint)
                    {
                        xlApp.Print(true);
                        xlApp.Dispose();
                        return true;
                    }
                    xlApp.Show();
                    return true;
                }
                catch (Exception)
                {
                    return false;
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
        //             //MessageBox.Show("Ֆայլի խմբագրումը հնարավոր չէ, խնդրում ենք փորձել մի փոքր ուշ կամ դիմել ադմինիստրատորին։" + e.Message, "Խմբագրում", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public static void ExportList(DataGrid dg)
        {
            var xlApp = new ExcelDataContent(false);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return;
            int indexC, indexR;
            try
            {
                MessageManager.ShowMessage(dg.Items.Count.ToString(), "Արտահանման սխալ", MessageBoxImage.Error);
                for (indexR = 0; indexR < dg.Items.Count; indexR++)
                {
                    var obj = (List<object>)dg.Items[indexR];
                    indexC = 1;
                    for (indexC = 0; indexC < obj.Count; indexC++)
                    {
                        xlWSh.Cells[indexR + 1, indexC + 1] = obj[indexC].ToString();
                    }
                }
                MessageManager.ShowMessage(indexR.ToString(), "Արտահանման սխալ",MessageBoxImage.Error);
                xlApp.Show();
                return;
            }
            catch (Exception)
            {
                MessageManager.ShowMessage("Արտահանման ժամանակ տեղի է ունեցել սխալ։", "Արտահանման սխալ", MessageBoxImage.Error);
                xlApp.Dispose();
                return;
            }
            finally
            {

            }


        }
        public static void ExportList(IEnumerable<object> list)
        {
            if (list == null || list.Count() == 0) return;
            var xlApp = new ExcelDataContent(false);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return;
            var indexR = 1;
            var indexC = 1;
            try
            {
                foreach (var propInfo in list.First().GetType().GetProperties())
                {
                    xlWSh.Cells[indexR, indexC] = propInfo.Name;
                    indexC++;
                }
                indexR++;
                foreach (object el in list)
                {
                    indexC = 1;
                    foreach (var propInfo in el.GetType().GetProperties())
                    {
                        var value = propInfo.GetValue(el, null);
                        xlWSh.Cells[indexR, indexC] =value!=null? value.ToString() : string.Empty;
                        indexC++;
                    }
                    indexR++;
                }
            }
            catch (Exception ex)
            {
                MessageManager.ShowMessage("Արտահանման ժամանակ տեղի է ունեցել սխալ։ \n " + ex.Message, "Արտահանման սխալ", MessageBoxImage.Error);
            }
            finally
            {

            }
            xlApp.Show();

        }
    }
}

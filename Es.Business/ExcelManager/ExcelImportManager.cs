using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using ES.Business.Managers;
using ES.Common.Managers;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using HG.Tools.Helper;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace ES.Business.ExcelManager
{
    public class ExcelImportManager
    {
        private void ImportdataFromExcel(string excelfilepath)
        {
            //declare variables - edit these based on your particular situation
            string ssqltable = "tdatamigrationtable";
            // make sure your sheet name is correct, here sheet name is sheet1, so you can change your sheet name if have

            string myexceldataquery = "select student,rollno,course from [sheet1$]";
            try
            {
                ////create our connection strings
                //string sexcelconnectionstring = @"provider=microsoft.jet.oledb.4.0;data source=" + excelfilepath +
                //";extended properties=" + "\"excel 8.0;hdr=yes;\"";

                ////series of commands to bulk copy data from the excel file into our sql table

                //Oledbconnection oledbconn = new oledbconnection(sexcelconnectionstring);
                //oledbcommand oledbcmd = new oledbcommand(myexceldataquery, oledbconn);
                //oledbconn.open();
                //oledbdatareader dr = oledbcmd.executereader();
                //sqlbulkcopy bulkcopy = new sqlbulkcopy(ssqlconnectionstring);
                //bulkcopy.destinationtablename = ssqltable;
                //while (dr.read())
                //{

                //}

                //oledbconn.Close();
            }
            catch (Exception ex)
            {
                //handle exception
            }
        }
        public static List<ProductModel> ImportProducts(string filePath)
        {
            var xlApp = new ExcelDataContent(filePath);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return null;
            xlWSh.Activate();
            var products = new List<ProductModel>();
            //var nextRow = 2;
            try
            {
                var memberId = ApplicationManager.Instance.GetMember.Id;
                var userId = ApplicationManager.GetEsUser.UserId;
                var range = xlWSh.UsedRange;
                //while (!string.IsNullOrEmpty(xlWSh.Cells[nextRow, 1].Text))
                for (var nextRow = 2; nextRow <= range.Rows.Count; nextRow++)
                {
                    var product = new ProductModel(memberId, userId, true)
                    {
                        Id = Guid.NewGuid(),
                        //Code
                        Code = xlWSh.Cells[nextRow, 1].Text,
                        //Barcode
                        Barcode = xlWSh.Cells[nextRow, 2].Text,
                        //HcdCs
                        HcdCs = xlWSh.Cells[nextRow, 3].Text,
                        //Description
                        Description = xlWSh.Cells[nextRow, 4].Text,
                        //Mu
                        MeasureUnit = ProductsManager.GetMeasureOfUnits().FirstOrDefault(s=>s.Name==xlWSh.Cells[nextRow, 5].Text),
                        //Note
                        Note = xlWSh.Cells[nextRow, 6].Text,
                        //Cost price
                        CostPrice = string.IsNullOrEmpty(xlWSh.Cells[nextRow, 7].Text) ? null : HgConvert.ToDecimal(xlWSh.Cells[nextRow, 7].Text, CultureInfo.InvariantCulture),
                        //Price
                        Price = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 8].Text, CultureInfo.InvariantCulture),
                        //Discount
                        Discount = string.IsNullOrEmpty(xlWSh.Cells[nextRow, 9].Text) ? null : HgConvert.ToDecimal(xlWSh.Cells[nextRow, 9].Text, CultureInfo.InvariantCulture),
                        //DealerPrice
                        DealerPrice = string.IsNullOrEmpty(xlWSh.Cells[nextRow, 10].Text) ? null : HgConvert.ToDecimal(xlWSh.Cells[nextRow, 10].Text, CultureInfo.InvariantCulture),
                        //DealerDiscount
                        DealerDiscount = string.IsNullOrEmpty(xlWSh.Cells[nextRow, 11].Text) ? null : HgConvert.ToDecimal(xlWSh.Cells[nextRow, 11].Text, CultureInfo.InvariantCulture),
                    };
                    //nextRow++;
                    //if(string.IsNullOrEmpty(product.Code)) continue;
                    if (string.IsNullOrEmpty(product.Description)) continue;
                    products.Add(product);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                xlApp.Dispose();
            }
            return products;
        }

        public static Tuple<InvoiceModel, List<InvoiceItemsModel>> ImportInvoice(string filePath)
        {
            var xlApp = new ExcelDataContent(filePath);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return null;
            xlWSh.Activate();
            var invoice = new InvoiceModel();
            var invoiceItems = new List<InvoiceItemsModel>();
            var nextRow = 13;
            try
            {
                invoice.CreateDate = HgConvert.ToDateTime(xlWSh.Cells[6, 5].Text);
                while (!string.IsNullOrEmpty(xlWSh.Cells[nextRow, 2].Text))
                {
                    var product = new ProductModel(memberId: 0, lastModifierId: 0, isEnable: true)
                    {
                        Code = xlWSh.Cells[nextRow, 2].Text
                    };
                    var invoiceItem = new InvoiceItemsModel
                    {
                        Product = product,
                        Quantity = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 5].Text, CultureInfo.InvariantCulture),
                        Price = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 6].Text, CultureInfo.InvariantCulture),
                        Note = xlWSh.Cells[nextRow, 8].Text
                    };
                    invoiceItems.Add(invoiceItem);
                    nextRow++;
                }
            }
            catch (Exception ex)
            {
                MessageManager.ShowMessage(ex.Message + " " + nextRow + " տող");
                return null;
            }
            finally
            {
                xlApp.Dispose();
            }
            return Tuple.Create(invoice, invoiceItems);
        }
        public static Tuple<InvoiceModel, List<InvoiceItemsModel>> ImportSaleInvoice(string filePath = null, bool addVAT = true)
        {
            var file = !string.IsNullOrEmpty(filePath) ? filePath : FileManager.FileManager.OpenExcelFile("Excel ֆայլի բեռնում", "Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls");
            if (file == null) return null;
            using (var xlApp = new ExcelDataContent(file))
            {
                var xlWSh = xlApp.GetWorksheet();
                if (xlWSh == null) return null;
                var invoice = new InvoiceModel();
                var invoiceItems = new List<InvoiceItemsModel>();
                try
                {
                    xlWSh.Activate();
                    invoice.ProviderName = xlWSh.Cells[1, 3].Text;
                    invoice.RecipientName = xlWSh.Cells[5, 6].Text;
                    invoice.InvoiceNumber = xlWSh.Cells[2, 7].Text;
                    invoice.CreateDate = HgConvert.ToDateTime(xlWSh.Cells[7, 5].Text);
                    var nextRowIndex = 16;

                    var nextCode = xlWSh.Cells[nextRowIndex, 2].Text;
                    while (!string.IsNullOrEmpty(nextCode))
                    {
                        var product = new ProductModel(0, 0, true)
                        {
                            Code = xlWSh.Cells[nextRowIndex, 2].Text,
                            Description = xlWSh.Cells[nextRowIndex, 3].Text,
                            //Mu = xlWSh.Cells[nextRowIndex, 4].Text,
                            Price = Math.Round(HgConvert.ToDecimal(xlWSh.Cells[nextRowIndex, 6].Text, CultureInfo.InvariantCulture), 2),
                            HcdCs = xlWSh.Cells[nextRowIndex, 8].Text
                        };
                        product.Price +=addVAT ?  Math.Round((decimal) (product.Price * 20 / 100),2) : 0;

                        var invoiceItem = new InvoiceItemsModel
                        {
                            ProductId = product.Id,
                            Product = product,
                            Code = product.Code,
                            Description = product.Description,
                            Quantity = HgConvert.ToDecimal(xlWSh.Cells[nextRowIndex, 5].Text, CultureInfo.InvariantCulture),
                            Price = product.Price,
                            Note = xlWSh.Cells[nextRowIndex, 8].Text
                        };
                        invoiceItems.Add(invoiceItem);
                        nextRowIndex++;
                        nextCode = xlWSh.Cells[nextRowIndex, 2].Text;
                    }
                    xlApp.GetWorkbook().Close(false);
                    invoice.Total = invoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
                    return new Tuple<InvoiceModel, List<InvoiceItemsModel>>(invoice, invoiceItems);
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    xlApp.Dispose();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using ES.DataAccess.Models;
using HG.Tools.Helper;

namespace ES.MsOffice.ExcelManager
{
    public class ExcelImportManager
    {
        private void IimportdataFromExcel(string excelfilepath)
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
        public static Tuple<List<Products>, List<ProductCategories>> ImportProducts(string filePath)
        {
            var xlApp = new ExcelDataContent(filePath);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return null;
            xlWSh.Activate();
            var products = new List<Products>();
            var productCategories = new List<ProductCategories>();
            var nextRow = 2;
            try
            {
                while (!string.IsNullOrEmpty(xlWSh.Cells[nextRow, 4].Text))
                {
                    var product = new Products
                    {
                        Id = Guid.NewGuid(),
                        Code = xlWSh.Cells[nextRow, 1].Text,
                        Description = xlWSh.Cells[nextRow, 4].Text,
                        Mu = xlWSh.Cells[nextRow, 5].Text,
                        Note = xlWSh.Cells[nextRow, 6].Text + xlWSh.Cells[nextRow, 2].Text + xlWSh.Cells[nextRow, 3].Text,
                        CostPrice = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 7].Text),
                        DealerPrice = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 8].Text),
                        DealerDiscount = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 9].Text),
                        Price = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 10].Text),
                        Discount = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 11].Text)
                    };
                    product.Brands = new Brands { BrandName = xlWSh.Cells[nextRow, 1].Text };
                    var category = new Categories() { CategoryName = xlWSh.Cells[nextRow, 2].Text };
                    productCategories.Add(new ProductCategories { Products = product, Categories = category });
                    products.Add(product);
                    nextRow++;
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
            return Tuple.Create(products, productCategories);
        }

        public static Tuple<Invoices ,List<ProductItems>> ImportInvoice(string filePath)
        {
            var xlApp = new ExcelDataContent(filePath);
            var xlWSh = xlApp.GetWorksheet();
            if (xlWSh == null) return null;
            xlWSh.Activate();
            var productItems = new List<ProductItems>();
            var nextRow = 14;
            try
            {
                while (!string.IsNullOrEmpty(xlWSh.Cells[nextRow, 1].Text))
                {
                    var product = new Products
                    {
                        Code = xlWSh.Cells[nextRow, 2].Text
                    };
                    var productItem = new ProductItems
                    {
                        Products = product,
                        Quantity = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 5].Text),
                        CostPrice = HgConvert.ToDecimal(xlWSh.Cells[nextRow, 6].Text)
                    };
                    productItems.Add(productItem);
                    nextRow++;
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
            return Tuple.Create(new Invoices(), productItems);
        }
        private void ImportInvoiceOld(string filePath)
        {
            using (var xlApp = new ExcelDataContent(filePath))
            {
                var xlWSh = xlApp.GetWorksheet();
                if (xlWSh == null) return;
                try
                {
                    //     txtCustomer.Text = Convert.ToString(((Range)xlWSh.Cells[5, 3]).Value2);
                    //    txtInvoice.Text = Convert.ToString(((Range)xlWSh.Cells[4, 7]).Value2);

                    //    var createDate = ((Range)xlWSh.Cells[6, 5]).Value2;
                    //    dtpCreatedDate.Value = createDate != null ? (DateTime.FromOADate((double)createDate)) : DateTime.Today;
                    //    var deliveryDate = ((Range)xlWSh.Cells[8, 5]).Value2;
                    //    dtpDeliveryDate.Value = deliveryDate != null ? DateTime.FromOADate((double)deliveryDate) : DateTime.Today;

                    //    var productList = new List<ProductList>();
                    //    var row = 12;
                    //    while (Convert.ToString(((Range)xlWSh.Cells[++row, 3]).Text) != string.Empty)
                    //    {
                    //        productList.Add(new ProductList
                    //        {
                    //            Id = Convert.ToInt16(((Range)xlWSh.Cells[row, 1]).Value2),
                    //            Code = Convert.ToString(((Range)xlWSh.Cells[row, 2]).Value2),
                    //            Description = Convert.ToString(((Range)xlWSh.Cells[row, 3]).Value2),
                    //            Mu = Convert.ToString(((Range)xlWSh.Cells[row, 4]).Value2),
                    //            Quantity = Convert.ToDouble(((Range)xlWSh.Cells[row, 5]).Value2),
                    //            Price = Convert.ToDouble(((Range)xlWSh.Cells[row, 6]).Value2),
                    //            Amount = Convert.ToDouble(((Range)xlWSh.Cells[row, 7]).Value2)
                    //        });
                    //    }
                    //    dgvList.DataSource = productList;

                    //    txtConformer.Text = Convert.ToString(((Range)xlWSh.Cells[156, 4]).Value2);
                    //    txtReceived.Text = Convert.ToString(((Range)xlWSh.Cells[158, 4]).Value2);
                    //    xlWorkbook.Close(true);
                    //    var pakingListType = DataInvoices.GetInvoiceType(DataInvoices.Type.InputProtokol);
                    //    txtInvoiceType.Tag = pakingListType.Id;
                    //    txtInvoiceType.Text = pakingListType.Description;
                }
                catch (Exception)
                {
                    return;
                }
                finally
                {
                    xlApp.Dispose();
                }
            }
        }
    }
}

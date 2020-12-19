using System;
using System.Linq;
using ES.Business.Helpers;
using ES.DataAccess.Models;
using MessageBox = System.Windows.MessageBox;

namespace ES.Business.Managers
{
    public class ExecuteManager : BaseManager
    {
        public static bool ExecuteTest()
        {
            //var aramdb = GetDataContext(ApplicationManager.AramConnectionString);
            //var db = GetDataContext(ApplicationManager.LocalhostConnectionString);
            //var productsFromAram = aramdb.Products;
            //var products = db.Products;
            //if (products == null || productsFromAram == null)
            //{
            //    MessageBox.Show("Error. Db is null.");
            //    return false;
            //}
            //foreach (var item in products)
            //{
            //    var exitem = productsFromAram.SingleOrDefault(s => s.Id == item.Id);
            //    if (exitem == null) { MessageBox.Show("Error. exitem is null.");
            //        return false;
            //    }
            //    exitem.MinQuantity = item.MinQuantity;
            //}
            //try
            //{
            //    aramdb.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error.Save error." + ex.Message);
            //    return false;
            //}


            using (var db = GetDataContext())
            {
                var productLists = db.Products.GroupBy(s => s.Code).Where(s => s.Count() > 1).Select(s => s.ToList()).ToList();
                foreach (var products in productLists)
                {
                    foreach (var product in products)
                    {
                        var p = product;
                        if (db.InvoiceItems.Any(s => s.ProductId == product.Id) || db.ProductItems.Any(s => s.ProductId == product.Id))
                        {
                            continue;
                        }
                        else
                        {
                            if (db.Products.Count(s => s.Code == product.Code) < 2) continue;
                            db.Products.Remove(product);
                            db.SaveChanges();
                        }
                    }

                }

            }
            return true;
        }
    }
}

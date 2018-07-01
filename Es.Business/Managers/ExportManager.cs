using System;
using System.Collections.Generic;
using System.IO;
using ES.Business.Models;
using ES.Common;
using ES.Common.Managers;
using Microsoft.Win32;
using ProductModel = ES.Data.Models.ProductModel;

namespace ES.Business.Managers
{
    public class ExportManager
    {
        public static bool ExportPriceForShtrikhM(List<ProductModel> items)
        {
            try
            {
                if (items == null || items.Count == 0) { return false; }
                var sfd = new SaveFileDialog();
                sfd.Filter = "Text files (*.txt)|*.txt";
                if (sfd.ShowDialog() != true)
                {
                    return false;
                }
                var doc = File.CreateText(sfd.FileName);
                var index = 0;
                foreach (var item in items)
                {
                    index++;
                    doc.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}",
                        index,
                        HgConvert.ToLatArm(item.Description),
                        string.Empty,
                        item.Price,
                        0,
                        0,
                        0,
                        item.Code,
                        0,
                        0,
                        0,
                        DateTime.Today.ToString("dd.MM.yyyy"),
                        0));
                }
                doc.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static bool ExportPriceForShtrikhK(List<ProductModel> items)
        {
            try
            {
                if (items == null || items.Count == 0) { return false; }
                var sfd = new SaveFileDialog();
                sfd.Filter = "Text files (*.txt)|*.txt";
                if (sfd.ShowDialog() != true)
                {
                    return false;
                }
                var doc = File.CreateText(sfd.FileName);
                var index = 0;
                foreach (var item in items)
                {
                    index++;
                    doc.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}",
                        index,
                        HgConvert.ToLatArm(item.Description),
                        string.Empty,
                        item.Price,
                        0,
                        0,
                        0,
                        item.Code,
                        0,
                        0,
                        0,
                        DateTime.Today.ToString("dd.MM.yyyy"),
                        0));
                }
                doc.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static bool ExportPriceForScaleToExcel(List<ProductModel> items)
        {
            try
            {
                if (items == null || items.Count == 0) { return false; }
                var sfd = new SaveFileDialog();
                sfd.Filter = "Text files (*.txt)|*.txt";
                if (sfd.ShowDialog() != true)
                {
                    return false;
                }
                var doc = File.CreateText(sfd.FileName);
                var index = 0;
                foreach (var item in items)
                {
                    index++;
                    doc.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}",
                        index,
                        HgConvert.ToLatArm(item.Description),
                        string.Empty,
                        item.Price,
                        0,
                        0,
                        0,
                        item.Code,
                        0,
                        0,
                        0,
                        DateTime.Today.ToString("dd.MM.yyyy"),
                        0));
                }
                doc.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static bool ExportPriceForScaleToXml(List<ProductModel> items)
        {
            try
            {
                if (items == null || items.Count == 0) { return false; }
                var weigherModel = new ScaleModel("");
                weigherModel.SetProducts(items);
                //XmlSerializer xp = new XmlSerializer(typeof(ScaleModel));

                //var xml = new XmlManager();
                //var sw = new StringWriter();
                //xp.Serialize(sw, weigherModel);
                //var text = sw.ToString();
                
                var sfd = new SaveFileDialog();
                sfd.Filter = "Xml Document (*.xml)|*.xml";
                if (sfd.ShowDialog() != true)
                {
                    return false;
                }
                XmlManager.Save(weigherModel.WeigherProducts, sfd.FileName);
                //var doc = File.CreateText(sfd.FileName);
                
                //doc.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}

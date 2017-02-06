using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
//using System.Runtime.InteropServices.WindowsRuntime;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace ES.Business.ExcelManager
{
    public class ExcelDataContent:IDisposable
    {
        #region Properties and variables
        private readonly Application _xlsApp;
        private readonly Workbook _xlsWorkbook;
        private readonly Worksheet _xlsWorksheet;
        public ExcelDataContent()
        {
            _xlsApp = new Application();
        }
        public ExcelDataContent(bool visible)
        {
            _xlsApp = new Application();
            _xlsWorkbook = _xlsApp.Workbooks.Add();
            _xlsWorksheet = _xlsWorkbook.Worksheets.Add();
            _xlsApp.Visible = visible;
        }

        public void Show()
        {
            _xlsApp.Visible = true;
        }
        public void Hide()
        {
            _xlsApp.Visible = false;
        }
        public ExcelDataContent(string filePath)
        {
            try
            {
                _xlsApp = new Application();
                _xlsWorkbook = System.IO.File.Exists(filePath) ? _xlsApp.Workbooks.Open(filePath,ReadOnly:true) : _xlsApp.Workbooks.Add();
            if (_xlsWorkbook.Worksheets.Count == 0) { _xlsWorkbook.Worksheets.Add(); }
                _xlsWorksheet = _xlsWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public ExcelDataContent(string filePath, string workSheet)
        {
            _xlsApp = new Application();
            _xlsWorkbook = _xlsApp.Workbooks.Open(filePath);
            _xlsWorksheet = _xlsWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(s => s.Name == workSheet);
        }

        public void Dispose()
        {
            try
            {
                _xlsWorkbook.Close(false);
                _xlsApp.Quit();
            }
            catch (Exception ex)
            {
              
            }
                
        }
        #endregion

        #region Methods
        public Application GetXlsApplication()
        {
            return _xlsApp;
        }
        public Workbook GetWorkbook()
        {
            return _xlsWorkbook;
        }
        public Workbook GetWorkbook(string filePath)
        {
            return System.IO.File.Exists(filePath) ? _xlsApp.Workbooks.Open(filePath) : _xlsWorkbook;
        }

        public void Print(bool showPrintPreview = false)
        {
            if (showPrintPreview)
            {
                _xlsApp.Visible = true;
                _xlsWorksheet.PrintPreview();
            }
            else
            {
                _xlsWorksheet.PrintOut();
            }
            
        }
        public void Save()
        {
            _xlsWorkbook.Save();
            }
        public void SaveAs(string newPath)
        {
            _xlsWorkbook.SaveAs(newPath);
        }
        public Worksheet GetWorksheet()
        {
            return _xlsWorksheet;
        }
        public Worksheet GetWorksheet(string worksheet)
        {
            return (_xlsWorkbook != null && string.IsNullOrEmpty(worksheet))? _xlsWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault(s => s.Name == worksheet): _xlsWorksheet;
        }

        #endregion

       
    }
}

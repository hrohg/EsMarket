using System;
using System.Linq;
using Microsoft.Build.Tasks;
using Microsoft.Office.Interop.Excel;

namespace ES.MsOffice.ExcelManager
{
    public class ExcelDataContent:IDisposable
    {
        #region Properties and variables
        private Application _xlsApp;
        private Workbook _xlsWorkbook;
        private Worksheet _xlsWorksheet;
        public ExcelDataContent()
        {
            _xlsApp = new Application();
        }

        public ExcelDataContent(string filePath)
        {
            _xlsApp = new Application();
            try
            {
            if (_xlsWorkbook == null && System.IO.File.Exists(filePath))
            {
                _xlsWorkbook = _xlsApp.Workbooks.Open(filePath);
                _xlsWorksheet = _xlsWorkbook.Worksheets.Cast<Worksheet>().FirstOrDefault();
            }
            }
            catch (Exception)
            {
               
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
            catch (Exception)
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

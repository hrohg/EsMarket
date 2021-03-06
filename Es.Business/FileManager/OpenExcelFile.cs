﻿using System.IO;
using Microsoft.Win32;

namespace ES.Business.FileManager
{
    public class FileManager
    {
        private const string InitialDirectory = @"C:\";
        private const string SeccondInitialDirectory = @"D:\Stores\Forms";

        public static string OpenFile(string title = "Open File", string filter = "All files |*.*")
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                InitialDirectory = InitialDirectory
            };
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }
        public static string[] OpenFiles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Մուտքագրման ակտի բեռնում",
                Filter = "All files |*.*",
                InitialDirectory = InitialDirectory
            };
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();
            return openFileDialog.FileNames;
        }
        public static string OpenExcelFile()
        {
            var openFileDialog = new OpenFileDialog
               {
                   Title = "Մուտքագրման ակտի բեռնում",
                   Filter = "Excel files(*.xlsx)|*.xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls",
                   InitialDirectory = InitialDirectory
               };
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }
        
        public static string OpenExcelFile(string filter, string title, string filePath = null)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                InitialDirectory = (string.IsNullOrEmpty(filePath) || !Directory.Exists(filePath)) ? InitialDirectory: filePath
            };
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }
        public static string OpenExcelFile(string title, string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                InitialDirectory = InitialDirectory
            };
            openFileDialog.ShowDialog();
            return openFileDialog.FileName;
        }

        public static string SaveFile(string title = "Save file", string filter = "All files |*.*")
        {
            var fileDialog = new SaveFileDialog()
            {
                Title = title,
                Filter = filter,
                InitialDirectory = InitialDirectory
            };
            return (fileDialog.ShowDialog()==true)?fileDialog.FileName:string.Empty;
        }
    }
}

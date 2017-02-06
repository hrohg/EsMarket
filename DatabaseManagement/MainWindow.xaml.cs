using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Cultures;
using Shared.Helpers;
using UserControls;


namespace DatabaseManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Backup File",
                RestoreDirectory = true,
                Filter = "Realtor Bakup Files (*.esl)|*.esl",
                AddExtension = true
            };

            string backupFilePath = string.Empty;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                backupFilePath = dialog.FileName;
            }
            else
            {
                return;
            }

            string password = string.Empty;

            InputPassword passWindow = new InputPassword();
            if (passWindow.ShowDialog() ?? false)
            {
                password = passWindow.Password;
            }
            else
            {
                return;
            }
            if (string.IsNullOrEmpty(password.Trim()))
            {
                MessageBox.Show(CultureResources.Inst["PasswordCanNotBeEmpty"], CultureResources.Inst["PasswordError"], MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Cursor = Cursors.Wait;

            string errorMessage = string.Empty;
            if (backupFilePath.EndsWith(".esl"))
            {
                backupFilePath = backupFilePath.Replace(".esl", string.Empty);
            }

            //if (!Session.Inst.BEManager.CreateDatabaseBackup(backupFilePath, DatabaseManager.GetConnectionString(), ref errorMessage))

            if (!Session.Inst.BEManager.CreateDatabaseBackup(backupFilePath, ApplicationManager.ConnectionString, ref errorMessage))
            {
                Cursor = null;
                MessageBox.Show(CultureResources.Inst["ArchiveIsNotCompletedTryLater"], CultureResources.Inst["ArchiveError"], MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //ZipFile zip = new ZipFile()

            if (!string.IsNullOrEmpty(backupFilePath))
            {
                Cryptography cryptography = new Cryptography(password);

                if (cryptography.Encrypt(backupFilePath, string.Format("{0}.esl", backupFilePath)))
                {
                    MessageBox.Show(CultureResources.Inst["ArchiveCreatedSuccessfully"], "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Cursor = null;
            }
            else
            {
                Cursor = null;
                MessageBox.Show(CultureResources.Inst["ArchiveIsNotCompletedTryLater"], CultureResources.Inst["ArchiveError"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                File.Delete(backupFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            RestoreDatabase(null);
        }

        public void RestoreDatabase(string filePath)
        {
            if (MessageBox.Show(CultureResources.Inst["RestoreWillDeleteAnyChanges"], CultureResources.Inst["ConfirmRestore"], MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var processes = Process.GetProcessesByName("RealEstateApp");
            if (processes.Length > 0)
            {
                if (MessageBox.Show(CultureResources.Inst["RealtorIsOpenAndWillBeClose"], CultureResources.Inst["ConfirmRestore"], MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                processes[0].Kill();
            }

            if (string.IsNullOrEmpty(filePath))
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
                                                                {
                                                                    CheckFileExists = true,
                                                                    Filter = "Rieltor Archive Files|*.esl",
                                                                    InitialDirectory =
                                                                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                                    Multiselect = false,
                                                                    RestoreDirectory = true,
                                                                    Title = CultureResources.Inst["SelectArchive"]
                                                                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            string restorePassword = string.Empty;
            PasswordForRestore passWindow = new PasswordForRestore();
            if (passWindow.ShowDialog() ?? false)
            {
                restorePassword = passWindow.Password;

                if (string.IsNullOrEmpty(restorePassword))
                {
                    MessageBox.Show(CultureResources.Inst["PasswordCanNotBeEmpty"], CultureResources.Inst["PasswordError"], MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                return;
            }
            var restoreFilePath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".zip";
            Cursor = Cursors.Wait;
            Cryptography cryptography = new Cryptography(restorePassword);
            try
            {
                if (!cryptography.Decrypt(filePath, restoreFilePath))
                {
                    Cursor = null;
                    MessageBox.Show(CultureResources.Inst["PasswordIsIncorrect"], "", MessageBoxButton.OK, MessageBoxImage.Information);
                    DeleteBackupFile(restoreFilePath);
                    return;
                }
                string errorMessage = string.Empty;
                //using (ZipFile zip = ZipFile.Read(restoreFilePath))
                //{
                //    zip.ExtractProgress += zip_ExtractProgress;
                //    string directoryName = Path.GetDirectoryName(restoreFilePath) + Path.GetFileNameWithoutExtension(restoreFilePath) + "temp";
                //    foreach (ZipEntry entry in zip)
                //    {
                //        entry.Extract(directoryName, ExtractExistingFileAction.OverwriteSilently);
                //    }
                //}
                Cursor = null;
                MessageBox.Show("OK");
            }
            catch
            {
                if (File.Exists(restoreFilePath))
                {
                    try
                    {
                        File.Delete(restoreFilePath);
                    }
                    catch { }
                }
            }
            //if (!Session.Inst.BEManager.RestoreDatabase(restoreFilePath, Constants.DababaseFolderPath, ref errorMessage))
            //{
            //    Cursor = null;
            //    MessageBox.Show(CultureResources.Inst["RestoreNotCompletedTryLater"], "", MessageBoxButton.OK, MessageBoxImage.Error);
            //    DeleteBackupFile(restoreFilePath);
            //}
            //else
            //{
            //    Cursor = null;
            //    MessageBox.Show(CultureResources.Inst["ArchiveRestored"], "", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

        }

        //void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        //{
        //    var v = e.TotalBytesToTransfer;
        //    var x = e.BytesTransferred;
        //}

        private void DeleteBackupFile(string restoreFilePath)
        {
            if (File.Exists(restoreFilePath))
            {
                try
                {
                    File.Delete(restoreFilePath);
                }
                catch { }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Resources["RestoreFilePath"] != null)
            {
                var filePath = this.Resources["RestoreFilePath"].ToString();
                RestoreDatabase(filePath);
                this.Resources["RestoreFilePath"] = null;
            }
        }
    }
}

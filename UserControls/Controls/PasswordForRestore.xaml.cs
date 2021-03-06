﻿using System.Windows;
using System.Windows.Input;
using ES.Common.Cultures;
using ES.Common.Managers;

namespace UserControls.Controls
{
	/// <summary>
	/// Interaction logic for PasswordForRestore.xaml
	/// </summary>
	public partial class PasswordForRestore : Window
	{
		public string Password { get; set; }
		public PasswordForRestore()
		{
			InitializeComponent();
			pswPassword.Focus();
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			ProcessOK();
		}

		private void ProcessOK()
		{
			if (string.IsNullOrEmpty(pswPassword.Password.Trim()))
			{
			    MessageManager.ShowMessage(CultureResources.Inst["InputPassword"], "", MessageBoxImage.Error);
				pswPassword.Focus();
				return;
			}

			DialogResult = true;
			Password = pswPassword.Password;
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void pswPassword_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				ProcessOK();
			}
		}
	}
}

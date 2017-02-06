using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace UserControls
{

	/// <summary>
	/// Interaction logic for ImageViewer.xaml
	/// </summary>
	public partial class ImageViewer : Window
	{


		public ObservableCollection<string> Images
		{
			get { return (ObservableCollection<string>)GetValue(ImagesProperty); }
			set { SetValue(ImagesProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Images.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ImagesProperty =
			DependencyProperty.Register("Images", typeof(ObservableCollection<string>), typeof(ImageViewer), new UIPropertyMetadata(null, OnImagesChanged));

		private static void OnImagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ImageViewer imgViewer = d as ImageViewer;
			if (imgViewer != null)
			{
				imgViewer.Images = (ObservableCollection<string>)e.NewValue;
			}
		}


		public ImageViewer()
		{
			InitializeComponent();
		}

		public ImageViewer(string imagePath)
			: this()
		{
			Images = new ObservableCollection<string>(new List<string> { imagePath });
		}

		public ImageViewer(ObservableCollection<string> images)
			: this()
		{
			Images = images;
		}

		private void imageViewer_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				Close();
			}
		}

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void mnuOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			var imageFile = ((MenuItem)sender).Tag.ToString();

			var directory = Path.GetDirectoryName(Images.FirstOrDefault(s => s == imageFile));
			if (!Directory.Exists(directory))
			{
				return;
			}

			string argument = string.Format("/select, {0}", imageFile);
			System.Diagnostics.Process.Start("explorer.exe", argument);
		}

		private void mnuSaveImages(object sender, RoutedEventArgs e)
		{
			var imageFile = ((MenuItem)sender).Tag.ToString();
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog
			{
				//Description = CultureResources.Inst["FolderToSaveImages"],
				RootFolder = Environment.SpecialFolder.MyComputer,
				ShowNewFolderButton = true
			};

			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			CopyImages(dialog.SelectedPath);
		}

		private void CopyImages(string folderToCopy)
		{
			foreach (string image in Images)
			{
				try
				{
					File.Copy(image, string.Format("{0}\\{1}", folderToCopy, System.IO.Path.GetFileName(image)), true);
				}
				catch { }
			}
		}
	}
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;


//using System.Web.Configuration;
//using System.Web.UI.WebControls;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;


namespace Shared.Helpers
{
	public class ImageHelper
	{

		//internal static bool UploadFile(string filePath, int imageID)
		//{
		//var thumb = CreateThumbnailImage(file, 100/*height*/);
		//if (!SaveThumbnail(thumb, imageID))
		//{
		//    return false;
		//}
		//var resizedImage = ResizeImage(file);
		//var img = WatermarkImage(resizedImage, WebConfigurationManager.AppSettings["WaterMarkText"]);
		//return SaveImage(img, imageID);
		//}

		//private static bool SaveImage(MemoryStream resizedImage, int imageID)
		//{
		//    if (resizedImage == null)
		//        return false;

		//    System.Drawing.Image img = System.Drawing.Image.FromStream(resizedImage);

		//    try
		//    {
		//        img.Save(Constants.EstateImagesFolderPhisicalPath + imageID + ".jpg", ImageFormat.Jpeg);
		//    }
		//    catch
		//    {
		//        return false;
		//    }

		//    return true;
		//}

		public static byte[] GetResizeImageStreamFromFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return null;
			if (!File.Exists(filePath)) return null;

			if (!IsImage(filePath))
			{
				return null;
			}

			MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath));
			//MemoryStream stream = new MemoryStream(fStream.b
			Image pic = Image.FromStream(stream);

			const int MAX_SIDE_SIZE = 800;

			int newWidth, newHeight;

			if (pic.Width > MAX_SIDE_SIZE || pic.Height > MAX_SIDE_SIZE)
			{
				int percentage;
				if (pic.Width > pic.Height)
				{
					newWidth = MAX_SIDE_SIZE;
					percentage = (int) (100.0 - (MAX_SIDE_SIZE/(pic.Width/100)));
					newHeight = pic.Height - (int) ((pic.Height/100.0)*percentage);
				}
				else
				{
					newHeight = MAX_SIDE_SIZE;
					percentage = (int) (100.0 - (MAX_SIDE_SIZE/(pic.Height/100)));
					newWidth = pic.Width - (int) ((pic.Width/100.0)*percentage);
				}
			}
			else
			{
				newWidth = pic.Width;
				newHeight = pic.Height;
			}

			Bitmap imgOutput = new Bitmap(newWidth, newHeight);
			imgOutput.MakeTransparent();
			imgOutput.MakeTransparent(Color.Black);

			Graphics newGraphics = Graphics.FromImage(imgOutput);
			newGraphics.Clear(Color.FromArgb(0, 255, 255, 255)); //blank the image

			newGraphics.CompositingQuality = CompositingQuality.HighQuality;
			newGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			newGraphics.DrawImage(pic, 0, 0, newWidth, newHeight);
			newGraphics.Flush();

			stream = new MemoryStream();
			imgOutput.Save(stream, pic.RawFormat);

			return stream.ToArray();
		}

		//private static bool SaveThumbnail(MemoryStream thumb, int imageID)
		//{
		//    if (thumb == null)
		//    {
		//        return false;
		//    }
		//    System.Drawing.Image img = System.Drawing.Image.FromStream(thumb);
		//    try
		//    {
		//        img.Save(Constants.EstateThumbnailsFolderPhisicalPath + imageID + ".jpg", ImageFormat.Jpeg);
		//    }
		//    catch
		//    {
		//        return false;
		//    }

		//    return true;
		//}

		//private static MemoryStream CreateThumbnailImage(FileUpload file, int height)
		//{
		//    if (file.HasFile)
		//    {
		//        if (IsImage(file.FileName))
		//        {
		//            MemoryStream stream = new MemoryStream(file.FileBytes);
		//            System.Drawing.Image pic = System.Drawing.Image.FromStream(stream);
		//            int width = (int)((double)pic.Width / pic.Height * (double)height);

		//            Bitmap imgOutput = new Bitmap(width, height);
		//            imgOutput.MakeTransparent();
		//            imgOutput.MakeTransparent(Color.Black);

		//            Graphics newGraphics = Graphics.FromImage(imgOutput);
		//            newGraphics.Clear(Color.FromArgb(0, 255, 255, 255)); //blank the image

		//            newGraphics.CompositingQuality = CompositingQuality.HighQuality;
		//            newGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		//            newGraphics.DrawImage(pic, 0, 0, width, height);
		//            newGraphics.Flush();

		//            stream = new MemoryStream();
		//            imgOutput.Save(stream, pic.RawFormat);
		//            return stream;
		//        }
		//    }
		//    return null;
		//}

		//private static MemoryStream WatermarkImage(MemoryStream resizedImage, string watermarkText)
		//{
		//    System.Drawing.Image img = System.Drawing.Image.FromStream(resizedImage);

		//    Graphics gr = Graphics.FromImage(img);
		//    Font font = new Font("Tahoma", 40);
		//    Color color = Color.FromArgb(50, 241, 235, 105);
		//    double tangent = (double)img.Height / (double)img.Width;
		//    double angle = Math.Atan(tangent) * (180 / Math.PI);
		//    double halfHypotenuse = Math.Sqrt((img.Height * img.Height) + (img.Width * img.Width)) / 2;
		//    double sin, cos, opp1, adj1, opp2, adj2;

		//    for (int i = 100; i > 0; i--)
		//    {
		//        font = new Font("Tahoma", i, FontStyle.Bold);
		//        SizeF sizef = gr.MeasureString(watermarkText, font, int.MaxValue);

		//        sin = Math.Sin(angle * (Math.PI / 180));
		//        cos = Math.Cos(angle * (Math.PI / 180));
		//        opp1 = sin * sizef.Width;
		//        adj1 = cos * sizef.Height;
		//        opp2 = sin * sizef.Height;
		//        adj2 = cos * sizef.Width;

		//        if (opp1 + adj1 < img.Height && opp2 + adj2 < img.Width)
		//            break;
		//        //
		//    }

		//    StringFormat stringFormat = new StringFormat();
		//    stringFormat.Alignment = StringAlignment.Center;
		//    stringFormat.LineAlignment = StringAlignment.Center;

		//    gr.SmoothingMode = SmoothingMode.AntiAlias;
		//    gr.RotateTransform((float)angle);
		//    gr.DrawString(watermarkText, font, new SolidBrush(color), new Point((int)halfHypotenuse, 0), stringFormat);

		//    MemoryStream stream = new MemoryStream();
		//    img.Save(stream, ImageFormat.Jpeg);

		//    return stream;
		//}

		private static bool IsImage(string fileName)
		{
			string ext = Path.GetExtension(fileName);
			switch (ext.ToLower())
			{
				case ".gif":
				case ".png":
				case ".jpg":
				case ".jpeg":
					return true;
				default:
					return false;
			}
		}
	}
}

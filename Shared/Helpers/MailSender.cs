using System;
using System.Net.Mail;
using System.Text;
using System.Net;

namespace Shared.Helpers
{
	/// <summary>
	/// Summary description for MailSender
	/// </summary>
	public class MailSender
	{
		public static bool Send(string sMailTo, string sSubject, string sBody)
		{
			MailMessage oMessage = new MailMessage();
			SmtpClient smtpClient = new SmtpClient("mail.rambler.ru");

			oMessage.From = new MailAddress("rieltor_mail@rambler.ru", "Realtor", Encoding.UTF8);

			oMessage.Subject = sSubject;
			oMessage.IsBodyHtml = true;
			oMessage.Body = sBody;

			smtpClient.Credentials = new NetworkCredential("rieltor_mail@rambler.ru", "rieltor123456");

			smtpClient.Port = 587;
			oMessage.To.Clear();
			oMessage.To.Add(new MailAddress(sMailTo));
			try
			{
				smtpClient.Send(oMessage);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void SendErrorReport(string exceptionText, string exceptionDetails)
		{
			Send("arsen.kostandyan@gmail.com", 
				string.Format("Rieltor Error from {0}", System.Configuration.ConfigurationManager.AppSettings["Company"]), 
				string.Format("Exception Text: <b>{0}</b><br />Exception Details: <b>{1}</b>", 
				exceptionText, 
				exceptionDetails));
		}
	}
}

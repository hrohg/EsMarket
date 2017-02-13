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
			SmtpClient smtpClient = new SmtpClient("mail.gmail.com");

            oMessage.From = new MailAddress("easyshoplogistics@gmail.com", "ES market", Encoding.UTF8);

			oMessage.Subject = sSubject;
			oMessage.IsBodyHtml = true;
			oMessage.Body = sBody;

            smtpClient.Credentials = new NetworkCredential("easyshoplogistics@gmail.com", "easyshop@)!$");

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
			Send("hrayrgishyan@gmail.com", 
				string.Format("ES Error from {0}", "Company"), 
				string.Format("Exception Text: <b>{0}</b><br />Exception Details: <b>{1}</b>", 
				exceptionText, 
				exceptionDetails));
		}
	}
}

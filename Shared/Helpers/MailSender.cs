using System.Net.Mail;
using System.Text;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using ES.Common.Helpers;
using ES.Data.Models;

namespace Shared.Helpers
{
    /// <summary>
    /// Summary description for MailSender
    /// </summary>
    public class MailSender
    {
        public static bool SendMessageToSupport(string sMailTo, string sSubject, string sBody)
        {
            var host = "mail.ess.am";
            var port = 26;


            MailMessage oMessage = new MailMessage();

            oMessage.From = new MailAddress("support@ess.am", "ES market", Encoding.UTF8);
            oMessage.HeadersEncoding = Encoding.UTF8;
            oMessage.BodyEncoding = Encoding.UTF8;
            oMessage.Subject = sSubject;
            oMessage.IsBodyHtml = true;
            oMessage.Body = sBody;
            oMessage.To.Clear();
            oMessage.To.Add(new MailAddress(sMailTo));

            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Host = host;
                smtpClient.Port = port;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = false;
                //smtpClient.Credentials = new NetworkCredential("easyshoplogistics@gmail.com", "easyshop@)!$");
                smtpClient.Credentials = new NetworkCredential("support@ess.am", "Jz.DNpJhx[p&'88Q");
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                try
                {
                    smtpClient.Send(oMessage);
                    smtpClient.Dispose();
                    return true;
                }
                catch
                {
                    smtpClient.Dispose();
                    MessageBox.Show(string.Format("Ողջույն \n'{0}' հաղորդագրությունն ուղարկել չի ստացվել: Խնդրում եմ փորձել ևս մեկ անգամ կամ դիմել ծրագրի սպասարկման թիմին {1} հասցեով: \nՇնորհակլություն համագործակցության համար:", sSubject, "support@ess.am"), "Հաղորդագրություններ", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return false;
                }
            }
        }

        public static void SendErrorReport(string exceptionText, string exceptionDetails, string note, string reporter)
        {

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Background, () =>
            {
                SendMessageToSupport("support@ess.am",
                    string.Format("ES Market exception ({0})", reporter),
                    string.Format(" <b>Ծանուցում սխալի վերաբերյալ</b><br/>" +

                                  "<p style=\"\"font-family:Arial;font-size: 12pt;>{0}</p><br/>" +

                                  "Exception Text: <b>{1}</b><br />" +

                                  "<p style=\"\"font-family:Arial;font-size: 12pt;\"\">" +
                                  "Exception Details: <b>{2}</b></p>",
                    note,
                    exceptionText,
                    exceptionDetails));
            });

        }

        public static bool SendMessageFromBrunch(string sMailTo, string sSubject, string sBody, BranchModel branch)
        {
            sBody = string.Format("{0}<br/><br/>{1}<br/><br/>{2}", branch.NoReplayMailSettings.MailHeader, sBody, branch.NoReplayMailSettings.MailFooter);
            MailMessage oMessage = new MailMessage();

            oMessage.From = new MailAddress(branch.NoReplayMailSettings.Email, branch.Name, Encoding.UTF8);
            oMessage.HeadersEncoding = Encoding.UTF8;
            oMessage.BodyEncoding = Encoding.UTF8;
            oMessage.Subject = sSubject;
            oMessage.IsBodyHtml = true;
            oMessage.Body = sBody;
            oMessage.To.Clear();
            oMessage.To.Add(new MailAddress(sMailTo));

            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Host = branch.NoReplayMailSettings.SmtpServer;
                smtpClient.Port = branch.NoReplayMailSettings.SmtpPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(branch.NoReplayMailSettings.Email, branch.NoReplayMailSettings.MailPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                try
                {
                    smtpClient.Send(oMessage);
                    smtpClient.Dispose();
                    return true;
                }
                catch
                {
                    smtpClient.Dispose();
                    MessageBox.Show(string.Format("Ողջույն \n'{0}' հաղորդագրությունն ուղարկել չի ստացվել: Խնդրում եմ փորձել ևս մեկ անգամ կամ դիմել ծրագրի սպասարկման թիմին {1} հասցեով: \nՇնորհակլություն համագործակցության համար:", sSubject, "support@ess.am"), "Հաղորդագրություններ", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return false;
                }
            }
        }
    }
}

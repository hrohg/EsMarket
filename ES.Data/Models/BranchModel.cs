using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Xml.Serialization;
using ES.Common.Helpers;

namespace ES.Data.Models
{
    [Serializable]
    public class BranchModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public List<EssMailSettings> MailSettings { get; set; }
        [XmlIgnore]
        public EssMailSettings NoReplayMailSettings { get { return MailSettings.FirstOrDefault(); } }

        public bool UseDiscountBond { get; set; }

        public double DefaultDiscount { get; set; }
        public bool IsActive { get; set; }
        public bool IsNotify { get; set; }

        public long MemberId { get; set; }

        public BranchModel(long memberId)
        {
            Id = Guid.NewGuid();
            MemberId = memberId;
            MailSettings = new List<EssMailSettings>(){new EssMailSettings(memberId)};
        }
    }
    [Serializable]
    public class EssMailSettings : ISerializable
    {
        public string Email { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string MailHeader { get; set; }
        public string MailFooter { get; set; }
        [XmlIgnore]
        public SecureString MailPassword { get; set; }
        public EmailType EmailType { get; set; }
        public long MemberId { get; set; }

        public EssMailSettings(long memberId)
        {
            MemberId = memberId;
        }
        public EssMailSettings(SerializationInfo info, StreamingContext context)
        {
            Email = (string)info.GetValue("Email", typeof(string));
            SmtpServer = (string)info.GetValue("SmtpServer", typeof(string));
            SmtpPort = (int)info.GetValue("SmtpPort", typeof(int));
            MailHeader = (string)info.GetValue("MailHeader", typeof(string));
            MailFooter = (string)info.GetValue("MailFooter", typeof(string));
            MailPassword = StringHelper.Decrypt((string)info.GetValue("MailPassword", typeof(string))).ToSecureString();
            EmailType = (EmailType)info.GetValue("EmailType", typeof(int));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Email", Email);
            info.AddValue("SmtpServer", SmtpServer);
            info.AddValue("SmtpPort", SmtpPort, typeof(int));
            info.AddValue("MailHeader", MailHeader);
            info.AddValue("MailFooter", MailFooter);
            info.AddValue("MailPassword", StringHelper.Encrypt(MailPassword.ToUnsecureString()));
            info.AddValue("EmailType", EmailType, typeof(int));
        }


    }

    public enum EmailType{NoReplay}
}

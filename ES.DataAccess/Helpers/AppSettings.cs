namespace ES.DataAccess.Helpers
{
    /// <summary>
    /// Contains properties returning values of the appSection in configuration file
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// Returns database connection provider for cash back from appSettings configuration section
        /// </summary>
        public static string EslServerConnectionProvider
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["EslServerConnectionProvider"];
                return retval != null ? retval.Replace("serverPas", "esstockdb@)!$") : "System.Data.SqlClient";
            }
        }
        /// <summary>
        /// Returns database connection provider for cash back from appSettings configuration section
        /// </summary>
        public static string EslConnectionProvider
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["EslConnectionProvider"];
                return retval != null ? retval : "System.Data.SqlClient";
            }
        }

        /// <summary>
        /// Returns relative url path of the account registration page from appSettings configuration section
        /// </summary>
        public static string AccountRegister
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["AccountRegister"];
                return retval != null ? retval : "/Account/Register.aspx";
            }
        }

        /// <summary>
        /// Returns relative url path of the invitation instructtions from appSettings configuration section
        /// </summary>
        public static string InviteInstructtions
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["InviteInstructtions"];
                return retval != null ? retval : "/instruction/cashback";
            }
        }

        /// <summary>
        /// Returns relative physical path of the mail template bodies dictory from appSettings configuration section
        /// </summary>
        public static string MailBodies
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["MailBodies"];
                return retval != null ? retval : @"\App_Data\MailBodies\";
            }
        }

        /// <summary>
        /// Returns file name of the russian template of the invitation mail body from appSettings configuration section
        /// </summary>
        public static string InviteMailBodyRu
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["Invite_Ru"];
                return retval != null ? retval : "Invite_Ru.html";
            }
        }

        /// <summary>
        /// Returns Cashback system's web host URL
        /// </summary>
        public static string EslWebHost
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["CashBackWebHost"];
                return retval != null ? retval : "https://www.Esl.am";
            }
        }

        /// <summary>
        /// Returns Cashback system's web help relative URL path
        /// </summary>
        public static string CashBackWebHelp
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["CashBackWebHelp"];
                return retval != null ? retval : "/Webhelp/Help.html";
            }
        }

        /// <summary>
        /// Returns file name of the russian template of the registration mail body from appSettings configuration section
        /// </summary>
        public static string RegisterMemberMailBodyRu
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["RegisterMember_Ru"];
                return retval != null ? retval : "RegisterMember_Ru.html";
            }
        }

        /// <summary>
        /// Returns member images' folder relative path from appSettings configuration section
        /// </summary>
        public static string MemberImages
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["MemberImages"];
                return retval != null ? retval : "MemberImages";
            }
        }

        /// <summary>
        /// Returns service member images' relative url from appSettings configuration section
        /// </summary>
        public static string ServiceMemberImagesRelativeURL
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["ServiceMemberImages"];
                return retval != null ? retval : "ServiceMemberImages";
            }
        }

        /// <summary>
        /// Returns super cashback images' relative url from appSettings configuration section
        /// </summary>
        public static string SuperCashbackImagesRelativeURL
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["SuperCashbackImages"];
                return retval != null ? retval : "SuperCashbackImages";
            }
        }

        /// <summary>
        /// Returns file name of the russian template of the service memeber registration mail body from appSettings configuration section
        /// </summary>
        public static string ServiceMemberRegisterRu
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["ServiceMemberRegister_Ru"];
                return retval != null ? retval : "ServiceMemberRegister_Ru.html";
            }
        }

        /// <summary>
        /// Returns file name of the russian template of the service memeber registration for manager mail body from appSettings configuration section
        /// </summary>
        public static string ServiceMemberRegisterForManagerRu
        {
            get
            {
                string retval = System.Configuration.ConfigurationManager.AppSettings["ServiceMemberRegisterForManager_Ru"];
                return retval != null ? retval : "ServiceMemberRegisterForManager_Ru.html";
            }
        }
    }
}

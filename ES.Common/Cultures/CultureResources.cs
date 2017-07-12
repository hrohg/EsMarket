using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace ES.Common.Cultures
{
    [Serializable]
    public class CustomCultureInfo : CultureInfo
    {
        public CustomCultureInfo(string name)
            : base(name)
        {
        }

        public CustomCultureInfo(string name, bool useUserOverride)
            : base(name, useUserOverride)
        {
        }

        public CustomCultureInfo(int culture)
            : base(culture)
        {
        }

        public CustomCultureInfo(int culture, bool useUserOverride)
            : base(culture, useUserOverride)
        {
        }

        public override string NativeName
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            return base.NativeName.Substring(0, base.NativeName.IndexOf("(") - 1).ToLower();
        }
    }

    public class CultureResources : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private static List<CustomCultureInfo> pSupportedCultures;
        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public static List<CustomCultureInfo> SupportedCultures
        {
            get
            {
                if (pSupportedCultures == null)
                {
                    pSupportedCultures = new List<CustomCultureInfo>();
                    LoadCultureResources();
                }
                return pSupportedCultures;
            }
        }

        private static void LoadCultureResources()
        {
            CultureInfo tCulture;
            String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            foreach (string dir in Directory.GetDirectories(Path.GetDirectoryName(exePath)))
            {
                try
                {
                    //see if this directory corresponds to a valid culture name
                    DirectoryInfo dirinfo = new DirectoryInfo(dir);
                    tCulture = CultureInfo.GetCultureInfo(dirinfo.Name);

                    //determine if a resources dll exists in this directory that matches the executable name
                    if (dirinfo.GetFiles(Path.GetFileName(exePath.Replace(".dll", ".resources.dll"))).Length > 0)
                    {
                        pSupportedCultures.Add(new CustomCultureInfo(tCulture.LCID));
                    }
                }
                catch (ArgumentException) //ignore exceptions generated for any unrelated directories in the bin folder
                {
                }
            }
        }


        private static ObjectDataProvider m_provider;
        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (m_provider == null)
                    m_provider = (ObjectDataProvider)Application.Current.FindResource("CultureResources");
                return m_provider;
            }
        }

        public static bool HasCulture
        {
            get
            {
                return Application.Current != null && ResourceProvider != null;
            }
        }

        private static CultureResources inst;
        public static CultureResources Inst
        {
            get
            {
                if (inst == null)
                    if (ResourceProvider != null)
                    {
                        inst = m_provider.ObjectInstance as CultureResources;
                    }
                    else
                    {
                        throw new Exception("Не найдены ресурсы");
                    }
                return inst;
            }
        }

        public string this[string resourceKey]
        {
            get
            {
                try
                {
                    return Resources.ResourceManager.GetString(resourceKey);
                }
                catch
                {
                    return resourceKey;
                }
            }
        }

        /// <summary>
        /// Change the current culture used in the application.  
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        /// <param name="culture">Culture to change to</param>
        public static void ChangeCulture(CustomCultureInfo culture)
        {
            //remain on the current culture if the desired culture cannot be found
            // - otherwise it would revert to the default resources set, which may or may not be desired.
            if (SupportedCultures.Contains(culture))
            {
                Resources.Culture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                ResourceProvider.Refresh();
                Inst.OnPropertyChanged("Inst");
            	InvokeCultureChanged();
            }
            else
                Debug.WriteLine(string.Format("culture {0} is not supported", culture.Name));
        }

    	public static event EventHandler CultureChanged;

    	public static void InvokeCultureChanged()
    	{
    		EventHandler handler = CultureChanged;
    		if (handler != null) handler(null, new EventArgs());
    	}
    }
}
using System.Windows;

namespace ResourceLibrary.Helper
{
    public class ResourceKeys
    {
        public static ComponentResourceKey NoChromeButtonKey { get { return new ComponentResourceKey(typeof(ResourceKeys), "NoChromeButton"); } }
        public static ComponentResourceKey NoChromeToggleButtonKey { get { return new ComponentResourceKey(typeof(ResourceKeys), "NoChromeToggleButton"); } }
    }
}

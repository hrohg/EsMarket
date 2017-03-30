using System;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    [Serializable]
    public class RootVersion : LayoutElement
    {
        public RootVersion()
        {
            ErrorMessage = "";
            ErrorCaption = "Error";
            Description = "";
            /*int major, int minor, int build, int revision*/
            Version = "0.0.0.0";
        }
        public string ErrorCaption { get; set; }
        public string ErrorMessage { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
    }
}

using System.IO;
using System.Windows.Input;
using ES.Common.Helpers;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace ES.Market.ViewModels
{
    public class AvalonDockLayoutViewModel
    {
        #region Internal properties

        private string _layoutFileName = "Layout.config";
        #endregion Internal properties

        #region External properties
        #endregion External properties

        #region Constructors
        public AvalonDockLayoutViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            LoadLayoutCommand = new RelayCommand(OnLoad);
            SaveLayoutCommand = new RelayCommand(OnSave);
        }
        private void OnLoad(object o)
        {
             DockingManager docManager = o as DockingManager;

            if (docManager == null)
                return;

            this.LoadDockingManagerLayout(docManager);
        }
        private void OnSave(object o)
        {
            string xmlLayout = o as string;

            if (xmlLayout == null)
                return;

            this.SaveDockingManagerLayout(xmlLayout);
        }

        #region LoadLayout
        /// <summary>
        /// Loads the layout of a particular docking manager instance from persistence
        /// and checks whether a file should really be reloaded (some files may no longer
        /// be available).
        /// </summary>
        /// <param name="docManager"></param>
        private void LoadDockingManagerLayout(DockingManager docManager)
        {
            //string layoutFileName = System.IO.Path.Combine(Workspace.DirAppData, Workspace.LayoutFileName);

            if (System.IO.File.Exists(_layoutFileName) == false)
                return;

            var layoutSerializer = new XmlLayoutSerializer(docManager);

            layoutSerializer.LayoutSerializationCallback += (s, args) =>
            {
                // This can happen if the previous session was loading a file
                // but was unable to initialize the view ...
                if (args.Model.ContentId == null)
                {
                    args.Cancel = true;
                    return;
                }

                AvalonDockLayoutViewModel.ReloadContentOnStartUp(args);
            };

            layoutSerializer.Deserialize(_layoutFileName);
        }

        private static void ReloadContentOnStartUp(LayoutSerializationCallbackEventArgs args)
        {
            string sId = args.Model.ContentId;

            // Empty Ids are invalid but possible if aaplication is closed with File>New without edits.
            if (string.IsNullOrWhiteSpace(sId) == true)
            {
                args.Cancel = true;
                return;
            }

            //if (args.Model.ContentId == FileStatsViewModel.ToolContentId)
            //    args.Content = Workspace.This.FileStats;
            else
            {
                args.Content = AvalonDockLayoutViewModel.ReloadDocument(args.Model.ContentId);

                if (args.Content == null)
                    args.Cancel = true;
            }
        }

        private static object ReloadDocument(string path)
        {
            object ret = null;

            if (!string.IsNullOrWhiteSpace(path))
            {
                switch (path)
                {
                    /***
                              case StartPageViewModel.StartPageContentId: // Re-create start page content
                                if (Workspace.This.GetStartPage(false) == null)
                                {
                                  ret = Workspace.This.GetStartPage(true);
                                }
                                break;
                    ***/
                    default:
                        // Re-create text document
                        //ret = Workspace.This.Open(path);
                        break;
                }
            }

            return ret;
        }
        #endregion LoadLayout

        #region SaveLayout
        private void SaveDockingManagerLayout(string xmlLayout)
        {
            // Create XML Layout file on close application (for re-load on application re-start)
            if (xmlLayout == null)
                return;

            //string fileName = System.IO.Path.Combine(Workspace.DirAppData, Workspace.LayoutFileName);

            //File.WriteAllText(fileName, xmlLayout);
        }
        #endregion SaveLayout

        #endregion methods

        #region External Methods

        #endregion External methods

        #region Commands
        public ICommand LoadLayoutCommand { get; private set; }

        public ICommand SaveLayoutCommand { get; private set; }
        #endregion Commands

    }

}

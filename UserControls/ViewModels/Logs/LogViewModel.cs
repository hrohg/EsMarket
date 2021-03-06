﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Logs
{
    public class LogViewModel : ToolsViewModel
    {
        #region Internal properties
        private readonly object _sync = new object();
        #endregion Internal properties

        #region External properties
        public ObservableCollection<MessageModel> Logs { get; set; }

        #region Current Log
        private MessageModel _currentLog;
        public MessageModel CurrentLog
        {
            get { return _currentLog; }
            private set
            {
                if (_currentLog == value) return;
                _currentLog = value;
                RaisePropertyChanged("CurrentLog");
            }
        }

        #endregion Current Log

        #endregion External properties

        #region Constructors

        public LogViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ստացված հաղորդագրություններ";
            CanFloat = true;
            IsClosable = false;
            AnchorSide = AnchorSide.Bottom;
            Logs = new ObservableCollection<MessageModel>();
        }
        #endregion Internal methods

        #region External methods

        public void AddLog(MessageModel log)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    lock (_sync)
                    {
                        Logs.Insert(0, log);
                        CurrentLog = log;
                    }
                }));
        }
        #endregion External methods

        #region Commands
        #endregion Commands
    }
}

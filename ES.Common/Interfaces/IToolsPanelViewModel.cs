using ES.Common.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ES.Common.Interfaces
{
    public interface IToolsPanelViewModel : IQueryable, IFindReplace, IDisposable
    {       
        TabBase Parent { get; set; }
        event EventHandler LoadedCompleted;
        void ShowParametersPopup(Action callBack);
        List<object> SelectedLayoutAnchorables { get; }
        bool IsSettingsOpen { get; set; }
        string Description { get; set; }
        void UpdateData();
        void UpdateEditorQuery();
        bool CheckParameters(Action onRunEvaluationResult);
        void DisplayAndAddMessage(string msg);
        void UpdateTimeZone();
        void UpdateDateTime();
        void UpdateTickType();
        void PopulateAntlrCollection();
        void PopulateOtherProperties();
        string GetParameterValue(string expression);
    }
}

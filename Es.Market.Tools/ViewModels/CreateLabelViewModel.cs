using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Es.Market.Tools.Helpers;
using Es.Market.Tools.Interfaces;
using Es.Market.Tools.Models;
using ES.Common.ViewModels.Base;

namespace Es.Market.Tools.ViewModels
{
    public class CreateLabelViewModel:ViewModelBase
    {
        public string Title { get { return "Mange Price tags";} }
        public ILabelTag Label { get; private set; }

        public CreateLabelViewModel(LabelType type)
        {
            Label = new LabelModelBase(type);
        }
    }
}

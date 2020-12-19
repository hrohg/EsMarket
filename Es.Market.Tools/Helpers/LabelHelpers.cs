using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Es.Market.Tools.Models;

namespace Es.Market.Tools.Helpers
{
    public static class LabelHelpers
    {
        public static LabelSize GetLableSize(LabelType type)
        {
            var labelSize = new LabelSize();
            switch (type)
            {
                case LabelType.Standard:
                    labelSize.SetWidthInCm(5);
                    labelSize.SetHeightInCm(4);
                    break;
                case LabelType.PriceDropped:
                    labelSize.SetWidthInCm(5);
                    labelSize.SetHeightInCm(4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }

            return labelSize;
        }
    }
}

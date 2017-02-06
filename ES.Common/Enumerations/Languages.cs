using System.Collections.Generic;

namespace ES.Common.Enumerations
{
	public class LanguageEntity
	{
		public string Name { get; set; }
		public string LanguageValue { get; set; }
		public int ID { get; set; }

		public static List<LanguageEntity> GetLanguages()
		{
			return new List<LanguageEntity>
			       	{
			       		new LanguageEntity{ ID=1, LanguageValue="hy-AM", Name="Հայերեն"},
			       		new LanguageEntity{ ID=2, LanguageValue="ru-RU", Name="Russian"},
			       		new LanguageEntity{ ID=3, LanguageValue="en-US", Name="English"},
			       		new LanguageEntity{ ID=4, LanguageValue="cs-CZ", Name="Czech"},
			       	};
		}
	}
}

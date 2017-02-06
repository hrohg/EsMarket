using System;
using System.Collections.Generic;

namespace ES.Common.Enumerations
{
	public class OfficePlaceTypeEntity
	{
		public string Name { get; set; }
		public int ID { get; set; }

		public static List<OfficePlaceTypeEntity> GetOfficePlaceTypes()
		{
			List<OfficePlaceTypeEntity> list = new List<OfficePlaceTypeEntity>();
			var enumValues = Enum.GetValues(typeof(OfficePlaceType));
			foreach (var value in enumValues)
			{
				OfficePlaceType type = (OfficePlaceType)value;
				list.Add(new OfficePlaceTypeEntity { ID = (int)type, Name = type.ToString() });
			}
			
			return list;
		}
	}

	public enum OfficePlaceType
	{
		ProductionArea = 1,
		ShoppingArea = 2,
		ServiceArea = 3,
	}
}

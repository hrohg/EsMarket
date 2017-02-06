using System;
using System.Collections.Generic;

namespace ES.Common.Enumerations
{
	public class EarthPlaceTypeEntity
	{
		public string Name { get; set; }
		public int ID { get; set; }

		public static List<EarthPlaceTypeEntity> GetEarthPlaceTypes()
		{
			List<EarthPlaceTypeEntity> list = new List<EarthPlaceTypeEntity>();
			var enumValues = Enum.GetValues(typeof(EarthPlaceType));
			foreach (var value in enumValues)
			{
				EarthPlaceType type = (EarthPlaceType	)value;
				list.Add(new EarthPlaceTypeEntity { ID = (int)type, Name = type.ToString() });
			}
			
			return list;
			
		}
	}

	public enum EarthPlaceType
	{
		NearHome = 1,
		Industrial = 2,
		Аgricultural=3,
		Society = 4,
		
	};
}

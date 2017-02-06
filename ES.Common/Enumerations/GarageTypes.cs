using System;
using System.Collections.Generic;

namespace ES.Common.Enumerations
{
	public class GarageTypeEntity
	{
		public string Name { get; set; }
		public int ID { get; set; }

		public static List<GarageTypeEntity> GetGarageTypes()
		{
			List<GarageTypeEntity> list = new List<GarageTypeEntity>();
			var enumValues = Enum.GetValues(typeof(GarageType));
			foreach (var value in enumValues)
			{
				GarageType type = (GarageType)value;
				list.Add(new GarageTypeEntity { ID = (int)type, Name = type.ToString() });
			}
			return list;
		}
	}

	public enum GarageType
	{
		FromSteel = 1,
		FromStone = 2,
		UnderGround = 3
	}

}

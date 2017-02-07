using System;
using System.Collections.Generic;

namespace ES.Common.Enumerations
{
	public enum ReportTypes
	{
		ShortReport,
		Report
	}

	public class ReportTypeEntity
	{
		public string Name { get; set; }
		public int ID { get; set; }

		public static List<ReportTypeEntity> GetReportTypes()
		{
			List<ReportTypeEntity> list = new List<ReportTypeEntity>();
			var enumValues = Enum.GetValues(typeof(ReportTypes));
			foreach (var value in enumValues)
			{
				ReportTypes type = (ReportTypes)value;
				list.Add(new ReportTypeEntity { ID = (int)type, Name = type.ToString() });
			}

			return list;
		}
	}
}

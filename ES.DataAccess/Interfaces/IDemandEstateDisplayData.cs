namespace ES.DataAccess.Interfaces
{
	public interface IDemandEstateDisplayData
	{
		string ClientName { get; }
		string Rooms { get; }
		string Square { get;  }
		string Regions { get;}
		string Price { get; }
		int ID { get; }
		string RentSell { get; }
		string EstateTypes { get; }
	}
}

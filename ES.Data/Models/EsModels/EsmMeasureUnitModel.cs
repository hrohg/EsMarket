namespace ES.Data.Models.EsModels
{
    public class EsmMeasureUnitModel
    {
        public short Id { get; set; }
        public int? DisplayOrder { get; set; }
        public string Key { get; set; }
        public string KeyValue { get { return Key; } }
        public string Name { get; set; }
        public string NameValue { get { return Key; } }
        public decimal? Ratio { get; set; }
        public bool? IsWeight { get; set; }
        public int? GroupId { get; set; } 
    }
}

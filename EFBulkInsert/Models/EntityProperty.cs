namespace EFBulkInsert.Models
{
    internal class EntityProperty
    {
        public string ColumnName { get; set; }
        public bool IsDbGenerated { get; set; }
        public string SqlServerType { get; set; }
        public bool IsNullable { get; set; }
        public string PropertyName { get; set; }
    }
}
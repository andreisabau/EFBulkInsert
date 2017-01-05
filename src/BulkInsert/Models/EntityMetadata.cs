using System;
using System.Collections.Generic;

namespace EFBulkExtensions.BulkInsert.Models
{
    internal class EntityMetadata
    {
        public string TempTableName { get; set; }
        public IEnumerable<EntityProperty> Properties { get; set; }
        public Type Type { get; set; }
        public string TableName { get; set; }
    }
}
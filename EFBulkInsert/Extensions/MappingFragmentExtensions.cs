using System.Data.Entity.Core.Mapping;

namespace EFBulkInsert.Extensions
{
    internal static class MappingFragmentExtensions
    {
        public static string GetTableName(this MappingFragment mappingFragment)
        {
            string schema = mappingFragment.StoreEntitySet.Schema;
            string tableName = (string) (mappingFragment.StoreEntitySet.MetadataProperties["Table"].Value ??
                                         mappingFragment.StoreEntitySet.Name);

            return !string.IsNullOrEmpty(schema) ? $"{schema}.{tableName}" : tableName;
        }
    }
}
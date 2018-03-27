using EFBulkInsert.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using static System.String;

namespace EFBulkInsert.Extensions
{
    internal static class DbContextExtensions
    {
        public static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            SqlConnection sqlConnection = (SqlConnection)dbContext.Database.Connection;

            return sqlConnection;
        }

        public static SqlTransaction GetSqlTransaction(this DbContext dbContext)
        {
            SqlTransaction sqlTransaction = (SqlTransaction)dbContext.Database.CurrentTransaction?.UnderlyingTransaction;

            return sqlTransaction;
        }

        public static ObjectContext GetObjectContext(this DbContext dbContext)
        {
            ObjectContext objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;

            return objectContext;
        }

        public static EntityMetadata GetEntityMetadata<T>(this DbContext dbContext)
        {
            MetadataWorkspace metadataWorkspace = dbContext.GetObjectContext().MetadataWorkspace;
            MappingFragment mappingFragment = metadataWorkspace.GetMapping<T>();
            EntityType storageSpaceEntityType = mappingFragment.StoreEntitySet.ElementType;

            EntityMetadata entityMetadata = new EntityMetadata
            {
                TempTableName = "##TEMP_" + Guid.NewGuid().ToString().Replace('-', '_'),
                Type = typeof(T),
                TableName = mappingFragment.GetTableName(),
                Properties = storageSpaceEntityType.Properties.Select(x => new EntityProperty
                {
                    ColumnName = x.Name,
                    IsDbGenerated = x.IsStoreGeneratedIdentity || x.IsStoreGeneratedComputed,
                    SqlServerType = GetSqlServerType(x),
                    IsNullable = x.Nullable,
                    PropertyName = GetPropertyName(mappingFragment, x)
                }).ToList()
            };

            return entityMetadata;
        }

        private static string GetPropertyName(MappingFragment mappingFragment, EdmProperty x)
        {
            ScalarPropertyMapping scalarPropertyMapping = mappingFragment.PropertyMappings.Select(y => (ScalarPropertyMapping)y)
                                                                                          .FirstOrDefault(y => y.Column.Name == x.Name);

            if (scalarPropertyMapping != null)
            {
                return scalarPropertyMapping.Property.Name;
            }

            throw new Exception($"Cannot extract property name {x.Name}.");
        }

        private static string GetSqlServerType(EdmProperty edmProperty)
        {
            string type = edmProperty.TypeName;

            if (!edmProperty.IsMaxLengthConstant)
            {
                string length = edmProperty.Scale.HasValue ? edmProperty.Precision.HasValue ? $"({edmProperty.Precision},{edmProperty.Scale})"
                                                                                            : $"({edmProperty.Precision})"
                                                           : edmProperty.MaxLength.HasValue ? $"({edmProperty.MaxLength})"
                                                                                            : Empty;

                type = type + length;
            }

            if (edmProperty.Nullable)
            {
                type = $"{type} NULL";
            }

            return type;
        }
    }
}
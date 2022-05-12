using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using EFBulkInsert.Extensions;
using EFBulkInsert.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace EFBulkInsert;

public static class BulkInsertExtension
{
    public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> entities, int batchSize = 5000)
    {
        T[] entitiesArray = entities.ToArray();

        if (entitiesArray.Any())
        {
            EntityMetadata entityMetadata = dbContext.GetEntityMetadata<T>();

            OpenDatabaseConnection(dbContext);

            DataTable dataTable = CreateTempTable<T>(dbContext, entityMetadata);

            for (int i = 0; i < entitiesArray.Length; i += batchSize)
            {
                T[] entitiesInBatch = entitiesArray.Skip(i).Take(batchSize).ToArray();

                InsertDataIntoTempTable(dbContext, entityMetadata, i, entitiesInBatch, dataTable);

                DataSet dataSet = MergeDataIntoOriginalTable(dbContext, entityMetadata, i);

                CopyGeneratedPropertiesToEntities(entityMetadata, dataSet, entitiesInBatch, i);
            }

            DropTempTable(dbContext, entityMetadata);
        }
    }

    private static DataTable CreateTempTable<T>(DbContext dbContext, EntityMetadata entityMetadata)
    {
        DataTable dataTable = new();

        List<string> columns = new();

        foreach (EntityProperty property in entityMetadata.Properties.Where(x => !x.IsDbGenerated))
        {
            columns.Add($"[{property.ColumnName}] {property.SqlServerType}");

            PropertyInfo propertyInfo = typeof(T).GetProperty(property.PropertyName);

            DataColumn dataColumn = new(property.ColumnName)
            {
                DataType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType,
                AllowDBNull = property.IsNullable,
                ColumnName = property.ColumnName
            };

            dataTable.Columns.Add(dataColumn);
        }

        dataTable.Columns.Add("ArrayIndex", typeof(long));
        columns.Add("ArrayIndex bigint");

        string createTableQuery = $"CREATE TABLE {entityMetadata.TempTableName} ({Join(",", columns)})";

        dbContext.Database.ExecuteSqlRaw(createTableQuery);

        return dataTable;
    }

    private static void CopyGeneratedPropertiesToEntities<T>(EntityMetadata entityMetadata, DataSet mergeResult,
        T[] entities, int startIndex)
    {
        foreach (EntityProperty property in entityMetadata.Properties.Where(x => x.IsDbGenerated))
        {
            for (int i = 0; i < mergeResult.Tables[0].Rows.Count; i++)
            {
                long index = (long)mergeResult.Tables[0].Rows[i]["ArrayIndex"] - startIndex;

                T entity = entities[index];

                entity.GetType().GetProperty(property.PropertyName)
                    .SetValue(entity, mergeResult.Tables[0].Rows[i][property.ColumnName]);
            }
        }
    }

    private static DataSet MergeDataIntoOriginalTable(DbContext dbContext, EntityMetadata entityMetadata,
        int startIndex)
    {
        string generatedColumnNames =
            entityMetadata.Properties.Any(x => x.IsDbGenerated)
                ? $", {Join(",", entityMetadata.Properties.Where(x => x.IsDbGenerated).Select(x => $"INSERTED.[{x.ColumnName}]"))}"
                : Empty;

        string columns = Join(",",
            entityMetadata.Properties.Where(x => !x.IsDbGenerated).Select(x => $"[{x.ColumnName}]"));

        SqlCommand sqlCommand = new($@"MERGE INTO {entityMetadata.TableName} AS DestinationTable	
                                                      USING (SELECT * FROM {entityMetadata.TempTableName} WHERE ArrayIndex >= {startIndex}) AS TempTable	
                                                      ON 1 = 2	
                                                      WHEN NOT MATCHED THEN INSERT ({columns}) VALUES ({columns})	
                                                      OUTPUT TempTable.ArrayIndex{generatedColumnNames};",
            dbContext.GetSqlConnection(), dbContext.GetSqlTransaction());

        SqlDataAdapter dataAdapter = new(sqlCommand);
        DataSet dataSet = new();
        dataAdapter.Fill(dataSet);

        return dataSet;
    }

    private static void InsertDataIntoTempTable<T>(DbContext dbContext, EntityMetadata entityMetadata,
        int startIndex, IReadOnlyList<T> entities, DataTable dataTable)
    {
        SqlBulkCopy sqlBulkCopy = new(dbContext.GetSqlConnection(), SqlBulkCopyOptions.Default,
            dbContext.GetSqlTransaction())
        {
            DestinationTableName = entityMetadata.TempTableName
        };

        dataTable.Clear();

        for (int i = 0; i < entities.Count; i++)
        {
            List<object> objects = entityMetadata.Properties.Where(x => !x.IsDbGenerated)
                .Select(property =>
                    GetPropertyValueOrDbNull(typeof(T).GetProperty(property.PropertyName)
                        .GetValue(entities[i], null)))
                .ToList();

            objects.Add(startIndex + i);
            dataTable.Rows.Add(objects.ToArray());
        }

        sqlBulkCopy.WriteToServer(dataTable);
    }

    private static void DropTempTable(DbContext dbContext, EntityMetadata entityMetadata)
    {
        string deleteCommand = $"DROP TABLE {entityMetadata.TempTableName}";

        dbContext.Database.ExecuteSqlRaw(deleteCommand);
    }

    private static void OpenDatabaseConnection(DbContext dbContext)
    {
        try
        {
            dbContext.Database.OpenConnection();
        }
        catch (Exception)
        {
            // ignored	
        }
    }

    private static object GetPropertyValueOrDbNull(object @object)
    {
        return @object ?? DBNull.Value;
    }
}

using System;
using System.Linq;
using EFBulkInsert.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFBulkInsert.Extensions;

internal static class DbContextExtensionsCore
{
    public static SqlConnection GetSqlConnection(this DbContext dbContext)
    {
        SqlConnection sqlConnection = (SqlConnection) dbContext.Database.GetDbConnection();

        return sqlConnection;
    }

    public static SqlTransaction GetSqlTransaction(this DbContext dbContext)
    {
        SqlTransaction sqlTransaction = dbContext.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;

        return sqlTransaction;
    }

    public static EntityMetadata GetEntityMetadata<T>(this DbContext dbContext)
    {
        IEntityType entityType = dbContext.Model.FindEntityType(typeof(T));

        EntityMetadata entityMetadata = new()
        {
            TempTableName = "##TEMP_" + Guid.NewGuid().ToString().Replace('-', '_'),
            Type = typeof(T),
            TableName = entityType.GetTableName(),
            Properties = entityType.GetProperties().Select(x => new EntityProperty
            {
                ColumnName = x.GetColumnName(),
                IsDbGenerated = x.ValueGenerated == ValueGenerated.OnAddOrUpdate || x.ValueGenerated == ValueGenerated.OnAdd,
                SqlServerType = GetSqlServerType(x),
                IsNullable = x.IsNullable,
                PropertyName = x.Name
            }).ToList()
        };

        return entityMetadata;
    }

    private static string GetSqlServerType(IProperty property)
    {
        string type = property.GetColumnType();

        if (property.IsNullable)
        {
            type = $"{type} NULL";
        }

        return type;
    }
}
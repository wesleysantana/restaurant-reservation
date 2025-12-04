using Microsoft.EntityFrameworkCore;

namespace RestaurantReservation.Identity.Context;

public static class ModelBuilderExtensions
{
    public static void ApplyLowerCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Tabela em minúsculas
            if (entity.GetTableName() is string tableName)
            {
                entity.SetTableName(tableName.ToLower());
            }

            // Colunas em minúsculas
            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnName() is string columnName)
                {
                    property.SetColumnName(columnName.ToLower());
                }
            }

            // Chaves primárias
            var primaryKey = entity.FindPrimaryKey();
            if (primaryKey?.GetName() is string pkName)
            {
                primaryKey.SetName(pkName.ToLower());
            }

            // Chaves estrangeiras
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                if (foreignKey.GetConstraintName() is string fkName)
                {
                    foreignKey.SetConstraintName(fkName.ToLower());
                }
            }

            // Índices
            foreach (var index in entity.GetIndexes())
            {
                if (index.GetDatabaseName() is string indexName)
                {
                    index.SetDatabaseName(indexName.ToLower());
                }
            }
        }
    }
}
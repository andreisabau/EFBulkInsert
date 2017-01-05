using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace EFBulkExtensions.BulkInsert.Extensions
{
    internal static class MetadataWorkspaceExtensions
    {
        public static EntityType GetObjectSpaceEntityByType<T>(this MetadataWorkspace metadataWorkspace)
        {
            ObjectItemCollection objectItemCollection = (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);

            EntityType entityType = objectItemCollection.GetItems<EntityType>()
                                                        .FirstOrDefault(x => objectItemCollection.GetClrType(x) == typeof(T));

            return entityType;
        }

        public static MappingFragment GetMappings<T>(this MetadataWorkspace metadataWorkspace)
        {
            EntityType objectSpaceEntityType = metadataWorkspace.GetObjectSpaceEntityByType<T>();

            EntitySet entitySet = metadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace)
                                                   .SelectMany(x => x.EntitySets)
                                                   .FirstOrDefault(x => x.ElementType.Name == objectSpaceEntityType.Name);

            EntitySetMapping entitySetMapping = metadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                                                 .SelectMany(x => x.EntitySetMappings)
                                                                 .First(x => x.EntitySet == entitySet);

            MappingFragment entityType = entitySetMapping.EntityTypeMappings.SelectMany(x => x.Fragments)
                                                                            .First();

            return entityType;
        }
    }
}
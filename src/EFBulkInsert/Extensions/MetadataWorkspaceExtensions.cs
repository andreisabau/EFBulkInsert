using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace EFBulkInsert.Extensions
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

        public static MappingFragment GetMapping<T>(this MetadataWorkspace metadataWorkspace)
        {
            EntityType entityType = metadataWorkspace.GetObjectSpaceEntityByType<T>();

            EntitySet entitySet = metadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace)
                                                   .SelectMany(x => x.EntitySets)
                                                   .FirstOrDefault(x => x.ElementType.Name == entityType.Name);

            EntitySetMapping entitySetMapping = metadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                                                 .SelectMany(x => x.EntitySetMappings)
                                                                 .First(x => x.EntitySet == entitySet);

            MappingFragment mappingFragment = entitySetMapping.EntityTypeMappings.SelectMany(x => x.Fragments)
                                                                                 .First();

            return mappingFragment;
        }
    }
}
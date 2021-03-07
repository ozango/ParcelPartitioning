using FluentNHibernate.Mapping;

namespace ParcelPartitioning.Data.Maps
{
    public class ParcelPartitionMap : ClassMap<ParcelPartition>
    {
        public ParcelPartitionMap()
        {
            Table("parcel_partition");
            Schema("parcel_partition");
            Id(x => x.Id, "id").Precision(8).GeneratedBy.Increment();
            Map(x => x.BoxId, "box_id").Precision(8).Not.Nullable();
            Map(x => x.PartWeight, "part_weight").Precision(4).Not.Nullable();
            Map(x => x.PartCost, "part_cost").Precision(8).Not.Nullable();
        }
    }
}

using FluentNHibernate.Mapping;

namespace ParcelPartitioning.Data.Maps
{
    public class ParcelMap : ClassMap<Parcel>
    {
        public ParcelMap()
        {
            Schema("parcel_partition");
            Table("parcel");
            Id(x => x.ParcelId, "parcel_id").Precision(8).GeneratedBy.Increment();
            Map(x => x.Weight, "weight").Precision(4).Not.Nullable();
            Map(x => x.PartCount, "part_count").Precision(2).Not.Nullable();
            HasMany<ParcelPartition>(x => x.ParcelPartition).KeyColumn("box_id").Inverse().LazyLoad();
        }
    }
}

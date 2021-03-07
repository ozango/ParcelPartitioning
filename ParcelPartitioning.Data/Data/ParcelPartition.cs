using NHibernate.Validator.Constraints;


namespace ParcelPartitioning.Data
{
    public class ParcelPartition
    {
        [NotNullNotEmpty]
        public virtual long BoxId { get; set; }
        [NotNullNotEmpty]
        public virtual long Id { get; set; }
        [NotNullNotEmpty]
        public virtual int PartWeight { get; set; }
        [NotNullNotEmpty]
        public virtual long PartCost { get; set; }
    }
}

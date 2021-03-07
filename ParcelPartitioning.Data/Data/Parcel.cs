using NHibernate.Validator.Constraints;
using System.Collections.Generic;

namespace ParcelPartitioning.Data
{
    public class Parcel
    {
        public Parcel()
        {
            ParcelPartition = new List<ParcelPartition>();
        }
        [NotNullNotEmpty]
        public virtual short PartCount { get; set; }
        [NotNullNotEmpty]
        public virtual int Weight { get; set; }
        [NotNullNotEmpty]
        public virtual long ParcelId { get; set; }
        public virtual IList<ParcelPartition> ParcelPartition { get; set; }
    }
}

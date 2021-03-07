using System.Collections.Generic;

namespace ParcelPartitioning.Data.Model
{
    public class ParcelCostReportResponse
    {
        public IEnumerable<ParcelPartition> parcelPartitions { get; set; }
        public long totalCost { get; set; }
    }
}

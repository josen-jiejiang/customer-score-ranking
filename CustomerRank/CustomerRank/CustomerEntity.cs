using Microsoft.Extensions.Caching.Memory;

namespace CustomerRank
{
    public class CustomerEntity
    {
        /// <summary>
        /// CustomerID
        /// </summary>
        public long CustomerID { set; get; }

        /// <summary>
        /// Score
        /// </summary>
        public double Score { set; get; }

        /// <summary>
        /// MemoryCache
        /// </summary>
        public IMemoryCache MemoryCache { set; get; }
    }

    public class CustomerResultEntity
    {
        /// <summary>
        /// CustomerID
        /// </summary>
        public long CustomerID { set; get; }

        /// <summary>
        /// Score
        /// </summary>
        public double Score { set; get; }

        /// <summary>
        /// Rank
        /// </summary>
        public int Rank { set; get; }
    }
}

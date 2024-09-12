using Microsoft.Extensions.Caching.Memory;

namespace CustomerRank
{
    public class CustomerScoreSortHandler : ChannelHandler<CustomerEntity>
    {
        private const string _customerScoreCacheListKey = "CustomerScoreCacheListKey";

        public CustomerScoreSortHandler()
        {
        }

        /// <summary>
        /// Implement customer score ranking
        /// </summary>
        /// <param name="customer"></param>
        public override void Invoke(CustomerEntity customer)
        {
            customer.MemoryCache.TryGetValue(_customerScoreCacheListKey, out List<CustomerEntity> value);

            var customers = value == null ? [] : value;

            var customerItem = customers.FirstOrDefault(s => s.CustomerID == customer.CustomerID);
            if (customerItem == null)
                customers.Add(customer);
            else
                customerItem.Score = customer.Score;

            customers = customers.Where(s => s.Score > 0).OrderByDescending(s => s.Score).ThenBy(s => s.CustomerID).ToList();

            customer.MemoryCache.Set(_customerScoreCacheListKey, customers);
        }
    }
}

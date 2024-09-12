using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;
using System.Reflection.Metadata;

namespace CustomerRank.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CustomerController : ControllerBase
    {
        #region Cache Achieve
        private readonly ILogger<CustomerController> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string _customerScoreCacheKey = "CustomerScoreCacheKey";
        private const string _customerScoreCacheListKey = "CustomerScoreCacheListKey";

        public CustomerController(ILogger<CustomerController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Update Score
        /// </summary>
        /// <param name="customerid"> arbitrary positive int64 number </param>
        /// <param name="score"> 
        /// Adecimal numberin range of [-1000, +1000]. When it is
        /// positive, this number represents the score to be increased by; When
        /// it is negative, this number represents the score to be decreased by
        /// </param>
        /// <returns>Current score after update</returns>
        [HttpPost]
        [Route("~/customer/{customerid}/score/{score}")]
        public async Task<double> UpdateScore(long customerid, [Range(-1000, 1000)] double score)
        {
            _memoryCache.TryGetValue(_customerScoreCacheKey + customerid, out double value);

            score = score + value < 0 ? 0 : score + value;

            _memoryCache.Set(_customerScoreCacheKey + customerid, score);

            _ = ChannelContext<CustomerEntity, CustomerScoreSortHandler>.UnBoundedChannel.Writer.TryWrite(new CustomerEntity() { CustomerID = customerid, Score = score, MemoryCache = _memoryCache });

            return score;
        }

        /// <summary>
        ///  Get customers by rank
        /// </summary>
        /// <param name="start">start rank, included in response if exists</param>
        /// <param name="end">endrank, included in response if exists</param>
        /// <returns>A JSON structure represents the found customers with rank and score.</returns>
        [HttpGet]
        [Route("~/leaderboard")]
        public async Task<string> GetCustomersByRank([Range(1, int.MaxValue), Required] int start, [Range(1, int.MaxValue), Required] int end)
        {
            //The query parameters must comply with logical specifications
            if (start > end)
                return "The ranking parameters for the query do not meet the specifications. Please re-enter";

            var customers = GetCustomers();

            if (customers.Count < start)
                return "There is no data within the current ranking range";

            //Obtain the number of rows for customer ranking
            int count = end - start + 1;
            if (customers.Count < end)
                count = customers.Count - start + 1;

            var items = customers.GetRange(start - 1, count);

            var result = new List<CustomerResultEntity>();

            foreach (var item in items)
            {
                result.Add(new CustomerResultEntity() { CustomerID = (long)item.CustomerID, Score = item.Score, Rank = start });
                start++;
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Get customers by customerid
        /// </summary>
        /// <param name="customerid"> customer to lookup in leaderboard </param>
        /// <param name="high">optional. Default zero. number of neighbors whose rank is higher than the specified customer.</param>
        /// <param name="low">optional. Default zero. number of neighbors whose rank is lower than the specified customer.</param>
        /// <returns> A JSON structure represents the found customer and its nearest neighborhoods.</returns>
        [HttpGet]
        [Route("~/leaderboard/{customerid}/")]
        public async Task<string> GetCustomersByCustomerid(long customerid, [Range(1, int.MaxValue)] int high, [Range(1, int.MaxValue)] int low)
        {
            var customers = GetCustomers();
            var customer = customers.FirstOrDefault(s => s.CustomerID == customerid);
            //Retrieve user information and return user ranking
            if (customer == null)
                return "The user currently being queried does not exist";

            var customerIndexOf = customers.IndexOf(customer);

            int beginIndexOf = customerIndexOf - high > 0 ? customerIndexOf - high : 0;
            int endIndexOf = customerIndexOf + low;
            if (customers.Count <= customerIndexOf + low)
                endIndexOf = customers.Count - 1;

            var items = customers.GetRange(beginIndexOf, endIndexOf - beginIndexOf + 1);

            var result = new List<CustomerResultEntity>();

            foreach (var item in items)
            {
                beginIndexOf++;
                result.Add(new CustomerResultEntity() { CustomerID = (long)item.CustomerID, Score = item.Score, Rank = beginIndexOf });
            }

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtain a collection of customer information
        /// </summary>
        /// <returns></returns>
        private List<CustomerEntity> GetCustomers()
        {
            _memoryCache.TryGetValue(_customerScoreCacheListKey, out List<CustomerEntity> value);
            return value == null ? [] : value;
        }
        #endregion

        #region Json Achieve (To be improved)
        //private readonly ILogger<CustomerController> _logger;
        //private List<CustomerEntity> customers = new List<CustomerEntity>();
        //public CustomerController(ILogger<CustomerController> logger)
        //{
        //    _logger = logger;
        //    Init();
        //}

        //[NonAction]
        //private void Init()
        //{
        //    var customerData = System.IO.File.ReadAllText("CustomerData.json");
        //    customerData = string.IsNullOrEmpty(customerData) ? "[]" : customerData;
        //    customers = JsonConvert.DeserializeObject<List<CustomerEntity>>(customerData);
        //}

        ///// <summary>
        ///// Update Score
        ///// </summary>
        ///// <param name="customerid"> arbitrary positive int64 number </param>
        ///// <param name="score"> 
        ///// Adecimal numberin range of [-1000, +1000]. When it is
        ///// positive, this number represents the score to be increased by; When
        ///// it is negative, this number represents the score to be decreased by
        ///// </param>
        ///// <returns>Current score after update</returns>
        //[HttpPost]
        //[Route("~/customer/{customerid}/score/{score}")]
        //public async Task<double> UpdateScore(long customerid, [Range(-1000, 1000)] double score)
        //{
        //    lock (this)
        //    {
        //        var customer = customers.FirstOrDefault(s => s.CustomerID == customerid);

        //        if (customer == null)
        //        {
        //            if (score <= 0) return 0;
        //            customers.Add(new CustomerEntity() { CustomerID = customerid, Score = score });
        //        }
        //        else
        //        {
        //            score = customer.Score += score;
        //            if (customer.Score <= 0)
        //                customers.Remove(customer);
        //        }

        //        customers = customers.OrderByDescending(s => s.Score).ThenBy(s => s.CustomerID).ToList();

        //        var customerData = JsonConvert.SerializeObject(customers);
        //        System.IO.File.WriteAllText("CustomerData.json", customerData);
        //        return score;
        //    }
        //}

        ///// <summary>
        /////  Get customers by rank
        ///// </summary>
        ///// <param name="start">start rank, included in response if exists</param>
        ///// <param name="end">endrank, included in response if exists</param>
        ///// <returns>A JSON structure represents the found customers with rank and score.</returns>
        //[HttpGet]
        //[Route("~/leaderboard")]
        //public async Task<string> GetCustomersByRank([Range(1, int.MaxValue), Required] int start, [Range(1, int.MaxValue), Required] int end)
        //{
        //    return "";
        //}

        ///// <summary>
        ///// Get customers by customerid
        ///// </summary>
        ///// <param name="customerid"> customer to lookup in leaderboard </param>
        ///// <param name="high">optional. Default zero. number of neighbors whose rank is higher than the specified customer.</param>
        ///// <param name="low">optional. Default zero. number of neighbors whose rank is lower than the specified customer.</param>
        ///// <returns> A JSON structure represents the found customer and its nearest neighborhoods.</returns>
        //[HttpGet]
        //[Route("~/leaderboard/{customerid}/")]
        //public async Task<string> GetCustomersByCustomerid(long customerid, [Range(1, int.MaxValue)] int high, [Range(1, int.MaxValue)] int low)
        //{
        //    return "";
        //}

        ///// <summary>
        ///// simultaneous requests Test
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<double> SimultaneousRequestsTest()
        //{
        //    _logger.LogInformation($"Concurrent task testing starts, start time:{DateTime.Now}");
        //    var beginDatetime = DateTime.Now;

        //    var stopwatch = Stopwatch.StartNew();

        //    //Concurrent 10000 tasks
        //    var tasks = new Task[10000];
        //    for (int i = 0; i < tasks.Length; i++)
        //    {
        //        double score = i * 2 - 7;
        //        tasks[i] = Task.Run(() => UpdateScore(i, score));
        //    }

        //    //Waiting for all tasks to be completed
        //    await Task.WhenAll(tasks);

        //    stopwatch.Stop();

        //    var endDatetime = DateTime.Now;

        //    _logger.LogInformation($"Concurrent task testing completed, end time:{DateTime.Now}");
        //    return (endDatetime - beginDatetime).TotalSeconds;
        //}
        #endregion

        #region Redis Achieve
        //private readonly ILogger<CustomerController> _logger;
        //private readonly RedisConnection _redisConnection;
        //private const string redisKey = "CustomerScoreRedisKey";

        //public CustomerController(ILogger<CustomerController> logger, RedisConnection redisConnection)
        //{
        //    _logger = logger;
        //    _redisConnection = redisConnection;
        //}

        ///// <summary>
        ///// Update Score
        ///// </summary>
        ///// <param name="customerid"> arbitrary positive int64 number </param>
        ///// <param name="score"> 
        ///// Adecimal numberin range of [-1000, +1000]. When it is
        ///// positive, this number represents the score to be increased by; When
        ///// it is negative, this number represents the score to be decreased by
        ///// </param>
        ///// <returns>Current score after update</returns>
        //[HttpPost]
        //[Route("~/customer/{customerid}/score/{score}")]
        //public async Task<double> UpdateScore(long customerid, [Range(-1000, 1000)] double score)
        //{
        //    var customeridString = customerid.ToString();

        //    //Get user score, return null if there is no user information
        //    var customerScore = _redisConnection.SortedSetScoreByMember(redisKey, customeridString);
        //    if (customerScore != null)
        //        score = customerScore.Value + score;

        //    score = score > 0 ? score : 0;

        //    //If the score is 0, remove the leaderboard
        //    if (score == 0)
        //        _ = await _redisConnection.SortedSetRemoveAsync(redisKey, customeridString);
        //    //Add user information, update if it exists
        //    else
        //        _ = await _redisConnection.SortedSetAddAsync(redisKey, customeridString, score);

        //    return score;
        //}

        ///// <summary>
        /////  Get customers by rank
        ///// </summary>
        ///// <param name="start">start rank, included in response if exists</param>
        ///// <param name="end">endrank, included in response if exists</param>
        ///// <returns>A JSON structure represents the found customers with rank and score.</returns>
        //[HttpGet]
        //[Route("~/leaderboard")]
        //public async Task<string> GetCustomersByRank([Range(1, int.MaxValue), Required] int start, [Range(1, int.MaxValue), Required] int end)
        //{
        //    //The query parameters must comply with logical specifications
        //    if (start > end)
        //        return "The ranking parameters for the query do not meet the specifications. Please re-enter";

        //    //The ranking of the query parameters needs to be converted into an index for querying Redis, and the scores should be queried in reverse order. The higher the score, the higher the ranking
        //    var itemsByRank = await _redisConnection.SortedSetRangeByRankWithScoresAsync(redisKey, start - 1, end - 1, Order.Descending);

        //    //Exception when there is no ranking data in the current index interval
        //    if (!itemsByRank.Any())
        //        return "There is no data in the current ranking range";

        //    var result = new List<CustomerResultEntity>();

        //    foreach (var item in itemsByRank)
        //    {
        //        result.Add(new CustomerResultEntity() { CustomerID = (long)item.Element, Score = item.Score, Rank = start });
        //        start++;
        //    }

        //    return JsonConvert.SerializeObject(result);
        //}

        ///// <summary>
        ///// Get customers by customerid
        ///// </summary>
        ///// <param name="customerid"> customer to lookup in leaderboard </param>
        ///// <param name="high">optional. Default zero. number of neighbors whose rank is higher than the specified customer.</param>
        ///// <param name="low">optional. Default zero. number of neighbors whose rank is lower than the specified customer.</param>
        ///// <returns> A JSON structure represents the found customer and its nearest neighborhoods.</returns>
        //[HttpGet]
        //[Route("~/leaderboard/{customerid}/")]
        //public async Task<string> GetCustomersByCustomerid(long customerid, [Range(1, int.MaxValue)] int high, [Range(1, int.MaxValue)] int low)
        //{
        //    //Retrieve user information and return user ranking
        //    var customerRenk = _redisConnection.SortedSetRankByMember(redisKey, customerid.ToString());
        //    if (customerRenk == null)
        //        return "The user currently being queried does not exist";

        //    //The query parameters need to be converted into indexes for querying Redis
        //    high = (int)customerRenk.Value - high > 0 ? (int)customerRenk.Value - high : 0;
        //    low = (int)customerRenk.Value + low;

        //    var itemsByRank = await _redisConnection.SortedSetRangeByRankWithScoresAsync(redisKey, high, low, Order.Descending);
        //    //Exception when there is no ranking data in the current index interval
        //    if (!itemsByRank.Any())
        //        return "There is no data in the current ranking range";

        //    var result = new List<CustomerResultEntity>();

        //    foreach (var item in itemsByRank)
        //    {
        //        high++;
        //        result.Add(new CustomerResultEntity() { CustomerID = (long)item.Element, Score = item.Score, Rank = high });
        //    }

        //    return JsonConvert.SerializeObject(result);
        //}

        ///// <summary>
        ///// simultaneous requests Test
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<double> SimultaneousRequestsTest()
        //{
        //    _logger.LogInformation($"Concurrent task testing starts, start time:{DateTime.Now}");
        //    var beginDatetime = DateTime.Now;

        //    var stopwatch = Stopwatch.StartNew();

        //    //Concurrent 100000 tasks
        //    var tasks = new Task[100000];
        //    for (int i = 1; i < tasks.Length; i++)
        //    {
        //        int taskId = i;
        //        double score = i * 2 - 7;
        //        tasks[i] = Task.Run(() => UpdateScore(taskId, score));
        //    }

        //    //Waiting for all tasks to be completed
        //    await Task.WhenAll(tasks);

        //    stopwatch.Stop();

        //    var endDatetime = DateTime.Now;

        //    _logger.LogInformation($"Concurrent task testing completed, end time:{DateTime.Now}");
        //    return (endDatetime - beginDatetime).TotalSeconds;
        //}
        #endregion
    }
}
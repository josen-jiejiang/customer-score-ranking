using StackExchange.Redis;

namespace CustomerRank
{
    public class RedisConnection
    {
        #region Init
        private static IDatabase _db;
        private static ConnectionMultiplexer _redis;
        private static IConfiguration _configuration;

        /// <summary>
        /// Constructor, register Redis events in it
        /// </summary>
        public RedisConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _redis = ConnectionMultiplexer.Connect(_configuration.GetValue("RedisClient:Connect", "127.0.0.1:6379,abortConnect=false,defaultDatabase=15"));
            _db = _redis.GetDatabase();
        }
        #endregion

        #region zset 
        /// <summary>
        /// Retrieve the score of the current key and member values, return null if none exist
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public double? SortedSetScoreByMember(string redisKey, string member)
        {
            if (string.IsNullOrEmpty(redisKey) || string.IsNullOrEmpty(member))
                return null;
            return _db.SortedSetScore(redisKey, member);
        }

        /// <summary>
        ///Retrieve the ranking of the current key and member values, return null if none exist
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public double? SortedSetRankByMember(string redisKey, string member, Order order = Order.Descending)
        {
            if (string.IsNullOrEmpty(redisKey) || string.IsNullOrEmpty(member))
                return null;
            return _db.SortedSetRank(redisKey, member, order);
        }

        /// <summary>
        /// Retrieve all member values for the current key
        /// </summary>
        /// <param name="redisKey"></param>
        /// <returns></returns>
        public async Task<RedisValue[]> SortedSetRangeByValueAsync(string redisKey)
        {
            return await _db.SortedSetRangeByValueAsync(redisKey);
        }

        /// <summary>
        /// Get elements by ranking, startRank=0, endRank=-1, take all
        /// </summary>
        /// <param name="redisKey">key</param>
        /// <param name="order">sort</param>
        /// <returns></returns>
        public async Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(string redisKey, long startRank, long endRank, Order order)
        {
            return await _db.SortedSetRangeByRankWithScoresAsync(redisKey, startRank, endRank, order);
        }

        /// <summary>
        /// Get elements by score range
        /// </summary>
        /// <param name="redisKey">key</param>
        /// <param name="order">sort</param>
        /// <returns></returns>
        public async Task<RedisValue[]> SortedSetRangeByScoreAsync(string redisKey, double startScore, double endScore)
        {
            return await _db.SortedSetRangeByScoreAsync(redisKey, startScore, endScore);
        }

        /// <summary>
        /// Add elements, update if they exist
        /// </summary>
        /// <param name="redisKey">key</param>
        /// <param name="member">member(</param>
        /// <param name="score">score</param>
        /// <returns></returns>
        public async Task<bool> SortedSetAddAsync(string redisKey, string member, double score)
        {
            return await _db.SortedSetAddAsync(redisKey, member, score);
        }

        /// <summary>
        /// Removing Elements
        /// </summary>
        /// <param name="redisKey">key</param>
        /// <param name="member">member</param>
        /// <returns></returns>
        public async Task<bool> SortedSetRemoveAsync(string redisKey, string member)
        {
            return await _db.SortedSetRemoveAsync(redisKey, member);
        }
        #endregion
    }
}
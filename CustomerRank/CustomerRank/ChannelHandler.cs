namespace CustomerRank
{
    /// <summary>
    /// Process pipeline communication processing program
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class ChannelHandler<TMessage>
    {
        /// <summary>
        /// Pipeline actuator
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract void Invoke(TMessage message);
    }
}

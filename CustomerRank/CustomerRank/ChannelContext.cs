using System.Reflection.Metadata;
using System.Threading.Channels;

namespace CustomerRank
{
    /// <summary>
    /// Communication context within the process pipeline
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    public sealed class ChannelContext<TMessage, THandler>
        where THandler : ChannelHandler<TMessage>
    
    {
        /// <summary>
        /// Create an infinite capacity channel through lazy loading
        /// </summary>
        private static readonly Lazy<Channel<TMessage>> _unBoundedChannel = new Lazy<Channel<TMessage>>(() =>
        {
            var channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions
            {
                // Allow multiple pipelines to read and write, providing pipeline throughput (unordered operation)
                SingleReader = false,   
                SingleWriter = false
            });

            StartReader(channel);
            return channel;
        });

        /// <summary>
        /// Unlimited capacity channel
        /// </summary>
        public static Channel<TMessage> UnBoundedChannel => _unBoundedChannel.Value;

        /// <summary>
        /// Create a reader
        /// </summary>
        /// <param name="channel"></param>
        private static void StartReader(Channel<TMessage> channel)
        {
            var reader = channel.Reader;

            // Create a long-term thread pipeline reader
            _ = Task.Factory.StartNew(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    if (!reader.TryRead(out var message))
                    {
                        continue;
                    }

                    try
                    {
                        var Handler = Activator.CreateInstance<THandler>();
                        Handler.Invoke(message);
                        Handler = null;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}

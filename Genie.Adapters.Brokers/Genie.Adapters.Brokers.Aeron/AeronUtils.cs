using Adaptive.Aeron.LogBuffer;
using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;

namespace Genie.Adapters.Brokers.Aeron;

public class AeronUtils
{
    const int fragmentLimitCount = 10;
    const int streamId = 10;


    public static AeronSubscription SetupSubscriber(Adaptive.Aeron.Aeron aeron, string host, int streamId)
    {
        var aeronsub = new AeronSubscription
        {
            Subscription = aeron.AddSubscription(host, streamId)
        };

        IIdleStrategy idleStrategy = new BusySpinIdleStrategy();

        var fragmentHandler = HandlerHelper.ToFragmentHandler((buffer, offset, length, header) =>
        {
            var data = new byte[length];
            buffer.GetBytes(offset, data);

            aeronsub.Data = data;
            aeronsub.ReceiveSignal.Set();
        });

        Task.Run(() =>
        {
            while (true)
            {
                var fragmentsRead = aeronsub.Subscription.Poll(fragmentHandler, fragmentLimitCount);
                idleStrategy.Idle(fragmentsRead);
            }
        });

        return aeronsub;
    }
}

public class AeronSubscription
{
    public Subscription Subscription { get; set; }
    public byte[] Data { get; set; }
    public AutoResetEvent ReceiveSignal = new(false);
}

using Adaptive.Aeron.LogBuffer;
using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genie.Common.Types;
using Chr.Avro.Abstract;

namespace Genie.Common.Utils
{
    public class AeronUtils
    {
        const int fragmentLimitCount = 10;
        const int streamId = 10;


        public static AeronSubscription SetupSubscriber(Aeron aeron, string host, int streamId)
        {
            var aeronsub = new AeronSubscription();
            aeronsub.Subscription = aeron.AddSubscription(host, streamId);

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
}

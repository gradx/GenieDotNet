using Apache.NMS;
using Apache.NMS.Util;
using Genie.Common;
using Genie.Common.Performance;

namespace Genie.Adapters.Brokers.ActiveMQ;

public class ActiveMQPooledObject : GeniePooledObject
{
    IConnection? ingress;
    IConnection? egress;
    public Apache.NMS.ISession? IngressSession { get; set; }
    public Apache.NMS.ISession? EgressSession { get; set; }
    public IMessageProducer? Producer { get; set; }
    public IMessageConsumer? Consumer { get; set; }

    public void Configure(GenieContext genieContext)
    {
        Uri connecturi = new(genieContext.ActiveMQ.ConnectionString);
        var factory = new NMSConnectionFactory(connecturi);
        ingress = factory.CreateConnection(genieContext.ActiveMQ.Username, genieContext.ActiveMQ.Password);
        egress = factory.CreateConnection(genieContext.ActiveMQ.Username, genieContext.ActiveMQ.Password);
        IngressSession = ingress.CreateSession();
        EgressSession = egress.CreateSession();

        // sharing ingressSession causes an interleaving message error
        // TODO: Fix retry with replyto set
        Consumer = EgressSession.CreateConsumer(SessionUtil.GetDestination(EgressSession, $@"queue://{this.EventChannel}")); // DestinationType.TemporaryQueue));

        Producer = IngressSession.CreateProducer(SessionUtil.GetDestination(IngressSession, genieContext.ActiveMQ.Ingress));
        Producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        Producer.RequestTimeout = new(300000);

        ingress.Start();
        egress.Start();
    }
}
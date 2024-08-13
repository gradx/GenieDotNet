using Apache.NMS;
using Apache.NMS.Util;
using Genie.Common;
using Genie.Common.Web;

namespace Genie.Web.Api.Common;

public class ActiveMQPooledObject : GeniePooledObject
{
    IConnection? ingress;
    IConnection? egress;
    Apache.NMS.ISession? ingressSession;
    Apache.NMS.ISession? egressSession;
    IMessageProducer? producer;
    IMessageConsumer? consumer;
    readonly AutoResetEvent semaphore = new(false);
    IMessage? response = null;
    readonly TimeSpan timeout = new(300000);

    public void Configure(GenieContext genieContext)
    {
        Uri connecturi = new("activemq:tcp://localhost:61616");
        var factory = new NMSConnectionFactory(connecturi);
        ingress = factory.CreateConnection("artemis", "artemis");
        egress = factory.CreateConnection("artemis", "artemis");
        ingressSession = ingress.CreateSession();
        egressSession = egress.CreateSession();

        // sharing ingressSession causes an interleaving message error
        // TODO: Fix retry with replyto set
        consumer = egressSession.CreateConsumer(SessionUtil.GetDestination(egressSession, $@"queue://{this.EventChannel}")); // DestinationType.TemporaryQueue));
        consumer.Listener += new MessageListener(OnMessage);


        producer = ingressSession.CreateProducer(SessionUtil.GetDestination(ingressSession, "queue://FOO.BAR"));
        //producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        producer.RequestTimeout = timeout;

        ingress.Start();
        egress.Start();
    }

    public IMessage? Send(byte[] message)
    {


        var request = ingressSession?.CreateBytesMessage(message)!;
        request.NMSCorrelationID = this.EventChannel;
        request.Properties["NMSXGroupID"] = "cheese";
        request.Properties["myHeader"] = "Cheddar";

        producer?.Send(request);

        semaphore.WaitOne();

        return response;
    }

    void OnMessage(IMessage receivedMsg)
    {
        response = receivedMsg;
        semaphore.Set();
    }
}
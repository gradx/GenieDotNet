using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
namespace SubscriberA
{
    class Program
    {
        public static IList<string> allowableCommandLineArgs
            = new[] { "TopicA", "TopicB", "All" };
        public static void Main(string[] args)
        {
            if (args.Length != 1 || !allowableCommandLineArgs.Contains(args[0]))
            {
                Console.WriteLine("Expected one argument, either " +
                                  "'TopicA', 'TopicB' or 'All'");
                Environment.Exit(-1);
            }
            string topic = args[0] == "All" ? "" : args[0];
            Console.WriteLine("Subscriber started for Topic : {0}", topic);
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:12345");
                subSocket.Subscribe(topic);
                Console.WriteLine("Subscriber socket connecting...");
                while (true)
                {
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    Console.WriteLine(messageReceived);
                }
            }
        }

        public static SubscriberSocket Test()
        {

            var subSocket = new SubscriberSocket();
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:12345");
                subSocket.Subscribe("TopicA");
            }

            return subSocket;
        }
    }
}
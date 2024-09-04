using Genie.IngressConsumer.Services;
using ZstdSharp;



while(true)
{
    Console.WriteLine(@"Enter (K)afka, (R)abbitMQ, (A)ctiveMQ, (Pr)otoActor, (Pu)lsar, (Ap)ache Pulsar, (G)enius, (Z)eroMQ, (Ae)ron, (Ae2)ron, (N)ATS, or (M)QTT");
    //var input = Console.ReadLine();
    string input = "g";

    Task task = input?.ToLower() switch
    {
        "k" => Task.Run(async () => { await KafkaService.Start(); }),
        "r" => Task.Run(async () => { await RabbitMQService.Start(); }),
        "a" => Task.Run(async () => { await ActiveMQService.Start(); }),
        "pr" => Task.Run(async () => { await ActorService.Start(); }),
        "pu" => Task.Run(async () => { await PulsarService.Start(); }),
        "ap" => Task.Run(async () => { await ApachePulsarService.Start(); }),
        "g" => Task.Run(async () => { await GeniusService.Start(); }),
        "z" => Task.Run(async () => { await ZeroMQService.Start(); }),
        "ae" => Task.Run(async () => { await AeronService.Start(); }),
        "ae2" => Task.Run(async () => { await AeronService2.Start(); }),
        "n" => Task.Run(async () => { await NatsService.Start(); }),
        "m" => Task.Run(async () => { await MQTTService.Start(); }),
        _ => Task.Run(() => { Console.WriteLine("Invalid input"); })
    };

    await Task.WhenAny([task]);
    Console.WriteLine("Consumer has exited");
    Console.WriteLine("");
    Console.WriteLine("");
}

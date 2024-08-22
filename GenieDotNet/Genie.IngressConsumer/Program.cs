using Genie.IngressConsumer.Services;
using ZstdSharp;



while(true)
{
    Console.WriteLine(@"Enter (K)afka, (R)abbitMQ, (A)ctiveMQ, (Pr)otoActor, (Pu)lsar or (Ap)ache Pulsar or (G)enius");
    var input = Console.ReadLine();

    Task task = input?.ToLower() switch
    {
        "k" => Task.Run(async () => { await KafkaService.Start(); }),
        "r" => Task.Run(async () => { await RabbitMQService.Start(); }),
        "a" => Task.Run(async () => { await ActiveMQService.Start(); }),
        "pr" => Task.Run(async () => { await ActorService.Start(); }),
        "pu" => Task.Run(async () => { await PulsarService.Start(); }),
        "ap" => Task.Run(async () => { await ApachePulsarService.Start(); }),
        "g" => Task.Run(async () => { await GeniusService.Start(); }),
        _ => Task.Run(() => { Console.WriteLine("Invalid input"); })
    };

    await Task.WhenAny([task]);
    Console.WriteLine("Consumer has exited");
    Console.WriteLine("");
    Console.WriteLine("");
}

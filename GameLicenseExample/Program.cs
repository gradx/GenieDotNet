// See https://aka.ms/new-console-template for more information
using GameLicenseExample;
using Genie.Common.Crypto.Adapters;
using Genie.Common.Crypto.Adapters.Pqc;
using Genie.Grpc;
using Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

// true true works
// false false



var game = new Game(100, KeyType.Dilithium3, KeyType.Kyber512);

var stopwatch = new Stopwatch();

Console.WriteLine("Enter to get license");
while(Console.ReadLine() != null)
{
    stopwatch.Start();
    Console.WriteLine("Getting license");
    await game.GetLicense();
    stopwatch.Stop();
    Console.WriteLine($"Completed in {stopwatch.ElapsedMilliseconds}ms");
    stopwatch.Reset();
    Console.WriteLine("Enter to get license");
}

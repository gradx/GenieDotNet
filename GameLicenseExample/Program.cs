// See https://aka.ms/new-console-template for more information
using GameLicenseExample;
using System.Diagnostics;

Console.WriteLine("Hello, World!");
var game = new Game(100);

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

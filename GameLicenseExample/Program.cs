// See https://aka.ms/new-console-template for more information
using GameLicenseExample;

Console.WriteLine("Hello, World!");
var game = new Game(100);

Console.WriteLine("Enter to get license");
while (Console.ReadLine() != null)
{
    Console.WriteLine("Getting license");
    await game.GetLicense();
}
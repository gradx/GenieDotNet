namespace GameLicenseExample;

public class GamePlay
{
    public void Test()
    {
        var game = new Game(100, Genie.Grpc.KeyType.Dilithium3, Genie.Grpc.KeyType.X25519);
        game.Risk = 1;

        var random = new Random();

        while (game.Credits > game.Risk)
        {

            var result = game.Play();

            if (game.ConsecutiveLosses > 7)
                game.Risk = 1;
            else if (game.ConsecutiveLosses > 5)
            {
                var decrease = random.Next(0, 3);
                if (decrease >= 1)
                    game.Risk = 1;
                else
                    game.Risk++;
            }
            else if (game.ConsecutiveLosses > 3)
            {
                var increase = random.Next(0, 5);
                if (increase >= 3)
                    game.Risk++;
            }

            Console.WriteLine($@"Won {result} with {game.Risk} risk and credits {game.Credits} and {game.GamesPlayed} games played");
            //Console.ReadLine();
        }

        Console.ReadLine();
    }
}
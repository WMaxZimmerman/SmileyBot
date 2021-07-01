using RLBotDotNet;

namespace SmileyBot.Console
{
    class Program
    {
        static void Main()
        {
            // Read the port from port.cfg.
            var port = int.Parse("45031");

            // BotManager is a generic which takes in your bot as its T type.
            var botManager = new BotManager<Bots.SmileyBot>();

            // Start the server on the port given in the port.cfg file.
            botManager.Start(port);
        }
    }
}

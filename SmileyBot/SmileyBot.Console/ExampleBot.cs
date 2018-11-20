using System;
using RLBotDotNet;
using rlbot.flat;
using SmileyBot.ApplicationCore.Services;

namespace SmileyBot.Console
{
    // We want to our bot to derive from Bot, and then implement its abstract methods.
    class ExampleBot : Bot
    {
        // We want the constructor for ExampleBot to extend from Bot, but we don't want to add anything to it.
        public ExampleBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) { }

        public override Controller GetOutput(GameTickPacket gameTickPacket)
        {
            // This controller object will be returned at the end of the method.
            // This controller will contain all the inputs that we want the bot to perform.
            Controller controller = new Controller();

            // // Wrap gameTickPacket retrieving in a try-catch so that the bot doesn't crash whenever a value isn't present.
            // // A value may not be present if it was not sent.
            // // These are nullables so trying to get them when they're null will cause errors, therefore we wrap in try-catch.
            // try
            // {
            //     // Store the required data from the gameTickPacket.
            //     var ball = gameTickPacket.Ball.Value;
            //     var myCar = gameTickPacket.Players(index).Value;

            //     controller.Steer = BallChasingService.GetSteeringValueToChaseBall(myCar, ball);
            // }
            // catch (Exception e)
            // {
            //     System.Console.WriteLine(e.Message);
            //     System.Console.WriteLine(e.StackTrace);
            // }

            // // Set the throttle to 1 so the bot can move.
            // controller.Throttle = 1;

            return controller;
        }
    }
}

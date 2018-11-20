using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Services;
using SmileyBot.ApplicationCore.Enums;

namespace SmileyBot.Console.Bots
{
    public class SmileyBot: Bot
    {
	private BotState State;
	private Controller Controller;
	private bool JustJumped = false;
	private bool JustDoubleJumped = false;
	private bool Flipping = false;
	
        public SmileyBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
        {
	    State = BotState.Kickoff;
        }

        public override Controller GetOutput(GameTickPacket gameTickPacket)
        {
            // This controller object will be returned at the end of the method.
            // This controller will contain all the inputs that we want the bot to perform.
            Controller = new Controller();

            // Wrap gameTickPacket retrieving in a try-catch so that the bot doesn't crash whenever a value isn't present.
            // A value may not be present if it was not sent.
            // These are nullables so trying to get them when they're null will cause errors, therefore we wrap in try-catch.
            try
            {
		PerformStateActions(gameTickPacket);
		ManageState(gameTickPacket);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
            }

            return Controller;
        }

	private void ManageState(GameTickPacket gameTickPacket)
	{
	    var ball = gameTickPacket.Ball.Value;
	    var car = gameTickPacket.Players(this.index).Value;

	    if (Flipping) FlipForwardAction(car);
	    
	    switch(State)
	    {
		case BotState.Kickoff:
		    //SendQuickChatFromAgent(false, QuickChatSelection.Information_InPosition);
		    CheckForStateChangeKickoff(car, ball);
		    break;
		case BotState.Chasing:
		    //SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
		    CheckForStateChangeChasing(gameTickPacket);
		    break;
	    }
	}

	private void CheckForStateChangeKickoff(PlayerInfo car, BallInfo ball)
	{
	    var ballLocation = ball.Physics?.Location;

	    if (ballLocation != null && (ballLocation.Value.X != 0 || ballLocation.Value.Y != 0))
	    {
		State = BotState.Chasing;
	    }
	}

	private void CheckForStateChangeChasing(GameTickPacket gameTickPacket)
	{
	    var gameInfo = gameTickPacket.GameInfo;

	    if (gameInfo != null && gameInfo.Value.IsKickoffPause)
	    {
		State = BotState.Kickoff;
	    }
	}

	private void PerformStateActions(GameTickPacket gameTickPacket)
	{
	    // Store the required data from the gameTickPacket.
	    var ball = gameTickPacket.Ball.Value;
	    var car = gameTickPacket.Players(this.index).Value;
	    
	    switch(State)
	    {
		case BotState.Kickoff:
		    PerformKickoff(car, ball);
		    break;
		case BotState.Chasing:
		    BallChasingService.ShootBallAtGoal(car, ball, ref Controller);
		    break;
	    }
	}

	private void PerformKickoff(PlayerInfo car, BallInfo ball)
        {	    
            var ballLocation = ball.Physics.Value.Location.Value;
            var carLocation = car.Physics.Value.Location.Value;
            var carRotation = car.Physics.Value.Rotation.Value;

            // Calculate to get the angle from the front of the bot's car to the ball.
            var botToTargetAngle = Math.Atan2(ballLocation.Y - carLocation.Y, ballLocation.X - carLocation.X);
            var botFrontToTargetAngle = botToTargetAngle - carRotation.Yaw;
            
            // Correct the angle
            if (botFrontToTargetAngle < -Math.PI)
	    {
		botFrontToTargetAngle += 2 * Math.PI;
	    }
            else if (botFrontToTargetAngle > Math.PI)
	    {
		botFrontToTargetAngle -= 2 * Math.PI;
	    }

            // Decide which way to steer in order to get to the ball.
            Controller.Steer = (float)botFrontToTargetAngle;
	    Controller.Throttle = 1;
	    Controller.Boost = true;

	    if (car.Boost < 1 && car.Physics?.Velocity?.Y != 0)
	    {
		FlipForward(car);
	    }
        }


	private void FlipForward(PlayerInfo car)
	{
	    if (Flipping) return;
	    FlipForwardAction(car);
	}
	
	private void FlipForwardAction(PlayerInfo car)
	{
	    Controller.Steer = 0;
	    Controller.Boost = false;
	    // System.Console.WriteLine($"{car.Name}");
	    // System.Console.WriteLine($"Jumped: {car.Jumped}");
	    // System.Console.WriteLine($"Double Jumped: {car.DoubleJumped}");
	    // System.Console.WriteLine($"Just Jumped {JustJumped}");
	    // System.Console.WriteLine($"Flipping: {Flipping}");
	    // System.Console.WriteLine($"Just Double Jumpded: {JustDoubleJumped}");
	    
	    if (JustDoubleJumped)
	    {
		//System.Console.WriteLine("Releasing seccond jump");
		Controller.Jump = false;
		if (car.HasWheelContact)
		{
		    //System.Console.WriteLine("We've landed");
		    JustDoubleJumped = false;
		    Controller.Pitch = 0;
		    Flipping = false;
		}
		return;
	    }
	    
	    if (Flipping == false)
	    {
		//Perform the First Flip
		//System.Console.WriteLine("Starting Flip");
		Flipping = true;
		Controller.Pitch = -1;
		Controller.Jump = true;
		JustJumped = true;
		return;
	    }

	    if (JustJumped)
	    {
		//Releasing first Jump
		//System.Console.WriteLine("Releasing First Jump");
		Controller.Jump = false;
		JustJumped = false;
		return;
	    }
	    else
	    {
		//System.Console.WriteLine("Pressing Second Jump");
		Controller.Pitch = -1;
		Controller.Jump = true;
		JustDoubleJumped = true;
		return;
	    }
	}
    }
}

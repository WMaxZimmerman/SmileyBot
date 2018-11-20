using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Services;
using SmileyBot.ApplicationCore.Enums;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Bots
{
    public class WrapBot: Bot
    {
	public BotState State;
	public BotAction Action;
	public Controller Controller;
	public BallInfo Ball;
	public PlayerInfo MyInfo;
	public GameInfo Game;
	
	// Shit Just For Flipping
	protected bool JustJumped = false;
	protected bool JustDoubleJumped = false;
	protected bool Flipping = false;
	protected bool SpecificFlipDirection = true;
	protected float FlipDirection = 0;
	
        public WrapBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
        {
	    State = BotState.Kickoff;
	    Action = BotAction.Default;
	    Controller = new Controller();
        }

        public override Controller GetOutput(GameTickPacket gameTickPacket)
        {
            try
            {
		//Set Reused Values
		Ball = gameTickPacket.Ball.Value;
		MyInfo = gameTickPacket.Players(this.index).Value;
		Game = gameTickPacket.GameInfo.Value;

		//Perform Bot Updates
		Update(gameTickPacket);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
            }

            return Controller;
        }

	protected virtual void Update(GameTickPacket gameTickPacket)
	{
	}

	protected void SetDesiredState(GameTickPacket gameTickPacket)	
	{
	    var ballLocation = Ball.Physics?.Location;
	    var myLocation = MyInfo.Physics?.Location;
	    
	    if (State != BotState.Kickoff)
	    {
		if (ShouldChangeToKickoff())
		{
		    SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
		    State = BotState.Kickoff;
		    return;
		}

		if (State == BotState.Chasing)
		{
		    if (ShouldChangeToDefending())
		    {
			SendQuickChatFromAgent(false, QuickChatSelection.Information_Defending);
			State = BotState.Defending;
			return;
		    }
		}
		else if (State == BotState.Defending)
		{
		    if (ShouldChangeToChasing())
		    {
			SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
			Controller.Jump = true;
			State = BotState.Chasing;
		    }
		}
	    }
	    else
	    {
		if (ballLocation != null && (ballLocation.Value.X != 0 || ballLocation.Value.Y != 0))
		{
		    SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
		    State = BotState.Chasing;
		}
	    }
	}
	
	protected virtual bool ShouldChangeToKickoff()
	{
	    return Game.IsKickoffPause;
	}

	protected virtual bool ShouldChangeToDefending()
	{
	    var shouldSwitch = false;

	    var goalLoaction = FieldService.GetMyGoal(MyInfo.Team);
	    var ballLocation = new Vec3(Ball.Physics.Value.Location.Value.X, Ball.Physics.Value.Location.Value.Y, Ball.Physics.Value.Location.Value.Z);
	    var carLocation = new Vec3(MyInfo.Physics.Value.Location.Value.X, MyInfo.Physics.Value.Location.Value.Y, MyInfo.Physics.Value.Location.Value.Z);
	    if (FieldService.CloserToTarget(ballLocation, carLocation, goalLoaction))
	    {
		shouldSwitch = true;
	    }

	    return shouldSwitch;
	}

	protected virtual bool ShouldChangeToChasing()
	{
	    var shouldSwitch = false;
	    var carY = MyInfo.Physics?.Location?.Y;
	    if (carY != null)
	    {
		var carYAbs = Math.Abs((float)carY);
		var myGoalYAbs = Math.Abs(FieldService.GetMyGoal(MyInfo.Team).Y);
		if (carYAbs >= 5000)
		{
		    shouldSwitch = true;
		}
	    }

	    return shouldSwitch;
	}

	public void Flip()
	{
	    if (Flipping) return;
	    SpecificFlipDirection = false;
	    FlipAction();
	}
	
	public void FlipForward()
	{
	    if (Flipping) return;
	    FlipDirection = 0;
	    SpecificFlipDirection = true;
	    FlipAction();
	}
	
	protected void FlipAction()
	{
	    if (SpecificFlipDirection) Controller.Steer = FlipDirection;
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
		if (MyInfo.HasWheelContact)
		{
		    //System.Console.WriteLine("We've landed");
		    JustDoubleJumped = false;
		    Controller.Pitch = 0;
		    Flipping = false;
		    Action = BotAction.Default;
		}
		return;
	    }
	    
	    if (Flipping == false)
	    {
		//Perform the First Flip
		//System.Console.WriteLine("Starting Flip");
		Action = BotAction.Flipping;
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

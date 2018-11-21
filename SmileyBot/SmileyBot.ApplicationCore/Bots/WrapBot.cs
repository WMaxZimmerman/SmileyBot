using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Services;
using SmileyBot.ApplicationCore.Enums;
using SmileyBot.ApplicationCore.Mappers;
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
	public FieldService Field;
	
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
	    Field = new FieldService(botTeam);
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
			TurnAroud();
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
	    var goalLocation = Field.GetMyGoal();
	    var ballLocation = VectorMapper.Map(Ball.Physics.Value.Location.Value);
	    var carLocation = VectorMapper.Map(MyInfo.Physics.Value.Location.Value);
	    var ballDistToGoal = Field.GetDist(ballLocation, goalLocation);

	    if (Field.IsBallOnMySide(Ball))
	    {
		if (Field.CloserToTarget(carLocation, ballLocation, goalLocation))
		{
		    shouldSwitch = true;
		}
	    }

	    return shouldSwitch;
	}

	protected virtual bool ShouldChangeToChasing()
	{
	    var shouldSwitch = false;
	    var carLocation = VectorMapper.Map(MyInfo.Physics.Value.Location.Value);
	    var ballLocation = VectorMapper.Map(Ball.Physics.Value.Location.Value);
	    var goalLocation = Field.GetMyGoal();
	    var carDistToGoal = Field.GetDist(carLocation, goalLocation);
	    var currentZone = Field.Zones.First(z => z.Rec.IsPointWithin(carLocation));

	    if ((currentZone.IsMySide && currentZone.HasGoal) || Field.MoreThanOneZoneAway(carLocation, ballLocation))
	    {
		shouldSwitch = true;
	    }

	    return shouldSwitch;
	}

	public void TurnAroud()
	{
	    Action = BotAction.TurningAround;
	    var carLocation = VectorMapper.Map(MyInfo.Physics.Value.Location.Value);
	    var ballLocation = VectorMapper.Map(Ball.Physics.Value.Location.Value);
	    var steerValue = Field.GetSteeringValueToward(MyInfo, ballLocation);
	    Controller.Steer = steerValue;
	    Controller.Handbrake = true;

	    var range = .25;
	    if (steerValue <= 0 + range && steerValue >= 0 - range)
	    {
		Action = BotAction.Default;
		Controller.Handbrake = false;
	    }
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
	    
	    if (JustDoubleJumped)
	    {
		Controller.Jump = false;
		if (MyInfo.HasWheelContact)
		{
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
		Controller.Jump = false;
		JustJumped = false;
		return;
	    }
	    else
	    {
		Controller.Pitch = -1;
		Controller.Jump = true;
		JustDoubleJumped = true;
		return;
	    }
	}

	public void GetSteeringValueToChaseBall()
        {
            var ballLocation = Ball.Physics.Value.Location.Value;

	    Controller.Steer = Field.GetSteeringValueToward(MyInfo, ballLocation);
	    Controller.Throttle = 1;
        }

	public void DribbleBallAtGoal()
        {
	    var enemyGoal = Field.GetEnemyGoal();
	    var strikeLocation = GetStrikeLocation(Ball, enemyGoal);
	    
            Controller.Steer = Field.GetSteeringValueToward(MyInfo, strikeLocation);
	    Controller.Throttle = 1;
        }
	
	public void GoToGoal()
        {
	    var myGoal = Field.GetMyGoal();	    

	    Controller.Steer = Field.GetSteeringValueToward(MyInfo, myGoal);
	    Controller.Throttle = 1;
        }

	public Vec3 GetStrikeLocation(BallInfo ball, Vec3 target)
	{
	    var ballLocation = VectorMapper.Map(ball.Physics.Value.Location.Value);
	    var hyotenuse = Field.GetDist(ballLocation, target);
	    var opposite = Math.Abs((ballLocation.X - target.X));
	    var theta = Math.Asin(opposite/hyotenuse);
	    var newHypotenuse = Field.BallRadius() + hyotenuse;

	    var strikeX = Field.BallRadius() * Math.Cos(theta) + ballLocation.X;
	    var strikeY = Field.BallRadius() * Math.Sin(theta) + ballLocation.Y;

	    return new Vec3((float)strikeX, (float)strikeY, target.Z);
	}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Bots;
using SmileyBot.ApplicationCore.Enums;
using SmileyBot.ApplicationCore.Services;

namespace SmileyBot.Console.Bots
{
    public class SmileyBot: WrapBot
    {
	public SmileyBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
        {
        }

	protected override void Update(GameTickPacket gameTickPacket)
	{
	    if (Action != BotAction.Default) PerformAction(gameTickPacket);
	    else PerformStateActions(gameTickPacket);
	}

	private void PerformAction(GameTickPacket gameTickPacket)
	{	    
	    switch(Action)
	    {
		case BotAction.Flipping:
		    FlipAction();
		    break;
		case BotAction.TurningAround:
		    TurnAroud();
		    break;
		case BotAction.Aerial:
		    Aerial();
		    break;
	    }
	}

	private void PerformStateActions(GameTickPacket gameTickPacket)
	{	    
	    switch(State)
	    {
		case BotState.Kickoff:
		    PerformKickoff();
		    break;
		case BotState.Chasing:
		    DribbleBallAtGoal();
		    CheckForShot();
		    break;
		case BotState.Defending:
		    GoToGoal();
		    break;
	    }
	    
	    SetDesiredState(gameTickPacket);
	}

	private void PerformKickoff()
	{	    
	    var ballLocation = Ball.Physics.Value.Location.Value;
	    var carLocation = MyInfo.Physics.Value.Location.Value;
	    var carRotation = MyInfo.Physics.Value.Rotation.Value;

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

	    if (MyInfo.Boost < 1 && MyInfo.Physics?.Velocity?.Y != 0)
	    {
		FlipForward();
	    }
	}

	private void CheckForShot()
	{
	    if (Field.BallIsInReach(MyInfo, Ball) && Field.BallInlineWithGoal(MyInfo, Ball))
	    {
		var target = Field.GetEnemyGoal();
		Controller.Steer = Field.GetSteeringValueToward(MyInfo, target);
		Flip();
	    }
	}
    }
}

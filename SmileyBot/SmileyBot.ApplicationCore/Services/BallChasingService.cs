using System;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Services
{
    public static class BallChasingService
    {
        public static void GetSteeringValueToChaseBall(PlayerInfo car, BallInfo ball, ref Controller controller)
        {
            var ballLocation = ball.Physics.Value.Location.Value;

	    controller.Steer = FieldService.GetSteeringValueToward(car, ballLocation);
	    controller.Throttle = 1;
        }

	public static void DribbleBallAtGoal(PlayerInfo car, BallInfo ball, ref Controller controller)
        {
	    var ballOffset = FieldService.BallRadius() / 2;
	    var enemyGoal = FieldService.GetEnemyGoal(car.Team);
	    var teamMultiplier = car.Team * 2 - 1;
	    
            var ballLocation = ball.Physics.Value.Location.Value;
	    var xMultiplier = ballLocation.X >= 0 ? 1 : -1;
	    var strikeLocation = new Vec3(ballLocation.X, ballLocation.Y, 0);

	    strikeLocation.Y = (float)(strikeLocation.Y + (ballOffset * teamMultiplier));
	    strikeLocation.X = (float)(strikeLocation.X + (ballOffset * xMultiplier));
	    
            controller.Steer = FieldService.GetSteeringValueToward(car, strikeLocation);
	    controller.Throttle = 1;
        }
	
	public static void GoToGoal(PlayerInfo car, ref Controller controller)
        {
	    var myGoal = FieldService.GetMyGoal(car.Team);	    

	    controller.Steer = FieldService.GetSteeringValueToward(car, myGoal);
	    controller.Throttle = 1;
        }
    }
}

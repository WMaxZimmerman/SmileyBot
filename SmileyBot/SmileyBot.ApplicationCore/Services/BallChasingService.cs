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
            controller.Steer = (float)botFrontToTargetAngle;
	    controller.Throttle = 1;
        }

	public static void ShootBallAtGoal(PlayerInfo car, BallInfo ball, ref Controller controller)
        {
	    var enemyGoal = FieldService.GetEnemyGoal(car.Team);
	    var teamMultiplier = car.Team * 2 - 1;
	    
            var ballLocation = ball.Physics.Value.Location.Value;
	    var xMultiplier = ballLocation.X >= 0 ? 1 : -1;
	    var strikeLocation = new Vec3(ballLocation.X, ballLocation.Y, 0);

	    strikeLocation.Y = (float)(strikeLocation.Y + (92.75 * teamMultiplier));
	    strikeLocation.X = (float)(strikeLocation.X + (92.75 * xMultiplier));
	    
	    var ballVelocity = ball.Physics.Value.Velocity.Value;
            var carLocation = car.Physics.Value.Location.Value;
            var carRotation = car.Physics.Value.Rotation.Value;

            // Calculate to get the angle from the front of the bot's car to the ball.
            // var botToTargetAngle = Math.Atan2(ballLocation.Y - carLocation.Y, ballLocation.X - carLocation.X);
            var botToTargetAngle = Math.Atan2(strikeLocation.Y - carLocation.Y, strikeLocation.X - carLocation.X);
            // var ballToTargetAngle = Math.Atan2(ballLocation.Y - enemyGoal.Y, ballLocation.X - enemyGoal.X);
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
            controller.Steer = (float)botFrontToTargetAngle;
	    controller.Throttle = 1;
        }
    }
}

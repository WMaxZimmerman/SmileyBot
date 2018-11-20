using System;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Services
{
    public static class FieldService
    {
        public static Vec3 GetMyGoal(int myTeam)
	{
	    var location = new Vec3(0, 0, 0);
	    location.Y = myTeam == 0 ? -10240 : 10240;

	    return location;
	}
	
        public static Vec3 GetEnemyGoal(int myTeam)
	{
	    var location = new Vec3(0, 0, 0);
	    location.Y = myTeam != 0 ? -10240 : 10240;

	    return location;
	}

	public static float BallRadius()
	{
	    return (float)92.75;
	}

	public static bool CloserToTarget(Vector3 locationA, Vector3 locationB, Vector3 target)
	{
	    var aVec3 = new Vec3(locationA.X, locationA.Y, locationA.Z);
	    var bVec3 = new Vec3(locationB.X, locationB.Y, locationB.Z);
	    var tVec3 = new Vec3(target.X, target.Y, target.Z);

	    return CloserToTarget(aVec3, bVec3, tVec3);
	}
	
	public static bool CloserToTarget(Vec3 locationA, Vec3 locationB, Vec3 target)
	{
	    var distA = GetDist(locationA, target);
	    var distB = GetDist(locationB, target);

	    return distA > distB;
	}

	public static double GetDist(Vec3 obj, Vec3 target)
	{
	    var deltaX = (obj.X - target.X);
	    var deltaY = (obj.Y - target.Y);
	    var dist = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
	    return dist;
	}

	public static bool BallIsInReach(PlayerInfo car, BallInfo ball)
	{
	    var ballInReach = false;
	    var ballLocation = ball.Physics?.Location;
	    var carLocation = car.Physics?.Location;

	    var ballRectX1 = ballLocation.Value.X + (FieldService.BallRadius() * 2);
	    var ballRectX2 = ballLocation.Value.X - (FieldService.BallRadius() * 2);
	    var ballRectY1 = ballLocation.Value.Y + (FieldService.BallRadius() * 2);
	    var ballRectY2 = ballLocation.Value.Y - (FieldService.BallRadius() * 2);

	    if (carLocation.Value.X <= ballRectX1 && carLocation.Value.X >= ballRectX2)
	    {
		if (carLocation.Value.Y <= ballRectY1 && carLocation.Value.Y >= ballRectY2)
		{
		    ballInReach = true;
		}
	    }
	    
	    return ballInReach;
	}

	public static bool BallInlineWithGoal(PlayerInfo car, BallInfo ball)
	{
	    var ballInlineWithGoal = false;
	    var ballLocation = ball.Physics?.Location;
	    var carLocation = car.Physics?.Location;
	    var enemyGoal = FieldService.GetEnemyGoal(car.Team);

	    var carToGoalAngle = Math.Atan2(enemyGoal.Y - carLocation.Value.Y, enemyGoal.X - carLocation.Value.X);
	    var ballToGoalAngle = Math.Atan2(enemyGoal.Y - ballLocation.Value.Y, enemyGoal.X - ballLocation.Value.X);

	    var range = 100;
	    
	    if (ballToGoalAngle <= (carToGoalAngle + range) && ballToGoalAngle >= (carToGoalAngle - range))
	    {
		// Correct the angle
		if (carToGoalAngle < -Math.PI)
		{
		    carToGoalAngle += 2 * Math.PI;
		}
		else if (carToGoalAngle > Math.PI)
		{
		    carToGoalAngle -= 2 * Math.PI;
		}
		
		ballInlineWithGoal = true;
	    }
	    
	    return ballInlineWithGoal;
	}

	public static float GetSteeringValueToward(PlayerInfo car, Vector3 targetLocation)
	{
	    var vec3 = new Vec3(targetLocation.X, targetLocation.Y, targetLocation.Z);
            return GetSteeringValueToward(car, vec3);
	}
	
	public static float GetSteeringValueToward(PlayerInfo car, Vec3 targetLocation)
	{
            var carLocation = car.Physics.Value.Location.Value;
            var carRotation = car.Physics.Value.Rotation.Value;

            // Calculate to get the angle from the front of the bot's car to the ball.
            var carToTargetAngle = Math.Atan2(targetLocation.Y - carLocation.Y, targetLocation.X - carLocation.X);
            var carFrontToTargetAngle = carToTargetAngle - carRotation.Yaw;
            
            // Correct the angle
            if (carFrontToTargetAngle < -Math.PI)
	    {
		carFrontToTargetAngle += 2 * Math.PI;
	    }
            else if (carFrontToTargetAngle > Math.PI)
	    {
		carFrontToTargetAngle -= 2 * Math.PI;
	    }

            // Decide which way to steer in order to get to the ball.
            return (float)carFrontToTargetAngle;
	}
    }
}

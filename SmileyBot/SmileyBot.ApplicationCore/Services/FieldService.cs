using System;
using System.Collections.Generic;
using System.Linq;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Services
{
    public class FieldService
    {
	private readonly int _team;

	public List<FieldZone> Zones { get; }

	public FieldService(int team)
	{
	    _team = team;

	    Zones = new List<FieldZone>();
	    for (var i = 1; i < 12; i++)
	    {
		Zones.Add(new FieldZone(i, _team));
	    }	    
	}
	
        public Vec3 GetMyGoal()
	{
	    var location = new Vec3(0, 0, 0);
	    location.Y = _team == 0 ? -5120 : 5120;

	    return location;
	}

	public bool MoreThanOneZoneAway(Vec3 obj, Vec3 target)
	{
	    var objZone = Zones.First(z => z.Rec.IsPointWithin(obj));
	    var targetZone = Zones.First(z => z.Rec.IsPointWithin(obj));

	    return (objZone.Id != targetZone.Id && objZone.IsTouchingZone(targetZone.Id) == false);
	}
	
        public Vec3 GetEnemyGoal()
	{
	    var location = new Vec3(0, 0, 0);
	    location.Y = _team != 0 ? -5120 : 5120;

	    return location;
	}

	public float BallRadius()
	{
	    return (float)92.75;
	}

	public bool CloserToTarget(Vector3 locationA, Vector3 locationB, Vector3 target)
	{
	    var aVec3 = new Vec3(locationA.X, locationA.Y, locationA.Z);
	    var bVec3 = new Vec3(locationB.X, locationB.Y, locationB.Z);
	    var tVec3 = new Vec3(target.X, target.Y, target.Z);

	    return CloserToTarget(aVec3, bVec3, tVec3);
	}
	
	public bool CloserToTarget(Vec3 locationA, Vec3 locationB, Vec3 target)
	{
	    var distA = GetDist(locationA, target);
	    var distB = GetDist(locationB, target);

	    return distA > distB;
	}

	public double GetDist(Vec3 obj, Vec3 target)
	{
	    var deltaX = (obj.X - target.X);
	    var deltaY = (obj.Y - target.Y);
	    var dist = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
	    return dist;
	}

	public bool BallIsInReach(PlayerInfo car, BallInfo ball)
	{
	    var ballInReach = false;
	    var ballLocation = ball.Physics?.Location;
	    var carLocation = car.Physics?.Location;

	    var ballRectX1 = ballLocation.Value.X + (BallRadius() * 2);
	    var ballRectX2 = ballLocation.Value.X - (BallRadius() * 2);
	    var ballRectY1 = ballLocation.Value.Y + (BallRadius() * 2);
	    var ballRectY2 = ballLocation.Value.Y - (BallRadius() * 2);

	    if (carLocation.Value.X <= ballRectX1 && carLocation.Value.X >= ballRectX2)
	    {
		if (carLocation.Value.Y <= ballRectY1 && carLocation.Value.Y >= ballRectY2)
		{
		    ballInReach = true;
		}
	    }
	    
	    return ballInReach;
	}

	public bool BallInlineWithGoal(PlayerInfo car, BallInfo ball)
	{
	    var ballInlineWithGoal = false;
	    var ballLocation = ball.Physics?.Location;
	    var carLocation = car.Physics?.Location;
	    var enemyGoal = GetEnemyGoal();

	    var carToGoalAngle = Math.Atan2(enemyGoal.Y - carLocation.Value.Y, enemyGoal.X - carLocation.Value.X);
	    var ballToGoalAngle = Math.Atan2(enemyGoal.Y - ballLocation.Value.Y, enemyGoal.X - ballLocation.Value.X);

	    var range = 45;
	    
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

	public float GetSteeringValueToward(PlayerInfo car, Vector3 targetLocation)
	{
	    var vec3 = new Vec3(targetLocation.X, targetLocation.Y, targetLocation.Z);
            return GetSteeringValueToward(car, vec3);
	}
	
	public float GetSteeringValueToward(PlayerInfo car, Vec3 targetLocation)
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

	public bool IsBallOnMySide(BallInfo ball)
	{
	    var ballY = ball.Physics.Value.Location.Value.Y;
	    var onMySide = false;

	    if (_team == 0)
	    {
		if (ballY < 0) onMySide = true;
	    }
	    else
	    {
		if (ballY > 0) onMySide = true;
	    }

	    return onMySide;
	}
    }
}

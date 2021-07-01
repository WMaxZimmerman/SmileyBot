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

        public bool BallIsInReach(PlayerWrapper car, BallWrapper ball)
        {
            var ballInReach = false;
            var ballRectX1 = ball.Location.X + (BallRadius() * 2);
            var ballRectX2 = ball.Location.X - (BallRadius() * 2);
            var ballRectY1 = ball.Location.Y + (BallRadius() * 2);
            var ballRectY2 = ball.Location.Y - (BallRadius() * 2);

            if (car.Location.X <= ballRectX1 && car.Location.X >= ballRectX2)
            {
                if (car.Location.Y <= ballRectY1 && car.Location.Y >= ballRectY2)
                {
                    ballInReach = true;
                }
            }

            return ballInReach;
        }

        public bool BallInlineWithGoal(PlayerWrapper car, BallWrapper ball)
        {
            var ballInlineWithGoal = false;
            var enemyGoal = GetEnemyGoal();

            var carToGoalAngle = Math.Atan2(enemyGoal.Y - car.Location.Y, enemyGoal.X - car.Location.X);
            var ballToGoalAngle = Math.Atan2(enemyGoal.Y - ball.Location.Y, enemyGoal.X - ball.Location.X);

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

        public float GetSteeringValueToward(PlayerWrapper car, Vec3 targetLocation)
        {
            // Calculate to get the angle from the front of the bot's car to the ball.
            var carToTargetAngle = Math.Atan2(targetLocation.Y - car.Location.Y, targetLocation.X - car.Location.X);
            var carFrontToTargetAngle = carToTargetAngle - car.Rotation.Yaw;

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

        public float GetPitchValueToward(PlayerWrapper car, Vec3 targetLocation)
        {
            // Calculate to get the angle from the front of the bot's car to the ball.
            var carToTargetAngle = Math.Atan2(targetLocation.Y - car.Location.Y, targetLocation.Z - car.Location.Z);
            var carFrontToTargetAngle = carToTargetAngle - car.Rotation.Pitch;

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

        public bool IsBallOnMySide(BallWrapper ball)
        {
            var ballY = ball.Location.Y;
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

        public bool TowardMySide(BallWrapper ball)
        {
            var towardMySide = false;

            if (_team == 0)
            {
                if (ball.Velocity.Y < 0) towardMySide = true;
            }
            else
            {
                if (ball.Velocity.Y > 0) towardMySide = true;
            }

            return towardMySide;
        }
    }
}

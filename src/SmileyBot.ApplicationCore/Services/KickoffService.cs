using System;
using rlbot.flat;
using RLBotDotNet;

namespace SmileyBot.ApplicationCore.Services
{
    public static class KickoffService
    {
        public static void PerformKickoff(PlayerInfo car, BallInfo ball, ref Controller controller)
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
            controller.Boost = true;

            if (car.Boost < 1)
            {
                controller.Pitch = -1;
                controller.Jump = true;
            }
        }
    }
}

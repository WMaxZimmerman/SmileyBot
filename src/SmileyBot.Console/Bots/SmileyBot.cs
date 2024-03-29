﻿using System;
using System.Linq;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Bots;
using SmileyBot.ApplicationCore.Enums;

namespace SmileyBot.Console.Bots
{
    public class SmileyBot : WrapBot
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
            switch (Action)
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
            switch (State)
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
                case BotState.WaitingForCenter:
                    WaitForCenter();
                    break;
            }

            SetDesiredState(gameTickPacket);
        }

        private void WaitForCenter()
        {
            var currentZone = Field.Zones.FirstOrDefault(z => z.Rec.IsPointWithin(Info.Location));
            if (currentZone == null) return;
            if (currentZone.Id == TargetZone.Id)
            {
                var ballZone = Field.Zones.FirstOrDefault(z => z.Rec.IsPointWithin(Ball.Location));
                if (ballZone != null && ballZone.HasGoal)
                {
                    DesiredState = BotState.Chasing;
                }
                else if (Field.TowardMySide(Ball))
                {
                    DesiredState = BotState.Defending;
                }
            }
            else
            {
                GoToZone();
            }
        }

        private void PerformKickoff()
        {
            // Calculate to get the angle from the front of the bot's car to the ball.
            var botToTargetAngle = Math.Atan2(Ball.Location.Y - Info.Location.Y, Ball.Location.X - Info.Location.X);
            var botFrontToTargetAngle = botToTargetAngle - Info.Rotation.Yaw;

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

            if (Info.Boost < 1 && Info.Velocity.Y != 0)
            {
                FlipForward();
            }
        }

        private void CheckForShot()
        {
            if (Field.BallIsInReach(Info, Ball) && Field.BallInlineWithGoal(Info, Ball))
            {
                var target = Field.GetEnemyGoal();
                Controller.Steer = Field.GetSteeringValueToward(Info, target);
                Flip();
            }
        }
    }
}

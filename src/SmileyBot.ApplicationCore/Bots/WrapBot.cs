using System;
using System.Linq;
using rlbot.flat;
using RLBotDotNet;
using SmileyBot.ApplicationCore.Services;
using SmileyBot.ApplicationCore.Enums;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Bots
{
    public class WrapBot : Bot
    {
        public BotState State;
        public BotState? DesiredState;
        public BotAction Action;
        public Controller Controller;
        public BallWrapper Ball;
        public PlayerWrapper Info;
        public GameWrapper Game;
        public FieldService Field;

        // Shit Just For Flipping
        protected bool JustJumped = false;
        protected bool JustDoubleJumped = false;
        protected bool Flipping = false;
        protected bool SpecificFlipDirection = true;
        protected float FlipDirection = 0;

        // Shit Just For Truning Around
        protected Vec3 TurnTarget = null;

        // Misc
        protected FieldZone TargetZone = null;

        public WrapBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
        {
            State = BotState.Kickoff;
            Action = BotAction.Default;
            Controller = new Controller();
            Field = new FieldService(botTeam);
            Ball = new BallWrapper();
            Info = new PlayerWrapper();
            Game = new GameWrapper();
            DesiredState = null;
        }

        public override Controller GetOutput(GameTickPacket gameTickPacket)
        {
            try
            {
                //Set Reused Values
                Ball.UpdateInfo(gameTickPacket.Ball);
                Info.UpdateInfo(gameTickPacket.Players(this.Index).Value);
                Game.UpdateInfo(gameTickPacket.GameInfo.Value);

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
            if (DesiredState != null)
            {
                State = DesiredState.Value;
                DesiredState = null;
                return;
            }

            if (State != BotState.Kickoff)
            {
                if (ShouldChangeToKickoff())
                {
                    SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
                    State = BotState.Kickoff;
                    return;
                }

                if (ShouldChangeToChasing() && State != BotState.Chasing)
                {
                    SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
                    TurnAroud();
                    State = BotState.Chasing;
                }
                else if (ShouldChangeToDefending() && State != BotState.Defending)
                {
                    SendQuickChatFromAgent(false, QuickChatSelection.Information_Defending);
                    State = BotState.Defending;
                    return;
                }
                else if (ShouldChangeToWaitingForCenter() && State != BotState.WaitingForCenter)
                {
                    SendQuickChatFromAgent(false, QuickChatSelection.Information_InPosition);
                    var enemyGoalZone = Field.Zones.First(z => z.IsMySide == false && z.HasGoal);
                    var targetZoneId = enemyGoalZone.Id > 6 ? enemyGoalZone.Id - 3 : enemyGoalZone.Id + 3;
                    TargetZone = Field.Zones.First(z => z.Id == targetZoneId);
                    TurnTarget = TargetZone.Rec.Center();
                    TurnAroud();
                    State = BotState.Chasing;
                }
            }
            else
            {
                if (Ball.Location != null && (Ball.Location.X != 0 || Ball.Location.Y != 0))
                {
                    SendQuickChatFromAgent(false, QuickChatSelection.Information_IGotIt);
                    State = BotState.Chasing;
                }
            }
        }

        protected virtual bool ShouldChangeToKickoff()
        {
            return Game.IsKickoff;
        }

        protected virtual bool ShouldChangeToWaitingForCenter()
        {
            var shouldSwitch = false;
            var goalLocation = Field.GetEnemyGoal();
            var ballZone = Field.Zones.FirstOrDefault(z => z.Rec.IsPointWithin(Ball.Location));

            if (ballZone != null && ballZone.IsMySide == false && ballZone.IsCornder)
            {
                if (Field.TowardMySide(Ball) == false)
                {
                    shouldSwitch = true;
                }
            }

            return shouldSwitch;
        }

        protected virtual bool ShouldChangeToDefending()
        {
            var shouldSwitch = false;
            var goalLocation = Field.GetMyGoal();
            var ballDistToGoal = Field.GetDist(Ball.Location, goalLocation);

            if (Field.IsBallOnMySide(Ball))
            {
                if (Field.CloserToTarget(Info.Location, Ball.Location, goalLocation))
                {
                    shouldSwitch = true;
                }
            }

            return shouldSwitch;
        }

        protected virtual bool ShouldChangeToChasing()
        {
            var shouldSwitch = false;
            var myGoalLocation = Field.GetMyGoal();
            var enemyGoalLocation = Field.GetEnemyGoal();

            if (State == BotState.Defending)
            {
                var carDistToGoal = Field.GetDist(Info.Location, myGoalLocation);
                var currentZone = Field.Zones.FirstOrDefault(z => z.Rec.IsPointWithin(Info.Location));

                if (currentZone != null && ((currentZone.IsMySide && currentZone.HasGoal) || Field.MoreThanOneZoneAway(Info.Location, Ball.Location)))
                {
                    shouldSwitch = true;
                }
            }
            else if (State == BotState.WaitingForCenter)
            {
                if (Ball.Location.X >= -893 && Ball.Location.X <= 893)
                {
                    shouldSwitch = true;
                }
            }

            return shouldSwitch;
        }

        public void Aerial()
        {
            Action = BotAction.Aerial;
            Controller.Jump = true;
            Controller.Boost = true;
            Controller.Steer = Field.GetSteeringValueToward(Info, Ball.Location);
            Controller.Pitch = Field.GetPitchValueToward(Info, Ball.Location);

            if (Ball.Location.Z < GameValuesService.BallRadius * 3)
            {
                Action = BotAction.Default;
                Controller.Jump = false;
                Controller.Boost = false;
                Controller.Pitch = 0;
            }
        }

        public void TurnAroud()
        {
            Action = BotAction.TurningAround;

            var target = TurnTarget;
            if (target == null) target = Ball.Location;

            var steerValue = Field.GetSteeringValueToward(Info, target);
            Controller.Steer = steerValue;
            Controller.Handbrake = true;

            var range = .33;
            if (steerValue <= 0 + range && steerValue >= 0 - range)
            {
                TurnTarget = null;
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
                if (Info.HasWheelContact)
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
            Controller.Steer = Field.GetSteeringValueToward(Info, Ball.Location);
            Controller.Throttle = 1;
        }

        public void DribbleBallAtGoal()
        {
            var enemyGoal = Field.GetEnemyGoal();
            var strikeLocation = GetStrikeLocation(Ball, enemyGoal);

            Controller.Steer = Field.GetSteeringValueToward(Info, strikeLocation);
            TurnTarget = Ball.Location;
            // Controller.Throttle = 1;

            var velocity = Info.GetForwardVelocity();
            var turnRadius = GameValuesService.TurnRadius(velocity);
            // var threshold = 400;
            var dist = Field.GetDist(Info.Location, Ball.Location);

            if (Controller.Steer < .5 && Controller.Steer > -.5)
            {
                SpeedUp();
            }
            else if (Controller.Steer > 1 || Controller.Steer < -1)
            {
                SlowDown();
            }
            else if (Controller.Steer > 2.5 || Controller.Steer < -2.5)
            {
                TurnAroud();
            }
            else
            {
                Controller.Throttle = 1;
                Controller.Boost = false;
            }

            // System.Console.WriteLine($"===========================");
            // System.Console.WriteLine($"Ball Dist: {dist}");
            // System.Console.WriteLine($"Steer: {Controller.Steer}");
            // System.Console.WriteLine($"Trun Radius: {turnRadius}");
            // System.Console.WriteLine($"Throttle: {Controller.Throttle}");
            // System.Console.WriteLine($"===========================");

            if (strikeLocation.Z >= GameValuesService.BallRadius * 3)
            {
                //Aerial();
            }
        }

        public void GoToZone()
        {
            Controller.Steer = Field.GetSteeringValueToward(Info, TargetZone.Rec.Center());
            Controller.Throttle = 1;

            var currentZone = Field.Zones.First(z => z.Rec.IsPointWithin(Info.Location));
            if (currentZone.Id == TargetZone.Id)
            {
                Controller.Throttle = .1f;
            }
        }

        public void GoToGoal()
        {
            var myGoal = Field.GetMyGoal();

            Controller.Steer = Field.GetSteeringValueToward(Info, myGoal);
            Controller.Throttle = 1;
        }

        public Vec3 GetStrikeLocation(BallWrapper ballWrappper, Vec3 target)
        {
            var ball = GetProjectedBall();
            // var ball = ballWrappper;
            if (ball == null) return new Vec3(0, 0, 0);

            // var opposite = Math.Abs((ball.Location.X - target.X));
            var hyotenuse = Field.GetDist(ball.Location, target);
            var opposite = ball.Location.X - target.X;
            var theta = Math.Asin(opposite / hyotenuse);
            var newHypotenuse = GameValuesService.BallRadius + hyotenuse;

            var strikeX = GameValuesService.BallRadius * Math.Cos(theta) + ball.Location.X;
            var strikeY = GameValuesService.BallRadius * Math.Sin(theta) + ball.Location.Y;

            strikeX += ball.Velocity.X;
            strikeY += ball.Velocity.Y;

            var strikeLocation = new Vec3((float)strikeX, (float)strikeY, ball.Location.Z);

            System.Console.WriteLine($"===========================");
            System.Console.WriteLine($"Actual: ({Ball.Location.X}, {Ball.Location.Y}, {Ball.Location.Z})");
            System.Console.WriteLine($"Predicted: ({ball.Location.X}, {ball.Location.Y}, {ball.Location.Z})");
            System.Console.WriteLine($"DeltaTime: {ball.DeltaTime}");
            System.Console.WriteLine($"GameTime: {ball.GameTime}");
            System.Console.WriteLine($"ActualTime: {Game.TimeElapsed}");
            System.Console.WriteLine($"===========================");

            return strikeLocation;
        }

        protected LocationPrediction GetProjectedBall()
        {
            var predictions = BallPredictionService.GetPredictions(Ball, Field, Game);
            LocationPrediction target = null;

            foreach (var prediction in predictions)
            {
                if (CanReachPositionInTime(prediction))
                {
                    return prediction;
                }

                target = prediction;
            }

            return target;
        }

        protected bool CanReachPositionInTime(LocationPrediction slice)
        {
            var dist = Field.GetDist(Info.Location, slice.Location);
            var speed = dist / slice.DeltaTime;

            var currentSpeed = (float)Math.Sqrt((Math.Pow(Info.Velocity.X, 2) + Math.Pow(Info.Velocity.Y, 2)));

            return speed < currentSpeed;
        }

        protected void SpeedUp()
        {
            if (Controller.Throttle == 1)
            {
                if (Info.Boost > 0)
                {
                    Controller.Boost = true;
                }
                else
                {
                    var distToTarget = Field.GetDist(Info.Location, TurnTarget);
                    if (distToTarget > 400 && (Controller.Steer < .1 && Controller.Steer > -.1))
                    {
                        Flip();
                    }
                }
            }
            else
            {
                Controller.Throttle += .1f;
                if (Controller.Throttle > 1) Controller.Throttle = 1;
            }

            if (Info.IsSuperSonic)
            {
                Controller.Boost = false;
            }
        }

        protected void SlowDown()
        {
            //Controller.Throttle = 0;
            return;
            if (Controller.Boost == true)
            {
                Controller.Boost = false;
            }
            else
            {
                Controller.Throttle -= .1f;
                if (Controller.Throttle < -1) Controller.Throttle = -1;
            }
        }
    }
}

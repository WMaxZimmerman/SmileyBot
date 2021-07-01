using rlbot.flat;
using SmileyBot.ApplicationCore.Mappers;

namespace SmileyBot.ApplicationCore.Models
{
    public class GameWrapper
    {
        public float TimeElapsed { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsKickoff { get; private set; }
        public bool IsMatchEnd { get; private set; }
        public bool IsOvertime { get; private set; }
        public bool IsRoundActive { get; private set; }
        public bool IsUnlimitedTime { get; private set; }
        private GameInfo Info;

        public GameWrapper()
        {

        }

        public void UpdateInfo(GameInfo? info)
        {
            if (info == null) return;
            Info = info.Value;

            TimeElapsed = Info.SecondsElapsed;
            TimeRemaining = Info.GameTimeRemaining;
            IsKickoff = Info.IsKickoffPause;
            IsMatchEnd = Info.IsMatchEnded;
            IsOvertime = Info.IsOvertime;
            IsRoundActive = Info.IsRoundActive;
            IsUnlimitedTime = Info.IsUnlimitedTime;
        }
    }
}

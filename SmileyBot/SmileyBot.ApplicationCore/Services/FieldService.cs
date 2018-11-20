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
    }
}

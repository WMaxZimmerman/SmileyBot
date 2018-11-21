using rlbot.flat;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Mappers
{
    public static class VectorMapper
    {
	public static Vec3 Map(Vector3 vector)
	{
	    return new Vec3(vector.X, vector.Y, vector.Z);
	}
    }
}

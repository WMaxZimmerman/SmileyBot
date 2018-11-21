using rlbot.flat;
using SmileyBot.ApplicationCore.Mappers;

namespace SmileyBot.ApplicationCore.Models
{
    public class BallWrapper
    {
	public Vec3 Location { get; private set; }
	public Vec3 Velocity { get; private set; }
	public Touch? LatestTouch { get; private set;}
	private BallInfo Info;

	public BallWrapper()
	{
	    Location = new Vec3(0, 0, Services.GameValuesService.BallRadius);
	    Velocity = new Vec3(0, 0, 0);
	}

	public void UpdateInfo(BallInfo? info)
	{
	    if (info == null) return;
	    Info = info.Value;
	    var physics = Info.Physics;

	    if (physics !=  null)
	    {
		var location = physics.Value.Location;
		Location = location != null ? VectorMapper.Map(location.Value) : null;

		var velocity = physics.Value.Velocity;
		Velocity = velocity != null ? VectorMapper.Map(velocity.Value) : null;
	    }

	    LatestTouch = Info.LatestTouch;
	}
    }
}

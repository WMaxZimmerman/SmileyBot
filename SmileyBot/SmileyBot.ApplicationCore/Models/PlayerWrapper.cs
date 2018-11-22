using System;
using rlbot.flat;
using SmileyBot.ApplicationCore.Mappers;

namespace SmileyBot.ApplicationCore.Models
{
    public class PlayerWrapper
    {
	public Vec3 Location { get; private set; }
	public Vec3 Velocity { get; private set; }
	public Rotator Rotation { get; private set; }
	public int Boost { get; private set; }
	public bool Jumped { get; private set; }
	public bool DoubleJumped { get; private set; }
	public bool HasWheelContact { get; private set; }
	public bool IsDemolished { get; private set; }
	public bool IsSuperSonic { get; private set; }
	public bool IsBot { get; private set; }
	public string Name { get; private set; }
	public int Team { get; private set; }
	public ScoreInfo Score { get; private set; }
	private PlayerInfo Info;

	public PlayerWrapper()
	{
	    Location = new Vec3(0, 0, Services.GameValuesService.BallRadius);
	    Velocity = new Vec3(0, 0, 0);
	    Rotation = new Rotator();
	    Score = new ScoreInfo();
	}

	public void UpdateInfo(PlayerInfo? info)
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

		var rotation = physics.Value.Rotation;
		if (rotation != null) Rotation = rotation.Value;
	    }

	    Boost = Info.Boost;
	    Jumped = Info.Jumped;
	    DoubleJumped = Info.DoubleJumped;
	    HasWheelContact = Info.HasWheelContact;
	    IsSuperSonic = Info.IsSupersonic;
	    IsBot = Info.IsBot;
	    IsDemolished = Info.IsDemolished;
	    Name = Info.Name;
	    Team = Info.Team;

	    if (Info.ScoreInfo != null) Score = Info.ScoreInfo.Value;
	}

	public float GetForwardVelocity()
	{
	    var velocity = Math.Sqrt(Math.Pow(Velocity.X, 2) + Math.Pow(Velocity.Y, 2));
	    
	    return (float)velocity;
	}
    }
}

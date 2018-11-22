namespace SmileyBot.ApplicationCore.Models
{
    public class LocationPrediction
    {
	public Vec3 Location { get; set; }
	public Vec3 Velocity { get; set; }
	public float GameTime { get; set; }
	public float DeltaTime { get; set; }
    }
}

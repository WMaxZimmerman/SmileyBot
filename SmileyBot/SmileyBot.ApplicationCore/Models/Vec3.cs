namespace SmileyBot.ApplicationCore.Models
{
    public class Vec3
    {
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }

	public Vec3()
	{
	    
	}
	
	public Vec3(float x, float y, float z)
	{
	    X = x;
	    Y = y;
	    Z = z;
	}
    }
}

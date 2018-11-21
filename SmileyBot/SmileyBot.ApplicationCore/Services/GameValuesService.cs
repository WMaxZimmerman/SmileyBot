namespace SmileyBot.ApplicationCore.Services
{
    public static class GameValuesService
    {
	public static float FieldLength => 10240;
	public static float FieldWidth => 8192;
	public static float SideWall => 4096;
	public static float BackWall => 5120;
	public static float Ceiling => 2044;
	public static float GoalHeight => 642.775f;
	public static float GoalWidth => 1786;
	//public static float BallRadius => 92.75f;
	public static float BallRadius => 327.5139f;
	public static float Gravity => 650;
	public static float MaxSpeed => 2300;
	public static float MaxSpeedNoBoost => 1400;

	public static float TurnRadius(float v)
	{
	    return Curvature(v);
	}
	
	private static float Curvature(float v)
	{
	    if (0 <= v && v < 500)
	    {
		return (float)(0.006900 - 5.84e-6 * v);
	    }
	    else if (500.0 <= v && v < 1000.0)
	    {
		return (float)(0.005610 - 3.26e-6 * v);
	    }
	    else if (1000.0 <= v &&v < 1500.0)
	    {
		return (float)(0.004300 - 1.95e-6 * v);
	    }
	    else if (1500.0 <= v && v < 1750.0)
	    {
		return (float)(0.003025 - 1.10e-6 * v);
	    }
	    else if (1750.0 <= v && v < 2500.0)
	    {
		return (float)(0.001800 - 0.40e-6 * v);
	    }
	    else
	    {
		return 0;
	    }
	}
    }
}

namespace SmileyBot.ApplicationCore.Models
{
    public class Rekt
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public bool IsHorizontal { get; set; }

        public Rekt()
        {
        }

        public Rekt(float x, float y, float width, float length)
        {
            X = x;
            Y = y;
            Width = width;
            Length = length;
            IsHorizontal = true;
        }

        public Rekt(float x, float y, float width, float length, bool isHorizontal)
        {
            X = x;
            Y = y;
            Width = width;
            Length = length;
            IsHorizontal = isHorizontal;
        }

        public bool IsPointWithin(Vec3 location)
        {
            var pointIsWith = false;

            if (location.X >= X && location.X <= X + Width)
            {
                if (location.Y >= Y && location.Y <= Y + Length)
                {
                    pointIsWith = true;
                }
            }

            return pointIsWith;
        }

        public Vec3 Center()
        {
            return new Vec3(X + Width / 2, Y + Length / 2, 0);
        }
    }
}

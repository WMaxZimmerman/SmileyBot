using System.Collections.Generic;

namespace SmileyBot.ApplicationCore.Models
{
    public class FieldZone
    {
        public int Id { get; set; }
        public Rekt Rec { get; set; }
        public bool IsCornder { get; set; }
        public bool IsMySide { get; set; }
        public bool HasGoal { get; set; }

        public FieldZone(int id, int team)
        {
            Id = id;

            IsCornder = (Id == 1 || Id == 3 || Id == 10 || Id == 12);

            if (team == 0)
            {
                if (Id <= 6) IsMySide = true;
            }
            else
            {
                if (id >= 7) IsMySide = true;
            }

            if (Id == 2 || Id == 11)
            {
                HasGoal = true;
            }

            float x = 0;
            float y = 0;
            float width = 8192 / 3;
            float length = 10240 / 4;

            //Determine X
            if (Id == 1 || Id == 4 || Id == 7 || Id == 10)
            {
                x = -4096;
            }
            else if (Id == 3 || Id == 6 || Id == 9 || Id == 12)
            {
                x = 4096 - width;
            }
            else
            {
                x = 4096 - (width * 2);
            }

            //Determine Y
            if (Id <= 3)
            {
                y = -5120;
            }
            else if (Id >= 10)
            {
                y = 5120 - length;
            }
            else if (Id >= 4 && Id <= 6)
            {
                y = -5120 + length;
            }
            else
            {
                y = -5120 + (length * 2);
            }

            Rec = new Rekt(x, y, width, length);
        }

        public bool IsTouchingZone(int zoneId)
        {
            var isTouching = false;

            switch (Id)
            {
                case 1:
                    if (new List<int> { 2, 4, 5 }.Contains(zoneId)) isTouching = true;
                    break;
                case 2:
                    if (new List<int> { 1, 3, 4, 5, 6 }.Contains(zoneId)) isTouching = true;
                    break;
                case 3:
                    if (new List<int> { 2, 5, 6 }.Contains(zoneId)) isTouching = true;
                    break;
                case 4:
                    if (new List<int> { 1, 2, 5, 7, 8 }.Contains(zoneId)) isTouching = true;
                    break;
                case 5:
                    if (new List<int> { 1, 2, 3, 4, 6, 7, 8, 9 }.Contains(zoneId)) isTouching = true;
                    break;
                case 6:
                    if (new List<int> { 2, 3, 5, 8, 9 }.Contains(zoneId)) isTouching = true;
                    break;
                case 7:
                    if (new List<int> { 4, 5, 8, 10, 11 }.Contains(zoneId)) isTouching = true;
                    break;
                case 8:
                    if (new List<int> { 4, 5, 6, 7, 9, 10, 11, 12 }.Contains(zoneId)) isTouching = true;
                    break;
                case 9:
                    if (new List<int> { 5, 6, 8, 11, 12 }.Contains(zoneId)) isTouching = true;
                    break;
                case 10:
                    if (new List<int> { 7, 8, 11 }.Contains(zoneId)) isTouching = true;
                    break;
                case 11:
                    if (new List<int> { 7, 8, 9, 10, 12 }.Contains(zoneId)) isTouching = true;
                    break;
                case 12:
                    if (new List<int> { 8, 9, 11 }.Contains(zoneId)) isTouching = true;
                    break;
            }

            return isTouching;
        }
    }
}

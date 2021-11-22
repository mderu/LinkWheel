using System.Drawing;

namespace CoreAPI.Utils
{
    public static class MathUtils
    {
        public static Point Add(this Point point, Point offset)
        {
            return new Point(point.X + offset.X, point.Y + offset.Y);
        }
        public static Point Subtract(this Point point, Point offset)
        {
            return new Point(point.X - offset.X, point.Y - offset.Y);
        }
        public static int GetSquareMagnitude(this Point point)
        {
            return point.X * point.X + point.Y * point.Y;
        }

        /// <param name="angleInQuestion">In degrees</param>
        /// <param name="startAngle">In degrees</param>
        /// <param name="endAngle">In degrees</param>
        /// <returns></returns>
        public static bool IsBetweenAngles(float angleInQuestion, float startAngle, float endAngle)
        {
            // Put all angles between -360 and 360.
            startAngle %= 360.0f;
            endAngle %= 360.0f;
            angleInQuestion %= 360.0f;

            // Always make the end angle higher than the start angle.
            if (endAngle < startAngle)
            {
                endAngle += 360.0f;
            }

            // If angleInQuestion is a circle greater than startAngle, reduce it.
            if (angleInQuestion - 360 >= startAngle)
            {
                angleInQuestion -= 360.0f;
            }
            // If angleInQuestion is less than startAngle, loop around so startAngle is always a lower bound.
            else if (angleInQuestion < startAngle)
            {
                angleInQuestion += 360.0f;
            }

            // angleInQuestion can only be between if endAngle is still an upper bound.
            return angleInQuestion <= endAngle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle1">In degrees</param>
        /// <param name="angle2">In degrees</param>
        /// <returns></returns>
        public static float ShortestAngleDifference(float angle1, float angle2)
        {
            float diff = (angle2 - angle1 + 180) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
    }
}

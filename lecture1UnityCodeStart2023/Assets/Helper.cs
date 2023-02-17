using UnityEngine;

namespace DefaultNamespace
{
    public class Helper
    {
        /// <summary>
        /// Takes two points and their value and interpolates between them
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="v1">Value of first point</param>
        /// <param name="v2">Value of second point</param>
        /// <param name="thresh">Value controlling what points should be used</param>
        /// <returns>A lerped point between p1 and p2</returns>
        public static Vector3 getEnd(Vector3 p1, Vector3 p2,float v1, float v2,float thresh)
        {
            float t = getT(v2, v1,thresh);
            Vector3 v;
            if (v1 < v2)
                v = Vector3.Lerp(p1,p2,t);
            else
                v = Vector3.Lerp(p2,p1,t);
            return v;
        }
    
        /// <summary>
        /// Calculates the value needed for lerping
        /// </summary>
        /// <param name="v1">Value of p1</param>
        /// <param name="v2">Value of p2</param>
        /// <param name="thresh">Value controlling what points should be used</param>
        /// <returns>Value needed for lerping</returns>
        public static float getT(float v1, float v2,float thresh)
        {
            float vMax = Mathf.Max(v2,v1);
            float vMin = Mathf.Min(v2,v1);
            return (thresh-vMin)/(vMax-vMin);
        }

        /// <summary>
        /// Calculates a points distance from the center of a sphere
        /// </summary>
        /// <param name="p">A point</param>
        /// <returns>The distance from origo or center</returns>
        public static float getDist(Vector3 p)
        {
            float f = Mathf.Sqrt(Mathf.Pow(p.x,2)+Mathf.Pow(p.y,2)+Mathf.Pow(p.z,2));
            return f;
        }
    }
}
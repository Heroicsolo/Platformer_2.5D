using UnityEngine;

namespace HeroicEngine.Utils.Math
{
    public class BezierCurve
    {
        private static float[] Factorial = new float[]
        {
            1.0f,
            1.0f,
            2.0f,
            6.0f,
        };

        private static float Binomial(int n, int i)
        {
            float ni;
            float a1 = Factorial[n];
            float a2 = Factorial[i];
            float a3 = Factorial[n - i];
            ni = a1 / (a2 * a3);
            return ni;
        }

        private static float Bernstein(int n, int i, float t)
        {
            float t_i = Mathf.Pow(t, i);
            float t_n_minus_i = Mathf.Pow((1 - t), (n - i));

            float basis = Binomial(n, i) * t_i * t_n_minus_i;
            return basis;
        }

        public static Vector3 GetCurvedPosition(float t, Vector3 startPos, Vector3 midPos, Vector3 endPos)
        {
            if (t <= 0) return startPos;
            if (t >= 1) return endPos;

            Vector3 p = new Vector3();

            Vector3 bn = Bernstein(2, 0, t) * startPos;
            p += bn;
            bn = Bernstein(2, 1, t) * midPos;
            p += bn;
            bn = Bernstein(2, 2, t) * endPos;
            p += bn;

            return p;
        }
    }
}
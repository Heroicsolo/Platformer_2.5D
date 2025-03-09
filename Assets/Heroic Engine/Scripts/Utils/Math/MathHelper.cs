using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Math
{
    public static class MathHelper
    {
        private static System.Random rng = new System.Random();

        /// <summary>
        /// This extension method creates string from given float number, rounded to certain count of digits after dot symbol.
        /// </summary>
        /// <param name="_value">Given number</param>
        /// <param name="_digits">Precision (number of digits after dot)</param>
        /// <returns>String with rounded number representation</returns>
        public static string ToRoundedString(this float _value, int _digits = 1)
        {
            string zeros = new string('0', _digits);
            return Mathf.Approximately(_value, Mathf.Floor(_value)) ? Mathf.FloorToInt(_value).ToString() : _value.ToString("0." + zeros);
        }

        /// <summary>
        /// This extension method returns shortened representation of given integer number.
        /// For example, 10.000 will be converted to "10k" and 1.500.000 will be converted to "1,5M".
        /// </summary>
        /// <param name="number">Given integer number</param>
        /// <returns>Shortened string representation of number</returns>
        public static string ToShortenedNumber(this int number)
        {
            if (number >= 1000 && number < 1000000)
            {
                string roundedNum = ((float)number / 1000).ToRoundedString(1);
                return $"{roundedNum}k";
            }
            
            if (number >= 1000000)
            {
                string roundedNum = ((float)number / 1000000).ToRoundedString(1);
                return $"{roundedNum}M";
            }

            return number.ToString();
        }

        public static Vector3 CubeBezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float r = 1f - t;
            float f0 = r * r * r;
            float f1 = r * r * t * 3;
            float f2 = r * t * t * 3;
            float f3 = t * t * t;
            return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
        }

        /// <summary>
        /// This extension method adds unique elements from given items collection to given list.
        /// It adds all elements which are not yet presented in this list.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="self">Given list</param>
        /// <param name="items">Given collection</param>
        public static void AddUnique<T>(this IList<T> self, IEnumerable<T> items)
        {
            foreach (var item in items)
                if (!self.Contains(item))
                    self.Add(item);
        }

        /// <summary>
        /// This extension method randomly shuffles given list.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">Given list</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// This extension method randomly shuffles given list.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">Given list</param>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// This extension method sorts given list of Transforms by distance from given point, from closest one to farthest one.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">Given list of Transforms</param>
        /// <param name="from">Given point</param>
        public static void SortByDistance<T>(this List<T> list, Vector3 from) where T : Transform
        {
            list.Sort((a, b) => Vector3.Distance(from, a.transform.position).CompareTo(Vector3.Distance(from, b.transform.position)));
        }

        /// <summary>
        /// This extension method returns random item from given list.
        /// If list is empty, it returns default value of given type.
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">Given list</param>
        /// <returns>Random element from list, but if list is empty, it returns default value of given type</returns>
        public static T GetRandomElement<T>(this List<T> list)
        {
            if (list.Count == 0) return default;
            return list[rng.Next(list.Count)];
        }

        /// <summary>
        /// This extension method returns random item from given list, except certain item.
        /// If it cannot find such item, it returns exceptedOne item instead
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="list">Given list</param>
        /// <param name="exceptedOne">Item that should be excepted</param>
        /// <returns>Random item from given list, if list is empty, returns exceptedOne item</returns>
        public static T GetRandomElementExceptOne<T>(this List<T> list, T exceptedOne)
        {
            List<T> newList = new List<T>(list);
            newList.Remove(exceptedOne);
            if (newList.Count > 0)
            {
                return newList[rng.Next(newList.Count)];
            }
            return exceptedOne;
        }
    }
}
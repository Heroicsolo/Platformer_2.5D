using Unity.VisualScripting;
using UnityEngine;

namespace HeroicEngine.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// This extension method colours given string by color.
        /// But be aware that such string will be displayed correctly only on Rich Text components.
        /// </summary>
        /// <param name="str">Given string</param>
        /// <param name="color">Needed color</param>
        /// <returns>Colored string</returns>
        public static string ToColorizedString(this string str, Color color)
        {
            string hexColor = color.ToHexString().Remove(6);
            return $"<color=#{hexColor}>{str}</color>";
        }

        /// <summary>
        /// This extension method makes given string bold.
        /// But be aware that such string will be displayed correctly only on Rich Text components.
        /// </summary>
        /// <param name="str">Given string</param>
        /// <returns>Bold string</returns>
        public static string ToBoldString(this string str)
        {
            return $"<b>{str}</b>";
        }

        public static string ToItalicString(this string str)
        {
            return $"<i>{str}</i>";
        }
    }
}
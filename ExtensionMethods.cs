#region License
/* Bridle
 * Copyright 2017 Walter Barrett
 *
 * This project is released under the 3-Clause BSD license.
 *
 * See License.md for details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bridle.Utilities
{
    public static class ExtensionMethods
    {
        #region Type Extension Methods
        public static bool IsSameOrSubclassOf(this Type potentialDescendant, Type potentialBase)
        {
            if (potentialDescendant == null) { throw new ArgumentNullException(nameof(potentialDescendant)); }
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

        public static bool ImplementsInterface(this Type potentialImplementer, Type interfaceType)
        {
            if (potentialImplementer == null) { throw new ArgumentNullException(nameof(potentialImplementer)); }
            return potentialImplementer.GetInterfaces().Contains(interfaceType);
        }
        #endregion

        #region String Extension Methods
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "CA1063 does not apply to argument \"text\", as it requires it to be non-null as an extension method of the class.")]
        public static string ReplaceFirst(this string @string, string search, string replace)
        {
            if (search != null && replace != null)
            {
                int pos = @string.IndexOf(search, StringComparison.Ordinal);
                if (pos < 0)
                {
                    return @string;
                }
                return @string.Substring(0, pos) + replace + @string.Substring(pos + search.Length);
            }
            return @string;
        }

        #region FNV-1a non-cryptographic hash functions.
        // Retrieved from http://gist.github.com/rasmuskl/3786618
        // Adapted from: http://github.com/jakedouglas/fnv-java
        // Licensed under the MIT License as of 6/13/2014 at 7:54AM

        const uint Fnv32Offset = 0x811C9DC5;
        const uint Fnv32Prime = 0x1000193;
        const ulong Fnv64Offset = 0xCBF29CE484222325;
        const ulong Fnv64Prime = 0x100000001B3;

        /// <summary>
        /// FNV-1a (64-bit) non-cryptographic hash function.
        /// </summary>
        /// <param name="string">The string to hash.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fnv")]
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        public static ulong ToFnv64Hash(this string @string)
        {
            ulong hash = Fnv64Offset;
            byte[] bytes = Encoding.UTF8.GetBytes(@string);
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash ^ bytes[i];
                hash *= Fnv64Prime;
            }
            return hash;
        }

        /// <summary>
        /// FNV-1a (32-bit) non-cryptographic hash function.
        /// </summary>
        /// <param name="string">The string to hash.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fnv")]
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        public static uint ToFnv32Hash(this string @string)
        {
            uint hash = Fnv32Offset;
            byte[] bytes = Encoding.UTF8.GetBytes(@string);
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash ^ bytes[i];
                hash *= Fnv32Prime;
            }
            return hash;
        }

        /// <summary>
        /// FNV-1a (24-bit) non-cryptographic hash function.
        /// </summary>
        /// <param name="string">The string to hash.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fnv")]
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        public static uint ToFnv24Hash(this string @string)
        {
            uint hash = Fnv32Offset;
            byte[] bytes = Encoding.UTF8.GetBytes(@string);
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash ^ bytes[i];
                hash *= Fnv32Prime;
            }

            hash = (hash >> 24) ^ (hash & 0xFFFFFF);

            return hash;
        }
        #endregion
        #endregion

        #region IList Extension Methods
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static T GetRandomItem<T>(this IList<T> list, Xorshift randomNumberGenerator)
        {
            if (randomNumberGenerator == null) { throw new ArgumentNullException(nameof(randomNumberGenerator)); }
            if (list != null && list.Count > 0)
            {
                return list[randomNumberGenerator.Next(list.Count)];
            }
            return default(T);
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static void Shuffle<T>(this IList<T> list, Xorshift randomNumberGenerator)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }
            if (randomNumberGenerator == null) { throw new ArgumentNullException(nameof(randomNumberGenerator)); }
            int i = list.Count - 1;
            while (i > 1)
            {
                int j = randomNumberGenerator.Next(i);
                T value = list[j];
                list[j] = list[i];
                list[i] = value;
                i--;
            }
        }

        public static T GetFirstOfType<TBase, T>(this IList<TBase> list) where TBase : class where T : class, TBase
        {
            foreach (TBase item in list)
            {
                if (item is T value)
                {
                    return value;
                }
            }

            return null;
        }

        public static List<T> GetAllOfType<TBase, T>(this IList<TBase> list) where TBase : class where T : class, TBase
        {
            List<T> ret = new List<T>();
            foreach (TBase item in list)
            {
                if (item is T value)
                {
                    ret.Add(value);
                }
            }

            return ret;
        }

        #endregion

        #region Jagged Array Extension Methods
        public static T[][] GetSection<T>(this T[][] byteArray, int xOffset, int yOffset, int width, int height)
        {
            if (byteArray == null) { throw new ArgumentNullException(nameof(byteArray)); }

            T[][] temp = new T[width][];

            for (int y = 0; y < temp.Length; y++)
            {
                temp[y] = new T[height];
                for (int x = 0; x < temp[y].Length; x++)
                {
                    if (yOffset + y < 0)
                    {
                        temp[y][x] = default(T);
                    }
                    else if (xOffset + x < 0)
                    {
                        temp[y][x] = default(T);
                    }
                    else if (yOffset + y >= byteArray.Length)
                    {
                        temp[y][x] = default(T);
                    }
                    else if (xOffset + x >= byteArray[yOffset + y].Length)
                    {
                        temp[y][x] = default(T);
                    }
                    else
                    {
                        temp[y][x] = byteArray[yOffset + y][xOffset + x];
                    }
                }
            }
            return temp;
        }
        #endregion

        #region Math Extension Methods
        public static double Clamp(this double value, double min, double max) => value < min ? min : value > max ? max : value;
        public static float Clamp(this float value, float min, float max) => value < min ? min : value > max ? max : value;
        public static long Clamp(this long value, long min, long max) => value < min ? min : value > max ? max : value;
        public static ulong Clamp(this ulong value, ulong min, ulong max) => value < min ? min : value > max ? max : value;
        public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;
        public static uint Clamp(this uint value, uint min, uint max) => value < min ? min : value > max ? max : value;
        public static short Clamp(this short value, short min, short max) => value < min ? min : value > max ? max : value;
        public static ushort Clamp(this ushort value, ushort min, ushort max) => value < min ? min : value > max ? max : value;
        public static sbyte Clamp(this sbyte value, sbyte min, sbyte max) => value < min ? min : value > max ? max : value;
        public static byte Clamp(this byte value, byte min, byte max) => value < min ? min : value > max ? max : value;

        public static int AsInt(this double value, RoundingType roundingType = RoundingType.NearestAwayFromZero)
        {
            switch (roundingType)
            {
                case RoundingType.Floor:
                    return (int)Math.Floor(value);
                case RoundingType.Ceiling:
                    return (int)Math.Ceiling(value);
                case RoundingType.NearestToEven:
                    return (int)Math.Round(value, MidpointRounding.ToEven);
                default:
                    return (int)Math.Round(value, MidpointRounding.AwayFromZero);
            }
        }

        public static int AsInt(this float value, RoundingType roundingType = RoundingType.NearestAwayFromZero)
        {
            switch (roundingType)
            {
                case RoundingType.Floor:
                    return (int)Math.Floor(value);
                case RoundingType.Ceiling:
                    return (int)Math.Ceiling(value);
                case RoundingType.NearestToEven:
                    return (int)Math.Round(value, MidpointRounding.ToEven);
                default:
                    return (int)Math.Round(value, MidpointRounding.AwayFromZero);
            }
        }
        #endregion
    }
}

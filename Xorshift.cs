#region License
/* Xorshift
 * Copyright 2013 Roman Starkov
 * 
 * This code is provided under the MIT license.
 * 
 * See Xorshift.license for details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridle.Utilities
{
    /// <summary>
    /// Cross-platform, deterministic random numbers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xorshift")]
    public class Xorshift
    {
        private uint _x = 123456789;
        private uint _y = 362436069;
        private uint _z = 521288629;
        private uint _w;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private Queue<byte> _bytes = new Queue<byte>();

        public Xorshift() : this(88675123) { }

        public Xorshift(uint seed)
        {
            _w = seed;
        }

        public byte[] NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            int offset = 0;
            while (_bytes.Any() && offset < buffer.Length)
            {
                buffer[offset++] = _bytes.Dequeue();
            }

            int length = ((buffer.Length - offset) / FillBufferMultipleRequired) * FillBufferMultipleRequired;
            if (length > 0)
            {
                FillBuffer(buffer, offset, offset + length);
            }

            offset += length;
            while (offset < buffer.Length)
            {
                if (_bytes.Count == 0)
                {
                    uint t = _x ^ (_x << 11);
                    _x = _y; _y = _z; _z = _w;
                    _w = _w ^ (_w >> 19) ^ (t ^ (t >> 8));
                    _bytes.Enqueue((byte)(_w & 0xFF));
                    _bytes.Enqueue((byte)((_w >> 8) & 0xFF));
                    _bytes.Enqueue((byte)((_w >> 16) & 0xFF));
                    _bytes.Enqueue((byte)((_w >> 24) & 0xFF));
                }
                buffer[offset++] = _bytes.Dequeue();
            }
            return buffer;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="F:System.Int32.MaxValue"/>.</returns>
        public int Next()
        {
            uint x = _x, y = _y, z = _z, w = _w;
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            _x = x; _y = y; _z = z; _w = w;
            return (int)(w & 0x7FFFFFFF);
        }

        /// <summary>Returns an unsigned random integer.</summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="F:System.UInt32.MaxValue"/>.</returns>
        public uint NextUInt32()
        {
            uint x = _x, y = _y, z = _z, w = _w;
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            _x = x; _y = y; _z = z; _w = w;
            return (w);
        }

        /// <summary>Returns a random floating-point number between 0.0 and 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public double NextDouble()
        {
            return Next() * 4.6566128752458E-10;
        }

        /// <summary>Returns a random floating-point number between 0.0 and 1.0.</summary>
        /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        public float NextFloat()
        {
            return Next() * 4.6566128752458E-10f;
        }

        /// <summary>Returns a non-negative random number less than the specified maximum.</summary>
        /// <returns>A 32-bit signed integer less than <paramref name="maxValue"/>.</returns>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> should be greater than zero.</param>
        public int Next(int maxValue)
        {
            uint x = _x, y = _y, z = _z, w = _w;
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            _x = x; _y = y; _z = z; _w = w;
            return (int)(w & 0x7FFFFFFF) % maxValue;
        }

        /// <summary>Returns an unsigned random number less than the specified maximum.</summary>
        /// <returns>A 32-bit unsigned integer less than <paramref name="maxValue"/>.</returns>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> should be greater than zero.</param>
        public uint NextUInt32(uint maxValue)
        {
            uint x = _x, y = _y, z = _z, w = _w;
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            w = w ^ (w >> 19) ^ (t ^ (t >> 8));
            _x = x; _y = y; _z = z; _w = w;
            return (w) % maxValue;
        }

        private const int FillBufferMultipleRequired = 4;

        private void FillBuffer(byte[] buf, int offset, int offsetEnd)
        {
            uint x = _x, y = _y, z = _z, w = _w;
            while (offset < offsetEnd)
            {
                uint t = x ^ (x << 11);
                x = y; y = z; z = w;
                w = w ^ (w >> 19) ^ (t ^ (t >> 8));
                buf[offset++] = (byte)(w & 0xFF);
                buf[offset++] = (byte)((w >> 8) & 0xFF);
                buf[offset++] = (byte)((w >> 16) & 0xFF);
                buf[offset++] = (byte)((w >> 24) & 0xFF);
            }
            _x = x; _y = y; _z = z; _w = w;
        }
    }
}

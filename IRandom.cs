#region License
/* Bridle
 * Copyright 2017 Walter Barrett
 *
 * This project is released under the 3-Clause BSD license.
 *
 * See License.md for details.
 */
#endregion

namespace Bridle.Utilities
{
    public interface IRandom
    {
        int Next();
        int Next(int maxValue);
        //int Next(int minValue, int maxValue);

        uint NextUInt32();
        uint NextUInt32(uint maxValue);
        //uint NextUInt32(uint minValue, uint maxValue);

        double NextDouble();

        byte[] NextBytes(byte[] buffer);
    }
}

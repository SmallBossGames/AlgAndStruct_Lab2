using System;
using System.Collections.Generic;
using System.Text;

namespace AlgAndStruct_Lab2Unsafe
{
    static class BitConverter
    {
        public static unsafe byte[] GetBytes(RoadsMeta value)
        {
            var buffer = new byte[sizeof(RoadsMeta)];

            fixed(byte* b = buffer)
            {
                *((RoadsMeta*)b) = value;
            }

            return buffer;
        }

        public static unsafe byte[] GetBytes(CitiesMeta value)
        {
            var buffer = new byte[sizeof(CitiesMeta)];

            fixed (byte* b = buffer)
            {
                *((CitiesMeta*)b) = value;
            }

            return buffer;
        }

        public static unsafe byte[] GetBytes(City value)
        {
            var buffer = new byte[sizeof(City)];

            fixed (byte* b = buffer)
            {
                *((City*)b) = value;
            }

            return buffer;
        }

        public static unsafe byte[] GetBytes(Path value)
        {
            var buffer = new byte[sizeof(Path)];

            fixed (byte* b = buffer)
            {
                *((Path*)b) = value;
            }

            return buffer;
        }

        public static unsafe RoadsMeta GetRoadsMeta(ReadOnlySpan<byte> value)
        {
            if(value.Length!= sizeof(RoadsMeta))
            {
                throw new FormatException();
            }

            fixed (byte* b = value)
            {
                return *((RoadsMeta*)b);
            }
        }

        public static unsafe CitiesMeta GetCitiesMeta(ReadOnlySpan<byte> value)
        {
            if (value.Length != sizeof(CitiesMeta))
            {
                throw new FormatException();
            }

            fixed (byte* b = value)
            {
                return *((CitiesMeta*)b);
            }
        }

        public static unsafe City GetCity(ReadOnlySpan<byte> value)
        {
            if (value.Length != sizeof(City))
            {
                throw new FormatException();
            }

            fixed (byte* b = value)
            {
                return *((City*)b);
            }
        }

        public static unsafe Path GetPath(ReadOnlySpan<byte> value)
        {
            if (value.Length != sizeof(Path))
            {
                throw new FormatException();
            }

            fixed (byte* b = value)
            {
                return *((Path*)b);
            }
        }
    }
}

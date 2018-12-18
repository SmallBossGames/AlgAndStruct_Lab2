using System;
using System.Collections.Generic;
using System.Text;

namespace AlgAndStruct_Lab2Unsafe
{
    internal unsafe struct RoadsMeta
    {
        //public long startPtr;
        public long removeStackPtr;
        public long elementCount;

        public static int Size => sizeof(RoadsMeta);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AlgAndStruct_Lab2Unsafe
{
    unsafe struct CitiesMeta
    {
        public long startPtr;
        public long removeStackPtr;
        public long elementCount;

        public static int Size => sizeof(CitiesMeta);
    }
}

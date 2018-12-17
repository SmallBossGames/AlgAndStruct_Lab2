using System;
using System.Collections.Generic;
using System.Text;

namespace AlgAndStruct_Lab2Unsafe
{
    internal unsafe struct Path
    {
        public long cityPtr;
        public long nextPtr;

        public static int Size => sizeof(Path);
    }
}

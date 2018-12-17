using System;
using System.Collections.Generic;
using System.Text;

namespace AlgAndStruct_Lab2Unsafe
{
    internal unsafe struct City
    {
        private fixed char _cityNameBuffer[20];
        public long nextPtr;
        public long roadsListPtr;

        public string Name
        {
            get
            {
                fixed(char* buff = _cityNameBuffer)
                {
                    return new string(buff);
                }
            }

            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    _cityNameBuffer[i] = value[i];
                }
            }
        }

        public static int Size => sizeof(City); 
    }
}

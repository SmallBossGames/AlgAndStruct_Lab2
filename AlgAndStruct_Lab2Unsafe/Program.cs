﻿using System;

namespace AlgAndStruct_Lab2Unsafe
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var list = new MultipleListOnFile("pidor1.bin", "pidor2.bin", OpenMode.Create))
            {
                list.AddCity("HHHHHHHH");
                list.AddCity("Aaaaaa");
                list.AddCity("OOOOOOOOO");
                list.AddCity("Bbbbbb");
                list.AddCity("kkkkkkk");
                list.AddCity("ggggggg");
                list.AddCity("PPPPPPPP");
                list.RemoveCity("Aaaaaa");
                list.AddCity("CC");

                list.AddPath("ggggggg", "kkkkkkk");
                list.AddPath("ggggggg", "CC");
                list.AddPath("ggggggg", "Bbbbbb");
                list.AddPath("Bbbbbb", "ggggggg");
                list.AddPath("kkkkkkk", "Bbbbbb");
                list.AddPath("Bbbbbb", "CC");

                list.RemoveCity("HHHHHHHH");
                list.RemoveCity("PPPPPPPP");
                list.RemoveCity("OOOOOOOOO");

                //var check1 = list.RemovePath("ggggggg", "CC");
                //var check2 = list.RemovePath("ggggggg", "kkkkkkk");
                //var check3 = list.RemovePath("ggggggg", "Bbbbbb");
                list.PackData();
                //list.PackData();
                var paths = list.TracePathMain("ggggggg", "CC");
            }

            Console.WriteLine("Hello World!");
        }
    }
}

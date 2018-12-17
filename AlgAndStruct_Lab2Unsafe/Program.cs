﻿using System;

namespace AlgAndStruct_Lab2Unsafe
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var list = new MultipleListOnFile("pidor1.bin", "pidor2.bin", OpenMode.Create))
            {
                list.AddCity("Aaaaaa");
                list.AddCity("Bbbbbb");
                list.AddCity("kkkkkkk");
                list.AddCity("ggggggg");
                list.RemoveCity("Aaaaaa");
                list.AddCity("CC");

                list.AddPath("ggggggg", "kkkkkkk");
                list.AddPath("ggggggg", "CC");
            }

            Console.WriteLine("Hello World!");
        }
    }
}
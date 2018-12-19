using System;

namespace AlgAndStruct_Lab2Unsafe
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var list = new MultipleListOnFile("pidor1.bin", "pidor2.bin", OpenMode.Create))
            {
                while (Console.ReadLine() != "7")
                {
                    Console.WriteLine("1 - Добавление города\n2 - Удаление города\n3 - Добавление дороги\n4 - Удаление дороги\n5 - Упаковка\n6 - Трассировка\n7 - Выход");
                    switch (Convert.ToInt32(Console.ReadLine()))
                    {
                        case 1:
                            Console.WriteLine("Введите название города: ");
                            list.AddCity(Console.ReadLine());
                            break;
                        case 2:
                            Console.WriteLine("Введите название города: ");
                            list.RemoveCity(Console.ReadLine());
                            break;
                        case 3:
                            Console.WriteLine("Введите название городов, между которыми проложить дорогу\n Город 1: ");
                            var first = Console.ReadLine();
                            Console.WriteLine("\nГород 2: ");
                            var second = Console.ReadLine();
                            list.AddPath(first, second);
                            break;
                        case 4:
                            Console.WriteLine("Введите название городов, между которыми удалить дорогу\n Город 1: ");
                            var firstRemove = Console.ReadLine();
                            Console.WriteLine("\nГород 2: ");
                            var secondRemove = Console.ReadLine();
                            list.RemovePath(firstRemove, secondRemove);
                            break;
                        case 5:
                            list.PackData();
                            break;
                        case 6:
                            Console.WriteLine("Введите название городов, между которыми вывести все дороги\n Город 1: ");
                            var firstTrace = Console.ReadLine();
                            Console.WriteLine("\nГород 2: ");
                            var secondTrace = Console.ReadLine();
                            var trace = list.TracePathMain(firstTrace, secondTrace);
                            foreach (var a in trace)
                                Console.WriteLine("a");
                            break;
                        default: break;
                    }
                }
                /* list.AddCity("HHHHHHHH");
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
                 var paths = list.TracePathMain("ggggggg", "CC");*/
            }

            //Console.WriteLine("Hello World!");
        }
    }
}

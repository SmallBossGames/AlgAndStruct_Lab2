using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AlgAndStruct_Lab2Unsafe
{
    class MultipleListOnFile : IDisposable
    {
        private readonly FileStream _cityFileStream;
        private readonly FileStream _pathFileStream;

        public MultipleListOnFile(string cityFileName, string pathFileName, OpenMode mode)
        {
            CitiesMeta citiesMeta;
            RoadsMeta roadsMeta;

            switch (mode)
            {
                case OpenMode.Create:
                    _cityFileStream = new FileStream(cityFileName, FileMode.Create, FileAccess.ReadWrite);
                    _pathFileStream = new FileStream(pathFileName, FileMode.Create, FileAccess.ReadWrite);
                    citiesMeta = new CitiesMeta()
                    {
                        startPtr = 0,
                        removeStackPtr = 0,
                        elementCount = 0,
                    };
                    roadsMeta = new RoadsMeta()
                    {
                        removeStackPtr = 0,
                    };
                    _cityFileStream.Write(BitConverter.GetBytes(citiesMeta));
                    _pathFileStream.Write(BitConverter.GetBytes(roadsMeta));
                    break;
                case OpenMode.Open:
                    _cityFileStream = new FileStream(cityFileName, FileMode.Open, FileAccess.ReadWrite);
                    _pathFileStream = new FileStream(cityFileName, FileMode.Open, FileAccess.ReadWrite);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public void AddCity(string name)
        {
            var offset = 0L;

            Span<byte> metaBuffer = stackalloc byte[CitiesMeta.Size];

            _cityFileStream.Seek(0, SeekOrigin.Begin);
            _cityFileStream.Read(metaBuffer);

            var meta = BitConverter.GetCitiesMeta(metaBuffer);

            var city = new City()
            {
                nextPtr = meta.startPtr,
                Name = name,
                roadsListPtr = 0,
            };

            Console.WriteLine(city.Name);

            if (meta.removeStackPtr == 0)
            {
                offset = CitiesMeta.Size + City.Size * meta.elementCount;
            }
            else
            {
                offset = meta.removeStackPtr;

                Span<byte> cityBuffer = stackalloc byte[City.Size];

                _cityFileStream.Seek(offset, SeekOrigin.Begin);
                _cityFileStream.Read(cityBuffer);

                var cityTemp = BitConverter.GetCity(cityBuffer);
                meta.removeStackPtr = cityTemp.nextPtr;
            }

            meta.elementCount++;
            meta.startPtr = offset;

            _cityFileStream.Seek(offset, SeekOrigin.Begin);
            _cityFileStream.Write(BitConverter.GetBytes(city));

            _cityFileStream.Seek(0, SeekOrigin.Begin);
            _cityFileStream.Write(BitConverter.GetBytes(meta));
        }

        public bool RemoveCity(string name)
        {
            var meta = ReadCitiesMeta();

            if(meta.startPtr == 0)
            {
                return false;
            }

            var offset = meta.startPtr;
            var city = ReadCity(offset);

            if(city.Name == name)
            {
                meta.startPtr = city.nextPtr;
                city.nextPtr = meta.removeStackPtr;
                meta.removeStackPtr = offset;

                _cityFileStream.Seek(offset, SeekOrigin.Begin);
                _cityFileStream.Write(BitConverter.GetBytes(city));

                _cityFileStream.Seek(0, SeekOrigin.Begin);
                _cityFileStream.Write(BitConverter.GetBytes(meta));
                return true;
            }

            while (city.nextPtr != 0)
            {
                var newOffset = city.nextPtr;
                var newCity = ReadCity(newOffset);

                if(newCity.Name == name)
                {
                    city.nextPtr = newCity.nextPtr;
                    newCity.nextPtr = meta.removeStackPtr;
                    meta.removeStackPtr = newOffset;

                    _cityFileStream.Seek(offset, SeekOrigin.Begin);
                    _cityFileStream.Write(BitConverter.GetBytes(city));

                    _cityFileStream.Seek(newOffset, SeekOrigin.Begin);
                    _cityFileStream.Write(BitConverter.GetBytes(newCity));

                    _cityFileStream.Seek(0, SeekOrigin.Begin);
                    _cityFileStream.Write(BitConverter.GetBytes(meta));

                    return true;
                }

                city = newCity;
                offset = newOffset;
            }

            return false;
        }

        public void AddPath(string cityFrom, string cityTo)
        {
            var meta = ReadRoadsMeta();
            var offset = 0L;

            var check = TryGetCityPtr(cityTo, out var cityToPtr);

            if(!check)
            {
                throw new ArgumentException();
            }

            check = TryGetCityPtr(cityFrom, out var cityFromPtr);

            if (!check)
            {
                throw new ArgumentException();
            }

            var cityFromData = ReadCity(cityFromPtr);

            var path = new Path()
            {
                nextPtr = cityFromData.roadsListPtr,
                cityPtr = cityToPtr,
            };


            if (meta.removeStackPtr == 0)
            {
                offset = RoadsMeta.Size + Path.Size * meta.elementCount;
            }
            else
            {
                offset = meta.removeStackPtr;

                Span<byte> pathBuffer = stackalloc byte[Path.Size];

                _pathFileStream.Seek(offset, SeekOrigin.Begin);
                _pathFileStream.Read(pathBuffer);

                var pathTemp = BitConverter.GetPath(pathBuffer);
                meta.removeStackPtr = pathTemp.nextPtr;
            }

            cityFromData.roadsListPtr = offset;
            meta.elementCount++;

            _pathFileStream.Seek(offset, SeekOrigin.Begin);
            _pathFileStream.Write(BitConverter.GetBytes(path));

            _cityFileStream.Seek(cityFromPtr, SeekOrigin.Begin);
            _cityFileStream.Write(BitConverter.GetBytes(cityFromData));

            _pathFileStream.Seek(0, SeekOrigin.Begin);
            _pathFileStream.Write(BitConverter.GetBytes(meta));
        }

        public bool RemovePath(string cityFrom, string cityTo)
        {
            var meta = ReadRoadsMeta();

            var check = TryGetCityPtr(cityFrom, out var cityFromPtr);

            if (!check)
            {
                return false;
            }

            check = TryGetCityPtr(cityTo, out var cityToPtr);

            if (!check)
            {
                return false;
            }

            var cityFromData = ReadCity(cityFromPtr);

            if(cityFromData.roadsListPtr == 0)
            {
                return false;
            }

            var offset = cityFromData.roadsListPtr;
            var path = ReadPath(offset);

            if (path.cityPtr == cityToPtr)
            {
                cityFromData.roadsListPtr = path.nextPtr;
                path.nextPtr = meta.removeStackPtr;
                meta.removeStackPtr = offset;

                _cityFileStream.Seek(cityFromPtr, SeekOrigin.Begin);
                _cityFileStream.Write(BitConverter.GetBytes(cityFromData));

                _pathFileStream.Seek(offset, SeekOrigin.Begin);
                _pathFileStream.Write(BitConverter.GetBytes(path));

                _pathFileStream.Seek(0, SeekOrigin.Begin);
                _pathFileStream.Write(BitConverter.GetBytes(meta));
                return true;
            }

            while (path.nextPtr != 0)
            {
                var newOffset = path.nextPtr;
                var newPath = ReadPath(newOffset);

                if (newPath.cityPtr == cityToPtr)
                {
                    path.nextPtr = newPath.nextPtr;
                    newPath.nextPtr = meta.removeStackPtr;
                    meta.removeStackPtr = newOffset;

                    _pathFileStream.Seek(offset, SeekOrigin.Begin);
                    _pathFileStream.Write(BitConverter.GetBytes(path));

                    _pathFileStream.Seek(newOffset, SeekOrigin.Begin);
                    _pathFileStream.Write(BitConverter.GetBytes(newPath));

                    _pathFileStream.Seek(0, SeekOrigin.Begin);
                    _pathFileStream.Write(BitConverter.GetBytes(meta));

                    return true;
                }

                offset = newOffset;
                path = newPath;
            }

            return false;
        }

        public List<string> TracePathMain(string from, string to)
        {
            var checkSet = new HashSet<long>();
            var paths = new List<string>();

            var dictonary = new Dictionary<long, bool>();

            var check = TryGetCityPtr(from, out var cityPtr);

            TracePath(cityPtr, to, string.Empty, checkSet, paths);

            return paths;
        }

        private void TracePath(long cityPtr, string target, string currentPath, HashSet<long> set, List<string> paths)
        {
            if(set.Contains(cityPtr))
            {
                return;
            }

            var city = ReadCity(cityPtr);

            currentPath += $"->{city.Name}";

            if(city.Name == target)
            {
                paths.Add(currentPath);
                return;
            }

            if(city.roadsListPtr == 0)
            {
                return;
            }

            set.Add(cityPtr);

            var offset = city.roadsListPtr;

            while (offset!=0)
            {
                var path = ReadPath(offset);
                TracePath(path.cityPtr, target, currentPath, set, paths);
                offset = path.nextPtr;
            }

            set.Remove(cityPtr);

            return;
        }

        private bool TryGetCityPtr(string name, out long ptr)
        {
            var meta = ReadCitiesMeta();

            if (meta.startPtr == 0) 
            {
                ptr = 0;
                return false;
            }

            while (meta.startPtr!=0)
            {
                var city = ReadCity(meta.startPtr);
                if(city.Name == name)
                {
                    ptr = meta.startPtr;
                    return true;
                }
                meta.startPtr = city.nextPtr;
            }

            ptr = 0;
            return false;
        }

        private CitiesMeta ReadCitiesMeta()
        {
            Span<byte> metaBuffer = stackalloc byte[CitiesMeta.Size];
            _cityFileStream.Seek(0, SeekOrigin.Begin);
            _cityFileStream.Read(metaBuffer);
            return BitConverter.GetCitiesMeta(metaBuffer);
        }

        private City ReadCity(long offset)
        {
            Span<byte> cityBuffer = stackalloc byte[City.Size];
            _cityFileStream.Seek(offset, SeekOrigin.Begin);
            _cityFileStream.Read(cityBuffer);
            return BitConverter.GetCity(cityBuffer);
        }

        private RoadsMeta ReadRoadsMeta()
        {
            Span<byte> metaBuffer = stackalloc byte[RoadsMeta.Size];
            _pathFileStream.Seek(0, SeekOrigin.Begin);
            _pathFileStream.Read(metaBuffer);
            return BitConverter.GetRoadsMeta(metaBuffer);
        }

        private Path ReadPath(long offset)
        {
            Span<byte> pathBuffer = stackalloc byte[Path.Size];
            _pathFileStream.Seek(offset, SeekOrigin.Begin);
            _pathFileStream.Read(pathBuffer);
            return BitConverter.GetPath(pathBuffer);
        }

        public void Dispose()
        {
            _cityFileStream.Dispose();
            _pathFileStream.Dispose();
        }
    }

    enum OpenMode
    {
        Create,
        Open,
    }
}

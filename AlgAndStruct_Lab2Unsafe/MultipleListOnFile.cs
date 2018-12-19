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
                meta.elementCount++;
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
                meta.elementCount++;
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

        public void PackData()
        {
            //Помечаем удаляемые города
            var citiesMeta = ReadCitiesMeta();

            var removedElementsFlags = new bool[citiesMeta.elementCount];

            var offset = citiesMeta.removeStackPtr;
            var removedElementsCount = 0;

            while(offset!=0)
            {
                var index = (offset - CitiesMeta.Size) / City.Size;
                removedElementsFlags[index] = true;
                removedElementsCount++;

                var city = ReadCity(offset);
                offset = city.nextPtr;
            }

            //Перемещаем данные
            long remIndex = 0, liveIndex = citiesMeta.elementCount - 1;

            var changedCityPtrs = new Dictionary<long, long>();

            while (remIndex < liveIndex)
            {
                while (remIndex < liveIndex && removedElementsFlags[remIndex] !=true)
                {
                    remIndex++;
                }

                while (remIndex < liveIndex && removedElementsFlags[liveIndex] != false)
                {
                    liveIndex--;
                }

                if (remIndex >= liveIndex)
                {
                    break;
                }

                removedElementsFlags[remIndex] = false;
                removedElementsFlags[liveIndex] = true;

                var oldPtr = liveIndex * City.Size + CitiesMeta.Size;
                var newPtr = remIndex * City.Size + CitiesMeta.Size;
                changedCityPtrs.Add(oldPtr, newPtr);

                var city = ReadCity(oldPtr);
                _cityFileStream.Seek(newPtr, SeekOrigin.Begin);
                _cityFileStream.Write(BitConverter.GetBytes(city));
            }

            citiesMeta.elementCount -= removedElementsCount;

            //Помечаем удаляемые дороги

            var pathMeta = ReadRoadsMeta();

            removedElementsFlags = new bool[pathMeta.elementCount];
            offset = pathMeta.removeStackPtr;
            removedElementsCount = 0;

            while (offset != 0)
            {
                var index = (offset - RoadsMeta.Size) / Path.Size;
                removedElementsFlags[index] = true;
                removedElementsCount++;

                var path = ReadPath(offset);
                offset = path.nextPtr;
            }

            //Перемещаем дороги

            remIndex = 0;
            liveIndex = pathMeta.elementCount - 1;

            var changedPathPtrs = new Dictionary<long, long>();

            while (remIndex < liveIndex)
            {
                while (remIndex < liveIndex && removedElementsFlags[remIndex] != true)
                {
                    remIndex++;
                }

                while (remIndex < liveIndex && removedElementsFlags[liveIndex] != false)
                {
                    liveIndex--;
                }

                if(remIndex >= liveIndex)
                {
                    break;
                }

                removedElementsFlags[remIndex] = false;
                removedElementsFlags[liveIndex] = true;

                var oldPtr = liveIndex * Path.Size + RoadsMeta.Size;
                var newPtr = remIndex * Path.Size + RoadsMeta.Size;
                changedPathPtrs.Add(oldPtr, newPtr);

                var path = ReadPath(oldPtr);
                _pathFileStream.Seek(newPtr, SeekOrigin.Begin);
                _pathFileStream.Write(BitConverter.GetBytes(path));
            }

            //Дальше будет адовый код для изменения указателей
            if (changedCityPtrs.ContainsKey(citiesMeta.startPtr))
            {
                citiesMeta.startPtr = changedCityPtrs[citiesMeta.startPtr];
            }

            for (int i = CitiesMeta.Size; i < citiesMeta.elementCount * City.Size + CitiesMeta.Size; i += City.Size)
            {
                var city = ReadCity(i);
                var changeFlag = false;

                if (changedCityPtrs.ContainsKey(city.nextPtr))
                {
                    city.nextPtr = changedCityPtrs[city.nextPtr];
                    changeFlag = true;
                }

                if(changedPathPtrs.ContainsKey(city.roadsListPtr))
                {
                    city.roadsListPtr = changedPathPtrs[city.roadsListPtr];
                    changeFlag = true;
                }

                if (changeFlag)
                {
                    _cityFileStream.Seek(i, SeekOrigin.Begin);
                    _cityFileStream.Write(BitConverter.GetBytes(city));
                }
            }

            for (int i = RoadsMeta.Size; i < citiesMeta.elementCount * Path.Size + RoadsMeta.Size; i += Path.Size)
            {
                var path = ReadPath(i);
                var changeFlag = false;

                if(changedCityPtrs.ContainsKey(path.cityPtr))
                {
                    path.cityPtr = changedCityPtrs[path.cityPtr];
                    changeFlag = true;
                }

                if(changedPathPtrs.ContainsKey(path.nextPtr))
                {
                    path.nextPtr = changedPathPtrs[path.nextPtr];
                    changeFlag = true;
                }

                if (changeFlag)
                {
                    _pathFileStream.Seek(i, SeekOrigin.Begin);
                    _pathFileStream.Write(BitConverter.GetBytes(path));
                }
            }

            citiesMeta.removeStackPtr = 0;
            pathMeta.removeStackPtr = 0;

            _cityFileStream.Seek(0, SeekOrigin.Begin);
            _cityFileStream.Write(BitConverter.GetBytes(citiesMeta));

            _pathFileStream.Seek(0, SeekOrigin.Begin);
            _pathFileStream.Write(BitConverter.GetBytes(pathMeta));
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            var cityMeta = ReadCitiesMeta();
            var pathMeta = ReadCitiesMeta();

            sb.AppendLine("Cities data:");
            var offset = cityMeta.startPtr;

            while (offset!=0)
            {
                var city = ReadCity(offset);

                sb.Append("NextPtr: ")
                    .Append(city.nextPtr)
                    .Append(" Paths ptr: ")
                    .Append(city.roadsListPtr)
                    .AppendLine(" List of roads:");

                var pathsOffset = city.roadsListPtr;

                while (pathsOffset != 0)
                {
                    var path = ReadPath(pathsOffset);

                    sb.Append("Next road ptr: ")
                        .Append(path.nextPtr)
                        .Append(" City ptr: ")
                        .Append(path.cityPtr)
                        .Append("; ");
                    pathsOffset = path.nextPtr;
                }
                sb.AppendLine();
                offset = city.nextPtr;
            }

            return sb.ToString();
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

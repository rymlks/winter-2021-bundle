using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets
{
    class ShpFile : IShpFile, IRenderable
    {
        private bool disposed;
        private FileStream fs;
        private BinaryReader br;

        public int FileCode { get; set; }
        public int FileLength { get; set; }
        public int FileVersion { get; set; }
        public ShapeType ShpType { get; set; }
        public RangeXY TotalXYRange { get; set; }
        public Range ZRange { get; set; }
        public Range MRange { get; set; }
        public int ContentLength { get; set; }
        public List<ShpRecord> RecordSet { get; set; }

        private string path;

        public ShpFile(string path)
        {
            this.path = path;
        }

        ~ShpFile() // the finalizer
        {
            Dispose(false);
        }

        public IEnumerator OpenFile()
        {
            if (File.Exists(path))
            {
                yield return new WaitForSeconds(2);
                fs = File.OpenRead(path);
                br = new BinaryReader(fs);
            }
            else
            {
                using (UnityWebRequest request = UnityWebRequest.Get(path))
                {
                    Debug.Log("Downloading file from Web: " + path);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.useHttpContinue = false;
                    yield return request.SendWebRequest();

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] results = request.downloadHandler.data;
                        Debug.Log("Downloaded file from Web. numbytes: " + results.Length + "Progress: " + request.downloadProgress);

                        Stream stream = new MemoryStream(results);
                        br = new BinaryReader(stream);
                        Debug.Log("Done.");
                    }

                }
            }
        }

        public void Load()
        {
            FileCode = Util.FromBigEndian(br.ReadInt32());
            br.BaseStream.Seek(20, SeekOrigin.Current);
            FileLength = Util.FromBigEndian(br.ReadInt32()) * 2;
            FileVersion = br.ReadInt32();
            ShpType = (ShapeType)br.ReadInt32();

            TotalXYRange = new RangeXY();
            ZRange = new Range();
            MRange = new Range();
            TotalXYRange.Load(ref br);
            ZRange.Load(ref br);
            MRange.Load(ref br);

            ContentLength = FileLength - 100;
            long curPoint = 0;

            RecordSet = new List<ShpRecord>();
            while (curPoint < ContentLength)
            {
                ShpRecord record = new ShpRecord(ShpType);
                record.Load(ref br);
                long size = record.GetLength();
                RecordSet.Add(record);

                curPoint += record.GetLength();
            }
        }

        public IRecord GetData(int index)
        {
            return RecordSet.ElementAt(index);
        }

        public IRecord GetData(ShapeType type, int offset, int length)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            IRecord record = new ShpRecord(type);
            record.Load(ref br);
            return record;
        }

        public void Render(Color color)
        {
            foreach (ShpRecord record in RecordSet)
            {
                record.Render(TotalXYRange, color);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                br.Dispose();
                fs.Dispose();
            }
            disposed = true;
        }
    }

    class ShxFile : IShpFile, IDisposable, IRenderable
    {
        private bool disposed;
        private FileStream fs;
        private BinaryReader br;
        private string shxPath;
        private string shpPath;
        private string dbfPath;

        public int FileCode { get; set; }
        public int FileLength { get; set; }
        public int FileVersion { get; set; }
        public ShapeType ShpType { get; set; }
        public RangeXY TotalXYRange { get; set; }
        public Range ZRange { get; set; }
        public Range MRange { get; set; }
        public int ContentLength { get; set; }
        public List<ShxRecord> RecordSet { get; set; }
        public ShpFile ContentsFile { get; set; }
        public DbfFile DatabseFile { get; set; }

        public ShxFile(string path)
        {
            shxPath = path;
            shpPath = path.Replace(".shx", ".shp");
            dbfPath = path.Replace(".shx", ".dbf");
        }

        ~ShxFile() // the finalizer
        {
            Dispose(false);
        }

        public IEnumerator OpenFile()
        {
            if (File.Exists(shxPath))
            {
                fs = File.OpenRead(shxPath);
                br = new BinaryReader(fs);
            } else
            {
                using (UnityWebRequest request = UnityWebRequest.Get(shxPath))
                {
                    Debug.Log("Downloading file from Web: " + shxPath);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.useHttpContinue = false;
                    yield return request.SendWebRequest();

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] results = request.downloadHandler.data;
                        Debug.Log("Downloaded file from Web. numbytes: " + results.Length + "Progress: " + request.downloadProgress);

                        Stream stream = new MemoryStream(results);
                        br = new BinaryReader(stream);
                        Debug.Log("Done.");
                    }

                }
            }
        }

        public void Load()
        {
            FileCode = Util.FromBigEndian(br.ReadInt32());
            br.BaseStream.Seek(20, SeekOrigin.Current);
            FileLength = Util.FromBigEndian(br.ReadInt32()) * 2;
            FileVersion = br.ReadInt32();
            ShpType = (ShapeType)br.ReadInt32();

            TotalXYRange = new RangeXY();
            ZRange = new Range();
            MRange = new Range();
            TotalXYRange.Load(ref br);
            ZRange.Load(ref br);
            MRange.Load(ref br);

            int ContentLength = FileLength - 100;
            long curPoint = 0;

            RecordSet = new List<ShxRecord>();
            while (curPoint < ContentLength)
            {
                ShxRecord record = new ShxRecord();
                record.Load(ref br);
                RecordSet.Add(record);

                curPoint += record.GetLength();
            }
            try
            {
                ContentsFile = (ShpFile)FileFactory.CreateInstance(shpPath);
            }
            catch (Exception e)
            {
                Debug.LogError("error reading shapefile: " + e);
                ContentsFile = null;
            }
            try
            {
                DatabseFile = (DbfFile)FileFactory.CreateInstance(dbfPath);
                //DatabseFile.Load();
            }
            catch (Exception e)
            {
                Debug.LogError("error reading dbf file: " + e);
                DatabseFile = null;
            }
        }

        public GISRecord GetData(int index)
        {
            if(ContentsFile != null)
            {
                ShxRecord shxRecord = (ShxRecord)RecordSet[index];
                ShpRecord shpRecord = (ShpRecord)ContentsFile.GetData(ShpType, shxRecord.Offset, shxRecord.Length);
                //DbfRecord dbfRecord = (DbfRecord)DatabseFile.GetData(shxRecord.Offset, shxRecord.Length);
                DbfRecord dbfRecord = DatabseFile.RecordSet[index];
                return new GISRecord(shpRecord, dbfRecord);
            }
            else
            {
                return null;
            }
        }

        public void Render(Color color)
        {
            ContentsFile.Render(color);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                br.Dispose();
                if (fs != null)
                    fs.Dispose();
                ContentsFile.Dispose();
            }
            disposed = true;
        }

        IRecord IFile.GetData(int index)
        {
            throw new NotImplementedException();
        }
    }

    class DbfFile : IFile
    {
        private bool disposed;
        private FileStream fs;
        private BinaryReader br;

        public DBFVersion Version { get; set; }
        public byte UpdateYear { get; set; }
        public byte UpdateMonth { get; set; }
        public byte UpdateDay { get; set; }
        public int UpdateDate { get { return UpdateYear * 10000 + UpdateMonth * 100 + UpdateDay; } }
        public int NumberOfRecords { get; set; }
        public short HeaderLength { get; set; }
        public short RecordLength { get; set; }
        public byte[] Reserved { get; set; }
        public List<DbfFieldDiscriptor> FieldList { get; set; }
        public List<DbfRecord> RecordSet { get; set; }

        private string path;

        public DbfFile(string path)
        {
            this.path = path;
        }

        public IEnumerator OpenFile()
        {
            if (File.Exists(path))
            {
                yield return new WaitForSeconds(2);
                fs = File.OpenRead(path);
                br = new BinaryReader(fs);
            }
            else
            {
                using (UnityWebRequest request = UnityWebRequest.Get(path))
                {
                    Debug.Log("Downloading file from Web: " + path);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.useHttpContinue = false;
                    yield return request.SendWebRequest();

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] results = request.downloadHandler.data;
                        Debug.Log("Downloaded file from Web. numbytes: " + results.Length + "Progress: " + request.downloadProgress);

                        Stream stream = new MemoryStream(results);
                        br = new BinaryReader(stream);
                        Debug.Log("Done.");
                    }

                }
            }
        }

        public void Load()
        {   
            Version = (DBFVersion)br.ReadByte();
            UpdateYear = br.ReadByte();
            UpdateMonth = br.ReadByte();
            UpdateDay = br.ReadByte();
            NumberOfRecords = br.ReadInt32();
            HeaderLength = br.ReadInt16();
            RecordLength = br.ReadInt16();
            Reserved = br.ReadBytes(20);

            FieldList = new List<DbfFieldDiscriptor>();
            RecordSet = new List<DbfRecord>();
            while (br.PeekChar() != 0x0d)
            {
                DbfFieldDiscriptor field = new DbfFieldDiscriptor();
                field.Load(ref br);
                FieldList.Add(field);
            }

            br.BaseStream.Position = HeaderLength;

            while (true)
            {
                DbfRecord record = new DbfRecord(FieldList);
                try
                {
                    record.Load(ref br);
                } catch (EndOfStreamException) {
                    break;
                }
                RecordSet.Add(record);
            }
        }


        public IRecord GetData(int offset, int length)
        {
            br.BaseStream.Seek(offset + HeaderLength, SeekOrigin.Begin);
            IRecord record = new DbfRecord(FieldList);
            record.Load(ref br);
            return record;
        }

        public IRecord GetData(int index)
        {
            throw new NotImplementedException();
        }
    }

    public class FileFactory
    {
        public static readonly IDictionary<string, Func<string, IFile>> Creators =
            new Dictionary<string, Func<string, IFile>>()
            {
                { ".shp", (path) => new ShpFile(path) },
                { ".shx", (path) => new ShxFile(path) },
                { ".dbf", (path) => new DbfFile(path) }
            };

        public static IFile CreateInstance(string path)
        {
            return Creators[Path.GetExtension(path)](path);
        }
    }
}

using ICSharpCode.SharpZipLib.Checksum;
using Ionic.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System.IO.Compression;
using System.Text;

namespace Shipeng.Util
{
    /// <summary>
    /// 文件压缩帮助类，基于ICSharpCode.SharpZipLib.Zip
    /// Author:李仕鹏
    /// </summary>
    public class FileZipHelper
    {
        #region ICSharpCode.SharpZipLib.Zip
        /// <summary>
        /// 压缩一个文件
        /// </summary>
        /// <param name="file"> 文件信息 </param>
        /// <returns> </returns>
        public static byte[] ZipFile(FileEntry file)
        {
            return ZipFile(new List<FileEntry> { file });
        }

        /// <summary>
        /// 压缩多个文件
        /// </summary>
        /// <param name="files"> 文件信息列表 </param>
        /// <returns> </returns>
        public static byte[] ZipFile(List<FileEntry> files)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ms))
                {
                    files.ForEach(aFile =>
                    {
                        byte[] fileBytes = aFile.FileBytes;
                        ICSharpCode.SharpZipLib.Zip.ZipEntry entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(aFile.FileName)
                        {
                            DateTime = DateTime.Now,
                            IsUnicodeText = true
                        };
                        zipStream.PutNextEntry(entry);
                        zipStream.Write(fileBytes, 0, fileBytes.Length);
                        zipStream.CloseEntry();
                    });

                    zipStream.IsStreamOwner = false;
                    zipStream.Finish();
                    zipStream.Close();
                    ms.Position = 0;

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        ///  压缩多个文件
        /// </summary>
        /// <param name="files">文件名</param>
        /// <param name="ZipedFileName">压缩包文件名</param>
        /// <returns></returns>
        public static void Zip(string[] files, string ZipedFileName)
        {
            Zip(files, ZipedFileName, string.Empty);
        }

        /// <summary>
        ///  压缩多个文件
        /// </summary>
        /// <param name="files">文件名</param>
        /// <param name="ZipedFileName">压缩包文件名</param>
        /// <param name="Password">解压码</param>
        /// <returns></returns>
        public static void Zip(string[] files, string ZipedFileName, string Password)
        {
            files = files.Where(f => File.Exists(f)).ToArray();
            if (files.Length == 0) throw new FileNotFoundException("未找到指定打包的文件");
            ICSharpCode.SharpZipLib.Zip.ZipOutputStream s = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(File.Create(ZipedFileName));
            s.SetLevel(6);
            if (!string.IsNullOrEmpty(Password.Trim())) s.Password = Password.Trim();
            ZipFileDictory(files, s);
            s.Finish();
            s.Close();
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="files"></param>
        /// <param name="s"></param>
        private static void ZipFileDictory(string[] files, ICSharpCode.SharpZipLib.Zip.ZipOutputStream s)
        {
            ICSharpCode.SharpZipLib.Zip.ZipEntry? entry = null;
            FileStream? fs = null;
            Crc32 crc = new Crc32();
            try
            {
                //创建当前文件夹
                entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("/");  //加上 “/” 才会当成是文件夹创建
                s.PutNextEntry(entry);
                s.Flush();
                foreach (string file in files)
                {
                    //打开压缩文件
                    fs = File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("/" + Path.GetFileName(file));
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                    entry = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// 将多个流进行zip压缩，返回压缩后的流
        /// </summary>
        /// <param name="streams">key：文件名；value：文件名对应的要压缩的流</param>
        /// <returns>压缩后的流</returns>
        public static Stream PackageManyZip(Dictionary<string, Stream> streams)
        {
            byte[] buffer = new byte[6500];
            MemoryStream returnStream = new MemoryStream();
            var zipMs = new MemoryStream();
            using (ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(zipMs))
            {
                zipStream.SetLevel(9);
                foreach (var kv in streams)
                {
                    string fileName = kv.Key;
                    using (var streamInput = kv.Value)
                    {
                        zipStream.PutNextEntry(new ICSharpCode.SharpZipLib.Zip.ZipEntry(fileName));
                        while (true)
                        {
                            var readCount = streamInput.Read(buffer, 0, buffer.Length);
                            if (readCount > 0)
                            {
                                zipStream.Write(buffer, 0, readCount);
                            }
                            else
                            {
                                break;
                            }
                        }
                        zipStream.Flush();
                    }
                }
                zipStream.Finish();
                zipMs.Position = 0;
                zipMs.CopyTo(returnStream, 5600);
            }
            returnStream.Position = 0;
            return returnStream;
        }
        #endregion ICSharpCode.SharpZipLib.Zip    
    }

    /// <summary>
    /// Zip操作基于DotNetZip的封装
    /// Author:李仕鹏
    /// </summary>
    public class DotNetZipHelper
    {
        #region Zip操作基于DotNetZip的封装
        /// <summary>
        /// 得到指定的输入流的ZIP压缩流对象【原有流对象不会改变】
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public static Stream ZipCompress(Stream sourceStream, string entryName = "zip")
        {
            MemoryStream compressedStream = new MemoryStream();
            if (sourceStream != null)
            {
                long sourceOldPosition = 0;
                try
                {
                    sourceOldPosition = sourceStream.Position;
                    sourceStream.Position = 0;
                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                    {
                        zip.AddEntry(entryName, sourceStream);
                        zip.Save(compressedStream);
                        compressedStream.Position = 0;
                    }
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        sourceStream.Position = sourceOldPosition;
                    }
                    catch
                    {
                    }
                }
            }
            return compressedStream;
        }

        /// <summary>
        /// 得到指定的字节数组的ZIP解压流对象
        /// 当前方法仅适用于只有一个压缩文件的压缩包，即方法内只取压缩包中的第一个压缩文件
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public static Stream ZipDecompress(byte[] data)
        {
            Stream decompressedStream = new MemoryStream();
            if (data != null)
            {
                try
                {
                    MemoryStream dataStream = new MemoryStream(data);
                    using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(dataStream))
                    {
                        if (zip.Entries.Count > 0)
                        {
                            zip.Entries.First().Extract(decompressedStream);
                            // Extract方法中会操作ms，后续使用时必须先将Stream位置归零，否则会导致后续读取不到任何数据
                            // 返回该Stream对象之前进行一次位置归零动作
                            decompressedStream.Position = 0;
                        }
                    }
                }
                catch
                {
                }
            }
            return decompressedStream;
        }

        /// <summary>
        /// 压缩ZIP文件
        /// 支持多文件和多目录，或是多文件和多目录一起压缩
        /// </summary>
        /// <param name="list">待压缩的文件或目录集合</param>
        /// <param name="strZipName">压缩后的文件名</param>
        /// <param name="isDirStruct">是否按目录结构压缩</param>
        /// <param name="splitString">文件/目录名称分割字符串</param>
        /// <param name="splitIndex">文件/目录名称按分割符截取的索引</param>
        /// <returns>成功：true/失败：false</returns>
        public static bool CompressMulti(List<string> list, string strZipName, bool isDirStruct,string splitString = "",int? splitIndex = null)
        {
            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.Default))//设置编码，解决压缩文件时中文乱码
                {
                    foreach (string path in list)
                    {
                        string fileName = Path.GetFileName(path);//取目录名称
                        if(!string.IsNullOrWhiteSpace(splitString) && fileName.Contains(splitString) && splitIndex != null)
                        {
                            fileName = fileName.Split(splitString)[(int)splitIndex];
                        }
                        //如果是目录
                        if (Directory.Exists(path))
                        {
                            if (isDirStruct)//按目录结构压缩
                            {
                                zip.AddDirectory(path, fileName);
                            }
                            else//目录下的文件都压缩到Zip的根目录
                            {
                                zip.AddDirectory(path);
                            }
                        }
                        if (File.Exists(path))//如果是文件
                        {
                            zip.AddFile(path);
                        }
                    }
                    zip.Save(strZipName);//压缩
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 多个文件压缩打包
        /// </summary>
        /// <param name="filePaths">待压缩的文件路径集合</param>
        /// <param name="zipPath">压缩后的文件存储路径</param>
        /// <param name="splitString">文件名称分割字符串</param>
        /// <param name="splitIndex">文件名称按分割符截取的索引</param>
        public static void CompressMulti(string[] filePaths, string zipPath, string splitString = "", int? splitIndex = null)
        {
            // 创建压缩文件
            using (FileStream zipFileStream = new FileStream(zipPath, FileMode.Create))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                {
                    foreach (string filePath in filePaths)
                    {
                        string fileName = Path.GetFileName(filePath);
                        if (!string.IsNullOrWhiteSpace(splitString) && fileName.Contains(splitString) && splitIndex != null)
                        {
                            fileName = fileName.Split(splitString)[(int)splitIndex];
                        }
                        zipArchive.CreateEntryFromFile(filePath, fileName);
                    }
                }
            }
        }

        /// <summary>
        /// 多个文件压缩打包
        /// </summary>
        /// <param name="filePath">多个文件的地址</param>
        /// <param name="split">文件名截取规则，如@@则表示将文件名按@@截取然后取后面</param>
        /// <param name="zipPath">压缩后的文件名</param>
        /// <returns></returns>
        public static bool CompressMulti(List<string> filePath, string split, string zipPath)
        {
            try
            {
                Dictionary<string, Stream> streams = new Dictionary<string, Stream>();
                foreach (string path in filePath)
                {
                    string newFileName = Path.GetFileName(path);
                    if (!string.IsNullOrWhiteSpace(split)) newFileName = newFileName.Split(split)[1];
                    if (streams.ContainsKey(newFileName)) newFileName = newFileName.Split('.')[0] + "(1)" + newFileName.Split('.')[1];
                    streams.Add(newFileName, FileHelper.FileToStream(path));
                }
                Stream zipStream = FileZipHelper.PackageManyZip(streams);
                FileHelper.StreamToFile(zipStream, zipPath);//将Stream写入文件
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 解压ZIP文件
        /// </summary>
        /// <param name="strZipPath">待解压的ZIP文件</param>
        /// <param name="strUnZipPath">解压的目录</param>
        /// <param name="overWrite">是否覆盖</param>
        /// <returns>成功：true/失败：false</returns>
        public static bool Decompression(string strZipPath, string strUnZipPath, bool overWrite)
        {
            try
            {
                ReadOptions options = new ReadOptions();
                options.Encoding = Encoding.Default;//设置编码，解决解压文件时中文乱码
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(strZipPath, options))
                {
                    foreach (Ionic.Zip.ZipEntry entry in zip)
                    {
                        if (string.IsNullOrEmpty(strUnZipPath))
                        {
                            strUnZipPath = strZipPath.Split('.').First();
                        }
                        if (overWrite)
                        {
                            entry.Extract(strUnZipPath, ExtractExistingFileAction.OverwriteSilently);//解压文件，如果已存在就覆盖
                        }
                        else
                        {
                            entry.Extract(strUnZipPath, ExtractExistingFileAction.DoNotOverwrite);//解压文件，如果已存在不覆盖
                        }
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 将一个文件夹的内容读取为Stream的压缩包
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="stream"></param>
        public static async Task ReadDirectoryToZipStreamAsync(DirectoryInfo directory, Stream stream)
        {
            var fileList = directory.GetFiles();
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create);
            foreach (var file in fileList)
            {
                var relativePath = file.FullName.Replace(directory.FullName, "");
                if (relativePath.StartsWith("\\") || relativePath.StartsWith("//"))
                {
                    relativePath = relativePath.Substring(1);
                }
                var zipArchiveEntry = zipArchive.CreateEntry(relativePath, System.IO.Compression.CompressionLevel.NoCompression);
                using (var entryStream = zipArchiveEntry.Open())
                {
                    using var toZipStream = file.OpenRead();
                    await toZipStream.CopyToAsync(stream);
                }
                await stream.FlushAsync();
            }
        }

        /// <summary>
        /// 压缩文件/文件夹
        /// </summary>
        /// <param name="filePath">需要压缩的文件/文件夹路径</param>
        /// <param name="zipPath">压缩文件路径（zip后缀）</param>
        /// <param name="password">密码</param>
        /// <param name="filterExtenList">需要过滤的文件后缀名</param>
        public static void CompressionFile(string filePath, string zipPath, string password = "", List<string> filterExtenList = null)
        {
            try
            {
                string saveRoot = Path.GetDirectoryName(zipPath);
                if (!Directory.Exists(saveRoot)) Directory.CreateDirectory(saveRoot);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.UTF8))
                {
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        zip.Password = password;//只有设置了zip.Password = "password"之后，被压缩的文件才会有密码保护
                    }
                    if (Directory.Exists(filePath))
                    {
                        if (filterExtenList == null)
                            zip.AddDirectory(filePath);
                        else
                            AddDirectory(zip, filePath, filePath, filterExtenList);
                    }
                    else if (File.Exists(filePath))
                    {
                        zip.AddFile(filePath, "");
                    }
                    zip.Save(zipPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("【DotNetZipHelper【CompressionFile】】 压缩文件/文件夹发生异常：" + ex);
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="zip">ZipFile对象</param>
        /// <param name="dirPath">需要压缩的文件夹路径</param>
        /// <param name="rootPath">根目录路径</param>
        /// <param name="filterExtenList">需要过滤的文件后缀名</param>
        public static void AddDirectory(Ionic.Zip.ZipFile zip, string dirPath, string rootPath, List<string> filterExtenList)
        {
            var files = Directory.GetFiles(dirPath);
            for (int i = 0; i < files.Length; i++)
            {
                //如果Contains不支持第二个参数，就用.ToLower()
                if (filterExtenList == null || (filterExtenList != null && !filterExtenList.Any(d => Path.GetExtension(files[i]).Contains(d, StringComparison.OrdinalIgnoreCase))))
                {
                    //获取相对路径作为zip文件中目录路径
                    zip.AddFile(files[i], Path.GetRelativePath(rootPath, dirPath));
                    //如果没有Path.GetRelativePath方法，可以用下面代码替换
                    //string relativePath = Path.GetFullPath(dirPath).Replace(Path.GetFullPath(rootPath), "");
                    //zip.AddFile(files[i], relativePath);
                }
            }
            var dirs = Directory.GetDirectories(dirPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                AddDirectory(zip, dirs[i], rootPath, filterExtenList);
            }
        }

        /// <summary> 
        /// 压缩指定文件或目录 
        /// </summary> 
        /// <param name="fileOrDirectoryName">要进行压缩的文件或目录名称</param> 
        /// <returns>生成的压缩文件名</returns> 
        public static string Compress(string fileOrDirectoryName)
        {
            string zipPath = _GetZipPath(fileOrDirectoryName);
            Compress(fileOrDirectoryName, zipPath);
            return zipPath;
        }

        /// <summary> 
        /// 压缩指定文件或目录 
        /// </summary> 
        /// <param name="fileOrDirectoryName">要进行压缩的文件或目录名称</param> 
        /// <param name="zipPath">生成的压缩文件路径</param> 
        public static void Compress(string fileOrDirectoryName, string zipPath)
        {
            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.GetEncoding("utf-8")))
            {
                zip.AddItem(fileOrDirectoryName, "");
                zip.Save(zipPath);
            }
        }

        /// <summary> 
        /// 分卷压缩指定文件或目录 
        /// </summary> 
        /// <param name="fileOrDirectoryName">要进行压缩的文件或目录名称</param> 
        /// <param name="segmentSize">分卷大小(MB)</param> 
        public static void Compress(string fileOrDirectoryName, int segmentSize)
        {
            string zipPath = _GetZipPath(fileOrDirectoryName);
            Compress(fileOrDirectoryName, zipPath, ZipDataUnit.MB, segmentSize);
        }

        /// <summary> 
        /// 分卷压缩指定文件或目录 
        /// </summary> 
        /// <param name="fileOrDirectoryName">要进行压缩的文件或目录名称</param> 
        /// <param name="zipPath">生成的压缩文件路径</param> 
        /// <param name="dataUnit">分卷数据单位</param> 
        /// <param name="segmentSize">分卷大小</param> 
        public static void Compress(string fileOrDirectoryName, string zipPath, ZipDataUnit dataUnit, int segmentSize)
        {
            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.GetEncoding("utf-8")))
                {
                    zip.MaxOutputSegmentSize = (int)dataUnit * segmentSize;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.BufferSize = 1024;
                    zip.CaseSensitiveRetrieval = true;
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                    zip.AddItem(fileOrDirectoryName, "");
                    zip.Save(zipPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("【DotNetZipHelper【Compress】】分卷压缩指定文件或目录发生异常：" + ex);
            }
        }

        private static string _GetZipPath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory)) directory = Path.GetPathRoot(path);
            string fileName = Path.GetFileName(path);
            string zipFileName = null;
            //文件路径 
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                zipFileName = Path.ChangeExtension(fileName, ".zip");
            }
            else
            {
                zipFileName = directory.Split('\\').Last();
                if (string.IsNullOrEmpty(zipFileName)) zipFileName = "未命名";
                else directory = directory.Replace(zipFileName, "");
                zipFileName = ".zip";
            }
            return Path.Combine(directory, zipFileName);
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="strZipPath">zip文件路径</param>
        /// <param name="strUnZipPath">解压文件夹</param>
        public static void DeCompress(string strZipPath, string strUnZipPath)
        {
            try
            {
                bool overWrite = true;//设置是否覆盖文件
                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(strZipPath))
                {
                    foreach (Ionic.Zip.ZipEntry entry in zip)
                    {
                        if (string.IsNullOrEmpty(strUnZipPath))
                        {
                            strUnZipPath = strZipPath.Split('.')[0];
                        }
                        if (overWrite)
                        {
                            entry.Extract(strUnZipPath, ExtractExistingFileAction.OverwriteSilently);//解压文件，如果已存在就覆盖
                        }
                        else
                        {
                            entry.Extract(strUnZipPath, ExtractExistingFileAction.DoNotOverwrite);//解压文件，如果已存在不覆盖
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("【DotNetZipHelper【DeCompress】】解压文件发生异常：" + ex);
            }
        }
        #endregion Zip操作基于DotNetZip的封装
    }

    /// <summary>
    /// SharpCompress，功能最强大的压缩、解压开源插件
    /// 它支持处理zip、rar、7z等多种格式的压缩文件，使用方式也很简单
    /// 缺陷：创建压缩文件的时候没法设置密码
    /// Author:李仕鹏
    /// </summary>
    public class SharpCompressHelper
    {
        /// <summary>
        /// 压缩文件/文件夹
        /// </summary>
        /// <param name="filePath">需要压缩的文件/文件夹路径</param>
        /// <param name="zipPath">压缩文件路径（zip后缀）</param>
        /// <param name="filterExtenList">需要过滤的文件后缀名</param>
        public static void CompressionFile(string filePath, string zipPath, List<string> filterExtenList = null)
        {
            try
            {
                using (var zip = File.Create(zipPath))
                {
                    var option = new WriterOptions(CompressionType.Deflate)
                    {
                        //SharpCompress版本不同，设置ArchiveEncoding的方式也不同，默认设置了UTF8防止解压乱码
                        ArchiveEncoding = new ArchiveEncoding()
                        {
                            Default = Encoding.UTF8
                        }
                    };
                    //通过设置ArchiveType切换生成不同格式压缩文件
                    using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, option))
                    {
                        if (Directory.Exists(filePath))
                        {
                            //添加文件夹
                            zipWriter.WriteAll(filePath, "*",
                                (path) => filterExtenList == null ? true : !filterExtenList.Any(d => Path.GetExtension(path).Contains(d, StringComparison.OrdinalIgnoreCase)), SearchOption.AllDirectories);
                        }
                        else if (File.Exists(filePath))
                        {
                            zipWriter.Write(Path.GetFileName(filePath), filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("【SharpCompressHelper【CompressionFile】】压缩文件/文件夹发生异常：" + ex);
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipPath">压缩文件路径</param>
        /// <param name="dirPath">解压到文件夹路径</param>
        /// <param name="password">密码</param>
        public static void DeCompressionFile(string zipPath, string dirPath, string password = "")
        {
            if (!File.Exists(zipPath))
            {
                throw new ArgumentNullException("zipPath压缩文件不存在");
            }
            Directory.CreateDirectory(dirPath);
            try
            {
                using (Stream stream = File.OpenRead(zipPath))
                {
                    var option = new ReaderOptions()
                    {
                        ArchiveEncoding = new ArchiveEncoding()
                        {
                            Default = Encoding.UTF8
                        }
                    };
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        option.Password = password;
                    }
                    var reader = ReaderFactory.Open(stream, option);
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(dirPath, reader.Entry.Key));
                        }
                        else
                        {
                            //创建父级目录，防止Entry文件,解压时由于目录不存在报异常
                            var file = Path.Combine(dirPath, reader.Entry.Key);
                            Directory.CreateDirectory(Path.GetDirectoryName(file));
                            reader.WriteEntryToFile(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("【SharpCompressHelper【DeCompressionFile】】解压文件发生异常：" + ex);
            }
        }
    }

    /// <summary> 
    /// 分卷数据单位 
    /// </summary> 
    public enum ZipDataUnit
    {
        /// <summary> 
        /// 1024字节 
        /// </summary> 
        KB = 1024,

        /// <summary> 
        /// 1024 * 1024字节 
        /// </summary> 
        MB = 1024 * 1024,

        /// <summary> 
        /// 1024 * 1024 * 1024字节 
        /// </summary> 
        GB = 1024 * 1024 * 1024
    }

}

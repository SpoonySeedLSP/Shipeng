using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using Minio.DataModel.Encryption;
using Minio.Exceptions;

namespace Shipeng.Util
{
    /// <summary>
    /// .net minio 帮助类
    /// Author:李仕鹏
    /// </summary>
    public class MinioHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="accessKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="withSSL"></param>
        /// <returns></returns>
        public static IMinioClient Create(string endPoint, string accessKey, string secretKey, bool withSSL = false)
        {
            var client = new MinioClient()
                                  .WithEndpoint(endPoint)
                                  .WithCredentials(accessKey,secretKey);
            if (withSSL)
            {
                client = client.WithSSL();
            }
            return client.Build();
        }

        #region 操作存储桶

        /// <summary>
        /// 创建存储桶
        /// <example >
        ///<code >
        /// await MinioHelper.MakeBucket(minio, buckName);    
        ///</code>
        /// </example>
        /// </summary>
        /// <param name = "minio"> 连接实例 </param >
        /// <param name="bucketName">存储桶名称</param>
        /// <param name = "loc"> 可选参数 </param >
        /// <returns ></returns >
        public static Task MakeBucket(IMinioClient minio, string bucketName, string loc = "us-east-1")
        {
            try
            {
                bool found = BucketExists(minio, bucketName);
                if (found)
                {
                    throw new Exception(string.Format("存储桶[{0}]已存在", bucketName));
                }

                MakeBucketArgs args = new MakeBucketArgs()
                                       .WithBucket(bucketName)
                                       .WithLocation(loc);

                return minio.MakeBucketAsync(args);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 校验是否存在,如果不存在则报错
        /// <example>
        /// 调用示例
        /// <code>
        /// bool exists = MinioHelper.BucketExists(minio, buckName);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="minio"></param>
        /// <param name="bucketName"></param>
        /// <exception cref="Exception"></exception>
        private static void CheckBucket(IMinioClient minio, string bucketName)
        {
            bool found = BucketExists(minio, bucketName);
            if (!found)
            {
                throw new Exception(string.Format("存储桶[{0}]不存在", bucketName));
            }
        }

        /// <summary>
        /// 列出所有的存储桶
        /// </summary>
        /// <example>
        /// <code>
        /// abc</code>
        /// </example>
        /// <param name="minio">连接实例</param>
        /// <returns></returns>
        public static object ListBuckets(IMinioClient minio)
        {
            return ListBucketsAsync(minio).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步列出所有存储桶。
        /// </summary>
        /// <param name="minio">MinIO 连接实例。</param>
        /// <returns>MinIO SDK 返回的存储桶列表结果。</returns>
        public static async Task<object> ListBucketsAsync(IMinioClient minio)
        {
            return await minio.ListBucketsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 检查存储桶是否存在
        /// </summary>
        /// <example>
        /// <code>
        /// var data = MinioHelper.ListBuckets(minio);
        /// </code>
        /// </example>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <returns></returns>
        public static bool BucketExists(IMinioClient minio, string bucketName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return BucketExistsAsync(minio, bucketName, cancellationToken).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 异步检查存储桶是否存在。
        /// </summary>
        /// <param name="minio">MinIO 连接实例。</param>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        public static async Task<bool> BucketExistsAsync(IMinioClient minio, string bucketName, CancellationToken cancellationToken = default(CancellationToken))
        {
            BucketExistsArgs args = new BucketExistsArgs()
                                            .WithBucket(bucketName);

            return await minio.BucketExistsAsync(args, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除一个存储桶
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <returns></returns>
        public static Task RemoveBucket(IMinioClient minio, string bucketName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                CheckBucket(minio, bucketName);

                RemoveBucketArgs args = new RemoveBucketArgs()
                                            .WithBucket(bucketName);
                return minio.RemoveBucketAsync(args, cancellationToken);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>列出存储桶里的对象
        /// 列出存储桶里的对象
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="prefix">对象的前缀</param>
        /// <param name="recursive">true代表递归查找，false代表类似文件夹查找，以'/'分隔，不查子文件夹</param>
        public static IAsyncEnumerable<Item> ListObjects(IMinioClient minio, string bucketName, string prefix = null, bool recursive = true)
        {
            try
            {
                ListObjectsArgs args = new ListObjectsArgs()
                                        .WithBucket(bucketName)
                                        .WithPrefix(prefix)
                                        .WithRecursive(recursive);

                IAsyncEnumerable<Item> data = minio.ListObjectsEnumAsync(args);

                return data;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion

        #region 操作文件对象
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <returns></returns>
        public static bool FileExist(IMinioClient minio, string bucketName, string objectName)
        {
            return FileExistAsync(minio, bucketName, objectName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步判断对象是否存在。
        /// </summary>
        /// <param name="minio">MinIO 连接实例。</param>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>对象存在返回 true，不存在返回 false。</returns>
        public static async Task<bool> FileExistAsync(IMinioClient minio, string bucketName, string objectName)
        {
            try
            {
                var obj = new StatObjectArgs().WithBucket(bucketName)
                                        .WithObject(objectName);
                await minio.StatObjectAsync(obj).ConfigureAwait(false);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
        }
      
        /// <summary>
        /// 从桶下载文件到流
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="sse"></param>
        /// <returns></returns>
        public static Task<ObjectStat> FGetObject(IMinioClient minio, string bucketName, string objectName, IServerSideEncryption sse = null, Action<Stream> cb = null)
        {
            CheckBucket(minio, bucketName);

            try
            {

                GetObjectArgs args = new GetObjectArgs()
                                        .WithBucket(bucketName)
                                        .WithObject(objectName)
                                        .WithServerSideEncryption(sse)
                                        .WithCallbackStream(cb);
                return minio.GetObjectAsync(args);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 从桶下载文件到本地
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="fileName">本地路径</param>
        /// <param name="sse"></param>
        /// <returns></returns>
        public static Task<ObjectStat> FGetObject(IMinioClient minio, string bucketName, string objectName, string fileName, IServerSideEncryption sse = null)
        {
            CheckBucket(minio, bucketName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            return FGetObject(minio, bucketName, objectName, sse, stream =>
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            });
        }

        /// <summary>
        /// 上传本地文件至存储桶
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="fileName">本地路径</param>
        /// <returns></returns>
        public static Task FPutObject(IMinioClient minio, string bucketName, string objectName, string fileName, string contentType = "application/octet-stream", Dictionary<string, string> metaData = null, IServerSideEncryption sse = null)
        {
            CheckBucket(minio, bucketName);

            try
            {
                //var data= minio.PutObjectAsync(bucketName, objectName, fileName, contentType: "application/octet-stream");
                PutObjectArgs args = new PutObjectArgs()
                                        .WithBucket(bucketName)
                                        .WithObject(objectName)
                                        .WithFileName(fileName)
                                        .WithContentType(contentType)
                                        .WithHeaders(metaData)
                                        .WithServerSideEncryption(sse);
                return minio.PutObjectAsync(args);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region Presigned操作

        /// <summary>
        /// 生成一个给HTTP GET请求用的presigned URL。
        /// 浏览器/移动端的客户端可以用这个URL进行下载，即使其所在的存储桶是私有的。
        /// 这个presigned URL可以设置一个失效时间，默认值是7天。
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="expiresInt">失效时间（以秒为单位），默认是7天，不得大于七天</param>
        /// <param name="reqParams">额外的响应头信息，支持response-expires、response-content-type、
        /// response-cache-control、response-content-disposition</param>
        /// <returns></returns>
        public static Task<string> PresignedGetObject(IMinioClient minio, string bucketName, string objectName, int expiresInt = 1000)
        {
            CheckBucket(minio, bucketName);

            try
            {
                //Dictionary<string, string> reqParams = new Dictionary<string, string> { { "response-content-type", "application/json" } };
                PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                                        .WithBucket(bucketName)
                                                        .WithObject(objectName)
                                                        //.WithHeaders(reqParams)
                                                        .WithExpiry(expiresInt);

                //.WithRequestDate(DateTime.Now.ToUniversalTime());

                return minio.PresignedGetObjectAsync(args);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 生成一个给HTTP PUT请求用的presigned URL。
        /// 浏览器/移动端的客户端可以用这个URL进行上传，即使其所在的存储桶是私有的。
        /// 这个presigned URL可以设置一个失效时间，默认值是7天。
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="expiresInt">失效时间（以秒为单位），默认是7天，不得大于七天</param>
        /// <returns></returns>
        public static Task<string> PresignedPutObject(IMinioClient minio, string bucketName, string objectName, int expiresInt = 1000)
        {
            CheckBucket(minio, bucketName);

            try
            {
                //string presignedUrl = await minio.PresignedPutObjectAsync(bucketName, objectName, expiresInt);
                //Ret = presignedUrl;
                //flag = true;

                PresignedPutObjectArgs args = new PresignedPutObjectArgs()
                                                        .WithBucket(bucketName)
                                                        .WithObject(objectName)
                                                        .WithExpiry(expiresInt);
                return minio.PresignedPutObjectAsync(args);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 允许给POST请求的presigned URL设置策略，
        /// 比如接收对象上传的存储桶名称的策略，key名称前缀，过期策略。
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="PostPolicy">对象的post策略</param>
        /// <returns></returns>
        public async static Task<(Uri, IDictionary<string, string>)> PresignedPostPolicy(IMinioClient minio)
        {
            try
            {
                PostPolicy form = new PostPolicy();
                DateTime expiration = DateTime.UtcNow;
                form.SetExpires(expiration.AddDays(10));
                form.SetKey("my-objectname");
                form.SetBucket("my-bucketname");

                (Uri, IDictionary<string, string>) data = await minio.PresignedPostPolicyAsync(form);
                return data;
                //tuple<uri, dictionary<string, string>> tuple =;

                //string curlCommand = "curl -X POST ";
                //foreach (KeyValuePair<string, string> pair in tuple.Item2)
                //{
                //    curlCommand = curlCommand + $" -F {pair.Key}={pair.Value}";
                //}
                //curlCommand = curlCommand + " -F file=@/etc/bashrc " + tuple.Item1; // https://s3.amazonaws.com/my-bucketname";
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region 操作对象
        /// <summary>
        /// 下载对象指定区域的字节数组做为流。offset和length都必须传
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="offset">offset 是起始字节的位置</param>
        /// <param name="length">length是要读取的长度</param>
        /// <param name="callback">处理流的回调函数</param>
        /// <returns></returns>
        public async static Task GetObjectAsync(IMinioClient minio, string bucketName, string objectName, long offset, long length, Action<Stream> callback)
        {
            CheckBucket(minio, bucketName);

            try
            {
                StatObjectArgs args = new StatObjectArgs()
                                        .WithBucket(bucketName)
                                        .WithObject(objectName)
                                        .WithServerSideEncryption(null);

                await minio.StatObjectAsync(args);

                GetObjectArgs objArgs = new GetObjectArgs()
                                        .WithBucket(bucketName)
                                        .WithObject(objectName)
                                        .WithCallbackStream(callback)
                                        .WithOffsetAndLength(offset, length)
                                        .WithServerSideEncryption(null);
                await minio.GetObjectAsync(objArgs);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 通过Stream上传对象
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <param name="data">要上传的Stream对象</param>
        /// <param name="size">流的大小</param>
        /// <param name="contentType">文件的Content type，默认是"application/octet-stream"</param>
        /// <param name="metaData">元数据头信息的Dictionary对象，默认是null</param>
        /// <returns></returns>
        public async static Task PutObjectAsync(IMinioClient minio, string bucketName, string objectName, Stream data, long size, string contentType = "application/octet-stream", Dictionary<string, string> metaData = null)
        {
            CheckBucket(minio, bucketName);

            try
            {
                //byte[] bs = File.ReadAllBytes(fileName);
                //System.IO.MemoryStream filestream = new System.IO.MemoryStream(bs);
                //await minio.PutObjectAsync(bucketName, objectName, data, size, contentType, metaData);
                PutObjectArgs args = new PutObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(objectName)
                                            .WithStreamData(data)
                                            .WithObjectSize(size)
                                            .WithContentType(contentType)
                                            .WithHeaders(metaData)
                                            .WithServerSideEncryption(null);
                await minio.PutObjectAsync(args);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 从objectName指定的对象中将数据拷贝到destObjectName指定的对象
        /// </summary>
        /// <param name="minio"></param>
        /// <param name="fromBucketName">源存储桶名称</param>
        /// <param name="fromObjectName">源存储桶中的源对象名称</param>
        /// <param name="destBucketName">目标存储桶名称</param>
        /// <param name="destObjectName">要创建的目标对象名称,如果为空，默认为源对象名称</param>
        /// <param name="copyConditions">拷贝操作的一些条件Map</param>
        /// <param name="sseSrc"></param>
        /// <param name="sseDest"></param>
        /// <returns></returns>
        public async static Task CopyObject(IMinioClient minio, string fromBucketName, string fromObjectName, string destBucketName, string destObjectName, CopyConditions copyConditions = null, IServerSideEncryption sseSrc = null, IServerSideEncryption sseDest = null, Dictionary<string, string> metadata = null)
        {
            CheckBucket(minio, fromBucketName);
            CheckBucket(minio, destBucketName);

            try
            {
                CopySourceObjectArgs cpSrcArgs = new CopySourceObjectArgs()
                                                            .WithBucket(fromBucketName)
                                                            .WithObject(fromObjectName)
                                                            .WithCopyConditions(copyConditions)
                                                            .WithServerSideEncryption(sseSrc);
                CopyObjectArgs args = new CopyObjectArgs()
                                                .WithBucket(destBucketName)
                                                .WithObject(destObjectName)
                                                .WithCopyObjectSource(cpSrcArgs)
                                                .WithHeaders(metadata)
                                                .WithServerSideEncryption(sseDest);
                await minio.CopyObjectAsync(args);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 删除一个对象
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">存储桶里的对象名称</param>
        /// <returns></returns>
        public static Task RemoveObject(IMinioClient minio, string bucketName, string objectName)
        {
            CheckBucket(minio, bucketName);

            try
            {
                RemoveObjectArgs args = new RemoveObjectArgs()
                                .WithBucket(bucketName)
                                .WithObject(objectName);
                return minio.RemoveObjectAsync(args);
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 删除多个对象
        /// </summary>
        /// <param name="minio">连接实例</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectsList">含有多个对象名称的IEnumerable</param>
        /// <returns></returns>
        public static async Task<bool> RemoveObjects(IMinioClient minio, string bucketName, List<string> objectsList)
        {
            CheckBucket(minio, bucketName);

            bool flag = false;
            try
            {
                if (objectsList != null)
                {
                    //IObservable<DeleteError> objectsOservable = await minio.RemoveObjectAsync(bucketName, objectsList).ConfigureAwait(false);
                    flag = true;
                    //IDisposable objectsSubscription = objectsOservable.Subscribe(
                    //    objDeleteError => Console.WriteLine($"Object: {objDeleteError.Key}"),
                    //        ex => Console.WriteLine($"OnError: {ex}"),
                    //        () =>
                    //        {
                    //            Console.WriteLine($"Removed objects in list from {bucketName}\n");
                    //        });
                    //return;
                    RemoveObjectsArgs args = new RemoveObjectsArgs()
                                            .WithBucket(bucketName)
                                            .WithObjects(objectsList);
                    await minio.RemoveObjectsAsync(args);
                }
            }
            catch (MinioException e)
            {
                throw new Exception(e.Message);
            }
            return flag;
        }

        #endregion
    }

}

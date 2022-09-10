using Abp;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PearAdmin.AbpTemplate.Admin.Models.Common;
using PearAdmin.AbpTemplate.Admin.Models.FileManage;
using PearAdmin.AbpTemplate.BinaryObjects;
using PearAdmin.AbpTemplate.Business.FileManage;
using PearAdmin.AbpTemplate.Business.FileManage.Dto;
using PearAdmin.AbpTemplate.Common.CommonDto;
using Polly;
using Shipeng.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PearAdmin.AbpTemplate.Admin.Controllers
{
    /// <summary>
    /// 文件管理/档案管理控制器
    /// </summary>
    [AbpMvcAuthorize]
    public class FileManageController : AbpTemplateControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IFileManageService _fileManageService;
        private const long MaxFileSize = 1024 * 1024 * 100;//目前限制文件上传100Mb以内，单位bytes
        private readonly string fileManageStorageAbsoluteDirectory = "";//文件管理/档案管理相关文件存储根目录
        private readonly string fileManageStorageDirectory = "";//文件管理/档案管理相关文件存储目录
        private readonly char directorySeparatorChar= Path.DirectorySeparatorChar;

        public FileManageController(IWebHostEnvironment env,IFileManageService fileManageService)
        {
            _env = env;
            _fileManageService = fileManageService;
            //文件管理/档案管理相关文件存储目录
            fileManageStorageDirectory = $"企业文件资料{directorySeparatorChar}文件管理_档案管理";
            //文件管理/档案管理相关文件存储根目录，包含应用程序内容文件的目录的绝对路径
            fileManageStorageAbsoluteDirectory = Path.Combine(_env.ContentRootPath, fileManageStorageDirectory);
        }

        public IActionResult Index()
        {
            //获取当前用户id或null,如果没有用户登录，则可以为空
            if (!AbpSession.UserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        #region 获取信息
        /// <summary>
        /// 分页获取文件管理/档案管理数据
        /// </summary>
        /// <param name="viewModel">分页查询文件管理/档案管理视图模型</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetPagedFileManageListAsync(QueryPagedFileManageViewModel viewModel)
        {
            try
            {
                var input = PagedViewModelMapToPagedInputDto<QueryPagedFileManageViewModel, QueryPagedFileManageInput>(viewModel);
                var result = await _fileManageService.GetPagedFileManageListAsync(input);
                return Json(new ResponseParamPagedViewModel<FileManageListDto>(result.TotalCount, result.Items));
            }
            catch (AbpException)
            {
                return Json(new ResponseParamPagedViewModel<FileManageListDto>(0, null,"发生异常请联系运营人员！",500));
            }
        }

        /// <summary>
        /// 根据id获取文件/档案信息
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetFileManageInfoAsync(long id)
        {
            try
            {
                var result = await _fileManageService.GetFileManageInfoAsync(id);
                if(result==null) return Json(new ResponseParamViewModel<FileManageInfoDto>(null, "信息为空！",200));
                return Json(new ResponseParamViewModel<FileManageInfoDto>(result));
            }
            catch (AbpException)
            {
                return Json(new ResponseParamViewModel<FileManageInfoDto>(null, "发生异常请联系运营人员！", 500));
            }
        }
        #endregion

        #region 文件操作
        /// <summary>
        /// 单文件上传，并返回文件上传成功后的信息，100MB以内文件上传使用
        /// </summary>
        /// <returns>文件上传成功后返回的文件相关信息</returns>
        //[RequestSizeLimit(MaxFileSize)]
        [HttpPost]
        public async Task<JsonResult> UploadFile()
        {
            try
            {
                //获取上传对象
                var file = Request.Form.Files.First();
                //判断是否选择文件
                if (file == null)
                {
                    throw new UserFriendlyException(L("请选择文件！"));
                }
                //判断文件大小（单位：字节）
                if (file.Length > MaxFileSize) //100MB = 1024 * 1024 * 100
                {
                    throw new UserFriendlyException(L("目前最大允许上传文件为100MB！"));
                }
                //文件的原始名称
                string FileOriginName = file.FileName;
                //文件存储绝对目录
                string path = fileManageStorageAbsoluteDirectory + directorySeparatorChar + AbpSession.UserId;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                //文件存储相对目录
                string relativePath = fileManageStorageDirectory + directorySeparatorChar + AbpSession.UserId;
                //生成文件的名称
                string Extension = Path.GetExtension(FileOriginName);//获取文件的源后缀
                if (string.IsNullOrEmpty(Extension))
                    return Json(new ResponseParamViewModel<FileUploadOutputDto>(null, "文件上传的原始名称好像不对哦，没有找到文件后缀！", 403));
                Guid fileId = SequentialGuidGenerator.Instance.Create();//文件id
                //Guid.NewGuid().ToString() + Extension;//通过uuid和原始后缀生成新的文件名
                string newFileName = FileOriginName.Split('.')[0] + fileId + Extension;
                //文件存储相对地址
                string filePath = relativePath + directorySeparatorChar+ newFileName;
                //最终生成的文件的绝对路径（xxx/xxx/xx.xx）
                string finalyFilePath = path + directorySeparatorChar+ newFileName;
               
                //打开上传文件的输入流
                Stream stream = file.OpenReadStream();
                //将文件流转为二进制数据
                byte[] fileBytes= stream.GetAllBytes();

                var result = new FileUploadOutputDto()
                {
                    FileId = fileId,//文件id
                    FileSize = stream.Length,//文件大小
                    FileName = FileOriginName,//文件名称
                    StorageFileName = newFileName,//存储文件名称
                    ContentType = Extension.Substring(1),//文件类型
                    Path= relativePath,//文件存储相对目录
                    FilePath = filePath,//文件相对路径
                    Bytes= fileBytes//文件二进制数据
                };

                //开始保存拷贝文件
                FileStream targetFileStream = new FileStream(finalyFilePath, FileMode.OpenOrCreate);
                await stream.CopyToAsync(targetFileStream);
                return Json(new ResponseParamViewModel<FileUploadOutputDto>(result,"文件上传成功！"));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new ResponseParamViewModel<FileUploadOutputDto>(null, "文件上传失败！"),500);
                throw new AbpException(message: $"文件上传失败：{ex.Message}");
            }
            catch (Exception ex)
            {
                return Json(new ResponseParamViewModel<FileUploadOutputDto>(null, "文件上传发生异常请联系运营人员！"), 500);
                throw new AbpException(message: $"文件上传失败！发生异常：{ex.Message}");
            }
        }

        #region 大文件分片上传
        /// <summary>
        /// 大文件分片上传，100MB以上文件请使用，单片文件请控制在100MB以内
        /// </summary>
        /// <param name="file">分片文件信息</param>
        /// <returns></returns>
        //[RequestSizeLimit(MaxFileSize)]
        [HttpPost]
        public async Task<ActionResult<SliceFileResponseDto>> RuleUploadFile([FromQuery] SliceFileInfo file)
        {
            try
            {
                string path = fileManageStorageAbsoluteDirectory;
                var files = Request.Form.Files;
                if(files==null || files.Count<=0) throw new UserFriendlyException(L("分片文件为空！"));
                if(files[0]==null) throw new UserFriendlyException(L("分片文件为空！"));
                if (files[0].Length > MaxFileSize) throw new UserFriendlyException(L("分片文件最大允许为100MB！"));
                var buffer = new byte[file.Size];
                var fileName = file.Name;    
                //分片文件存储相对目录
                string relativePath = fileManageStorageDirectory + directorySeparatorChar 
                    + AbpSession.UserId+ directorySeparatorChar + fileName.Split('.')[0];
                //分片文件存储绝对目录
                path = path + directorySeparatorChar+ AbpSession.UserId + directorySeparatorChar + fileName.Split('.')[0];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string name = fileName + "^" + file.Number;//当前分片文件名称
                //分片文件存储绝对地址            
                string filePath = path + directorySeparatorChar + name;
                //分片文件存储相对地址
                relativePath = relativePath + directorySeparatorChar + name;
                using (var stream = new FileStream(filePath, FileMode.Append))
                {
                    await files[0].CopyToAsync(stream);
                }
                var filesList = Directory.GetFiles(Path.GetDirectoryName(path));
                //当顺序号等于分片总数量 合并文件
                bool completed = false;//是否完成
                string relativeDirectory = "";//合并后文件存储相对目录   
                string relativeFilePath = "";//合并后文件存储相对地址
                Guid? fileId=null;//合并文件id
                string storageFileName = "";//合并后存储文件名称
                string contentType = "";//合并后文件类型
                long mergeFileSize = 0;//合并后文件大小
                if ((file.Number + 1) == file.Count || filesList.Length == file.Count)
                {
                    //Item1.是否成功 Item2.提示消息 Item3.合并后文件存储绝对目录 Item4.合并后的文件绝对地址
                    //Item5.合并文件存储相对目录 Item6.合并后文件相对地址 Item7.合并后文件名称
                    //Item8.合并后文件大小 Item9.合并的文件id
                    (bool, string, string, string, string, string,string,long,Guid) merge = await MergeFile(file);
                    completed = merge.Item1;
                    relativeDirectory = merge.Item5;//合并文件存储相对目录
                    relativeFilePath = merge.Item6;//合并后文件相对地址
                    fileId = merge.Item9;//合并文件id
                    storageFileName = merge.Item7;//合并后文件名称
                    contentType = Path.GetExtension(storageFileName).Substring(1);//合并后文件类型
                    mergeFileSize = merge.Item8;//合并后文件大小
                }
                var result = new SliceFileResponseDto()
                {
                    Name= name,//当前分片文件名称
                    FileName= fileName,//文件名称
                    FilePath= relativePath,//分片文件相对路径
                    Completed = completed,//是否完成
                    Number= file.Number,//当前分块序号
                    Count= file.Count,//所有块数
                    FileSize = file.Size,//当前分片文件大小
                    FileId = fileId,//合并文件id
                    StorageFileName= storageFileName,//合并后存储文件名称
                    ContentType = contentType,//合并后文件类型
                    MergeFileSize= mergeFileSize,//合并后文件大小
                    Path = relativeDirectory,//合并后文件存储相对目录
                    RelativeFilePath = relativeFilePath//合并后文件存储相对地址
                };
                return Json(new ResponseParamViewModel<SliceFileResponseDto>(result, "分片上传完成！"));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new ResponseParamViewModel<SliceFileResponseDto>(null, "分片上传失败！"), 500);
                throw new AbpException(message: $"文件上传失败：{ex.Message}");
            }
            catch (Exception ex)
            {
                return Json(new ResponseParamViewModel<SliceFileResponseDto>(null, "分片上传发生异常请联系运营人员！"), 500);
                throw new AbpException(message: $"大文件分片上传失败！发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="file">分片文件信息</param>
        /// <returns>Item1.是否成功 Item2.提示消息 Item3.合并后文件存储绝对目录 Item4.合并后的文件绝对地址 
        /// Item5.合并文件存储相对目录 Item6.合并后文件相对地址 Item7.合并后文件名称 Item8.合并后文件大小 Item9.合并的文件id</returns>
        private async Task<(bool,string,string,string,string,string,string,long,Guid)> MergeFile(SliceFileInfo file)
        {
            try
            {
                long fileSize = 0;
                string fileName = file.Name;//文件名
                //分片文件存储绝对目录
                string path = fileManageStorageAbsoluteDirectory + directorySeparatorChar
                    + AbpSession.UserId + directorySeparatorChar + fileName.Split('.')[0];
                string Extension = Path.GetExtension(fileName);//获取文件的源后缀
                Guid fileId = SequentialGuidGenerator.Instance.Create();//合并后的文件id
                //Guid.NewGuid().ToString() + Extension;//通过uuid和原始后缀生成新的文件名
                string newFileName = fileName.Split('.')[0] + fileId + Extension;
                //合并后文件存储绝对目录
                string filePath = fileManageStorageAbsoluteDirectory + directorySeparatorChar + AbpSession.UserId;
                //合并文件存储相对目录
                string relativePath = fileManageStorageDirectory + directorySeparatorChar + AbpSession.UserId;
                //合并后的文件绝对地址
                string baseFileName = filePath + directorySeparatorChar + newFileName;
                //合并后文件相对地址
                string relativeFileName = relativePath + directorySeparatorChar + newFileName;
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                var filesList = Directory.GetFiles(Path.GetDirectoryName(path));
                if (filesList.Length != file.Count)
                {
                    return (false, "分片文件存储目录下的文件数量与分片数量不符合并文件失败！", filePath, 
                        baseFileName, relativePath, relativeFileName, newFileName, fileSize,fileId);
                }
                List<FileSort> lstFile = new List<FileSort>();
                foreach (var item in filesList)
                {
                    lstFile.Add(new FileSort()
                    {
                        Name = item,
                        NumBer = Convert.ToInt32(item.Substring(item.IndexOf('^') + 1))
                    });
                }
                //排一下序，保证从0-N Write
                lstFile = lstFile.OrderBy(x => x.NumBer).ToList();
                using (var fileStream = new FileStream(baseFileName, FileMode.Create))
                {
                    await Policy.Handle<IOException>()
                    .RetryForeverAsync()
                    .ExecuteAsync(async () =>
                    {
                        foreach (var fileSort in lstFile)
                        {
                            using (FileStream fileChunk = new FileStream(fileSort.Name, FileMode.Open))
                            {
                                await fileChunk.CopyToAsync(fileStream);
                            }

                        }
                    });
                    fileSize = fileStream.Length;
                }
                //删除分片文件
                foreach (var dirfile in filesList)
                {
                    System.IO.File.Delete(dirfile);
                }
                //删除分片文件存储目录
                if (!Directory.Exists(path))
                    Directory.Delete(path);

                return (true,"合并文件成功！",filePath, baseFileName, relativePath, relativeFileName, newFileName, fileSize, fileId);
            }
            catch (Exception ex)
            {
                throw new AbpException(message: $"大文件分片上传完成，合并文件发生异常：{ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// 删除附件文件
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> DeleteFile(long id)
        {
            try
            {
                await _fileManageService.DeleteFileAsync(id);
                return Json(new ResponseParamViewModel("删除附件文件成功！"));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new ResponseParamViewModel($"删除附件文件失败！{ex.Message}", 500));
                throw new AbpException(message: $"删除附件文件失败：{ex.Message}");
            }
            catch (Exception e)
            {
                return Json(new ResponseParamViewModel("删除附件文件发生异常请联系运营人员！",500));
                throw new AbpException(message: $"删除附件文件发生异常：{e.Message}");
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="id">唯一标识id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFile(long id)
        {
            try
            {
                //Item1.附件文件信息 Item2.文件绝对路径
                var fileInfo = await _fileManageService.GetFileInfoAsync(id);
                if (fileInfo.Item1 == null) throw new UserFriendlyException(L("找不到附件文件信息！"));
                //文件二进制数据
                byte[] bytes = fileInfo.Item1.Bytes;
                if((bytes == null || bytes.Length <= 0) && !System.IO.File.Exists(fileInfo.Item2))
                    throw new UserFriendlyException(L("文件已丢失或已损坏！"));
                if ((bytes == null || bytes.Length <= 0) && System.IO.File.Exists(fileInfo.Item2))
                {
                    bytes = FileHelper.FileToBytes(fileInfo.Item2);
                }
                return File(bytes, fileInfo.Item1.ContentType);
            }
            catch (UserFriendlyException ex)
            {
                return Json(new ResponseParamViewModel($"获取文件信息失败！{ex.Message}", 500));
                throw new AbpException(message: $"获取文件信息失败：{ex.Message}");
            }
            catch (Exception e)
            {
                return Json(new ResponseParamViewModel("获取文件信息发生异常请联系运营人员！", 500));
                throw new AbpException(message: $"获取文件信息发生异常：{e.Message}");
            }
        }

        #endregion

    }
}

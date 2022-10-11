using AutoMapper;
using Dms.API.Services;
using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Entities.Dms_DTO;
using Dms.Domain.Repositories;
using Dms.Domain.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Dms.API.Controllers
{
    [Route("api/Storage/[action]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IMapper _IMapper;
        private readonly IDataBaseRepository _IDataBaseRepository;
        private readonly IGoogleDriveFilesRepository _IGoogleDriveFilesRepository;

        public StorageController(IMapper IMapper, IDataBaseRepository IDataBaseRepository, IGoogleDriveFilesRepository IGoogleDriveFilesRepository)
        {
            _IMapper = IMapper;
            _IDataBaseRepository = IDataBaseRepository;
            _IGoogleDriveFilesRepository = IGoogleDriveFilesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetStorageFiles()
        {
            ResultObjectDTO<List<FileResponseDTO>> result = new ResultObjectDTO<List<FileResponseDTO>>();
            var configuration = await _IDataBaseRepository.GetConfiguration();
          
            if (configuration.is_connect)
            {
                
                    result =_IMapper.Map<ResultObjectDTO<List<FileResponseDTO>>>(await _IDataBaseRepository.GetFilesAll());
            }

            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetStorageFiles(string id)
        {
            ResultObjectDTO<List<FileResponse>> result = new ResultObjectDTO<List<FileResponse>>();
            var configuration = await _IDataBaseRepository.GetConfiguration();

            if (configuration.is_connect)
            {
                if (configuration.cloud_type.Equals(1))
                    result = await _IGoogleDriveFilesRepository.GetDriveFiles(id);
                else

                    result = await _IDataBaseRepository.GetFilesAll();
            }

            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);

        }


        [HttpPost]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<IActionResult> FileUpload([FromForm] FileRequestApiModel fileRequest)
        {
            ResultObjectDTO<string> postResult = new ResultObjectDTO<string>();
            try
            {
                
                if (fileRequest.file!=null)
                {
                    var configuration = await _IDataBaseRepository.GetConfiguration();
                   
                    if (configuration.is_connect)
                    {

                        if (configuration.cloud_type.Equals(1))
                        {
                            var driveResult = await _IGoogleDriveFilesRepository.FileUpload(fileRequest);
                            if (driveResult.Result == ResultType.Success)
                            {
                                
                                fileRequest.cloud_type = configuration.cloud_type;
                                fileRequest.unique_id = Guid.NewGuid().ToString();
                                fileRequest.file_name = Path.GetFileName(fileRequest.file.FileName);
                                fileRequest.file_extension = Path.GetExtension(fileRequest.file.FileName);
                                fileRequest.file_time_stamp = DateTime.Now;
                                fileRequest.file_content_type = fileRequest.file.ContentType;
                                fileRequest.file_id = driveResult.ResultData.file_id;
                                fileRequest.webViewLink = driveResult.ResultData.webViewLink;

                                var dbResult = await _IDataBaseRepository.FileUpload(fileRequest);
                                postResult.Result = dbResult.Result;
                                postResult.ResultMessage = dbResult.ResultMessage;
                                postResult.ResultData = dbResult.ResultData.unique_id;
                            }
                            else
                            {
                                postResult.Result = driveResult.Result;
                                postResult.ResultMessage = driveResult.ResultMessage;
                            }


                        }
                        else
                        {
                            fileRequest.cloud_type = configuration.cloud_type;
                            fileRequest.unique_id = Guid.NewGuid().ToString();
                            fileRequest.file_name = Path.GetFileName(fileRequest.file.FileName);
                            fileRequest.file_extension = Path.GetExtension(fileRequest.file.FileName);
                            fileRequest.file_time_stamp = DateTime.Now;
                            fileRequest.file_content_type = fileRequest.file.ContentType;
                            
                            var bytes = await fileRequest.file.GetBytes();
                            var hexString = Convert.ToBase64String(bytes);
                            fileRequest.file_data = hexString;
                            var dbResult = await _IDataBaseRepository.FileUpload(fileRequest);
                            postResult.Result = dbResult.Result;
                            postResult.ResultMessage = dbResult.ResultMessage;
                            postResult.ResultData = dbResult.ResultData.unique_id;

                        }
                       

                    }
                    else
                    {
                        postResult.Result = ResultType.Error;
                        postResult.ResultMessage = "Please Connect Cloud";
                    }
                }
                else
                {
                    postResult.Result = ResultType.Error;
                    postResult.ResultMessage ="Please Upload File";
                }
            }
            catch (Exception ex)
            {
                postResult.Result = ResultType.Error;
                postResult.ResultMessage=ex.Message;
            }

            return Ok(postResult);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            ResultObjectDTO<string> result = new ResultObjectDTO<string>();
            var configuration = await _IDataBaseRepository.GetConfiguration();
           
            if (configuration.is_connect)
            {
                var file = await _IDataBaseRepository.GetFileById(id);
                if (file.Result == ResultType.Success)
                {
                    if (file.ResultData.cloud_type.Equals(1))
                    {
                        var drivRresult = await _IGoogleDriveFilesRepository.FileDownload(file.ResultData.file_id);
                        if (drivRresult.Result == ResultType.Success)
                        {
                            byte[] bytes = System.IO.File.ReadAllBytes(drivRresult.ResultData);
                            //Send the File to Download.
                            return File(bytes, "application/octet-stream", Path.GetFileName(drivRresult.ResultData));

                        }
                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(file.ResultData.file_data);
                        var memoryStream = new MemoryStream(bytes);

                        return File(memoryStream, $"{file.ResultData.file_content_type}", file.ResultData.file_name);
                    }

                }
                else
                {
                    result.Result = file.Result;
                    result.ResultMessage = file.ResultMessage;
                }



            }
            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> SearchFiles(FileRequestApiModel fileRequest)
        {
            ResultObjectDTO<List<FileResponseDTO>> result = new ResultObjectDTO<List<FileResponseDTO>>();
            result = _IMapper.Map<ResultObjectDTO<List<FileResponseDTO>>>(await _IDataBaseRepository.SearchFiles(fileRequest));
            return Ok(result);

        }

        [HttpGet]
        public async Task<IActionResult> FolderLists()
        {
            ResultObjectDTO<List<FileResponse>> result = new ResultObjectDTO<List<FileResponse>>();
            var configuration = await _IDataBaseRepository.GetConfiguration();

            if (configuration.is_connect)
            {
                if (configuration.cloud_type.Equals(1))
                    result = await _IGoogleDriveFilesRepository.GetDriveFolders();
               
            }
            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> ViewFile(string id)
        {
            ResultObjectDTO<string> result = new ResultObjectDTO<string>();
            var configuration = await _IDataBaseRepository.GetConfiguration();
            
            if (configuration.is_connect)
            {
                var file = await _IDataBaseRepository.GetFileById(id);
                if (file.Result == ResultType.Success)
                {
                 
                    byte[] bytes = Convert.FromBase64String(file.ResultData.file_data);
                    var memoryStream = new MemoryStream(bytes);
                    Response.Headers.Add("Content-Disposition", "inline; filename=" + file.ResultData.file_name);
                    return new FileStreamResult(memoryStream, file.ResultData.file_content_type);
                }
                else
                {
                    result.Result = file.Result;
                    result.ResultMessage = file.ResultMessage;
                }


            }
            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);
        }
        
        
    }

    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}

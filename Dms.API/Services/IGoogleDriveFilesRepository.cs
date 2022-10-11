using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Utilities;
using Google.Apis.Drive.v3;

namespace Dms.API.Services
{
    public interface IGoogleDriveFilesRepository : IDisposable
    {
        Task<DriveService> GetService();

        Task<ResultObjectDTO<List<FileResponse>>> GetDriveFiles(string fileId = null);

        Task<ResultObjectDTO<string>> GenerateRefeshToken(string authorization_code);

        Task<ResultObjectDTO<FileResponse>> FileUpload(FileRequestApiModel fileRequest);

        Task<ResultObjectDTO<string>> FileDownload(string fileId);

        Task<ResultObjectDTO<List<FileResponse>>> SearchFiles(FileRequestApiModel requestApiModel);

        Task<ResultObjectDTO<List<FileResponse>>> GetDriveFolders();

        Task<ResultObjectDTO<FileResponse>> GetFileInfo(string fileId);

        Task<ResultObjectDTO<string>> CreateFolder(FileRequestApiModel fileRequest);

        Task<ResultObjectDTO<string>> DeleteFile(string fileId);

    }
}

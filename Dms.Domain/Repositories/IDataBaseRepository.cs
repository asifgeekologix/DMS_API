using Dms.Domain.ApiModels;
using Dms.Domain.Entities;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Repositories
{
    public interface IDataBaseRepository: IDisposable
    {
       
        Task<UserConfiguration> GetConfiguration();
        Task<ResultObjectDTO<UserConfiguration>> SaveConfiguration(ConfigurationApiModel configurationApiModel);

        Task<ResultObjectDTO<FileResponse>> FileUpload(FileRequestApiModel requestApiModel);

        Task<ResultObjectDTO<List<FileResponse>>> GetFilesAll();

        Task<ResultObjectDTO<FileResponse>> GetFileById(string unique_id);

        Task<ResultObjectDTO<List<FileResponse>>> SearchFiles(FileRequestApiModel requestApiModel);
    }
}

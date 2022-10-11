using Dms.Domain.ApiModels;
using Dms.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Dms.Domain.Utilities;
using Dms.API.Services;
using Dms.Domain.Entities.Dms_DTO;
using AutoMapper;

namespace Dms.API.Controllers
{

    [Route("api/Configuration/[action]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IDataBaseRepository _IDataBaseRepository;
        private readonly IGoogleDriveFilesRepository _IGoogleDriveFilesRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _IMapper;
        public ConfigurationController(IMapper IMapper,IDataBaseRepository IDataBaseRepository,IGoogleDriveFilesRepository IGoogleDriveFilesRepository, IWebHostEnvironment env)
        {
            _IMapper = IMapper;
            _IDataBaseRepository = IDataBaseRepository;
            _IGoogleDriveFilesRepository = IGoogleDriveFilesRepository;
            this._env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfiguration()
        {
            ResultObjectDTO<UserConfigurationDTO> result = new ResultObjectDTO<UserConfigurationDTO>();
            var resultConfig = _IMapper.Map<UserConfigurationDTO>(await _IDataBaseRepository.GetConfiguration());
            
            result.Result = ResultType.Success;
            result.ResultMessage = ResultType.Success.ToString();
            result.ResultData = resultConfig;
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfiguration(ConfigurationApiModel configurationApiModel)
        {
            ResultObjectDTO<UserConfigurationDTO> result = new ResultObjectDTO<UserConfigurationDTO>();
            if (configurationApiModel.cloud_type.Equals(1))
            {
                if (!string.IsNullOrEmpty(configurationApiModel.auth_code))
                {
                    var resultRefreshToken = await _IGoogleDriveFilesRepository.GenerateRefeshToken(configurationApiModel.auth_code);
                    if (resultRefreshToken.Result == ResultType.Success)
                    {
                        configurationApiModel.refresh_token = resultRefreshToken.ResultData;
                        result = _IMapper.Map<ResultObjectDTO<UserConfigurationDTO>>(await _IDataBaseRepository.SaveConfiguration(configurationApiModel));
                       
                    }
                    else
                    {
                        result.Result = resultRefreshToken.Result;
                        result.ResultMessage = resultRefreshToken.ResultMessage;

                    }
                    
                }
                else
                {
                   result = _IMapper.Map < ResultObjectDTO < UserConfigurationDTO >>(await _IDataBaseRepository.SaveConfiguration(configurationApiModel));
                   
                }
                
            }
            else
            {
                result = _IMapper.Map < ResultObjectDTO < UserConfigurationDTO >>(await _IDataBaseRepository.SaveConfiguration(configurationApiModel));
               
            }


           
            return Ok(result);
        }

    }
}

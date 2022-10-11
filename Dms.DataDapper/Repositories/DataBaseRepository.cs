using Dapper;
using Dms.Domain.ApiModels;
using Dms.Domain.DbInfo;
using Dms.Domain.Entities;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Repositories;
using Dms.Domain.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dms.DataDapper.Repositories
{
    public class DataBaseRepository:BaseRepository,IDataBaseRepository
    {
        public DataBaseRepository(DbInfo dbInfo) : base(dbInfo)
        {

        }

        public void Dispose()
        {

        }

        public async Task<UserConfiguration> GetConfiguration()
        {
            UserConfiguration resQuote =new UserConfiguration();
            try
            {
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    resQuote = await conn.QueryFirstOrDefaultAsync<UserConfiguration>("SELECT  * FROM user_configuration limit 1;");
                }

            }
            catch (Exception ex)
            {
                throw;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<UserConfiguration>> SaveConfiguration(ConfigurationApiModel configurationApiModel)
        {
            ResultObjectDTO<UserConfiguration> resQuote = new ResultObjectDTO<UserConfiguration>();
            try
            {
                using (conn=new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    string sql = $"Update user_configuration set cloud_type=@cloud_type,refresh_token=@refresh_token,is_connect=@is_connect";
                    await conn.ExecuteAsync(sql, configurationApiModel);
                    resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<UserConfiguration>("SELECT  * FROM user_configuration limit 1;");
                }

                if (resQuote.ResultData != null)
                {
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Suceess";
                }
                
            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<FileResponse>> FileUpload(FileRequestApiModel requestApiModel)
        {
            ResultObjectDTO<FileResponse> resQuote = new ResultObjectDTO<FileResponse>();
            try
            {
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    string sql = $"Insert Into drive_files (unique_id,cloud_type,file_name,file_time_stamp,file_attribute,file_extension,file_content_type,file_data,file_id,webViewLink) Values (@unique_id,@cloud_type,@file_name,@file_time_stamp,@file_attribute,@file_extension,@file_content_type,@file_data,@file_id,@webViewLink)";
                    await conn.ExecuteAsync(sql, requestApiModel);
                    resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<FileResponse>("SELECT  unique_id,cloud_type,file_name,file_time_stamp,file_attribute,file_extension,file_content_type,file_id,webViewLink FROM drive_files where unique_id=@unique_id;", new { unique_id=requestApiModel.unique_id });
                }

                if (resQuote.ResultData != null)
                {
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Suceess";
                }

            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<List<FileResponse>>> GetFilesAll()
        {
            ResultObjectDTO<List<FileResponse>> resQuote = new ResultObjectDTO<List<FileResponse>>();
            try
            {
                string sql = $"SELECT df.unique_id,df.cloud_type,df.file_name,df.file_time_stamp,df.file_attribute,df.file_extension,df.file_content_type,(CASE WHEN df.cloud_type=1 THEN df.webViewLink ELSE  CONCAT('https://dev-api.niftytrader.in:83/api/Storage/ViewFile/', df.unique_id) END) AS webViewLink,1 AS capabilities_can_download,CONCAT('https://dev-api.niftytrader.in:83/api/Storage/DownloadFile/', df.unique_id) AS file_download_link FROM drive_files df INNER JOIN user_configuration uc ON df.cloud_type=uc.cloud_type  order by df.file_time_stamp desc;";
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    var dbResult = await conn.QueryAsync<FileResponse>(sql);

                    if (dbResult.Count() > 0)
                    {
                        resQuote.Result = ResultType.Success;
                        resQuote.ResultMessage = "Success";
                        resQuote.ResultData = dbResult.ToList();
                    }
                    else
                    {
                        resQuote.Result = ResultType.Error;
                        resQuote.ResultMessage = "No Record Found";
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<FileResponse>> GetFileById(string unique_id)
        {
            ResultObjectDTO<FileResponse> resQuote = new ResultObjectDTO<FileResponse>();
            try
            {
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                     resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<FileResponse>("SELECT * FROM drive_files where unique_id=@unique_id;", new { unique_id });
                }

                if (resQuote.ResultData != null)
                {
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Suceess";
                }
                else
                {
                    resQuote.Result = ResultType.Error;
                    resQuote.ResultMessage = "No Record Found";
                }

            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<List<FileResponse>>> SearchFiles(FileRequestApiModel requestApiModel)
        {
            ResultObjectDTO<List<FileResponse>> resQuote = new ResultObjectDTO<List<FileResponse>>();
            try
            {
                string sql = $"SELECT df.unique_id,df.cloud_type,df.file_name,df.file_time_stamp,df.file_attribute,df.file_extension,df.file_content_type,(CASE WHEN df.cloud_type=1 THEN df.webViewLink ELSE  CONCAT('https://dev-api.niftytrader.in:83/api/Storage/ViewFile/', df.unique_id) END) AS webViewLink,1 AS capabilities_can_download,CONCAT('https://dev-api.niftytrader.in:83/api/Storage/DownloadFile/', df.unique_id) AS file_download_link FROM drive_files df INNER JOIN user_configuration uc ON df.cloud_type=uc.cloud_type WHERE df.file_attribute LIKE '%{requestApiModel.file_attribute}%' OR df.file_name LIKE '%{requestApiModel.file_attribute}%' order by df.file_time_stamp desc;";
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    var dbResult = await conn.QueryAsync<FileResponse>(sql);

                    if (dbResult.Count() > 0)
                    {
                        resQuote.Result = ResultType.Success;
                        resQuote.ResultMessage = "Success";
                        resQuote.ResultData = dbResult.ToList();
                    }
                    else
                    {
                        resQuote.Result = ResultType.Error;
                        resQuote.ResultMessage = "No Record Found";
                    }
                }


            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }
    }
}

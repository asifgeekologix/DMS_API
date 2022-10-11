using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.ApiModels
{
    public class ConfigurationApiModel
    {
         public int cloud_type { get; set; }

        public string auth_code { get; set; }

        public string refresh_token { get; set; }
      
        public bool is_connect { get; set; }



    }
}

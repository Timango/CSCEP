using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    public class AccountKeys
    {
        public AccountKeys(String apiKey, String secretKey)
        {
            ApiKey = apiKey;
            SecretKey = secretKey;
        }

        public String ApiKey { get; private set; }
        public String SecretKey { get; private set; }
    }
}

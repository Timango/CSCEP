using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    public class BittrexExchange : CryptocoinExchange
    {
        #region Constructors
        public BittrexExchange() : base() { ApiUrl = "https://api.kraken.com"; }
        public BittrexExchange(String apiKey, String secretKey) : base(apiKey, secretKey) { ApiUrl = "https://api.kraken.com"; }
        #endregion
    }
}

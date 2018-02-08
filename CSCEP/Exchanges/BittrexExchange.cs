using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    public class BittrexExchange : CryptocoinExchange
    {
        #region Constructors
        public BittrexExchange() : base() { ApiUrl = "https://bittrex.com/api/v1.1"; }
        public BittrexExchange(String apiKey, String secretKey) : base(apiKey, secretKey) { ApiUrl = "https://bittrex.com/api/v1.1"; }
        #endregion

        #region API Calls
        #region Public Api
        public
        #endregion

        #region Market Api
        #endregion

        #region Account Api
        #endregion
        #endregion

        #region Helper methods

        #endregion

        #region Core calls
        public async Task<JObject> CallPublicBittrexAPIAsync<T>
            (string function, Dictionary<String, String> data = null)
        {
            string address = string.Format("/public/{1}", ApiUrl, function);
            if (data != null && data.Count != 0)
            {
                StringBuilder sb = new StringBuilder("?");
                foreach (var kvp in data)
                {
                    sb.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
                }
                sb.Length--; //Strip extra &
                address = String.Format("{0}?{1}", address, sb.ToString());
            }
            
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(address);
            webRequest.Method = "POST";
            var reqStream = await webRequest.GetRequestStreamAsync();

            try
            {
                var fullResult = await CallApiAsync<JObject>(webRequest);
                return fullResult;
            }
            finally { }
        }

        //public async Task<T> CallPrivateBittrexAPIAsync<T>
        //    (string function, AccountKeys keys, Dictionary<String, String> data = null)
        //{
        //    var account = keys ?? DefaultAccountKeys;
        //    if (account == null) throw new ArgumentNullException("accountKeys", "DefaultAccountKeys was not set and no keys were provided.");
            
        //    // generate a 64 bit nonce using a timestamp at tick resolution
        //    var nonce = GetNonce();

        //    string uri = String.Format("{0}/{1}?apikey={2}&nonce={3}", BasicConfigInfo.ApiUrl, function, callOptions.Account.ApiKey, nonce);

        //    //Add arguments
        //    if (arguments != null)
        //    {
        //        foreach (var kvp in arguments)
        //        {
        //            uri += String.Format("&{0}={1}", kvp.Key, kvp.Value);
        //        }
        //    }

        //    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
        //    webRequest.Timeout = callOptions.Timeout;
        //    webRequest.Method = "GET";

        //    byte[] secretKey = DefaultEncoding.GetBytes(callOptions.Account.SecretKey);
        //    var sign = base.HashAsHMACSHA512(secretKey, DefaultEncoding.GetBytes(uri));
        //    webRequest.Headers.Add("apisign", ByteArrayToHexString(sign));

        //    try
        //    {
        //        var fullResult = await CallApiAsync<JObject>(webRequest);
        //        DefaultAPIResultValidation(fullResult, webRequest, uri);
        //        var resultData = fullResult.GetValue("result");
        //        if (resultData.Type != JTokenType.Null)
        //            return (T)(resultData as Object);
        //        else return default(T);
        //    }
        //    catch (WebException we)
        //    {

        //        var resp = we.Response as HttpWebResponse;
        //        if (resp != null && resp.StatusCode == (HttpStatusCode)429)
        //            throw new ExchangeException(this, we.Message, ExchangeExceptionStatus.CallRateLimitExceeded);
        //        else
        //            throw;
        //    }
        //}
        #endregion
    }
}

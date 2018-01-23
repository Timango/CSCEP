using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    [DebuggerDisplay("{Name}")]
    public abstract class CryptocoinExchange
    {
        #region Delegate
        protected Func<Int64> ObtainNonce;
        #endregion


        #region Class Properties
        public CultureInfo ExchangeCulture { get; protected set; }
        public Encoding ExchangeEncoding { get; protected set; }
        public AccountKeys DefaultAccountKeys { get; private set; }
        public String ApiUrl { get; set; }

        #region Derived
        public String Name { get { return this.GetType().Name; }}
        #endregion
        #endregion
        #region Constructors
        public CryptocoinExchange()
        {
            ExchangeEncoding = Encoding.UTF8;
            ExchangeCulture = CultureInfo.InvariantCulture;
            if (ObtainNonce == null)
                ObtainNonce = GenerateDefaultNonce;
        }

        public CryptocoinExchange(String apiKey, String secretKey) : this()
        {
            DefaultAccountKeys = new AccountKeys(apiKey, secretKey);
        }
        #endregion

        #region Helper methods
               
        private long _lastNonce = 0;
        private Object _nonceLock = new Object();
        protected Int64 GenerateDefaultNonce()
        {
            Int64 nonce = DateTime.UtcNow.Ticks;
            lock (_nonceLock)
            {
                if (_lastNonce >= nonce)
                {
                    nonce = _lastNonce + 1;
                }
                _lastNonce = nonce;
                return _lastNonce;
            }
        }

        #region Cryptographic
        protected byte[] HashAsSHA256(byte[] source)
        {
            using (SHA256 hash = SHA256.Create())
            {
                Byte[] result = hash.ComputeHash(source);
                return result;
            }
        }

        protected byte[] HashAsHMACSHA256(byte[] key, byte[] source)
        {
            using (var hmacsha256 = new HMACSHA256(key))
            {
                Byte[] result = hmacsha256.ComputeHash(source);
                return result;
            }
        }

        protected byte[] HashAsHMACSHA384(byte[] key, byte[] source)
        {
            using (var hmacsha384 = new HMACSHA384(key))
            {
                Byte[] result = hmacsha384.ComputeHash(source);
                return result;
            }
        }

        protected byte[] HashAsHMACSHA512(byte[] key, byte[] source)
        {
            using (var hmacsha512 = new HMACSHA512(key))
            {
                Byte[] result = hmacsha512.ComputeHash(source);
                return result;
            }
        }
        
        #endregion
        #endregion


        #region Web

        protected async Task<T> CallApiAsync<T>(HttpWebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException("webRequest");
            var t = await webRequest.GetResponseAsync();
            using (WebResponse webResponse = t)
            {
                var rep = (T)(await WebResponseToJTokenAsync(webResponse) as Object);
                return rep;
            }
        }

        private async Task<JToken> WebResponseToJTokenAsync(WebResponse webResponse)
        {
            Stream str = null;
            try
            {
                str = webResponse.GetResponseStream();
                using (var sr = new StreamReader(str))
                {
                    str = null;
                    var json = await sr.ReadToEndAsync();
                    var obj = JsonConvert.DeserializeObject(json);
                    if (obj as JToken != null)
                        return obj as JToken;
                    else
                        return new JValue(obj);
                }
            }
            finally
            {
                if (str != null)
                    str.Dispose();
            }
        }
        #endregion
    }
}

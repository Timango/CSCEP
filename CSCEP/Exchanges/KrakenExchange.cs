using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Specialized;

namespace Dauros.Timango.CSCEP.Exchanges
{
    public class KrakenExchange : CryptocoinExchange
    {


        #region Constructors
        public KrakenExchange() : base() { ApiUrl = "https://api.kraken.com"; }
        public KrakenExchange(String apiKey, String secretKey) : base(apiKey, secretKey) { ApiUrl = "https://api.kraken.com"; }
        #endregion

        #region API Calls
        #region Public market data
        /// <summary>
        /// Get server time.
        /// </summary>
        /// <returns>Server's time</returns>
        /// <example>{"unixtime":1505992256,"rfc1123":"Thu, 21 Sep 17 11:10:56 +0000"}</example>
        /// <remarks>This is to aid in approximating the skew time between the server and client.</remarks>
        public Task<JObject> API_GetServerTimeAsync()
        {
            return CallPublicKrakenAPIAsync("Time");
        }

        /// <summary>
        /// Get asset info.
        /// </summary>
        /// <param name="info">info to retrieve (optional): info = all info (default)</param>
        /// <param name="assetClass">asset class (optional): currency (default)</param>
        /// <param name="assets">comma delimited list of assets to get info on (optional.  default = all for given asset class)</param>
        /// <returns>Array of asset names and their info.</returns>
        /// <example>{"BCH":{"aclass":"currency","altname":"BCH","decimals":10,"display_decimals":5},"DASH":{"aclass":"currency","altname":"DASH","decimals":10,"display_decimals":5}}</example>
        public Task<JObject> API_GetAssetInfoAsync(IEnumerable<String> assets = null, String info = null, String assetClass = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("info", info);
            data.AddOptional("aclass", assetClass);
            data.AddOptional("asset", assets);
            return CallPublicKrakenAPIAsync("Assets", data);
        }

        /// <summary>
        /// Get tradable asset pairs.
        /// </summary>
        /// <param name="info">
        /// info to retrieve (optional):
        /// info = all info (default)
        /// leverage = leverage info
        /// fees = fees schedule
        /// margin = margin info</param>
        /// <param name="pairs">comma delimited list of asset pairs to get info on (optional.  default = all)</param>
        /// <returns>array of pair names and their info</returns>
        public Task<JObject> API_GetTradableAssetPairsAsync(String info = null, IEnumerable<String> pairs = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("info", info);
            data.AddOptional("pair", pairs);
            return CallPublicKrakenAPIAsync("AssetPairs", data);
        }

        /// <summary>
        /// Get ticker information
        /// </summary>
        /// <param name="pairs">comma delimited list of asset pairs to get info on</param>
        /// <returns>array of pair names and their ticker info</returns>
        /// <remarks>Today's prices start at 00:00:00 UTC</remarks>
        public Task<JObject> API_GetTickerInformationAsync(IEnumerable<String> pairs)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("pair", pairs);
            return CallPublicKrakenAPIAsync("Ticker");
        }

        /// <summary>
        /// Get OHLC data
        /// </summary>
        /// <param name="pair">asset pair to get OHLC data for</param>
        /// <param name="interval">time frame interval in minutes (optional): 1 (default), 5, 15, 30, 60, 240, 1440, 10080, 21600</param>
        /// <param name="since">return committed OHLC data since given id (optional.  exclusive)</param>
        /// <returns>array of pair name and OHLC data</returns>
        /// <remarks>the last entry in the OHLC array is for the current, not-yet-committed frame and will always be present, regardless of the value of "since".</remarks>
        public Task<JObject> API_GetOHLCDataAsync(String pair, int? interval = null, DateTime? since = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("interval", interval, this.ExchangeCulture);
            Double? sinceUnix = since.HasValue ? since.Value.ToUnixTimestamp() : (Double?)null;
            data.AddOptional("since", sinceUnix, this.ExchangeCulture);
            return CallPublicKrakenAPIAsync("OHLC", data);
        }

        /// <summary>
        /// Get order book
        /// </summary>
        /// <param name="pair">asset pair to get market depth for</param>
        /// <param name="count">maximum number of asks/bids (optional): 100 (default)</param>
        /// <returns>array of pair name and market depth</returns>
        public Task<JObject> API_GetOrderbookAsync(String pair, int? count = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("count", count, this.ExchangeCulture);
            return CallPublicKrakenAPIAsync("Depth", data);
        }

        /// <summary>
        /// Get recent trades
        /// </summary>
        /// <param name="pair">asset pair to get trade data for</param>
        /// <param name="since">return trade data since given id (optional.  exclusive)</param>
        /// <returns>array of pair name and recent trade data</returns>
        public Task<JObject> API_GetRecentTradesAsync(String pair, DateTime? since = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("since", since.HasValue ? since.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            return CallPublicKrakenAPIAsync("Trades", data);
        }

        /// <summary>
        /// Get recent spread data
        /// </summary>
        /// <param name="pair">asset pair to get spread data for</param>
        /// <param name="since">return spread data since given id (optional.  inclusive)</param>
        /// <returns>array of pair name and recent spread data</returns>
        /// <remarks>"since" is inclusive so any returned data with the same time as the previous set should overwrite all of the previous set's entries at that time</remarks>
        public Task<JObject> API_GetRecentSpreadDataAsync(String pair, DateTime? since = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("since", since.HasValue ? since.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            return CallPublicKrakenAPIAsync("Spread", data);
        }




        #endregion

        #region Private user data



        /// <summary>
        /// Retrieves account balance for provided account.
        /// </summary>
        /// <param name="keys">The account information. Uses this.DefaultAccountKeys when null.</param>
        /// <returns>array of asset names and balance amount</returns>
        public async Task<JObject> API_GetAccountBalance(AccountKeys keys = null)
        {
            return await CallPrivateKrakenApiAsync("Balance", keys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aclass">asset class (optional): currency (default)</param>
        /// <param name="asset">base asset used to determine balance (default = ZUSD)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>array of trade balance info</returns>
        public async Task<JObject> API_GetTradeBalanceAsync(string aclass = null,
            string asset = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("aclass", aclass);
            data.AddOptional("asset", asset);
            return await CallPrivateKrakenApiAsync("TradeBalance", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trades">whether or not to include trades in output (optional.  default = false)</param>
        /// <param name="userref">restrict results to given user reference id (optional)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>array of order info in open array with txid as the key</returns>
        public async Task<JObject> API_GetOpenOrdersAsync(Boolean? trades = null,
            String userref = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("trades", trades);
            data.AddOptional("userref", userref);
            return await CallPrivateKrakenApiAsync("OpenOrders", keys, data);
        }

        /// <summary>
        /// Returns result set of closed orders matching the provided criteria.
        /// </summary>
        /// <param name="trades">whether or not to include trades in output (optional.  default = false)</param>
        /// <param name="userref">restrict results to given user reference id (optional)</param>
        /// <param name="start">starting unix timestamp or order tx id of results (optional.  exclusive)</param>
        /// <param name="end">ending unix timestamp or order tx id of results (optional.  inclusive)</param>
        /// <param name="ofs">skip the N first rows in a result set before starting to return any rows </param>
        /// <param name="closetime">which time to use (optional)    open    close    both (default)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>array of order info</returns>
        public async Task<JObject> API_GetClosedOrdersAsync(
            Boolean? trades = null,
            String userref = null,
            DateTime? start = null,
            DateTime? end = null,
            Int32? ofs = null, 
            String closetime = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("trades", trades);
            data.AddOptional("userref", userref);
            data.AddOptional("start", start.HasValue ? start.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("end", end.HasValue ? end.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("ofs",ofs);
            data.AddOptional("closetime", closetime);
            return await CallPrivateKrakenApiAsync("ClosedOrders", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid">list of transaction ids to query info about (20 maximum)</param>
        /// <param name="trades">whether or not to include trades in output (optional.  default = false)</param>
        /// <param name="userref">restrict results to given user reference id (optional)</param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<JObject> API_QueryOrdersInfoAsync(
            IEnumerable<String> txid,
            Boolean? trades = null,
            String userref = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("trades", trades);
            data.AddOptional("userref", userref);
            data.AddOptional("txid", txid);
            return await CallPrivateKrakenApiAsync("QueryOrders", keys, data);
        }
        

        #endregion

        #region Private user trading

        #endregion

        #region Core calls

        private async Task<JObject> CallPrivateKrakenApiAsync
            (string function, AccountKeys keys = null, Dictionary<String, String> data = null)
        {
            var account = keys ?? DefaultAccountKeys;
            if (account == null) throw new ArgumentNullException("accountKeys", "DefaultAccountKeys is not set and no keys were provided.");
            Stream reqStream = null;
            string path = string.Format("/0/private/{0}", function);
            string address = ApiUrl + path;
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(address);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.Headers.Add("API-Key", account.ApiKey);
            reqStream = await webRequest.GetRequestStreamAsync();

            var nonce = ObtainNonce.Invoke();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("nonce={0}", nonce);
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    sb.AppendFormat("&{0}={1}", kvp.Key, kvp.Value);
                }
            }

            byte[] base64DecodedSecret = Convert.FromBase64String(account.SecretKey);
            var np = nonce + Convert.ToChar(0) + sb.ToString();
            var pathBytes = ExchangeEncoding.GetBytes(path);
            var hash256Bytes = HashAsSHA256(ExchangeEncoding.GetBytes(np));
            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());

            var signature = HashAsHMACSHA512(base64DecodedSecret, z);
            webRequest.Headers.Add("API-Sign", Convert.ToBase64String(signature));
            if (data != null)
            {
                using (var writer = new StreamWriter(reqStream))
                {
                    writer.Write(sb.ToString());
                }
            }

            var fullResult = await base.CallApiAsync<JObject>(webRequest);
            return fullResult;
        }

        private async Task<JObject> CallPublicKrakenAPIAsync
            (string function, Dictionary<String, String> data = null)
        {
            string address = string.Format("{0}/0/public/{1}", ApiUrl, function);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(address);
            webRequest.Method = "POST";
            var reqStream = await webRequest.GetRequestStreamAsync();

            if (data != null && data.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kvp in data)
                {
                    sb.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
                }
                sb.Length--; //Strip extra &

                using (var writer = new StreamWriter(reqStream))
                {
                    writer.Write(sb.ToString());
                }
            }
            try
            {
                var fullResult = await CallApiAsync<JObject>(webRequest);
                return fullResult;
            }
            finally { }
        }
        #endregion
        #endregion
        #region Helper methods

        #endregion

    }
}

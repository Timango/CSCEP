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
        /// <returns>Server's time (UTC)</returns>
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
        /// <param name="assets">comma delimited sb of assets to get info on (optional.  default = all for given asset class)</param>
        /// <returns>Array of asset names and their info.</returns>
        /// <example>{"BCH":{"aclass":"currency","altname":"BCH","decimals":10,"display_decimals":5},"DASH":{"aclass":"currency","altname":"DASH","decimals":10,"display_decimals":5}}</example>
        public Task<JObject> API_GetAssetInfoAsync(IEnumerable<String> assets = null, String info = null, String assetClass = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("info", info);
            data.AddOptional("aclass", assetClass);
            data.AddOptional("asset", ConcatString(assets));
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
        /// <param name="pairs">comma delimited sb of asset pairs to get info on (optional.  default = all)</param>
        /// <returns>array of pair names and their info</returns>
        public Task<JObject> API_GetTradableAssetPairsAsync(String info = null, IEnumerable<String> pairs = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("info", info);
            data.AddOptional("pair", ConcatString(pairs));
            return CallPublicKrakenAPIAsync("AssetPairs", data);
        }

        /// <summary>
        /// Get ticker information
        /// </summary>
        /// <param name="pairs">comma delimited sb of asset pairs to get info on</param>
        /// <returns>array of pair names and their ticker info</returns>
        /// <remarks>Today's prices start at 00:00:00 UTC</remarks>
        public Task<JObject> API_GetTickerInformationAsync(IEnumerable<String> pairs)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", ConcatString(pairs));
            return CallPublicKrakenAPIAsync("Ticker", data);
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
            data.AddOptional("since", since.HasValue ? since.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
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
        public Task<JObject> API_GetRecentTradesAsync(String pair, Int64? since = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("since", since);
            //data.AddOptional("since", since.HasValue ? since.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            return CallPublicKrakenAPIAsync("Trades", data);
        }

        /// <summary>
        /// Get recent spread data
        /// </summary>
        /// <param name="pair">asset pair to get spread data for</param>
        /// <param name="since">return spread data since given id (optional.  inclusive)</param>
        /// <returns>array of pair name and recent spread data</returns>
        /// <remarks>"since" is inclusive so any returned data with the same time as the previous set should overwrite all of the previous set's entries at that time</remarks>
        public Task<JObject> API_GetRecentSpreadDataAsync(String pair, Int64? since = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.AddOptional("since", since, this.ExchangeCulture);
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
        /// <remarks>Unless otherwise stated, costs, fees, prices, and volumes are in the asset pair's scale, not the currency's scale. For example, if the asset pair uses a lot size that has a scale of 8, the volume will use a scale of 8, even if the currency it represents only has a scale of 2. Similarly, if the asset pair's pricing scale is 5, the scale will remain as 5, even if the underlying currency has a scale of 8.</remarks>
        public async Task<JObject> API_GetOpenOrdersAsync(Boolean? trades = null,
            Int32? userref = null,
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
        /// <remarks>Times given by order tx ids are more accurate than unix timestamps. If an order tx id is given for the time, the order's open time is used</remarks>
        public async Task<JObject> API_GetClosedOrdersAsync(
            Boolean? trades = null,
            Int32? userref = null,
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
            data.AddOptional("ofs", ofs);
            data.AddOptional("closetime", closetime);
            return await CallPrivateKrakenApiAsync("ClosedOrders", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid">sb of transaction ids to query info about (20 maximum)</param>
        /// <param name="trades">whether or not to include trades in output (optional.  default = false)</param>
        /// <param name="userref">restrict results to given user reference id (optional)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array of orders info</returns>
        public async Task<JObject> API_QueryOrdersInfoAsync(
            IEnumerable<String> txid,
            Boolean? trades = null,
            Int32? userref = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("trades", trades);
            data.AddOptional("userref", userref);
            data.AddOptional("txid", ConcatString(txid));
            return await CallPrivateKrakenApiAsync("QueryOrders", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">type of trade (optional)
        ///all = all types (default)
        ///any position = any position (open or closed)
        ///closed position = positions that have been closed
        ///closing position = any trade closing all or part of a position
        ///no position = non-positional trades</param>
        /// <param name="trades">whether or not to include trades related to position in output (optional.  default = false)</param>
        /// <param name="start">starting unix timestamp or trade tx id of results (optional.  exclusive)</param>
        /// <param name="end">ending unix timestamp or trade tx id of results (optional.  inclusive)</param>
        /// <param name="ofs">skip the N first rows in a result set before starting to return any rows </param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>array of trade info</returns>
        /// <remarks>Unless otherwise stated, costs, fees, prices, and volumes are in the asset pair's scale, not the currency's scale.
        ///Times given by trade tx ids are more accurate than unix timestamps.</remarks>
        public async Task<JObject> API_GetTradesHistoryAsync(
            String type = null,
            Boolean? trades = null,
            DateTime? start = null,
            DateTime? end = null,
            Int32? ofs = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("trades", trades);
            data.AddOptional("start", start.HasValue ? start.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("end", end.HasValue ? end.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("ofs", ofs);
            return await CallPrivateKrakenApiAsync("TradesHistory", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid">comma delimited sb of transaction ids to query info about (20 maximum)</param>
        /// <param name="trades">whether or not to include trades related to position in output (optional.  default = false)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array of trades info</returns>
        public async Task<JObject> API_QueryTradesInfoAsync(
            IEnumerable<String> txid,
            Boolean? trades = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("txid", ConcatString(txid));
            data.AddOptional("trades", trades);
            return await CallPrivateKrakenApiAsync("QueryTrades", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid">comma delimited sb of transaction ids to restrict output to</param>
        /// <param name="docals">whether or not to include profit/loss calculations (optional.  default = false)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array of open position info</returns>
        public async Task<JObject> API_GetOpenPositionsAsync(
            IEnumerable<String> txid,
            Boolean? docals = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("txid", ConcatString(txid));
            data.AddOptional("docals", docals);
            return await CallPrivateKrakenApiAsync("OpenPositions", keys, data);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aclass">asset class (optional):    currency (default)</param>
        /// <param name="asset">comma delimited sb of assets to restrict output to (optional.  default = all)</param>
        /// <param name="type">type of ledger to retrieve (optional):
        ///all (default)
        ///deposit
        ///withdrawal
        ///trade
        ///margin</param>
        /// <param name="start">starting unix timestamp or ledger id of results (optional.  exclusive)</param>
        /// <param name="end">ending unix timestamp or ledger id of results (optional.  inclusive)</param>
        /// <param name="ofs">skip the N first rows in a result set before starting to return any rows</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array of ledgers info</returns>
        /// <remarks>Times given by ledger ids are more accurate than unix timestamps.</remarks>
        public async Task<JObject> API_GetLedgersInfoAsync(
            String aclass = null,
            IEnumerable<String> asset = null,
            String type = null,
            DateTime? start = null,
            DateTime? end = null,
            Int32? ofs = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("aclass", aclass);
            data.AddOptional("asset", ConcatString(asset));
            data.AddOptional("type", type);
            data.AddOptional("start", start.HasValue ? start.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("end", end.HasValue ? end.Value.ToUnixTimestamp() : (Double?)null, this.ExchangeCulture);
            data.AddOptional("ofs", ofs);
            return await CallPrivateKrakenApiAsync("Ledgers", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">sb of ledger ids to query info about (20 maximum)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array of ledgers info</returns>
        public async Task<JObject> API_QueryLedgersAsync(
            IEnumerable<String> id,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("id", ConcatString(id));
            return await CallPrivateKrakenApiAsync("QueryLedgers", keys, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair">sb of asset pairs to get fee info on (optional)</param>
        /// <param name="feeinfo">whether or not to include fee info in results (optional)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>associative array</returns>
        /// <remarks>If an asset pair is on a maker/taker fee schedule, the taker side is given in "fees" and maker side in "fees_maker". For pairs not on maker/taker, they will only be given in "fees".</remarks>
        public async Task<JObject> API_GetTradeVolumeAsync(
            IEnumerable<String> pair = null,
            Boolean? feeinfo = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("pair", ConcatString(pair));
            data.AddOptional("fee-info", feeinfo);
            return await CallPrivateKrakenApiAsync("TradeVolume", keys, data);
        }
        #endregion

        #region Private user trading
              
        /// <summary>
        /// Creates an order in the orderbook.
        /// </summary>
        /// <param name="pair">asset pair</param>
        /// <param name="type">type of order (buy/sell)</param>
        /// <param name="ordertype">order type:
        ///market
        ///limit (price = limit price)
        ///stop-loss (price = stop loss price)
        ///take-profit (price = take profit price)
        ///stop-loss-profit (price = stop loss price, price2 = take profit price)
        ///stop-loss-profit-limit (price = stop loss price, price2 = take profit price)
        ///stop-loss-limit (price = stop loss trigger price, price2 = triggered limit price)
        ///take-profit-limit (price = take profit trigger price, price2 = triggered limit price)
        ///trailing-stop (price = trailing stop offset)
        ///trailing-stop-limit (price = trailing stop offset, price2 = triggered limit offset)
        ///stop-loss-and-limit (price = stop loss price, price2 = limit price)
        ///settle-position</param>
        /// <param name="volume">order volume in lots</param>
        /// <param name="price">price (optional.  dependent upon ordertype).</param>
        /// <param name="price2">secondary price (optional.  dependent upon ordertype)</param>
        /// <param name="leverage">amount of leverage desired (optional.  default = none)</param>
        /// <param name="oflags">comma delimited list of order flags (optional):
        ///viqc = volume in quote currency (not available for leveraged orders)
        ///fcib = prefer fee in base currency
        ///fciq = prefer fee in quote currency
        ///nompp = no market price protection
        ///post = post only order (available when ordertype = limit)</param>
        /// <param name="starttm">scheduled start time (optional):
        ///0 = now (default)
        ///+<n> = schedule start time <n> seconds from now
        ///<n> = unix timestamp of start time</param>
        /// <param name="expiretm">expiration time (optional):
        ///0 = no expiration (default)
        ///+<n> = expire <n> seconds from now
        ///<n> = unix timestamp of expiration time</param>
        /// <param name="userref">user reference id.  32-bit signed number. (optional)</param>
        /// <param name="validate">validate inputs only.  do not submit order (optional)</param>
        /// optional closing order to add to system when order gets filled:
        /// <param name="close_ordertype">order type</param>
        /// <param name="close_price">price</param>
        /// <param name="close_price2">secondary price</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns>descr = order description info
        ///order = order description
        ///close = conditional close order description (if conditional close set)
        ///txid = array of transaction ids for order (if order was added successfully)</returns>
        /// <remarks>Prices can be preceded by +, -, or # to signify the price as a relative amount (with the exception of trailing stops, which are always relative). + adds the amount to the current offered price. - subtracts the amount from the current offered price. # will either add or subtract the amount to the current offered price, depending on the type and order type used. Relative prices can be suffixed with a % to signify the relative amount as a percentage of the offered price.
        ///For orders using leverage, 0 can be used for the volume to auto-fill the volume needed to close out your position.
        ///If you receive the error "EOrder:Trading agreement required", refer to your API key management page for further details.</remarks>
        public async Task<JObject> API_AddStandardOrderAsync(
            String pair,
            String type,
            String ordertype,
            Decimal volume,
            String price = null,
            String price2 = null,
            String leverage = null,
            IEnumerable<String> oflags = null,
            String starttm = null,
            String expiretm = null,
            Int32? userref = null,
            Boolean? validate = null,
            String close_ordertype = null,
            String close_price = null,
            String close_price2 = null,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("pair", pair);
            data.Add("type", type);
            data.Add("ordertype", ordertype);
            data.Add("volume", volume.ToString(this.ExchangeCulture));
            data.AddOptional("price", price);
            data.AddOptional("price2", price2);
            data.AddOptional("leverage", leverage);
            data.AddOptional("oflags", this.ConcatString(oflags));
            data.AddOptional("starttm", starttm);
            data.AddOptional("expiretm", expiretm);
            data.AddOptional("usserref", userref, this.ExchangeCulture);
            data.AddOptional("validate", validate);
            data.AddOptional("close[ordertype]", close_ordertype);
            data.AddOptional("close[price]", close_price);
            data.AddOptional("close[price2", close_price2);
            return await CallPrivateKrakenApiAsync("AddOrder", keys, data);
        }

        /// <summary>
        /// Uses decimals for price arguments. Cannot be used for relative prices using % or #.
        /// </summary>
        public Task<JObject> API_AddStandardOrderAsync(
            String pair,
            String type,
            String ordertype,
            Decimal volume,
            Decimal? price = null,
            Decimal? price2 = null,
            String leverage = null,
            IEnumerable<String> oflags = null,
            String starttm = null,
            String expiretm = null,
            Int32? userref = null,
            Boolean? validate = null,
            String close_ordertype = null,
            Decimal? close_price = null,
            Decimal? close_price2 = null,
            AccountKeys keys = null)
        {
            return this.API_AddStandardOrderAsync(pair, type, ordertype, volume,
                price.HasValue ? price.Value.ToString(this.ExchangeCulture) : null,
                price2.HasValue ? price2.Value.ToString(this.ExchangeCulture) : null,
                leverage, oflags, starttm, expiretm, userref, validate, close_ordertype,
                close_price.HasValue ? close_price.Value.ToString(this.ExchangeCulture) : null,
                close_price2.HasValue ? close_price2.Value.ToString(this.ExchangeCulture) : null,
                keys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid">order id</param>
        /// <param name="keys"></param>
        /// <returns></returns>
        /// <remarks> txid may be a user reference id</remarks>
        public async Task<JObject> API_CancelOpenOrder(
            String txid,
            AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("txid", txid);
            return await CallPrivateKrakenApiAsync("CancelOrder", keys, data);
        }

        #endregion

        #region Core calls

        private async Task<JObject> CallPrivateKrakenApiAsync
            (string function, AccountKeys keys, Dictionary<String, String> data = null)
        {
            var account = keys ?? DefaultAccountKeys;
            if (account == null) throw new ArgumentNullException("accountKeys", "DefaultAccountKeys was not set and no keys were provided.");
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
            using (var writer = new StreamWriter(reqStream))
            {
                writer.Write(sb.ToString());
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


        /// <summary>
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private String ConcatString(IEnumerable<String> items, Char divider = ',')
        {
            if (items == null || items.Count() == 0) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.AppendFormat("{0}{1}", item, divider);
            }
            sb.Length--;
            return sb.ToString();
        }

        #endregion

    }
}

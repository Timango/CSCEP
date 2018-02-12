using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        /// <summary>
        /// Used to get the open and available trading markets at Bittrex along with other meta data. 
        /// </summary>
        /// <returns></returns>
        public Task<JObject> API_GetMarketsAsync()
        {
            return CallPublicBittrexAPIAsync("getmarkets");
        }

        /// <summary>
        /// Used to get all supported currencies at Bittrex along with other meta data. 
        /// </summary>
        /// <returns></returns>
        public Task<JObject> API_GetCurrenciesAsync()
        {
            return CallPublicBittrexAPIAsync("getcurrencies");
        }

        /// <summary>
        /// Used to get the current tick values for a market. 
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <returns></returns>
        public Task<JObject> API_GetTickerAsync(String market)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            return CallPublicBittrexAPIAsync("getticker",data);
        }

        /// <summary>
        /// Used to get the last 24 hour summary of all active exchanges 
        /// </summary>
        /// <returns></returns>
        public Task<JObject> API_GetMarketSummariesAsync()
        {
            return CallPublicBittrexAPIAsync("getmarketsummaries");
        }

        /// <summary>
        /// Used to get the last 24 hour summary of all active exchanges 
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <returns></returns>
        public Task<JObject> API_GetMarketSummaryAsync(String market)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            return CallPublicBittrexAPIAsync("getmarketsummary", data);
        }

        /// <summary>
        /// Used to get retrieve the orderbook for a given market 
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <param name="type">'buy', 'sell' or 'both' to identify the type of orderbook to return. </param>
        /// <returns></returns>
        public Task<JObject> API_GetOrderbookAsync(String market, String type)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            data.Add("type", type);
            return CallPublicBittrexAPIAsync("getorderbook", data);
        }

        /// <summary>
        /// Used to retrieve the latest trades that have occured for a specific market.
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <returns>List of trades objects </returns>
        public Task<JObject> API_GetMarketHistoryAsync(String market)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            return CallPublicBittrexAPIAsync("getmarkethistory", data);
        }
        #endregion

        #region Market Api

        /// <summary>
        /// Used to place a buy order in a specific market. Use buylimit to place limit orders. Make sure you have the proper permissions set on your API keys for this call to work 
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <param name="quantity">the amount to purchase</param>
        /// <param name="rate">the rate at which to place the order.</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_BuyLimitAsync
            (String market, Decimal quantity, Decimal rate, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            data.Add("quantity", quantity.ToString(this.ExchangeCulture));
            data.Add("rate", rate.ToString(this.ExchangeCulture));
            return CallPrivateBittrexAPIAsync("market/buylimit", keys, data);
        }

        /// <summary>
        /// Used to place an sell order in a specific market. Use selllimit to place limit orders. Make sure you have the proper permissions set on your API keys for this call to work 
        /// </summary>
        /// <param name="market">a string literal for the market (ex: BTC-LTC)</param>
        /// <param name="quantity">the amount to purchase</param>
        /// <param name="rate">the rate at which to place the order.</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_SellLimitAsync
            (String market, Decimal quantity, Decimal rate, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("market", market);
            data.Add("quantity", quantity.ToString(this.ExchangeCulture));
            data.Add("rate", rate.ToString(this.ExchangeCulture));
            return CallPrivateBittrexAPIAsync("market/selllimit", keys, data);
        }

        /// <summary>
        /// Used to cancel a buy or sell order. 
        /// </summary>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_CancelAsync
            (String uuid, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("uuid", uuid);
            return CallPrivateBittrexAPIAsync("market/cancel", keys, data);
        }

        /// <summary>
        /// Get all orders that you currently have opened. A specific market can be requested 
        /// </summary>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetOpenOrdersAsync
            (String market = null, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("market", market);
            return CallPrivateBittrexAPIAsync("market/getopenorders", keys, data);
        }

        #endregion

        #region Account Api

        /// <summary>
        /// Used to retrieve all balances from your account 
        /// </summary>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetBalancesAsync(AccountKeys keys = null)
        {
            return CallPrivateBittrexAPIAsync("account/getbalances", keys);
        }

        /// <summary>
        /// Used to retrieve the balance from your account for a specific currency. 
        /// </summary>
        /// <param name="currency">a string literal for the currency (ex: LTC)</param>
        /// <returns></returns>
        public Task<JObject> API_GetBalanceAsync(String currency, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("currency", currency);
            return CallPrivateBittrexAPIAsync("account/getbalance", keys, data);
        }

        /// <summary>
        /// Used to retrieve the balance from your account for a specific currency. 
        /// </summary>
        /// <param name="currency">a string literal for the currency (ie. BTC)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetDepositAddressAsync(String currency, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("currency", currency);
            return CallPrivateBittrexAPIAsync("account/getdepositaddress", keys, data);
        }

        /// <summary>
        /// Used to withdraw funds from your account. note: please account for txfee.
        /// </summary>
        /// <param name="currency">a string literal for the currency (ie. BTC)</param>
        /// <param name="quantity">the quantity of coins to withdraw</param>
        /// <param name="address"> the address where to send the funds</param>
        /// <param name="paymentid">used for CryptoNotes/BitShareX/Nxt optional field (memo/paymentid)</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_WithdrawAsync
            (String currency, Decimal quantity, String address, String paymentid = null, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("currency", currency);
            data.Add("quantity", quantity.ToString(this.ExchangeCulture));
            data.Add("address", address);
            data.AddOptional("paymentid", paymentid);
            return CallPrivateBittrexAPIAsync("account/withdraw", keys, data);
        }

        /// <summary>
        /// Used to retrieve a single order by uuid.
        /// </summary>
        /// <param name="uuid">the uuid of the buy or sell order</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetOrderAsync
            (String uuid, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.Add("uuid", uuid);
            return CallPrivateBittrexAPIAsync("account/getorder", keys, data);
        }

        /// <summary>
        /// Used to retrieve your order history. 
        /// </summary>
        /// <param name="market">a string literal for the market (ie. BTC-LTC). If ommited, will return for all markets</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetOrderHistoryAsync
            (String market = null, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("market", market);
            return CallPrivateBittrexAPIAsync("account/getorderhistory", keys, data);
        }

        /// <summary>
        /// Used to retrieve your withdrawal history. 
        /// </summary>
        /// <param name="currency">a string literal for the currecy (ie. BTC). If omitted, will return for all currencies</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetWithdrawalHistoryAsync
            (String currency = null, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("currency", currency);
            return CallPrivateBittrexAPIAsync("account/getwithdrawalhistory", keys, data);
        }

        /// <summary>
        /// Used to retrieve your deposit history. 
        /// </summary>
        /// <param name="currency">a string literal for the currecy (ie. BTC). If omitted, will return for all currencies</param>
        /// <param name="keys">The account information. Uses 'DefaultAccountKeys' when null.</param>
        /// <returns></returns>
        public Task<JObject> API_GetDepositHistoryAsync
            (String currency = null, AccountKeys keys = null)
        {
            var data = new Dictionary<String, String>();
            data.AddOptional("currency", currency);
            return CallPrivateBittrexAPIAsync("account/getdeposithistory", keys, data);
        }
        #endregion

        #region Core calls
        public async Task<JObject> CallPublicBittrexAPIAsync
            (string function, Dictionary<String, String> data = null)
        {
            string address = string.Format("{0}/public/{1}", ApiUrl, function);
            if (data != null && data.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
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

        public async Task<JObject> CallPrivateBittrexAPIAsync
            (string function, AccountKeys keys = null, Dictionary<String, String> data = null)
        {
            var account = keys ?? DefaultAccountKeys;
            if (account == null) throw new ArgumentNullException("accountKeys", "DefaultAccountKeys was not set and no keys were provided.");


            var nonce = ObtainNonce.Invoke();
            var uri = String.Format("{0}/{1}?apikey={2}&nonce={3}", ApiUrl, function, account.ApiKey, nonce);

            StringBuilder sb = new StringBuilder(uri);
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    sb.AppendFormat("&{0}={1}", kvp.Key, kvp.Value);
                }
            }
            uri = sb.ToString();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            webRequest.Method = "GET";

            byte[] secretKey = ExchangeEncoding.GetBytes(account.SecretKey);
            var sign = base.HashAsHMACSHA512(secretKey, ExchangeEncoding.GetBytes(uri));
            webRequest.Headers.Add("apisign", ByteArrayToHexString(sign));

            try
            {
                var fullResult = await CallApiAsync<JObject>(webRequest);
                return fullResult;
            }
            finally
            {

            }
        }
        #endregion
        #endregion

        #region Helper methods

        #endregion

        
    }
}

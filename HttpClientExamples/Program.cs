using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace HttpClientExamples
{

    public class Config
    {
        public string BizaccountAuthenticationToken { get; set; }
        public string BizaccountHost { get; set; }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            //netstat -n | select-string -pattern "178.248.237.68"
            ConnectionLeaks();

            // Настройки
            BizaccountHttpClient(new Config
            {
                BizaccountAuthenticationToken = "token",
                BizaccountHost = "host"
            });

            // 2 as default, 5 to show
            //ConnectionLimits(5);

            // DNS cache
            //DnsCache();
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly MediaTypeWithQualityHeaderValue _contentType = new MediaTypeWithQualityHeaderValue(@"application/json");

        public static void BizaccountHttpClient(Config configuration)
        {
            var config = configuration;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(config.BizaccountAuthenticationToken);
            if (!_httpClient.DefaultRequestHeaders.Accept.Contains(_contentType))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(_contentType);
            }

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(config.BizaccountHost);
            }
        }

        public static void ConnectionLeaks()
        {
            for (var i = 0; i <= 10; i++)
            {
                var container = new CookieContainer();
                var handler = new HttpClientHandler
                {
                    CookieContainer = container,
                    UseDefaultCredentials = true,
                    AutomaticDecompression = DecompressionMethods.GZip
                };

                using (var client = new HttpClient(handler))
                {
                    client.GetStringAsync("https://habr.com").Wait();
                }
            }

            Console.ReadKey();

            // Ключ реестра
            // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\TcpTimedWaitDelay.
        }


        public static void ConnectionLimits(int connectionLimitsCount = 2)
        {

            ServicePointManager.DefaultConnectionLimit = connectionLimitsCount;

            var client = new HttpClient();
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(SendRequest(client, "https://habr.com"));
            }

            Task.WaitAll(tasks.ToArray());
            Console.ReadKey();
        }

        public static void DnsCache()
        {
            // Указывает, сколько времени будет закеширован полученный IP адрес для каждого доменного имени, значение по умолчанию - 120 секунд
            ServicePointManager.DnsRefreshTimeout = int.MinValue;
            var servicePoint = ServicePointManager.FindServicePoint(new Uri("http://your-Url"));

            // Сколько времени соединение может удерживаться открытым. Значение по умолчанию - 60 секунд.
            servicePoint.ConnectionLeaseTimeout = int.MinValue;

            // После какого времени бездействия соединение будет закрыто. Значение по умолчанию 10 секунд.
            servicePoint.MaxIdleTime = int.MinValue;
        }

        private static async Task SendRequest(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            Console.WriteLine($"Received response {response.StatusCode} from {url} in {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
        }
    }
}


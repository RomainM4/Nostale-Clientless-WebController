using Contracts.Responses.Authentication;
using FluentResults;
using Microsoft.Net.Http.Headers;
using NosCore.AuthApi.HttpClients.Interfaces;
using System.Globalization;

namespace NosCore.AuthApi.HttpClients
{
    public class DateTimeClient : IDateTimeClient
    {
        private readonly HttpClient _httpClient;

        public DateTimeClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClient();
        }
        public async Task<Result<string>> GetDateTime()
        {
            var httpResponse = await _httpClient.GetAsync("tnt/final-ms3/clientversioninfo.json");

            if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return Result.Fail<string>(new Error("AuthApi.Login.GameforgeClient", new Error("Http request failled")));

            try
            {
                var DateTimeRaw = httpResponse.Headers.GetValues("Date").FirstOrDefault();

                var DateTimeStr = DateTime.Parse(DateTimeRaw).ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);

                return Result.Ok(DateTimeStr);
            }
            catch {}

            return Result.Fail<string>(new Error("AuthApi.Login.GameforgeClient", new Error("Failled to extract DateTime")));
        }

        private void ConfigureClient()
        {
            _httpClient.BaseAddress = new Uri("https://gameforge.com");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "fr-FR,en,*");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Connection, "keep-alive");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Host, "gameforge.com");
        }

    }
}

using Contracts.Responses.Authentication;
using FluentResults;
using Microsoft.Net.Http.Headers;
using NosCore.AuthApi.Data;
using NosCore.AuthApi.HttpClients.Interfaces;
using System.Text.Json.Nodes;

namespace NosCore.AuthApi.HttpClients
{
    public class VersionClient : IVersionClient
    {

        private readonly HttpClient _httpClient;

        public VersionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClient();
        }

        public async Task<Result<VersionResponse>> GetVersion()
        {
           var httpResponse = await _httpClient.GetAsync("tnt/final-ms3/clientversioninfo.json");

            if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                return Result.Fail<VersionResponse>(new Error("AuthApi.Login.Dl.Tnt.Client", new Error("Http request failled")));

            try
            {
                var ResponseCompressed = await httpResponse.Content.ReadAsByteArrayAsync();

                var ResponseJson = JsonNode.Parse(GZip.DecompressBytes(ResponseCompressed));

                VersionResponse Response = new VersionResponse();

                Response.Version = ResponseJson["version"].ToString();
                Response.MinimumVersionForDelayedUpdate = ResponseJson["minimumVersionForDelayedUpdate"].ToString();

                return Result.Ok(Response);
            }
            catch { }

            return Result.Fail<VersionResponse>(new Error("AuthApi.Login.Dl.Tnt.Client", new Error("Failed to parse Version")));
        }

        private void ConfigureClient()
        {
            _httpClient.BaseAddress = new Uri("http://dl.tnt.gameforge.com");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "fr-FR,en,*");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Connection, "keep-alive");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Host, "dl.tnt.gameforge.com");
        }

    }
}

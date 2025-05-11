using System.Net;
using System.Text;
using Contracts.Requests.Authentication;
using Contracts.Responses.Authentication;
using FluentResults;
using MediatR;
using Microsoft.Net.Http.Headers;
using NosCore.AuthApi.HttpClients.Interfaces;

namespace NosCore.AuthApi.HttpClients
{
    public class ChallengeClient : IChallengeClient
    {
        private readonly HttpClient _httpClient;
        public ChallengeClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClient();
        }

        public async Task<Result<ChallengeResponse>> GetChallenge(ChallengeRequest request)
        {
            var httpResponseMessage = await _httpClient.GetAsync("challenge/" + request.Id + "/" + request.Locale);

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return new ChallengeResponse() { id = "null", lastUpdated = 0, status = "error" };
            }

            var gfChallengePresentation = await httpResponseMessage.Content.ReadFromJsonAsync<ChallengeResponse>();

            if (gfChallengePresentation == null)
                return new ChallengeResponse() { id = "null", lastUpdated = 0, status = "error" };

            return Result.Ok(gfChallengePresentation);
        }

        public async Task<Result<string>> GetTextImage(ChallengeRequest request)
        {

            var httpResponseMessage = await _httpClient.GetAsync("challenge/" + request.Id + "/" + request.Locale + "/text?" + request.LastUpdate.ToString());

            byte[] content = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            return Result.Ok(Convert.ToBase64String(content));
        }

        public async Task<Result<string>> GetIconsImage(ChallengeRequest request)
        {

            var httpResponseMessage = await _httpClient.GetAsync("challenge/" + request.Id + "/" + request.Locale + "/drag-icons?" + request.LastUpdate.ToString());

            byte[] content = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            return Result.Ok(Convert.ToBase64String(content));
        }

        public async Task<Result<string>> GetTargetIconImage(ChallengeRequest request)
        {

            var httpResponseMessage = await _httpClient.GetAsync("challenge/" + request.Id + "/" + request.Locale + "/drop-target?" + request.LastUpdate.ToString());

            byte[] content = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            return Result.Ok(Convert.ToBase64String(content));
        }

        public async Task<Result<ChallengeResponse>> CompleteChallenge(ChallengeRequest request)
        {
    
            var httpResponseMessage = await _httpClient.PostAsync("challenge/" + request.Id + "/" + request.Locale, new StringContent("{\"answer\":" + request.Answer + "}"));

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                ChallengeResponse? Response = await httpResponseMessage.Content.ReadFromJsonAsync<ChallengeResponse>();

                if(Response != null)
                    return Result.Ok(Response);           
            }
            return Result.Fail<ChallengeResponse>(new Error("AuthApi.Challenge.Client", new Error("Http request failed")));
        }

        private void ConfigureClient()
        {
            _httpClient.BaseAddress = new Uri("https://image-drop-challenge.gameforge.com");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "*/*");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "*");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Origin, "spark://www.gameforge.com");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Connection, "keep-alive");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Host, "image-drop-challenge.gameforge.com");
        }

    }
}

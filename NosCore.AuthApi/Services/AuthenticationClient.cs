using Microsoft.Net.Http.Headers;
using NosCore.AuthApi.HttpClients.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts.Responses.Authentication;
using NosCore.AuthApi.Contracts;
using FluentResults;
using Contracts.Requests.Authentication;
using NosCore.AuthApi.Data;
using Microsoft.AspNetCore.Http;

namespace NosCore.AuthApi.HttpClients
{
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly HttpClient _httpClient;

        public AuthenticationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConfigureClientDefault();
        }

        public async Task<Result<LoginResponse>> GetLoginToken(LoginRequest request)
        {
            Activity.Current = null;

            var LoginResponse = new LoginResponse();

            LoginResponse.Locale = request.Locale;

            JsonSerializerOptions Options = new()
            {
                WriteIndented = true
            };

            _httpClient.DefaultRequestHeaders.Add("TNT-Installation-Id", request.InstallationId);

            var HttpResponse = await _httpClient.PostAsync("api/v1/auth/sessions", new StringContent(JsonSerializer.Serialize(request, Options), Encoding.UTF8, "application/json"));

            if (HttpResponse.StatusCode != HttpStatusCode.Created)
            {
                if (HttpResponse.StatusCode == HttpStatusCode.Conflict)
                {

                    LoginResponse.IsChallenge = true;
                    LoginResponse.Token = "null";

                    LoginResponse.ChallengeId = HttpResponse.Headers.GetValues("gf-challenge-id").FirstOrDefault().Split(";").First();

                    return Result.Ok(LoginResponse);
                }
                else
                {
                    return Result.Fail<LoginResponse>(new Error("AuthApi.Login.Client", new Error("Http request failed")));
                }
            }

            var token = await HttpResponse.Content.ReadAsStringAsync();


            LoginResponse.IsChallenge   = false;
            LoginResponse.Token         = token;

            return Result.Ok(LoginResponse);
        }

        public async Task<Result<AccountsResponse>> GetAccounts(AccountsRequest request)
        {
            Activity.Current = null;

            AccountsResponse Response = new AccountsResponse();

            _httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + request.Token);
            _httpClient.DefaultRequestHeaders.Add("TNT-Installation-Id", request.InstallationId);

            var httpResponseMessage = await _httpClient.GetAsync("api/v1/user/accounts");

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return Result.Fail<AccountsResponse>(new Error("AuthApi.Accounts.Client", new Error("Http request failled")));

            var Data = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            var AccountsJson = JsonObject.Parse(GZip.DecompressBytes(Data));

            if (AccountsJson == null) 
                return Result.Fail<AccountsResponse>(new Error("AuthApi.Accounts.Client", new Error("Failed to parse response")));

             

            foreach ( var Account in AccountsJson.AsObject() )
            {
                try
                {
                    var AccountData = AccountsJson[Account.Key].AsObject();

                    var guls = AccountData["guls"].AsObject();

                    if (guls["game"].ToString() != "nostale")
                        continue;

                    Response.GameAccounts.Add(
                        new GameAccount
                        {
                            Id = AccountData["id"].ToString(),
                            DisplayName = AccountData["displayName"].ToString()
                        });
                }
                catch (Exception)
                {
                    return Result.Fail<AccountsResponse>(new Error("AuthApi.Accounts.Client", new Error("Failled to get accounts")));
                }
            }

            if (Response.GameAccounts.Count < 1)
                return Result.Fail<AccountsResponse>(new Error("AuthApi.Accounts.Client", new Error("No accounts found")));

            return Result.Ok(Response);
        }

        public async Task<Result<CodeResponse>> GetNostaleToken(CodeRequestEx request)
        {
            _httpClient.DefaultRequestHeaders.Add("TNT-Installation-Id", request.InstallationId);

            var Status = await SendIovation(request.Token, request.AccountId, request.Blackbox);

            if (!Status)
                return Result.Fail<CodeResponse>(new Error("AuthApi.Code.Client", new Error("IoVation failed")));

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Chrome/" + request.ChromeVersion + " (" + request.Magic + ")");

            JsonObject JsonObj = new JsonObject();

            JsonObj["platformGameAccountId"] = request.AccountId;
            JsonObj["gsid"] = request.Gsid;
            JsonObj["blackbox"] = request.BlackboxEncrypted;
            JsonObj["gameId"] = "dd4e22d6-00d1-44b9-8126-d8b40e0cd7c9";


            var HttpResponse = await _httpClient.PostAsync("api/v1/auth/thin/codes", new StringContent(JsonSerializer.Serialize(JsonObj), Encoding.UTF8, "application/json"));

            var Data = await HttpResponse.Content.ReadAsByteArrayAsync();

            var ResponseJson = JsonObject.Parse(Data);

            var Code = ResponseJson["code"].ToString();

            Code = BitConverter.ToString(Encoding.UTF8.GetBytes(Code)).ToUpper().Replace("-", "");

            return Result.Ok(new CodeResponse { Code = Code});
        }

        private async Task<bool> SendIovation(string gfToken, string accountId, string blackbox)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + gfToken);

            var JsonObject = new JsonObject();

            JsonObject["accountId"] = accountId;
            JsonObject["blackbox"] = blackbox;
            JsonObject["type"] = "play_now";

            var HttpResponse = await _httpClient.PostAsync("api/v1/auth/iovation", new StringContent(JsonSerializer.Serialize(JsonObject), Encoding.UTF8, "application/json"));

            var Data = await HttpResponse.Content.ReadAsByteArrayAsync();

            var ResponseJson = JsonObject.Parse(Data);

            if (ResponseJson["status"].ToString() == "ok")
                return true;

            return false;
        }



        private void ConfigureClientDefault()
        {
            _httpClient.BaseAddress = new Uri("https://spark.gameforge.com");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "fr-FR,en,*");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36");

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Origin, "spark://www.gameforge.com");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Connection, "keep-alive");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Host, "spark.gameforge.com");
        }


    }
}

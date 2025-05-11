using Contracts.Requests.Authentication;
using Contracts.Responses.Authentication;
using FluentResults;

namespace NosCore.AuthApi.HttpClients.Interfaces
{
    public interface IChallengeClient
    {
        Task<Result<ChallengeResponse>> GetChallenge(ChallengeRequest request);

        Task<Result<string>> GetTextImage(ChallengeRequest request);
        Task<Result<string>> GetIconsImage(ChallengeRequest request);
        Task<Result<string>> GetTargetIconImage(ChallengeRequest request);

        Task<Result<ChallengeResponse>> CompleteChallenge(ChallengeRequest request);

    }
}

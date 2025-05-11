using Carter;
using Contracts.Requests.Authentication;
using Contracts.Responses.Authentication;
using FluentResults;
using FluentValidation;
using Mapster;
using MediatR;
using NosCore.AuthApi.Contracts;
using NosCore.AuthApi.Data;
using NosCore.AuthApi.HttpClients.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System;

namespace NosCore.AuthApi.Features.Authentication
{
    public static class Code
    {
        public class Command : IRequest<Result<CodeResponse>>
        {
            public string AccountId { get; set; }
            public string Blackbox { get; set; }
            public string InstallationId { get; set; }
            public string Token { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.AccountId).NotEmpty();
                RuleFor(c => c.Blackbox).NotEmpty();
                RuleFor(c => c.InstallationId).NotEmpty();
                RuleFor(c => c.Token).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<CodeResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly IAuthenticationClient _authClient;
            private readonly IVersionClient _versionClient;
            private readonly IDateTimeClient _dateTimeClient;

            public Handler(IValidator<Command> validator, IAuthenticationClient authClient, IWebHostEnvironment webHostEnvironment, IVersionClient versionClient, IDateTimeClient dateTimeClient)
            {
                _validator = validator;
                _authClient = authClient;
                _webHostEnvironment = webHostEnvironment;
                _versionClient = versionClient;
                _dateTimeClient = dateTimeClient;
            }

            public async Task<Result<CodeResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<CodeResponse>(new Error("AuthApi.Login", new Error("Wrong parameters")));
                }

                var CodeRequest = request.Adapt<CodeRequest>();

                var NostaleCodeRequestEx = new CodeRequestEx
                {
                    AccountId = CodeRequest.AccountId,
                    Blackbox = CodeRequest.Blackbox,
                    InstallationId = CodeRequest.InstallationId,
                    Token = CodeRequest.Token,
                };

                await CodeCryptography.Initialize(_webHostEnvironment, _versionClient);

                Identity Identity = new Identity(CodeRequest.Blackbox, _dateTimeClient);
                Identity IdentityCopy = new Identity(CodeRequest.Blackbox, _dateTimeClient);

                var GuidStr = System.Guid.NewGuid() + "-" + CodeCryptography.Random.Next(1000, 9999).ToString();

                var MagicCode = CodeCryptography.GenerateMagicUserAgent(CodeRequest.AccountId, CodeRequest.InstallationId);


                await Identity.UpdateIdentity(null);

                await IdentityCopy.UpdateIdentity(CodeCryptography.CreateRequest(GuidStr, CodeRequest.InstallationId));


                var Key = GuidStr + "-" + CodeRequest.AccountId;

                using (SHA512 Sha512 = SHA512.Create())
                {
                    Key = BitConverter.ToString(Sha512.ComputeHash(Encoding.UTF8.GetBytes(Key))).ToLower().Replace("-", "");
                }

                var BlackboxEncrypted = Convert.ToBase64String(CodeCryptography.EncryptBlackbox(
                    Encoding.UTF8.GetBytes(Blackbox.EncodeBlackbox(IdentityCopy.GetBlackbox())), Encoding.UTF8.GetBytes(Key)
                    ));

                CodeRequest.Blackbox = Blackbox.EncodeBlackbox(Identity.GetBlackbox());

                NostaleCodeRequestEx.Blackbox           = CodeRequest.Blackbox;
                NostaleCodeRequestEx.BlackboxEncrypted  = BlackboxEncrypted;
                NostaleCodeRequestEx.ChromeVersion      = CodeCryptography.ChromeVersion;
                NostaleCodeRequestEx.Magic              = MagicCode;
                NostaleCodeRequestEx.Gsid               = GuidStr;

                var CodeResult = await _authClient.GetNostaleToken(NostaleCodeRequestEx);

                if(CodeResult.IsSuccess)
                    return Result.Ok(CodeResult.Value);

                return Result.Fail<CodeResponse>(CodeResult.Errors);
            }
        }
    }

    public class CodeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/code", async (CodeRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Code.Command>();

                var Result = await sender.Send(Command);

                if (Result.IsFailed)
                {
                    return Results.BadRequest(Result.Errors);
                }

                return Results.Ok(Result.Value);
            });
        }
    }
}

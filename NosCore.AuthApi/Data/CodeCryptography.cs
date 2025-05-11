using Microsoft.AspNetCore.Hosting;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text.Json.Nodes;
using System.Text;
using System;
using Google.Protobuf.Compiler;
using Contracts.Responses.Authentication;
using System.Security.Cryptography;
using NosCore.AuthApi.HttpClients.Interfaces;
using FluentResults;

namespace NosCore.AuthApi.Data
{
    public static class CodeCryptography
    {

        public static Random            Random = new Random();
        public static VersionResponse   GameforgeVersion { get; set; }
        public static string            Certificate { get; set; }
        public static string            ChromeVersion { get; set; }


        static void InitCertificate(IWebHostEnvironment _webHostEnvironment)
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "static", "new_cert.pem");

            var fileContent = "";

            using (var fileSteam = new FileStream(filePath, FileMode.Open))
            {
                byte[] bytes = new byte[fileSteam.Length];

                int numBytesToRead = (int)fileSteam.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fileSteam.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;


                fileContent = Encoding.UTF8.GetString(bytes, 0, numBytesToRead);
            }

            var begin = fileContent.IndexOf("-----BEGIN CERTIFICATE-----");
            var end = fileContent.IndexOf("-----END CERTIFICATE-----", begin);

            Certificate = fileContent.Substring(begin, end - begin + "-----END CERTIFICATE-----".Length + 1);
        }

        public static async Task<Result> Initialize(IWebHostEnvironment webHostEnvironment, IVersionClient versionClient )
        {
            var VersionResult = await versionClient.GetVersion();

            if (VersionResult.IsFailed)
                return Result.Fail(new Error("AuthApi.Code.Cryptography", new Error("Failed to recover gameforge Version")));

            GameforgeVersion = VersionResult.Value;
            ChromeVersion = "C" + GameforgeVersion.Version;

            InitCertificate(webHostEnvironment);

            return Result.Ok();
        }

        public static string GenerateMagicUserAgent(string accountId, string installationId)
        {

            string AccountFirstLetter = accountId.Substring(0, 2);
            char? InstallIdFirstLetter = null;

            string HashOfCert = null;
            string HashOfVersion = null;
            string HashOfInstallationId = null;
            string HashOfAccountId = null;
            string HashOfSum = null;

            for (int i = 0; i < installationId.Length; i++)
            {
                if (char.IsDigit(installationId[i]))
                {
                    InstallIdFirstLetter = installationId[i];
                    break;
                }

            }

            if (InstallIdFirstLetter == null || char.GetNumericValue((char)InstallIdFirstLetter) % 2 == 0)
            {
                using (SHA256 Sha256 = SHA256.Create())
                {
                    HashOfCert = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(Certificate))).ToLower().Replace("-", "");
                    HashOfInstallationId = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(installationId))).ToLower().Replace("-", "");
                }

                using (SHA1 Sha1 = SHA1.Create())
                {
                    HashOfVersion = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(ChromeVersion))).ToLower().Replace("-", "");
                    HashOfAccountId = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(accountId))).ToLower().Replace("-", "");
                }

                using (SHA256 Sha256 = SHA256.Create())
                {
                    HashOfSum = BitConverter.ToString(Sha256.ComputeHash(
                        Encoding.UTF8.GetBytes(HashOfCert + HashOfVersion + HashOfInstallationId + HashOfAccountId))
                        ).ToLower().Replace("-", "");
                }

                return AccountFirstLetter + HashOfSum.Substring(0, 8);
            }
            else
            {
                using (SHA1 Sha1 = SHA1.Create())
                {
                    HashOfCert = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(Certificate))).ToLower().Replace("-", "");
                    HashOfInstallationId = BitConverter.ToString(Sha1.ComputeHash(Encoding.UTF8.GetBytes(installationId))).ToLower().Replace("-", "");
                }

                using (SHA256 Sha256 = SHA256.Create())
                {
                    HashOfVersion = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(ChromeVersion))).ToLower().Replace("-", "");
                    HashOfAccountId = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(accountId))).ToLower().Replace("-", "");
                }

                using (SHA256 Sha256 = SHA256.Create())
                {
                    HashOfSum = BitConverter.ToString(Sha256.ComputeHash(
                        Encoding.UTF8.GetBytes(HashOfCert + HashOfVersion + HashOfInstallationId + HashOfAccountId))
                        ).ToLower().Replace("-", "");
                }

                return AccountFirstLetter + HashOfSum.Substring(HashOfSum.Length - 8, 8);
            }
        }

        public static JsonObject CreateRequest(string guid, string installationId)
        {
            JsonArray JsonArr = new JsonArray();
            JsonObject JsonObj = new JsonObject();

            JsonArr.Add(Random.Next(1, int.MaxValue));

            JsonObj["features"] = JsonArr;
            JsonObj["installation"] = installationId;
            JsonObj["session"] = guid.Substring(0, guid.LastIndexOf('-'));

            return JsonObj;
        }

        public static byte[] EncryptBlackbox(byte[] blackbox, byte[] key)
        {
            List<Byte> BlackboxEncryptedBuffer = new List<Byte>();

            for (int i = 0; i < blackbox.Length; ++i)
            {
                int key_index = i % key.Length;
                BlackboxEncryptedBuffer.Add((byte)(blackbox[i] ^ key[key_index] ^ key[key.Length - key_index - 1]));
            }

            return BlackboxEncryptedBuffer.ToArray();
        }

    }
}

using NosCore.AuthApi.HttpClients;
using NosCore.AuthApi.HttpClients.Interfaces;
using System.Globalization;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace NosCore.AuthApi.Data
{
    public record Blackbox
    {
        #region Attribute
        public int v { get; set; }
        public string tz { get; set; }
        public bool dnt { get; set; }
        public string product { get; set; }
        public string osType { get; set; }
        public string app { get; set; }
        public string vendor { get; set; }
        public int mem { get; set; }   //ram go
        public int con { get; set; }
        public string lang { get; set; }
        public string plugins { get; set; }
        public string gpu { get; set; }
        public string fonts { get; set; }
        public string audioC { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int depth { get; set; }
        public string video { get; set; }
        public string audio { get; set; }
        public string media { get; set; }
        public string permissions { get; set; }
        public double audioFP { get; set; }
        public string webglFP { get; set; }
        public double canvasFP { get; set; }
        public string creation { get; set; }
        public string uuid { get; set; }
        public int d { get; set; }
        public string osVersion { get; set; }
        public string vector { get; set; }
        public string userAgent { get; set; }
        public string serverTimeInMS { get; set; }
        public JsonObject? request { get; set; }

        #endregion


        static int FIELDS_COUNT = 32;

        public static string Decode(string blackbox)
        {
            List<byte> BlackboxDecodedData = new List<byte>();

            if (!blackbox.Contains("tra:"))
                throw new Exception("Your blackbox must start by \"tra:\"");

            var BlackboxRaw = blackbox.Replace("tra:", "").Replace("_", "/").Replace("-", "+");

            if (BlackboxRaw[BlackboxRaw.Length - 1] != '=')
                BlackboxRaw = BlackboxRaw + "=";

            var BlackboxEncodedData = Convert.FromBase64String(BlackboxRaw);

            BlackboxDecodedData.Add(BlackboxEncodedData[0]);

            for (int i = 1; i < BlackboxEncodedData.Length; ++i)
            {
                byte b = BlackboxEncodedData[i - 1];
                byte a = BlackboxEncodedData[i];
                byte c = (byte)(a - (uint)b);

                BlackboxDecodedData.Add(c);
            }

            string Blackbox = Encoding.UTF8.GetString(
                        HttpUtility.UrlDecodeToBytes(
                            BlackboxDecodedData.ToArray()
                        ));

            return Blackbox;
        }
        public static JsonArray DecodeAsJsonArray(string blackbox)
        {

            JsonNode? BlackboxJson;

            try
            {
                BlackboxJson = JsonNode.Parse(Blackbox.Decode(blackbox));
            }
            catch (Exception)
            {
                throw new JsonException("Invalid JSON");
            }

            if (BlackboxJson == null)
                throw new JsonException("JSON is null");

            if (BlackboxJson.AsArray().Count() != FIELDS_COUNT)
            {
                throw new JsonException($"Wrong fields count. Received : {BlackboxJson.AsArray().Count():i}, Expected : {FIELDS_COUNT}");
            }

            return BlackboxJson.AsArray();

        }
        public static string EncodeBlackbox(Blackbox blackbox)
        {
            List<byte> BlackboxData = new List<byte>();

            string BlackboxEncoded;
            byte[] IdentityEncodedBuffer;

            try
            {
                IdentityEncodedBuffer = Encoding.UTF8.GetBytes(
                    WebUtility.UrlEncode(
                        SerializeBlackboxAsJson(blackbox))
                    .Replace("+", "%20"));
            }
            catch (Exception)
            {

                throw new Exception("Encoding failed");
            }


            BlackboxData.Add((byte)IdentityEncodedBuffer[0]);

            for (int i = 1; i < IdentityEncodedBuffer.Length; ++i)
            {
                byte a = BlackboxData[i - 1];
                byte b = IdentityEncodedBuffer[i];
                byte c = (byte)(a + (uint)b);
                BlackboxData.Add(c);
            }

            try
            {
                BlackboxEncoded = "tra:" + Convert.ToBase64String(BlackboxData.ToArray()).Replace("/", "_").Replace("+", "-").Replace("=", "");
            }
            catch (Exception)
            {

                throw new Exception("Encoding failed");
            }

            return BlackboxEncoded;

        }
        public static string SerializeBlackboxAsJson(Blackbox blackbox)
        {
            JsonArray json = new JsonArray();
            json.Add(blackbox.v);
            json.Add(blackbox.tz);
            json.Add(blackbox.dnt);
            json.Add(blackbox.product);
            json.Add(blackbox.osType);
            json.Add(blackbox.app);
            json.Add(blackbox.vendor);
            json.Add(blackbox.mem);
            json.Add(blackbox.con);
            json.Add(blackbox.lang);
            json.Add(blackbox.plugins);
            json.Add(blackbox.gpu);
            json.Add(blackbox.fonts);
            json.Add(blackbox.audioC);
            json.Add(blackbox.width);
            json.Add(blackbox.height);
            json.Add(blackbox.depth);
            json.Add(blackbox.video);
            json.Add(blackbox.audio);
            json.Add(blackbox.media);
            json.Add(blackbox.permissions);
            json.Add(blackbox.audioFP);
            json.Add(blackbox.webglFP);
            json.Add(blackbox.canvasFP);
            json.Add(blackbox.creation);
            json.Add(blackbox.uuid);
            json.Add(blackbox.d);
            json.Add(blackbox.osVersion);
            json.Add(blackbox.vector);
            json.Add(blackbox.userAgent);
            json.Add(blackbox.serverTimeInMS);
            json.Add(blackbox.request);


            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = false };

            return JsonSerializer.Serialize(json, options);

        }


    }

    public class Identity
    {



        private readonly IDateTimeClient _gameFailClient;

        private readonly Random _random = new Random();
        private Blackbox _blackbox { get; set; } = new Blackbox();

        public Identity(string blackbox, IDateTimeClient gameFailClient)
        {
            _gameFailClient = gameFailClient;

            FillAttributes(Blackbox.DecodeAsJsonArray(blackbox));
        }
        public Blackbox GetBlackbox()
        {
            return _blackbox;
        }
        public async Task UpdateIdentity(JsonObject? requestJson)
        {
            var Result  = await _gameFailClient.GetDateTime();
            string ServerDateTime = "";

            if (Result.IsSuccess)
                ServerDateTime = Result.Value;

            string? VectorUpdate    = Encoding.Latin1.GetString(Convert.FromBase64String(_blackbox.vector));

            VectorUpdate = VectorUpdate.Substring(0, VectorUpdate.LastIndexOf(" "));
            VectorUpdate = VectorUpdate.Substring(1) + GetRandomAscii();

            _blackbox.vector         = Convert.ToBase64String(Encoding.Latin1.GetBytes(VectorUpdate + " " + DateTimeOffset.Now.ToUnixTimeMilliseconds()));
            _blackbox.serverTimeInMS = ServerDateTime;
            _blackbox.creation       = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            _blackbox.d              = _random.Next(10, 300);
            _blackbox.request        = requestJson;
        }  

        private void FillAttributes(JsonArray blackboxJsonArray)
        {
            try
            {
                _blackbox.v = (int)blackboxJsonArray[0];
                _blackbox.tz = (string)blackboxJsonArray[1];
                _blackbox.dnt = (bool)blackboxJsonArray[2];
                _blackbox.product = (string)blackboxJsonArray[3];
                _blackbox.osType = (string)blackboxJsonArray[4];
                _blackbox.app = (string)blackboxJsonArray[5];
                _blackbox.vendor = (string)blackboxJsonArray[6];
                _blackbox.mem = (int)blackboxJsonArray[7];
                _blackbox.con = (int)blackboxJsonArray[8];
                _blackbox.lang = (string)blackboxJsonArray[9];
                _blackbox.plugins = (string)blackboxJsonArray[10];
                _blackbox.gpu = (string)blackboxJsonArray[11];
                _blackbox.fonts = (string)blackboxJsonArray[12];
                _blackbox.audioC = (string)blackboxJsonArray[13];
                _blackbox.width = (int)blackboxJsonArray[14];
                _blackbox.height = (int)blackboxJsonArray[15];
                _blackbox.depth = (int)blackboxJsonArray[16];
                _blackbox.video = (string)blackboxJsonArray[17];
                _blackbox.audio = (string)blackboxJsonArray[18];
                _blackbox.media = (string)blackboxJsonArray[19];
                _blackbox.permissions = (string)blackboxJsonArray[20];
                _blackbox.audioFP = (float)blackboxJsonArray[21];
                _blackbox.webglFP = (string)blackboxJsonArray[22];
                _blackbox.canvasFP = (float)blackboxJsonArray[23];
                _blackbox.creation = (string)blackboxJsonArray[24];
                _blackbox.uuid = (string)blackboxJsonArray[25];
                _blackbox.d = (int)blackboxJsonArray[26];
                _blackbox.osVersion = (string)blackboxJsonArray[27];
                _blackbox.vector = (string)blackboxJsonArray[28];
                _blackbox.userAgent = (string)blackboxJsonArray[29];
                _blackbox.serverTimeInMS = (string)blackboxJsonArray[30];

                if (_blackbox.request != null)
                {
                    _blackbox.request = blackboxJsonArray[31].AsObject();

                }

                _blackbox.request = null;

            }
            catch (Exception)
            {

                throw new InvalidOperationException("One or more fields are corrupted");
            }
        }

        private char GetRandomAscii()
        {
            return (char)_random.Next(32, 126);
        }

    }
}

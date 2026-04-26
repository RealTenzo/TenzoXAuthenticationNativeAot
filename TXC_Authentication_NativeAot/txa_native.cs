using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace TXAAuth
{
    public class TXA
    {
        private const string EmbeddedPublicKeyPem = @"-----BEGIN PUBLIC KEY-----
MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAh3fjJEqt8/GbGNkhn9ws
8v7cStTdgEv2712vsJUhyJXS/hhG6wLcTHCk/hY/+jICvAF7lsSAMmz4Nwntp62B
cPj+OP6eWcX4WSSciK0O+i1qiF0QxXEFchvQCcUa3GVxrDLKFPB5/44ct+INqUV5
dZZYhZl39zQcs+2zvY3kJGvOafopGhsuedMh7eLkPP09lUAXnX30yOyU4G71MXut
mKo1V8M3F4O7G91s6bZLhxONOU6NhgSuykCM2u3hzP34nXC4uJe0Lx/8ENftWNwZ
3Qf3cuXcXCZJsWSzEhfYSZX5waQOUoE5qqqslygoCt40lCP7qk1Z9drP9C9losxy
f1vHTTismKkTnVHSZJRXu1wtYC79J8F3f8oG97uwo3p+p1LA+CdF1X69xSY0nFZu
QF1qxkOV4NUrcOXra+blw8FaowKahBBzjJeAzjoTa02DxexQSk2kDVvPmUrOv68U
L/i6HsvOzaC62R7mNOKiqaDB9bircvGj/BknhX5Etf5RAgMBAAE=
-----END PUBLIC KEY-----";

        private const string TamperMessage = "Tamper detected. Access blocked.";
        private const int AllowedClockSkewSeconds = 120;

        public string AppName { get; private set; }
        public string Secret { get; private set; }
        public string Version { get; private set; }

        private readonly string ApiUrl = "https://tenxoxauthentication.qzz.io";
        private readonly RSA signingKey;
        private readonly SemaphoreSlim initializationLock = new SemaphoreSlim(1, 1);
        private Task? initializationTask;

        public bool IsInitialized { get; private set; } = false;
        public bool IsLoggedIn { get; private set; } = false;
        public UserData? User { get; private set; }
        public string ResponseMessage { get; private set; } = "";

        public string Response => ResponseMessage;
        public string this[string name] => Var(name);

        public Dictionary<string, string> Variables { get; private set; } = new Dictionary<string, string>();
        public bool IsApplicationActive { get; private set; } = false;
        public bool IsVersionCorrect { get; private set; } = false;
        public string ServerVersion { get; private set; } = "";

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public class ApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
            [JsonPropertyName("username")]
            public string Username { get; set; } = string.Empty;
            [JsonPropertyName("subscription")]
            public string Subscription { get; set; } = string.Empty;
            [JsonPropertyName("expiry")]
            public string Expiry { get; set; } = string.Empty;
            [JsonPropertyName("serverVersion")]
            public string ServerVersion { get; set; } = string.Empty;
            [JsonPropertyName("value")]
            public string Value { get; set; } = string.Empty;
            [JsonPropertyName("variables")]
            public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
            [JsonPropertyName("requestNonce")]
            public string RequestNonce { get; set; } = string.Empty;
            [JsonPropertyName("serverTimestamp")]
            public string ServerTimestamp { get; set; } = string.Empty;
            [JsonPropertyName("signature")]
            public string Signature { get; set; } = string.Empty;
        }

        public TXA(string name, string secret, string version)
        {
            AppName = name;
            Secret = secret;
            Version = version;

            signingKey = RSA.Create();
            ImportPublicKey(signingKey, EmbeddedPublicKeyPem);
        }

        public void Init()
        {
            try
            {
                EnsureInitializedAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
                ShowError("Security Alert", TamperMessage);
                Environment.Exit(0);
            }
        }

        public Task InitAsync()
        {
            return EnsureInitializedAsync();
        }

        private void ShowError(string title, string message)
        {
            HideAllWindows();
            AllocConsole();
            IntPtr consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                ShowWindow(consoleHandle, SW_SHOW);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            string border = new string('=', 70);
            Console.WriteLine($"\n[{border}]");
            Console.WriteLine($"[ {title.PadRight(69)} ]");
            Console.WriteLine($"[{border}]");

            foreach (string line in message.Split('\n'))
            {
                Console.WriteLine($"[ {line.PadRight(69)} ]");
            }

            Console.WriteLine($"[{border}]");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            FreeConsole();
        }

        private void HideAllWindows()
        {
            int currentProcessId = Process.GetCurrentProcess().Id;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out int processId);
                if (processId == currentProcessId && IsWindowVisible(hWnd))
                {
                    ShowWindow(hWnd, SW_HIDE);
                }
                return true;
            }, IntPtr.Zero);
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            ResponseMessage = "";
            var loginResult = new LoginResult();

            try
            {
                await EnsureInitializedAsync().ConfigureAwait(false);

                var response = await SendRequest("login", new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["password"] = password,
                    ["secret"] = Secret,
                    ["appName"] = AppName,
                    ["appVersion"] = Version,
                    ["hwid"] = GetHWID()
                });

                if (response.Success)
                {
                    IsLoggedIn = true;
                    User = new UserData
                    {
                        Username = response.Username,
                        Subscription = response.Subscription,
                        Expiry = response.Expiry
                    };

                    await LoadUserVariables();
                    ResponseMessage = $"Login successful! Welcome, {User.Username}";
                    loginResult.Success = true;
                    loginResult.Message = ResponseMessage;
                    loginResult.User = User;
                    return loginResult;
                }

                ResponseMessage = FormatErrorMessage(response.Message, "login");
                loginResult.Success = false;
                loginResult.Message = ResponseMessage;
                return loginResult;
            }
            catch
            {
                ResponseMessage = TamperMessage;
                loginResult.Success = false;
                loginResult.Message = ResponseMessage;
                return loginResult;
            }
        }

        public async Task<RegisterResult> Register(string username, string password, string license)
        {
            ResponseMessage = "";
            var registerResult = new RegisterResult();

            try
            {
                await EnsureInitializedAsync().ConfigureAwait(false);

                var response = await SendRequest("register", new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["password"] = password,
                    ["licenseKey"] = license,
                    ["secret"] = Secret,
                    ["appName"] = AppName,
                    ["appVersion"] = Version,
                    ["hwid"] = GetHWID()
                });

                if (response.Success)
                {
                    ResponseMessage = "Registration successful! You can login now";
                    registerResult.Success = true;
                    registerResult.Message = ResponseMessage;
                    return registerResult;
                }

                ResponseMessage = FormatErrorMessage(response.Message, "register");
                registerResult.Success = false;
                registerResult.Message = ResponseMessage;
                return registerResult;
            }
            catch
            {
                ResponseMessage = TamperMessage;
                registerResult.Success = false;
                registerResult.Message = ResponseMessage;
                return registerResult;
            }
        }

        public string Var(string varName)
        {
            return Variables.TryGetValue(varName, out string value) ? value : "VARIABLE_NOT_FOUND";
        }

        public T Get<T>(string varName)
        {
            string value = Var(varName);
            if (value == "VARIABLE_NOT_FOUND") return default;

            try
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)(value.Equals("true", StringComparison.OrdinalIgnoreCase));
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public async Task<string> GetVariable(string varName)
        {
            ResponseMessage = "";

            if (Variables.ContainsKey(varName))
            {
                ResponseMessage = $"Variable '{varName}' retrieved from cache";
                return Variables[varName];
            }

            try
            {
                await EnsureInitializedAsync().ConfigureAwait(false);

                var response = await SendRequest("getvariable", new Dictionary<string, string>
                {
                    ["secret"] = Secret,
                    ["appName"] = AppName,
                    ["appVersion"] = Version,
                    ["varName"] = varName
                });

                if (response.Success && !string.IsNullOrEmpty(response.Value))
                {
                    Variables[varName] = response.Value;
                    ResponseMessage = $"Variable '{varName}' retrieved successfully";
                    return response.Value;
                }

                ResponseMessage = response.Message == "VARIABLE_NOT_FOUND"
                    ? $"Variable '{varName}' not found"
                    : $"Failed to get variable '{varName}': {response.Message}";
                return null;
            }
            catch
            {
                ResponseMessage = TamperMessage;
                return null;
            }
        }

        public async Task<bool> RefreshVariables()
        {
            ResponseMessage = "";

            try
            {
                await EnsureInitializedAsync().ConfigureAwait(false);
                bool result = await LoadApplicationVariables();
                ResponseMessage = result
                    ? $"Successfully refreshed {Variables.Count} variables"
                    : "No variables found or failed to load";
                return result;
            }
            catch
            {
                ResponseMessage = TamperMessage;
                return false;
            }
        }

        private async Task<bool> CheckIfPaused()
        {
            var response = await SendRequest("isapplicationpaused", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName
            });

            return response.Success && response.Message == "APPLICATION_PAUSED";
        }

        private async Task<(bool isValid, string serverVersion)> CheckVersionWithDetails()
        {
            var response = await SendRequest("versioncheck", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName,
                ["appVersion"] = Version
            });

            if (response.Success && response.Message == "VERSION_OK")
            {
                return (true, Version);
            }

            if (response.Message == "VERSION_MISMATCH")
            {
                return (false, response.ServerVersion ?? "Unknown");
            }

            throw new InvalidOperationException(TamperMessage);
        }

        private async Task<bool> LoadApplicationVariables()
        {
            var response = await SendRequest("getvariables", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName
            });

            if (response.Success && response.Message != "NO_VARIABLES" && response.Variables != null)
            {
                Variables.Clear();
                foreach (var kvp in response.Variables)
                {
                    Variables[kvp.Key] = kvp.Value;
                }
                return Variables.Count > 0;
            }

            return false;
        }

        private async Task LoadUserVariables()
        {
            if (IsLoggedIn && User != null)
            {
                await GetVariable($"user_{User.Username}_settings");
                await GetVariable($"permissions_{User.Subscription}");
            }
        }

        private static string GetHWID()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User?.Value ?? "HWID_FAIL";
            }
            catch
            {
                return "HWID_FAIL";
            }
        }

        private async Task<ApiResponse> SendRequest(string endpoint, Dictionary<string, string> payload)
        {
            string clientNonce = Guid.NewGuid().ToString("N").ToUpperInvariant();
            string clientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            payload["clientNonce"] = clientNonce;
            payload["clientTimestamp"] = clientTimestamp;

            try
            {
                string json = JsonSerializer.Serialize(payload, TxaJsonContext.Default.DictionaryStringString);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                var request = WebRequest.Create($"{ApiUrl}/{endpoint}");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = jsonBytes.Length;
                request.Headers["X-TXA-Nonce"] = clientNonce;
                request.Headers["X-TXA-Timestamp"] = clientTimestamp;

                using (var stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                }

                using (var response = await request.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseString = await reader.ReadToEndAsync();
                    var parsed = ParseJson(responseString);
                    VerifyResponseSignature(endpoint, clientNonce, parsed);
                    return parsed;
                }
            }
            catch (WebException webEx)
            {
                try
                {
                    if (webEx.Response != null)
                    {
                        using (var stream = webEx.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string errorResponse = await reader.ReadToEndAsync();
                            var parsed = ParseJson(errorResponse);
                            VerifyResponseSignature(endpoint, clientNonce, parsed);
                            return parsed;
                        }
                    }
                }
                catch
                {
                }

                throw new InvalidOperationException(TamperMessage);
            }
            catch
            {
                throw new InvalidOperationException(TamperMessage);
            }
        }

        private ApiResponse ParseJson(string json)
        {
            return JsonSerializer.Deserialize(json, TxaJsonContext.Default.ApiResponse) ?? new ApiResponse();
        }

        private async Task EnsureInitializedAsync()
        {
            if (IsInitialized)
            {
                return;
            }

            Task? pendingTask = initializationTask;
            if (pendingTask != null)
            {
                await pendingTask.ConfigureAwait(false);
                return;
            }

            await initializationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsInitialized)
                {
                    return;
                }

                if (initializationTask == null)
                {
                    initializationTask = InitializeCoreAsync();
                }

                pendingTask = initializationTask;
            }
            finally
            {
                initializationLock.Release();
            }

            await pendingTask.ConfigureAwait(false);
        }

        private async Task InitializeCoreAsync()
        {
            if (string.IsNullOrEmpty(AppName) || string.IsNullOrEmpty(Secret) || string.IsNullOrEmpty(Version))
            {
                throw new InvalidOperationException(TamperMessage);
            }

            bool paused = await CheckIfPaused().ConfigureAwait(false);
            if (paused)
            {
                throw new InvalidOperationException("Application is currently paused by administrator");
            }

            IsApplicationActive = true;

            var versionCheck = await CheckVersionWithDetails().ConfigureAwait(false);
            IsVersionCorrect = versionCheck.isValid;
            ServerVersion = versionCheck.serverVersion;

            if (!IsVersionCorrect)
            {
                throw new InvalidOperationException($"Version mismatch! Your version: {Version}, Server version: {ServerVersion}");
            }

            await LoadApplicationVariables().ConfigureAwait(false);
            IsInitialized = true;
            ResponseMessage = "TXA SDK initialized with signed-response verification.";
        }

        private void VerifyResponseSignature(string endpoint, string clientNonce, ApiResponse response)
        {
            if (response == null ||
                string.IsNullOrWhiteSpace(response.RequestNonce) ||
                string.IsNullOrWhiteSpace(response.ServerTimestamp) ||
                string.IsNullOrWhiteSpace(response.Signature))
            {
                throw new InvalidOperationException(TamperMessage);
            }

            if (!string.Equals(response.RequestNonce, clientNonce, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(TamperMessage);
            }

            if (!long.TryParse(response.ServerTimestamp, out long serverTimestamp))
            {
                throw new InvalidOperationException(TamperMessage);
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - serverTimestamp) > AllowedClockSkewSeconds)
            {
                throw new InvalidOperationException(TamperMessage);
            }

            string payload = BuildSignaturePayload(endpoint, response);
            byte[] signatureBytes = Convert.FromBase64String(response.Signature);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            if (!signingKey.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            {
                throw new CryptographicException(TamperMessage);
            }
        }

        private string BuildSignaturePayload(string endpoint, ApiResponse response)
        {
            var variables = response.Variables ?? new Dictionary<string, string>();
            var keys = new List<string>(variables.Keys);
            keys.Sort(StringComparer.Ordinal);

            StringBuilder variableBuilder = new StringBuilder();
            foreach (string key in keys)
            {
                variableBuilder.Append(key);
                variableBuilder.Append('=');
                variableBuilder.Append(NormalizeString(variables[key]));
                variableBuilder.Append('\n');
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("endpoint=").Append(Sha256Hex(endpoint)).Append('\n');
            builder.Append("requestNonce=").Append(Sha256Hex(response.RequestNonce)).Append('\n');
            builder.Append("serverTimestamp=").Append(Sha256Hex(response.ServerTimestamp)).Append('\n');
            builder.Append("success=").Append(response.Success ? "1" : "0").Append('\n');
            builder.Append("message=").Append(Sha256Hex(response.Message)).Append('\n');
            builder.Append("username=").Append(Sha256Hex(response.Username)).Append('\n');
            builder.Append("subscription=").Append(Sha256Hex(response.Subscription)).Append('\n');
            builder.Append("expiry=").Append(Sha256Hex(response.Expiry)).Append('\n');
            builder.Append("serverVersion=").Append(Sha256Hex(response.ServerVersion)).Append('\n');
            builder.Append("value=").Append(Sha256Hex(response.Value)).Append('\n');
            builder.Append("variables=").Append(Sha256Hex(variableBuilder.ToString())).Append('\n');
            return builder.ToString();
        }

        private static string Sha256Hex(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(NormalizeString(value)));
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }

        private static string NormalizeString(string value)
        {
            return value ?? string.Empty;
        }

        private static void ImportPublicKey(RSA rsa, string pem)
        {
            string publicKey = pem
                .Replace("-----BEGIN PUBLIC KEY-----", string.Empty)
                .Replace("-----END PUBLIC KEY-----", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();

            byte[] keyBytes = Convert.FromBase64String(publicKey);
            RSAParameters parameters = DecodeSubjectPublicKeyInfo(keyBytes);
            rsa.ImportParameters(parameters);
        }

        private static RSAParameters DecodeSubjectPublicKeyInfo(byte[] subjectPublicKeyInfo)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(subjectPublicKeyInfo)))
            {
                ReadAsn1Sequence(reader);
                SkipAsn1Element(reader);

                byte bitStringTag = reader.ReadByte();
                if (bitStringTag != 0x03)
                {
                    throw new CryptographicException("Invalid public key format.");
                }

                ReadAsn1Length(reader);
                reader.ReadByte();

                return DecodeRsaPublicKey(reader);
            }
        }

        private static RSAParameters DecodeRsaPublicKey(BinaryReader reader)
        {
            ReadAsn1Sequence(reader);

            byte[] modulus = ReadAsn1Integer(reader);
            byte[] exponent = ReadAsn1Integer(reader);

            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
        }

        private static void ReadAsn1Sequence(BinaryReader reader)
        {
            byte tag = reader.ReadByte();
            if (tag != 0x30)
            {
                throw new CryptographicException("Invalid ASN.1 sequence.");
            }

            ReadAsn1Length(reader);
        }

        private static void SkipAsn1Element(BinaryReader reader)
        {
            reader.ReadByte();
            int length = ReadAsn1Length(reader);
            reader.ReadBytes(length);
        }

        private static byte[] ReadAsn1Integer(BinaryReader reader)
        {
            byte tag = reader.ReadByte();
            if (tag != 0x02)
            {
                throw new CryptographicException("Invalid ASN.1 integer.");
            }

            int length = ReadAsn1Length(reader);
            byte[] value = reader.ReadBytes(length);

            if (value.Length > 1 && value[0] == 0x00)
            {
                byte[] trimmed = new byte[value.Length - 1];
                Buffer.BlockCopy(value, 1, trimmed, 0, trimmed.Length);
                return trimmed;
            }

            return value;
        }

        private static int ReadAsn1Length(BinaryReader reader)
        {
            int length = reader.ReadByte();
            if ((length & 0x80) == 0)
            {
                return length;
            }

            int byteCount = length & 0x7F;
            if (byteCount == 0 || byteCount > 4)
            {
                throw new CryptographicException("Invalid ASN.1 length.");
            }

            int value = 0;
            for (int i = 0; i < byteCount; i++)
            {
                value = (value << 8) | reader.ReadByte();
            }

            return value;
        }

        private string FormatErrorMessage(string errorMessage, string operation)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return $"{operation} failed";

            string upperMsg = errorMessage.ToUpperInvariant();

            if (operation == "login")
            {
                if (upperMsg.Contains("INVALID_CREDENTIALS") || upperMsg.Contains("INVALID USERNAME OR PASSWORD"))
                    return "Invalid username or password";
                if (upperMsg.Contains("HWID_RESET") || upperMsg.Contains("HWID_MISMATCH"))
                    return "HWID mismatch. Please contact support to reset your HWID";
                if (upperMsg.Contains("BANNED") || upperMsg.Contains("SUSPENDED"))
                    return "Account has been banned or suspended";
                if (upperMsg.Contains("EXPIRED"))
                    return "Subscription has expired";
                if (upperMsg.Contains("MAX_DEVICES"))
                    return "Maximum number of devices reached";
            }
            else if (operation == "register")
            {
                if (upperMsg.Contains("INVALID_LICENSE"))
                    return "Invalid license key";
                if (upperMsg.Contains("USERNAME_TAKEN"))
                    return "Username is already taken";
                if (upperMsg.Contains("LICENSE_USED"))
                    return "License key has already been used";
                if (upperMsg.Contains("LICENSE_EXPIRED"))
                    return "License key has expired";
                if (upperMsg.Contains("WEAK_PASSWORD"))
                    return "Password is too weak. Please use a stronger password";
                if (upperMsg.Contains("INVALID_USERNAME"))
                    return "Invalid username format";
            }

            return $"{operation} failed: {errorMessage}";
        }

        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public UserData? User { get; set; }
        }

        public class RegisterResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class UserData
        {
            public string Username { get; set; } = string.Empty;
            public string Subscription { get; set; } = string.Empty;
            public string Expiry { get; set; } = string.Empty;
        }
    }
}

public class TXA : TXAAuth.TXA
{
    public TXA(string name, string secret, string version)
        : base(name, secret, version)
    {
    }
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(TXAAuth.TXA.ApiResponse))]
internal partial class TxaJsonContext : JsonSerializerContext
{
}

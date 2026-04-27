using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TXAAuth
{
    public class TXA
    {
        public string AppName { get; private set; }
        public string Secret { get; private set; }
        public string Version { get; private set; }

        private readonly string ApiUrl = "https://tenxoxauthentication.qzz.io";
        private readonly SemaphoreSlim initializationLock = new SemaphoreSlim(1, 1);
        private Task? initializationTask;

        public bool IsInitialized { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public UserData? User { get; private set; }
        public string ResponseMessage { get; private set; } = "";

        public string Response => ResponseMessage;
        public string this[string name] => Var(name);

        public Dictionary<string, string> Variables { get; private set; } = new Dictionary<string, string>();
        public bool IsApplicationActive { get; private set; }
        public bool IsVersionCorrect { get; private set; }
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
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Subscription { get; set; } = string.Empty;
            public string Expiry { get; set; } = string.Empty;
            public string ServerVersion { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        }

        public TXA(string name, string secret, string version)
        {
            AppName = name;
            Secret = secret;
            Version = version;
        }

        public void Init()
        {
            try
            {
                EnsureInitializedAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ShowError("Initialization Failed", string.IsNullOrWhiteSpace(ex.Message) ? "Initialization failed" : ex.Message);
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
                }).ConfigureAwait(false);

                if (response.Success)
                {
                    IsLoggedIn = true;
                    User = new UserData
                    {
                        Username = response.Username,
                        Subscription = response.Subscription,
                        Expiry = response.Expiry
                    };

                    await LoadUserVariables().ConfigureAwait(false);
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
            catch (Exception ex)
            {
                ResponseMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Login failed" : ex.Message;
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
                }).ConfigureAwait(false);

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
            catch (Exception ex)
            {
                ResponseMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Registration failed" : ex.Message;
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
            if (value == "VARIABLE_NOT_FOUND") return default!;

            try
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)value.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default!;
            }
        }

        public async Task<string?> GetVariable(string varName)
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
                }).ConfigureAwait(false);

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
            catch (Exception ex)
            {
                ResponseMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Variable request failed" : ex.Message;
                return null;
            }
        }

        public async Task<bool> RefreshVariables()
        {
            ResponseMessage = "";

            try
            {
                await EnsureInitializedAsync().ConfigureAwait(false);
                bool result = await LoadApplicationVariables().ConfigureAwait(false);
                ResponseMessage = result
                    ? $"Successfully refreshed {Variables.Count} variables"
                    : "No variables found or failed to load";
                return result;
            }
            catch (Exception ex)
            {
                ResponseMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Variable refresh failed" : ex.Message;
                return false;
            }
        }

        private async Task<bool> CheckIfPaused()
        {
            var response = await SendRequest("isapplicationpaused", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName
            }).ConfigureAwait(false);

            return response.Success && response.Message == "APPLICATION_PAUSED";
        }

        private async Task<(bool isValid, string serverVersion)> CheckVersionWithDetails()
        {
            var response = await SendRequest("versioncheck", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName,
                ["appVersion"] = Version
            }).ConfigureAwait(false);

            if (response.Success && response.Message == "VERSION_OK")
            {
                return (true, Version);
            }

            if (response.Message == "VERSION_MISMATCH")
            {
                return (false, response.ServerVersion ?? "Unknown");
            }

            throw new InvalidOperationException(response.Message ?? "Version check failed");
        }

        private async Task<bool> LoadApplicationVariables()
        {
            var response = await SendRequest("getvariables", new Dictionary<string, string>
            {
                ["secret"] = Secret,
                ["appName"] = AppName
            }).ConfigureAwait(false);

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
                await GetVariable($"user_{User.Username}_settings").ConfigureAwait(false);
                await GetVariable($"permissions_{User.Subscription}").ConfigureAwait(false);
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
            try
            {
                string json = JsonSerializer.Serialize(payload);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                var request = WebRequest.Create($"{ApiUrl}/{endpoint}");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = jsonBytes.Length;

                using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length).ConfigureAwait(false);
                }

                using (var response = await request.GetResponseAsync().ConfigureAwait(false))
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string responseString = await reader.ReadToEndAsync().ConfigureAwait(false);
                    return ParseJson(responseString);
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (var stream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string errorResponse = await reader.ReadToEndAsync().ConfigureAwait(false);
                        return ParseJson(errorResponse);
                    }
                }

                throw new InvalidOperationException("Network error");
            }
        }

        private ApiResponse ParseJson(string json)
        {
            return JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ApiResponse();
        }

        private async Task EnsureInitializedAsync()
        {
            if (IsInitialized) return;

            Task? pendingTask = initializationTask;
            if (pendingTask != null)
            {
                await pendingTask.ConfigureAwait(false);
                return;
            }

            await initializationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsInitialized) return;
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
                throw new InvalidOperationException("App configuration is invalid");
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
            ResponseMessage = "TXA SDK initialized successfully.";
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

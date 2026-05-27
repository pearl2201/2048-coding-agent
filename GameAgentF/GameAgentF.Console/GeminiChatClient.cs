//using Microsoft.Extensions.AI;
//using System.Net.Http.Json;
//using System.Runtime.CompilerServices;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace GameAgentF.Console
//{
//    public class GeminiChatClient : IChatClient
//    {
//        private readonly string _apiKey;
//        private readonly string _model;
//        private readonly HttpClient _httpClient;
//        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
//        private readonly ChatClientMetadata _metadata;

//        // Better Practice: Inject HttpClient to prevent socket exhaustion
//        public GeminiChatClient(HttpClient httpClient, string apiKey, string model = "gemini-2.5-flash")
//        {
//            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
//            _model = model;
//            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
//            _metadata = new ChatClientMetadata("Gemini", new Uri(BaseUrl), _model);
//        }

//        public async Task<ChatResponse> GetResponseAsync(
//            IEnumerable<ChatMessage> chatMessages,
//            ChatOptions? options = null,
//            CancellationToken cancellationToken = default)
//        {
//            var request = BuildRequest(chatMessages);
//            var url = $"{BaseUrl}/{_model}:generateContent?key={_apiKey}";

//            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
//            response.EnsureSuccessStatusCode();

//            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
//            if (result?.Candidates == null || result.Candidates.Count == 0)
//            {
//                throw new InvalidOperationException("No response from Gemini API");
//            }

//            var content = result.Candidates[0].Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
//            return new ChatResponse([new ChatMessage(ChatRole.Assistant, content)])
//            {
//                CreatedAt = DateTimeOffset.UtcNow
//            };
//        }

//        // Fixed: Real asynchronous streaming via server-sent chunks
//        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
//            IEnumerable<ChatMessage> chatMessages,
//            ChatOptions? options = null,
//            [EnumeratorCancellation] CancellationToken cancellationToken = default)
//        {
//            var request = BuildRequest(chatMessages);
//            // Gemini uses 'streamGenerateContent' for real-time tokens
//            var url = $"{BaseUrl}/{_model}:streamGenerateContent?key={_apiKey}";

//            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
//            response.EnsureSuccessStatusCode();

//            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
//            // Gemini's streaming API returns a JSON array over time or line-by-line objects wrapped in elements
//            using var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

//            foreach (var element in jsonDocument.RootElement.EnumerateArray())
//            {
//                var chunkText = element.GetProperty("candidates")[0]
//                                       .GetProperty("content")
//                                       .GetProperty("parts")[0]
//                                       .GetProperty("text").GetString();

//                if (!string.IsNullOrEmpty(chunkText))
//                {
//                    yield return new ChatResponseUpdate
//                    {
//                        Contents = [new TextContent(chunkText)],
//                        Role = ChatRole.Assistant,
//                        CreatedAt = DateTimeOffset.UtcNow
//                    };
//                }
//            }
//        }

//        public object? GetService(Type serviceType, object? serviceKey = null)
//        {
//            return serviceKey is not null ? null :
//                serviceType == typeof(ChatClientMetadata) ? _metadata :
//                serviceType?.IsInstanceOfType(this) is true ? this :
//                null;
//        }

//        public void Dispose()
//        {
//            // Do not dispose _httpClient if it is injected from outside via DI
//        }

//        private static GeminiRequest BuildRequest(IEnumerable<ChatMessage> chatMessages)
//        {
//            var contents = new List<GeminiContent>();
//            GeminiContent? systemInstruction = null;

//            foreach (var message in chatMessages)
//            {
//                if (message.Role == ChatRole.System)
//                {
//                    // Map System Instructions into its own distinct object property
//                    systemInstruction = new GeminiContent
//                    {
//                        Parts = [new GeminiPart { Text = message.Text ?? string.Empty }]
//                    };
//                    continue;
//                }

//                var role = message.Role == ChatRole.User ? "user" : "model";
//                contents.Add(new GeminiContent
//                {
//                    Role = role,
//                    Parts = [new GeminiPart { Text = message.Text ?? string.Empty }]
//                });
//            }

//            return new GeminiRequest
//            {
//                Contents = contents,
//                SystemInstruction = systemInstruction
//            };
//        }

//        // --- Streamlined JSON DTOs ---
//        private class GeminiRequest
//        {
//            [JsonPropertyName("contents")]
//            public List<GeminiContent> Contents { get; set; } = [];

//            [JsonPropertyName("systemInstruction")]
//            public GeminiContent? SystemInstruction { get; set; }
//        }

//        private class GeminiContent
//        {
//            [JsonPropertyName("role")]
//            public string? Role { get; set; }

//            [JsonPropertyName("parts")]
//            public List<GeminiPart> Parts { get; set; } = [];
//        }

//        private class GeminiPart
//        {
//            [JsonPropertyName("text")]
//            public string Text { get; set; } = string.Empty;
//        }

//        private class GeminiResponse
//        {
//            [JsonPropertyName("candidates")]
//            public List<GeminiCandidate> Candidates { get; set; } = [];
//        }

//        private class GeminiCandidate
//        {
//            [JsonPropertyName("content")]
//            public GeminiContent? Content { get; set; }
//        }
//    }
//}
#r "Newtonsoft.Json.dll"
#load "Log.csx"
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


public class OpenAISummarizer
{
    private readonly string apiKey;

    public OpenAISummarizer(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> SummarizeText(string text)
    {
        var endpoint = "https://api.openai.com/v1/completions";
        var prompt = "次の文章から単語をカンマ区切りでリストアップしてください。重複したものは破棄してください。特徴的なものだけに絞ってください。\n" + text + "\n";
        var requestBody = new
        {
            model = "text-davinci-003",
            prompt = prompt,
            max_tokens = 200,
            n = 1,
            temperature = 0.9
        };

        var requestJson = JsonConvert.SerializeObject(requestBody);
            
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
        {
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = content;
            var client = new HttpClient();
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var res = await reader.ReadToEndAsync();
                var chunk = JsonConvert.DeserializeObject<CompletionsGPTResponse>(res);
                Console.WriteLine(chunk);
                var summary = chunk.choices[0].text;
                return summary;
            }
        }
    }
}

public class CompletionsGPTRequest
{
    public string model;
    public string prompt;
    public int max_tokens;
    public int top_p;
    public int n;
    public int logprobs;
    public float temperature;
    public bool stream;
    public string stop;
}
public class CompletionsGPTResponse
{
    public string id;
    public string @object;
    public int created;
    public string model;
    public CompletionsGPTUsage usage;
    public CompletionsGPTChoice[] choices;
}
public class CompletionsGPTUsage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}
public class CompletionsGPTChoice
{
    public string text;
    public string finish_reason;
    public int index;
}
public class ChatGPTTalk
{
    public string Response { get; private set; } = string.Empty;
    public bool IsProcessing { get; private set; }

    public ChatGPTTalk(string apiKey, ChatGPTRequest chatGPTRequest)
    {
        _ = Process(apiKey, chatGPTRequest);
    }

    async Task Process(string apiKey, ChatGPTRequest chatGPTRequest)
    {
        IsProcessing = true;
        try
        {
            if (!chatGPTRequest.stream)
                throw new InvalidOperationException("stream should be true");

            var result = "";
            var endpoint = "https://api.openai.com/v1/chat/completions";

            var requestBody = new
            {
                model = chatGPTRequest.model,
                messages = chatGPTRequest.messages
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            var content_str = await content.ReadAsStringAsync();
            Log.WriteAllText(Log.Prompt, "BeginTalk: " + content_str);
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;
                var client = new HttpClient();
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    var res = await reader.ReadToEndAsync();
                    var chunk = JsonConvert.DeserializeObject<ChatGPTResponse>(res);
                    var delta = chunk.choices[0].message.content;
                    if (!string.IsNullOrEmpty(delta))
                    {
                        result += delta;
                        Response = result;
                    }
                    //Log.WriteAllText(Log.Response, "Response: " + Response);
                }
            }
        }
        catch (Exception e)
        {
            Response = e.ToString();
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
public class ChatGPTRequest
{
    public string model;
    public ChatGPTMessage[] messages;
    public bool stream;
}
public class ChatGPTMessage
{
    public string role;
    public string content;
}
public class ChatGPTResponse
{
    public string id;
    public string @object;
    public int created;
    public string model;
    public ChatGPTUsage usage;
    public ChatGPTChoice[] choices;
}
public class ChatGPTUsage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}
public class ChatGPTChoice
{
    public ChatGPTMessage message;
    public string finish_reason;
    public int index;
}
public class ChatGPTStreamChunk
{
    public string id;
    public string @object;
    public int created;
    public string model;
    public ChatGPTChoiceDelta[] choices;
    public int index;
    public string finish_reason;
}
public class ChatGPTChoiceDelta
{
    public ChatGPTMessage delta;
    public string finish_reason;
    public int index;
}
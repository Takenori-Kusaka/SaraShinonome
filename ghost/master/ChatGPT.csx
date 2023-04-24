#r "Newtonsoft.Json.dll"
#r "Microsoft.Extensions.Http.dll"
#r "Microsoft.Bcl.AsyncInterfaces.dll"
#r "OpenAI_API.dll"
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
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
public class ChatGPTRequest
{
    private readonly OpenAI_API.APIAuthentication apiKey;
    private OpenAI_API.OpenAIAPI api;
    public string Response { get; private set; } = string.Empty;
    public bool IsProcessing { get; private set; }

    private string ChatMessagesToString(ChatMessage[] messages)
    {
        string result = $@"System: {messages[0].Content}
User: {messages[1].Content}";
        return result;
    }
    public ChatGPTRequest(string apiKey, ChatMessage[] messages)
    {
        this.apiKey = new OpenAI_API.APIAuthentication(apiKey);
        this.api = new OpenAI_API.OpenAIAPI(this.apiKey);
        _ = ChatGPTRequestMessage(messages);
    }

    async Task ChatGPTRequestMessage(ChatMessage[] messages)
    {
        IsProcessing = true;
        try{
            Log.WriteAllText(Log.Prompt, "BeginTalk: " + ChatMessagesToString(messages));
            OpenAI_API.Chat.ChatResult results = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,
                Messages = messages
            });
            OpenAI_API.Chat.ChatChoice choice = results.Choices[0];
            Response = choice.Message.Content;
        }
        catch (Exception e)
        {
            Response = e.ToString();
        }
        finally
        {
            IsProcessing = false;
        }
//
    }

}
public class CompletionsGPTRequest
{
    private readonly OpenAI_API.APIAuthentication apiKey;
    private OpenAI_API.OpenAIAPI api;
    public string Response { get; private set; } = string.Empty;
    public bool IsProcessing { get; private set; }

    public CompletionsGPTRequest(string apiKey, string text)
    {
        this.apiKey = new OpenAI_API.APIAuthentication(apiKey);
        this.api = new OpenAI_API.OpenAIAPI(this.apiKey);
        _ = ChatGPTRequestSummary(text);
    }

    async Task ChatGPTRequestSummary(string text)
    {
        IsProcessing = true;
        try{
            Log.WriteAllText(Log.Prompt, "BeginSummary: " + text);
            OpenAI_API.Completions.CompletionResult results = await api.Completions.CreateCompletionsAsync(new OpenAI_API.Completions.CompletionRequest()
            {
                Model = Model.DavinciText, 
                Temperature = 0.7,
                Prompt = text
            });
            OpenAI_API.Completions.Choice completion = results.Completions[0];
            Response = completion.Text;
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
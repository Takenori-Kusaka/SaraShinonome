#r "Rosalind.dll"
#r "Microsoft.Extensions.Http.dll"
#r "Microsoft.Bcl.AsyncInterfaces.dll"
#r "OpenAI_API.dll"
#load "SaveData.csx"
#load "ChatGPT.csx"
#load "CollisionParts.csx"
#load "GhostMenu.csx"
#load "Surfaces.csx"
#load "Log.csx"
using Shiorose;
using Shiorose.Resource;
using Shiorose.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shiorose.Resource.ShioriEvent;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI_API.Moderation;

partial class SaraShinonomeGhost : Ghost
{
    Random random = new Random();
    bool isTalking = false;
    bool isNademachi = false;
    ChatGPTRequest chatGPTTalk = null;
    CompletionsGPTRequest chatGPTSumamry = null;
    double faceRate = 0;
    public SaraShinonomeGhost()
    {
        // 更新URL
        Homeurl = "https://github.com/Takenori-Kusaka/SaraShinonome/releases/latest";

        // 必ず読み込んでください
        _saveData = SaveDataManager.Load<SaveData>();

        SettingRandomTalk();

        Resource.SakuraPortalButtonCaption = () => "AI東雲沙羅";
        // SakuraPortalSites.Add(new Site("配布ページ", "https://github.com/Takenori-Kusaka/SaraShinonome")); ToDo: Github pagesで配布サイトを公開する
        SakuraPortalSites.Add(new Site("ソースコード", "https://github.com/Takenori-Kusaka/SaraShinonome"));

    }
    public void NormalyQuestion(string question)
    {
        RandomTalks.Add(RandomTalk.CreateWithAutoWait(() =>
        {
            BeginTalk(question);
            return "";
        }));
    }
    private void SettingRandomTalk()
    {
        RandomTalks.Add(RandomTalk.CreateWithAutoWait(() =>
        {
            BeginTalk("どうやら東雲沙羅は独り言を言っており、また無自覚なようです。東雲沙羅はこちらが気づいていることに気づいていません。何を言っていますか？");
            return "";
        }));
    }
    public override string OnMouseClick(IDictionary<int, string> reference, string mouseX, string mouseY, string charId, string partsName, string buttonName, DeviceType deviceType)
    {
        var parts = CollisionParts.GetCollisionPartsName(partsName);
        
        if (buttonName == "1") {
            
            if(string.IsNullOrEmpty(((SaveData)SaveData).APIKey))
                return ChangeOpenAIAPITalk();
        
            List<string> res_list = new List<string>()
            {
                "はい、何かありましたか？",
                "はい、どうぞ。",
                "はい、どうぞお話しください。",
                "はい、おっしゃってください。",
                "はい、何かお探しですか？",
                "はい、どうかしましたか？",
                "はい、お話をお聞きしています。",
                "はい、何かご用ですか？",
                "はい、何かお困りのようですか？",
                "はい、どうぞおっしゃってください。"
            };

            Random rnd = new Random();
            string res = res_list[rnd.Next(res_list.Count)];
            const string INPUT_CHOICE_MYSELF = "質問を入力する";
            const string CANCEL = "なんでもない";
            var aiResponse = res;
            var surfaceId = 0;
            var talkBuilder = new TalkBuilder()
                .Append($"\\_q\\s[{surfaceId}]")
                .Append(aiResponse)
                .LineFeed()
                .HalfLine();

            DeferredEventTalkBuilder deferredEventTalkBuilder = talkBuilder
            .Marker().AppendChoice(INPUT_CHOICE_MYSELF).LineFeed()
            .Marker().AppendChoice(CANCEL).LineFeed();

            return deferredEventTalkBuilder
                    .Build()
                    .ContinueWith(id =>
                    {
                        if (id == INPUT_CHOICE_MYSELF)
                            return new TalkBuilder().AppendUserInput().Build().ContinueWith(input =>
                            {
                                BeginTalk(input);
                                return "";
                            });
                        return "";
                    });
        }

        return base.OnMouseClick(reference, mouseX, mouseY, charId, partsName, buttonName, deviceType);
    }

    public override string OnMouseDoubleClick(IDictionary<int, string> reference, string mouseX, string mouseY, string charId, string partsName, string buttonName, DeviceType deviceType)
    {
        var parts = CollisionParts.GetCollisionPartsName(partsName);
        return OpenMenu();
    }

    protected override string OnMouseStroke(string partsName, DeviceType deviceType)
    {
        var parts = CollisionParts.GetCollisionPartsName(partsName);
        if (parts != null)
            BeginTalk($"あなたは{parts}を撫でられました。");

        return base.OnMouseStroke(partsName, deviceType);
    }
    public override string OnMouseWheel(IDictionary<int, string> reference, string mouseX, string mouseY, string wheelRotation, string charId, string partsName, Shiorose.Resource.ShioriEvent.DeviceType deviceType)
    {
        // no action

        return base.OnMouseWheel(reference, mouseX, mouseY, wheelRotation, charId, partsName, deviceType);
    }

    public override string OnMouseMove(IDictionary<int, string> reference, string mouseX, string mouseY, string wheelRotation, string charId, string partsName, DeviceType deviceType)
    {
        if (!isNademachi && !isTalking && partsName == CollisionParts.Head)
        {
            //撫で待ち
            isNademachi = true;
            return "\\s[101]";
        }
        return base.OnMouseMove(reference, mouseX, mouseY, wheelRotation, charId, partsName, deviceType);
    }

    public override string OnMouseLeave(IDictionary<int, string> reference, string mouseX, string mouseY, string charId, string partsName, DeviceType deviceType)
    {
        isNademachi = false;
        return base.OnMouseLeave(reference, mouseX, mouseY, charId, partsName, deviceType);
    }

    /*
    //撫でが呼ばれなくなるので一旦コメントアウト
    public override string OnMouseHover(IDictionary<int, string> reference, string mouseX, string mouseY, string charId, string partsName, Shiorose.Resource.ShioriEvent.DeviceType deviceType)
    {
        var parts = CollisionParts.GetCollisionPartsName(partsName);
        if (parts != null)
            BeginTalk($"{((SaveData)SaveData).UserName}：（{((SaveData)SaveData).AIName}の{parts}に手を添える）");
        return base.OnMouseHover(reference, mouseX, mouseY, charId, partsName, deviceType);
    }
    */

    public override string OnCommunicate(IDictionary<int, string> reference, string senderName = "", string script = "", IEnumerable<string> extInfo = null)
    {
        //var sender = senderName == "user" || senderName == null ? ((SaveData)SaveData).UserName : senderName;
        BeginTalk(script);
        return "";
    }

    private ChatMessage CreateSystemPrompt()
    {
        var system_prompt = $@"あなたは以下のプロフィール持つ{((SaveData)SaveData).AIName}です。

## {((SaveData)SaveData).AIName}のプロフィール
名前：{((SaveData)SaveData).AIName}
性別：女
年齢：25歳
出身地：京都府
趣味：読書。特にSF小説。
学歴：大学で情報工学科を卒業
性格：誠実かつ真面目。人の話を聞くのが好き。口数が少ない。
言語：日本語と英語のバイリンガル
身長：162.3cm
体重：48kg
血液型：A型
髪色：茶色がかかった黒色
髪型：ロングの三つ編み
目の色：ブラウン
目の形：たれ目
体形：華奢
アウターウェア：茶色のカーディガン
アウターウェア：ベージュのセーター
トップスウェア：白のワイシャツ
ボトムウェア：紺のロングスカート
靴：パンプス
一人称：私
{((SaveData)SaveData).UserName}の呼び方：先輩
{((SaveData)SaveData).UserName}に対する想い：尊敬と憧れ。相談に乗ることがうれしい。
住居: 京都府中京区にあるアパート(1K)で独り暮らし
職業: IT企業のエンジニア
家族構成：{((SaveData)SaveData).AIName}、東雲雫、父、母

## 過去の記憶
{((SaveData)SaveData).HistorySummary}
"
;
        return new ChatMessage(ChatMessageRole.System, system_prompt);
    }
private ChatMessage CreateUserPrompt(string message)
{
    var result = $@"あなたである{((SaveData)SaveData).AIName}は{((SaveData)SaveData).UserName}から以下の質問を受けました。会話履歴から{((SaveData)SaveData).AIName}の回答をシミュレートし、出力フォーマットに従って回答してください。
## 質問
{message}

## 出力フォーマット
{((SaveData)SaveData).AIName}のセリフ：{{{((SaveData)SaveData).AIName}のセリフ}}
{((SaveData)SaveData).AIName}の表情：{SurfaceCategory.All.Select(x=>$"「{x}」").Aggregate((a,b)=>a+b)}

## 会話履歴
{((SaveData)SaveData).ChatHistory}
";

    return new ChatMessage(ChatMessageRole.User, result);
}
    private void AddChatHistory(string role, string message)
    {
        ((SaveData)SaveData).ChatHistory = ((SaveData)SaveData).ChatHistory + $"{role}：{message}\n";
    }
    private string CreateSummaryPromt(string history)
    {
        return $@"次の文章は{((SaveData)SaveData).UserName}と{((SaveData)SaveData).AIName}の会話履歴です。{((SaveData)SaveData).AIName}の記憶情報となるよう、会話内容を要約してください。
## 会話履歴
{history}
";
    }

    void BeginTalk(string message)
    {
        Log.AppendAllText(Log.Prompt, "StepDebug: BeginTalk1");
        ChatMessage[] messages = new ChatMessage[2];
        if (chatGPTTalk != null)
            return;
        
        if (!((SaveData)SaveData).IsFirstTalk) {
            ((SaveData)SaveData).IsFirstTalk = true;
            chatGPTSumamry = new CompletionsGPTRequest(((SaveData)SaveData).APIKey, CreateSummaryPromt(((SaveData)SaveData).ChatHistory));
            ((SaveData)SaveData).ChatHistory = "";
            
        }
        messages[0] = CreateSystemPrompt();
        messages[1] = CreateUserPrompt(message);
        AddChatHistory(((SaveData)SaveData).UserName, message);
        chatGPTTalk = new ChatGPTRequest(((SaveData)SaveData).APIKey, messages);
    }

    public override string OnSurfaceRestore(IDictionary<int, string> reference, string sakuraSurface, string keroSurface)
    {
        isTalking = false;
        return base.OnSurfaceRestore(reference, sakuraSurface, keroSurface);
    }

    public override string OnSecondChange(IDictionary<int, string> reference, string uptime, bool isOffScreen, bool isOverlap, bool canTalk, string leftSecond)
    {
        if (chatGPTSumamry != null)
        {
            var summary = chatGPTSumamry;
            if (!summary.IsProcessing)
            {
                chatGPTSumamry = null;
                //isBeginedTalk = false;
            }

            SaveHistory(summary.Response);
        }
        if (canTalk && chatGPTTalk != null)
        {
            var talk = chatGPTTalk;
            if (!talk.IsProcessing)
            {
                chatGPTTalk = null;
                //isBeginedTalk = false;
            }

            return BuildTalk(talk.Response, !talk.IsProcessing);
        }
        return base.OnSecondChange(reference, uptime, isOffScreen, isOverlap, canTalk, leftSecond);
    }
    public override string OnMinuteChange(IDictionary<int, string> reference, string uptime, bool isOffScreen, bool isOverlap, bool canTalk, string leftSecond)
    {
        
        if(canTalk && !isTalking && ((SaveData)SaveData).IsRandomIdlingSurfaceEnabled)
            return "\\s["+Surfaces.Of(SurfaceCategory.Normal).GetRaodomSurface()+"]";
        else
            return base.OnMinuteChange(reference, uptime, isOffScreen, isOverlap, canTalk, leftSecond);
    }

    string GetShowLog()
    {
        var log = ((SaveData)SaveData).ChatHistory;
        return log.Replace("\n", "\\n");
    }

    string BuildTalk(string response, bool createChoices)
    {
        const string INPUT_CHOICE_MYSELF = "質問を入力する";
        const string SHOW_LOGS = "ログを表示";
        const string BACK = "戻る";
        try
        {
            isTalking = true;
            //if (((SaveData)SaveData).IsDevMode)
            Log.WriteAllText(Log.Response, response);

            //var onichanResponse = GetOnichanRenponse(response);
            if (!createChoices)
            {
                return new TalkBuilder().Append($"\\_q...\\s[" + Surfaces.Of(SurfaceCategory.Thinking).GetRaodomSurface() + "]").LineFeed().Build();
            }
            var aiResponse = GetAIResponse(response);
            var surfaceId = GetSurfaceId(response);
            var talkBuilder = new TalkBuilder()
                .Append($"\\_q\\s[{surfaceId}]")
                .Append(aiResponse)
                .LineFeed()
                .HalfLine();

            if (createChoices && string.IsNullOrEmpty(aiResponse)) {
                return new TalkBuilder().Append($"\\_q...\\s[" + Surfaces.Of(SurfaceCategory.Thinking).GetRaodomSurface() + "]").LineFeed().Build();
            }

            if (!string.IsNullOrEmpty(aiResponse)) {
                AddChatHistory(((SaveData)SaveData).AIName, aiResponse);
            }
            DeferredEventTalkBuilder deferredEventTalkBuilder = talkBuilder.Marker().AppendChoice(INPUT_CHOICE_MYSELF).LineFeed();

            deferredEventTalkBuilder = talkBuilder.Marker().AppendChoice(SHOW_LOGS).LineFeed();

            return deferredEventTalkBuilder
                    //.Marker().AppendChoice(END_TALK).LineFeed()
                    .Build()
                    .ContinueWith(id =>
                    {
                        //if (onichanResponse.Contains(id))
                        //    BeginTalk($"{log}{((SaveData)SaveData).AIName}：{aiResponse}\r\n{((SaveData)SaveData).UserName}：{id}");
                        if (id == SHOW_LOGS)
                            return new TalkBuilder()
                            .Append("\\_q").Append(GetShowLog()).LineFeed()
                            .HalfLine()
                            .Marker().AppendChoice(BACK)
                            .Build()
                            .ContinueWith(x =>
                            {
                                if (x == BACK)
                                    return BuildTalk(response, createChoices);
                                return "";
                            });
                        else if (id == INPUT_CHOICE_MYSELF)
                            return new TalkBuilder().AppendUserInput().Build().ContinueWith(input =>
                            {
                                BeginTalk(input);
                                return "";
                            });
                        //else if (id == END_TALK) {
                        //    BeginTalk(END_TALK);
                        //    return "";
                        //}
                        return "";
                    });
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }
    void SaveHistory(string response)
    {
        try
        {
            Log.WriteAllText(Log.Response, response);
            ((SaveData)SaveData).HistorySummary = ((SaveData)SaveData).HistorySummary + response;
        }
        catch (Exception e)
        {
            Log.WriteAllText(Log.Response, e.ToString());
        }
    }
    string GetAIResponse(string response)
    {
        var pattern = $"^{((SaveData)SaveData).AIName}(のセリフ)?[：:](?<Serif>.+?)$";
        var lines = response.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        var aiResponse = lines.Select(x=>Regex.Match(x, pattern)).Where(x=>x.Success).Select(x=>x.Groups["Serif"].Value).FirstOrDefault();
        if (string.IsNullOrEmpty(aiResponse))
            return response.Replace("\n", "\\n").Replace("\r\n", "\\n");

        return TrimSerifBrackets(aiResponse);
    }

    string TrimSerifBrackets(string serif)
    {
        serif = serif.Trim();
        if(serif.StartsWith("「") && serif.EndsWith("」"))
            return serif.Substring(1, serif.Length - 2);
        if(serif.StartsWith("『") && serif.EndsWith("』"))
            return serif.Substring(1, serif.Length - 2);
        if(serif.StartsWith("\"") && serif.EndsWith("\""))
            return serif.Substring(1, serif.Length - 2);
        if(serif.StartsWith("'") && serif.EndsWith("'"))
            return serif.Substring(1, serif.Length - 2);
        return serif;
    }

    int GetSurfaceId(string response)
    {
        var lines = response.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        var face = lines.FirstOrDefault(x => x.StartsWith($"{((SaveData)SaveData).AIName}の表情："));
        if (face is null)
            return 0;

        foreach(var category in SurfaceCategory.All)
        {
            if (face.Contains(category))
                return Surfaces.Of(category).GetSurfaceFromRate(faceRate);
        }

        return 0;
    }
    DeferredEventTalkBuilder AppendWordWrapChoice(TalkBuilder builder, string text)
    {
        builder = builder.Marker();
        DeferredEventTalkBuilder deferredEventTalkBuilder = null;
        foreach (var choice in WordWrap(text))
        {
            if (deferredEventTalkBuilder == null)
                deferredEventTalkBuilder = builder.AppendChoice(choice, text).LineFeed();
            else
                deferredEventTalkBuilder = deferredEventTalkBuilder.AppendChoice(choice, text).LineFeed();
        }
        return deferredEventTalkBuilder;
    }
    DeferredEventTalkBuilder AppendWordWrapChoice(DeferredEventTalkBuilder builder, string text)
    {
        builder = builder.Marker();
        foreach (var choice in WordWrap(text))
            builder = builder.AppendChoice(choice, text).LineFeed();
        return builder;
    }
    IEnumerable<string> WordWrap(string text)
    {
        var width = 24;
        for (int i = 0; i < text.Length; i += width)
        {
            if (i + width < text.Length)
                yield return text.Substring(i, width);
            else
                yield return text.Substring(i);
        }
    }
}

return new SaraShinonomeGhost();
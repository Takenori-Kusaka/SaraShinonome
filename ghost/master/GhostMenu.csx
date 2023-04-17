#r "Rosalind.dll"
#load "SaveData.csx"
#load "Log.csx"
using Shiorose;
using Shiorose.Resource;
using Shiorose.Resource.ShioriEvent;
using Shiorose.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class SaraShinonomeGhost : Ghost
{

    private string OpenMenu()
    {
        if(string.IsNullOrEmpty(((SaveData)SaveData).APIKey))
            return ChangeOpenAIAPITalk();

        string[] conversationTopics = new string[]{
            "最近はどうですか？",
            "最近何か変わったことはありますか？",
            "最近読んだ本は何ですか？",
            "最近行った旅行先はどこでしたか？",
            "趣味について話してみましょうか？",
            "最近見た映画についてどう思いましたか？",
            "お勧めのレストランはありますか？",
            "何か新しいことに挑戦していますか？",
            "最近は何か楽しいことがありましたか？"
        };
        Random random = new Random();
        int index = random.Next(conversationTopics.Length);
        string topic = conversationTopics[index];
        const string RAND = "独り言";
        const string COMMUNICATE = "聞きたいことがある";
        const string SETTINGS = "設定を変えたい";
        const string CANCEL = "なんでもない";

        string[] responses = { "はい、どうかしましたか？", "お呼びでしょうか？", "何かお手伝いできますか？", "何かお困りのようですね" };
    
        Random rand = new Random();
        index = rand.Next(responses.Length);
        
        return new TalkBuilder().Append(responses[index]).LineFeed()
                                .HalfLine()
                                .Marker().AppendChoice(RAND).LineFeed()
                                .Marker().AppendChoice(topic).LineFeed()
                                .Marker().AppendChoice(COMMUNICATE).LineFeed()
                                .HalfLine()
                                                                .Marker().AppendChoice(SETTINGS).LineFeed()
                                .HalfLine()
                                .Marker().AppendChoice(CANCEL)
                                .BuildWithAutoWait()
                                .ContinueWith((id) =>
                                {
                                    switch (id)
                                    {
                                        case RAND:
                                            return OnRandomTalk();
                                        case COMMUNICATE:
                                            return new TalkBuilder().Append("はい、どうぞお話しください").AppendCommunicate().Build();
                                        case SETTINGS:
                                            return SettingsTalk();
                                        default:
                                            return new TalkBuilder().Append("はい、わかりました").BuildWithAutoWait();
                                    }
                                });
    }

    private string SettingsTalk(){
        const string CHANGE_OPENAI_API = "OpenAIのAPIキーを変更する";
        const string CHANGE_RANDOMTALK_INTERVAL = "ランダムトークの頻度を変更する";
        string CHANGE_RANDOM_IDLING_SURFACE = "定期的に身じろぎする（現在："+(((SaveData)SaveData).IsRandomIdlingSurfaceEnabled ? "有効" : "無効")+"）";
        string CHANGE_DEVMODE = "開発者モードを変更する（現在："+(((SaveData)SaveData).IsDevMode ? "有効" : "無効")+"）";
        const string BAKC = "戻る";
        return new TalkBuilder()
        .Append("何か契約条件が変わりますか？")
        .LineFeed()
        .HalfLine()
        .Marker().AppendChoice(CHANGE_OPENAI_API).LineFeed()
        .HalfLine()
        .Marker().AppendChoice(CHANGE_RANDOMTALK_INTERVAL).LineFeed()
        .Marker().AppendChoice(CHANGE_RANDOM_IDLING_SURFACE).LineFeed()
        .HalfLine()
        .Marker().AppendChoice(CHANGE_DEVMODE).LineFeed()
        .HalfLine()
        .Marker().AppendChoice(BAKC)
        .BuildWithAutoWait()
        .ContinueWith(id=>
        {
            if (id == CHANGE_OPENAI_API)
                return ChangeOpenAIAPITalk();
            else if (id == CHANGE_RANDOMTALK_INTERVAL)
                return ChangeRandomTalkIntervalTalk();
            else if (id == CHANGE_RANDOM_IDLING_SURFACE)
            {
                ((SaveData)SaveData).IsRandomIdlingSurfaceEnabled = !((SaveData)SaveData).IsRandomIdlingSurfaceEnabled;
                return SettingsTalk();
            }
            else if (id == CHANGE_DEVMODE)
            {
                ((SaveData)SaveData).IsDevMode = !((SaveData)SaveData).IsDevMode;
                return SettingsTalk();
            }
            else
                return OpenMenu();
        });
    }

    private string ChangeOpenAIAPITalk(){
        return new TalkBuilder().Append("配属先変更ですね？ID(OpenAI API key)を教えてください。")
                                .AppendPassInput(defValue:((SaveData)SaveData).APIKey)
                                .Build()
                                .ContinueWith(apiKey=>
                                {
                                    ((SaveData)SaveData).APIKey = apiKey;
                                    return new TalkBuilder().Append("ありがとうございます、引き続きよろしくお願いいたします").BuildWithAutoWait();
                                });
    }

    private string ChangeRandomTalkIntervalTalk(){
        return new TalkBuilder().Append("うるさかったですか？")
                                .LineFeed()
                                .HalfLine()
                                .Marker().AppendChoice("10秒").LineFeed()
                                .Marker().AppendChoice("30秒").LineFeed()
                                .Marker().AppendChoice("1分").LineFeed()
                                .Marker().AppendChoice("5分").LineFeed()
                                .Marker().AppendChoice("10分").LineFeed()
                                .Marker().AppendChoice("30分").LineFeed()
                                .Marker().AppendChoice("1時間").LineFeed()
                                .Marker().AppendChoice("静かにしていて").LineFeed()
                                .Marker().AppendChoice("なんでもない")
                                .BuildWithAutoWait()
                                .ContinueWith(id=>
                                {
                                    switch(id)
                                    {
                                        case "10秒":
                                            ((SaveData)SaveData).TalkInterval2 = 10;
                                            return new TalkBuilder().Append("常に話しますね。").BuildWithAutoWait();
                                        case "30秒":
                                            ((SaveData)SaveData).TalkInterval = 30;
                                            return new TalkBuilder().Append("多めに話しますね").BuildWithAutoWait();
                                        case "1分":
                                            ((SaveData)SaveData).TalkInterval = 60;
                                            return new TalkBuilder().Append("普通に話しますね").BuildWithAutoWait();
                                        case "5分":
                                            ((SaveData)SaveData).TalkInterval = 300;
                                            return new TalkBuilder().Append("ときどき話しますね").BuildWithAutoWait();
                                        case "10分":
                                            ((SaveData)SaveData).TalkInterval = 600;
                                            return new TalkBuilder().Append("たまに話しますね").BuildWithAutoWait();
                                        case "30分":
                                            ((SaveData)SaveData).TalkInterval = 1800;
                                            return new TalkBuilder().Append("気が向いたら話しますね").BuildWithAutoWait();
                                        case "1時間":
                                            ((SaveData)SaveData).TalkInterval = 3600;
                                            return new TalkBuilder().Append("まれに話しますね").BuildWithAutoWait();
                                        case "静かにしていて":
                                            ((SaveData)SaveData).TalkInterval = 0;
                                            return new TalkBuilder().Append("静かにしてますね").BuildWithAutoWait();
                                        default:
                                            return new TalkBuilder().Append("また気になるようでしたら声かけてください").BuildWithAutoWait();
                                    }
                                });
    }

    private string TrimLength(string text, int maxLength){
        if(text.Length > maxLength)
            return text.Substring(0, maxLength) + "…";
        else
            return text;
    }
}

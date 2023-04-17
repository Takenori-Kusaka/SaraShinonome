#r "Rosalind.dll"
#load "SaveData.csx"
#load "ChatGPT.csx"
using Shiorose;
using Shiorose.Resource;
using Shiorose.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class SaraShinonomeGhost : Ghost
{
    private string IsToday(string LastTalkDate) {
        // 日付文字列をDateTime型に変換する
        DateTime date = DateTime.ParseExact(LastTalkDate, "yyyy/MM/dd", null);

        // 今日の日付を取得する
        DateTime today = DateTime.Today;

        // 日付が今日でない場合に実行する処理を記述する
        if (date != today)
        {
            return today.ToString("yyyy/MM/dd");
        }
        else
        {
            return null;
        }
    }
    private string GenerateSummaryHistory() {
        OpenAISummarizer summarizer = new OpenAISummarizer(((SaveData)SaveData).APIKey);
        var summaryTask = summarizer.SummarizeText(((SaveData)SaveData).ChatHistory);
        summaryTask.Wait();
        return summaryTask.Result;
    }
    public override string OnBoot(IDictionary<int, string> references, string shellName = "", bool isHalt = false, string haltGhostName = "")
    {  
        if (((SaveData)SaveData).IsFirstTalk) {
            var today = IsToday(((SaveData)SaveData).LastTalkDate);
            if (today!=null) {
                var summary = "";//GenerateSummaryHistory();
                string historySummary = ((SaveData)SaveData).HisotrySummary;
                summary = String.Format("{0}{1}: {2}\n", ((SaveData)SaveData).HisotrySummary, today, summary);
                ((SaveData)SaveData).HisotrySummary = summary;
                ((SaveData)SaveData).IsFirstTalk = false;
                return new TalkBuilder()
                .AppendLine("すいません、昨日までのことを思い出してました。")
                .AppendLine("今日も一日よろしくお願いいたします。")
                .BuildWithAutoWait();
            } else {
                return new TalkBuilder()
                .AppendLine("またお会いしましたね、どうされましたか？")
                .BuildWithAutoWait();
            }
        }
        return new TalkBuilder()
        .AppendLine("お疲れ様です、今日もよろしくお願いします。")
        .BuildWithAutoWait();
    }

    public override string OnFirstBoot(IDictionary<int, string> reference, int vanishCount = 0)
    {
        return new TalkBuilder()
            .AppendLine("初めまして、今日から配属されました東雲沙羅です。").LineFeed()
            .AppendLine("さしあたって、配属先のID(OpenAI API key)を教えてください。").LineFeed()
            .AppendPassInput(defValue:((SaveData)SaveData).APIKey)
            .Build()
            .ContinueWith(apiKey=>
            {
                ((SaveData)SaveData).APIKey = apiKey;
                return new TalkBuilder().Append("ありがとうございます、これからよろしくお願いします").BuildWithAutoWait();
            });
    }

    public override string OnClose(IDictionary<int, string> reference, string reason = "")
    {
        return 
        new TalkBuilder()
        .Append("今日もお疲れ様でした。またよろしくお願いいたします。")
        .EmbedValue("\\-")
        .BuildWithAutoWait();
    }
}
#r "Rosalind.dll"
using Shiorose;
using Shiorose.Resource;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[DataContract]
public class SaveData : BaseSaveData {
    /* BaseSaveDataで定義済み */
    // int TalkInterval => ランダムトークの間隔
    // string UserName => ユーザ名

    //ベースクラスのTalkIntervalに[DataMember]が付いていないので値が保存されない
    [DataMember]
    public int TalkInterval2 { get=>TalkInterval; set=>TalkInterval=value; }

    // 項目追加したい場合の定義はこんな感じ
    [DataMember]
    public string APIKey { get; set; }

    [DataMember]
    public bool IsDevMode { get; set; } = false;

    [DataMember]
    public bool IsFirstTalk { get; set; } = false;
    
    [DataMember]
    public string LastTalkDate { get; set; } = "1970/01/01";
    
    [DataMember]
    public string HisotrySummary { get; set; } = "";
    
    [DataMember]
    public string ChatHistory { get; set; } = "";

    [DataMember]
    public bool IsRandomIdlingSurfaceEnabled { get; set; } = false;

    /// <summary>
    /// デフォルト値はここで設定
    /// ただしsavedataのファイルがある際は初期化されないので、
    /// 後からメンバを増やした際は注意！！
    /// </summary>
    public SaveData()
    {
        UserName = "先輩";
        TalkInterval = 1800;
        IsFirstTalk = false;
        IsDevMode = false;
    }
}

// SaveFileの名前を変えたい場合
// SaveDataManager.SaveFileName = "save.json";
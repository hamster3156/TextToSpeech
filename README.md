# このリポジトリについて
神ゲー創造エボリューションのゲーム作品で、テキストから音声合成を行う機能の実装を行いました。このリポジトリでは作成したソースコードをまとめています。

# ダウンロード方法
[release](https://github.com/hamster3156/TextToSpeech/releases/tag/v.1.0.0)からunitypackageをダウンロードしてください

# 必要なツール
・[UniTask](https://github.com/Cysharp/UniTask) \
・[SpeechSDK](https://learn.microsoft.com/ja-jp/azure/ai-services/speech-service/speech-sdk) 

SpeechSDKのダウンロードについてですが、[Azureのオンラインドキュメント]([https://learn.microsoft.com/ja-jp/azure/ai-services/speech-service/how-to-speech-synthesis?tabs=browserjs%2Cterminal&pivots=programming-language-csharp](https://learn.microsoft.com/ja-jp/azure/ai-services/speech-service/quickstarts/setup-platform?tabs=windows%2Cubuntu%2Cdotnetcli%2Cdotnet%2Cjre%2Cmaven%2Cnodejs%2Cmac%2Cpypi&pivots=programming-language-csharp#install-the-speech-sdk-for-c))の方法でダウンロードができなかったので、[Azureの音声合成，音声認識をUnityから利用](https://akihiro-document.azurewebsites.net/post/azure/azure_speechsdk/)の記事で紹介されている方法でダウンロードしました。

# 参考にした記事
・[Azureクイックスタートのサンプルスクリプト](https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/quickstart/csharp/unity/text-to-speech/Assets/Scripts/HelloWorld.cs) \
・[Azureの音声合成，音声認識をUnityから利用](https://akihiro-document.azurewebsites.net/post/azure/azure_speechsdk/#azure-%E5%81%B4%E8%A8%AD%E5%AE%9A)

# 注意点
音声合成の機能を利用するには、[Microsoft Azure portal](https://azure.microsoft.com/ja-jp/get-started/azure-portal/)の[Azure AI services](https://azure.microsoft.com/ja-jp/products/ai-services)の[音声サービス](https://azure.microsoft.com/ja-jp/products/ai-services/ai-speech)を利用してSpeechKeyを作成する必要があります。また、利用状況によって料金がかかるのでご注意ください。

また、今回の音声は英語の読み上げを利用しているため日本語を入力するとエラーが出てしまうのでご注意ください。

# 利用方法
1. GameObjectにTextToSpeechPlayerをアタッチします。\
![image](https://github.com/user-attachments/assets/d9f2e705-1071-422a-b78a-ec112329315c)

2. SpeechKeyとRegionをインスペクターに入力します。\
![image](https://github.com/user-attachments/assets/63029c10-d23c-4a80-b2c0-9005578ee26b)

3. 読み上げ音声のタイプを設定することができます。\
![image](https://github.com/user-attachments/assets/c9d8fb6e-1fe8-48a6-864a-d0853d523e1c) \
Azureのドキュメントに[読み上げ音声の一覧表](https://learn.microsoft.com/ja-jp/azure/ai-services/speech-service/language-support?tabs=tts#multilingual-voices)があります。音声を追加したい場合は、ReadingVoiceNameListのenumに名前を追加してください。ドキュメントでは、ハイフンで記載されていますがエディタ上でエラーが出てしまうのでアンダースコアで記述しています。
```C#

// ここに名前追加する
public enum ReadingVoiceNameList
{
    en_KE_ChilembaNeural,
    en_US_EricNeural,
    en_US_RyanMultilingualNeural,
    en_US_SteffanNeural
}

// アンダースコアをハイフンに変換しているクラス
sealed internal class ReadingVoiceNameListConverter
{
    /// <summary>
    /// enumをAzureの読み上げ音声名に変換して返す
    /// </summary>
    public string GetConvertVoiceName(ReadingVoiceNameList azureReadingVoiceName)
    {
        StringBuilder stringBuilder = new();

        // enumの名前を入れる
        stringBuilder.Append(azureReadingVoiceName.ToString());

        // アンダースコアをハイフンに変換
        stringBuilder.Replace("_", "-");

        // 変換した文字列を返す
        return stringBuilder.ToString();
    }
}
```

4. 音声再生を行うために、AudioSourceをシーン上に配置します。\
開始、ループ、終了時のAudioSourceをインスペクターにアタッチすることで会話中の音声に効果音や環境音を追加することができます。\
![image](https://github.com/user-attachments/assets/e6ec8f09-0df0-4422-81f5-3a5b8370daa2)

5. インスペクターから音声再生を行うことができます。\
プレイモード中にしか実行できないようになっています。\
![image](https://github.com/user-attachments/assets/64174757-8ec0-4ff8-901b-7fd9aa098edc)

# 参照方法
インタフェースのITextToSpeechPlayerをTextToSpeechPlayerに実装しているので、インタフェースから参照することができます。

```C#
public interface ITextToSpeechPlayer
{
    /// <summary>
    /// 読み上げ音声を変更する
    /// </summary>
    /// <param name="selectName">選択する名前</param>
    void ChangeReadingVoice(ReadingVoiceNameList selectName);

    /// <summary>
    /// 会話を再生する
    /// </summary>
    /// <param name="speakContext">会話内容</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    UniTask PlaySpeakAsync(string speakContext, CancellationToken ct);

    /// <summary>
    /// 会話を止める
    /// </summary>
    void StopSpeak();
}
```

今回のコードでは含まれていませんが、私はサービスロケーターを利用して参照を行いました。

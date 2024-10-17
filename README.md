# このリポジトリについて
神ゲー創造エボリューションのゲーム作品で、テキストから音声合成を行う機能の実装を行いました。このリポジトリでは作成したソースコードをまとめています。

# ダウンロード方法
[releaseからunitypackageをダウンロードしてください](https://github.com/hamster3156/TextToSpeech/releases/tag/v.1.0.0)

# 必要なツール
会話音声を再生する処理が非同期処理になっているため、UniTaskをプロジェクトに入れる必要があります。\
https://github.com/Cysharp/UniTask

```C#
 /// <summary>
 /// 会話を再生する
 /// </summary>
 /// <param name="speakContext">会話内容</param>
 public async UniTask PlaySpeakAsync(string speakContext, CancellationToken ct)
 {
     // 入力内容が無ければ再生を中断する
     if (string.IsNullOrEmpty(speakContext))
     {
         Debug.LogWarning("speakContextに何も入力されていないです");
         return;
     }

     if (_startPlayer != null && _startPlayer.clip != null)
     {
         _startPlayer.Play();

         // 開始音声の再生時間を取得
         var startPlayerLength = Mathf.RoundToInt(_startPlayer.clip.length);

         // 再生が終わるまで待機
         await UniTask.Delay(startPlayerLength, cancellationToken: ct);
     }

     using (var result = _speechSynthesizer.StartSpeakingTextAsync(speakContext).Result)
     {
         var audioDataStream = AudioDataStream.FromResult(result);
         var isFirstAudioChunk = true;
         var audioClip = AudioClip.Create(
             "Speech",
             _rate * 600, // Can speak 10mins audio as maximum
             1,
             _rate,
             true,
             (float[] audioChunk) =>
             {
                 var chunkSize = audioChunk.Length;
                 var audioChunkBytes = new byte[chunkSize * 2];
                 var readBytes = audioDataStream.ReadData(audioChunkBytes);
                 if (isFirstAudioChunk && readBytes > 0)
                 {
                     isFirstAudioChunk = false;
                 }

                 for (int i = 0; i < chunkSize; ++i)
                 {
                     if (i < readBytes / 2)
                     {
                         audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                     }
                     else
                     {
                         audioChunk[i] = 0.0f;
                     }
                 }

                 if (readBytes == 0)
                 {
                     // メインスレッドから呼び出す処理はここには書けない
                     Thread.Sleep(200);
                     _isStoping = true;
                 }

             });

         _speakPlayer.clip = audioClip;
         PlaySpeakSoundEffect(true);
     }
 }
```

また、SpeechSDKをダウンロードする必要もあります。Azureのオンラインドキュメントの方法で失敗してしまったので、[akihiro0105さん](https://github.com/akihiro0105)が公開しているSpeechSDKHelperでSDKをダウンロードしました。本当にありがとうございます!\
https://github.com/akihiro0105/SpeechSDKHelper

# 参考にした記事
Azureのリポジトリで公開されているクイックスタートのサンプルスクリプトをベースに作成しています。

https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/quickstart/csharp/unity/text-to-speech/Assets/Scripts/HelloWorld.cs

https://akihiro-document.azurewebsites.net/post/azure/azure_speechsdk/#azure-%E5%81%B4%E8%A8%AD%E5%AE%9A

# 利用方法
GameObjectにTextToSpeechPlayerをアタッチして利用を行います。\
![image](https://github.com/user-attachments/assets/580865cb-588d-4efd-8ab2-9bafc56accbe)

音声合成の機能を利用するには、[Microsoft Azure portal](https://azure.microsoft.com/ja-jp/get-started/azure-portal/)のAzure AI servicesの音声サービスを利用してSpeechKeyを作成する必要があります。また、利用状況によって料金がかかるのでご注意ください。\
SpeechKeyを作成したら、インスペクター入力します。\
![image](https://github.com/user-attachments/assets/63029c10-d23c-4a80-b2c0-9005578ee26b)

読み上げ音声のタイプに関しですが、今回は4つの中から選ぶことができます。\
![image](https://github.com/user-attachments/assets/292843a4-a1ef-460a-8e52-a3746b24fa63)

Azureのドキュメントに[読み上げ音声の一覧表](https://learn.microsoft.com/ja-jp/azure/ai-services/speech-service/language-support?tabs=tts#multilingual-voices)があります。音声を追加したい場合は、ReadingVoiceNameのenumに名前を追加してください。ドキュメントでは、ハイフンで記載されていますがエディタ上でエラーが出てしまうのでアンダースコアで記述しています。

音声再生を行うために、AudioSourceをシーン上に配置します。会話音声を再生させるために必要なので、必ず配置してください。\
![image](https://github.com/user-attachments/assets/9d41803a-e107-47f7-a323-aa64fc1facf2)

音声機能を利用するにはインタフェースのITextToSpeechを取得して利用します。

```C#
 public interface ITextToSpeechPlayer
 {
     /// <summary>
     /// 読み上げ音声を変更する
     /// </summary>
     /// <param name="selectName">選択する名前</param>
     void ChangeReadingVoice(ReadingVoiceName selectName);

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

ソースコード上では記載されていませんが、インタフェースの参照方法として自分はサービスロケーターを利用しました。
https://github.com/hamster3156/ServiceLocator

最後にですが、改良できるようにunitypackageを作成したのでぜひご活用ください。

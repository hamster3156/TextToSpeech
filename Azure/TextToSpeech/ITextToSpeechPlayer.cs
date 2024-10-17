using Cysharp.Threading.Tasks;
using System.Threading;

namespace Hamster.Azure.TextToSpeech
{
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
}

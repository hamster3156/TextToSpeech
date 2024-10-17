using Cysharp.Threading.Tasks;
using System.Threading;

namespace Hamster.Azure.TextToSpeech
{
    public interface ITextToSpeechPlayer
    {
        void ChangeReadingVoice(ReadingVoiceName selectName);

        UniTask PlaySpeakAsync(string speakContext, CancellationToken ct);

        void StopSpeak();
    }
}

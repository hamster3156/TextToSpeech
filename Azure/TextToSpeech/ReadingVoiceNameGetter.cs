using System.Text;

namespace Hamster.Azure.TextToSpeech
{
    sealed internal class ReadingVoiceNameGetter
    {
        /// <summary>
        /// enumをAzureの読み上げ音声名に変換して返す
        /// </summary>
        public string GetConvertVoiceName(ReadingVoiceName azureReadingVoiceName)
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
}

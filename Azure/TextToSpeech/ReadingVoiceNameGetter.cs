using System.Text;

namespace Hamster.Azure.TextToSpeech
{
    sealed internal class ReadingVoiceNameGetter
    {
        /// <summary>
        /// enum��Azure�̓ǂݏグ�������ɕϊ����ĕԂ�
        /// </summary>
        public string GetConvertVoiceName(ReadingVoiceName azureReadingVoiceName)
        {
            StringBuilder stringBuilder = new();

            // enum�̖��O������
            stringBuilder.Append(azureReadingVoiceName.ToString());

            // �A���_�[�X�R�A���n�C�t���ɕϊ�
            stringBuilder.Replace("_", "-");

            // �ϊ������������Ԃ�
            return stringBuilder.ToString();
        }
    }
}

using Cysharp.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading;
using UnityEngine;

namespace Hamster.Azure.TextToSpeech
{
    sealed internal class TextToSpeechPlayer : MonoBehaviour, ITextToSpeechPlayer
    {
        [SerializeField, Header("キー設定")]
        private string _speechKeySetting;

        [SerializeField, Header("地域設定")]
        private string _regionSetting;

        [SerializeField, Header("読み上げ音声のタイプ")]
        private ReadingVoiceName _selectReadingVoiceName;

        [SerializeField, Header("音楽再生のAudioSource")]
        private AudioSource _startPlayer;

        [SerializeField, Header("読み上げ音声のAudioSource")]
        private AudioSource _speakPlayer;

        [SerializeField, Header("ループ再生のAudioSource")]
        private AudioSource _backloopPlayer;

        [SerializeField, Header("会話終了音声のAudioSource")]
        private AudioSource _endPlayer;

        // 音声名を変換して取得するクラス
        private ReadingVoiceNameGetter _readingVoiceNameGetter = new();

        // Azureの音声合成関連のクラス
        private SpeechConfig _speechConfig;
        private SpeechSynthesizer _speechSynthesizer;

        // 再生時間
        private int _rate = 24000;

        // 再生停止フラグ
        private bool _isStoping = false;

        /// <summary>
        /// 読み上げ音声を変更する
        /// </summary>
        public void ChangeReadingVoice(ReadingVoiceName chageName)
        {
            _speechConfig.SpeechSynthesisVoiceName = _readingVoiceNameGetter.GetConvertVoiceName(chageName);
            _speechSynthesizer = new SpeechSynthesizer(_speechConfig, null);
        }

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

        /// <summary>
        /// 効果音を再生する
        /// </summary>
        /// <param name="isPlaying">再生フラグ</param>
        private void PlaySpeakSoundEffect(bool isPlaying)
        {
            if (isPlaying)
            {
                if (_speakPlayer != null)
                {
                    _speakPlayer.Play();
                }

                if (_backloopPlayer != null && _backloopPlayer.clip != null)
                {
                    _backloopPlayer.Play();
                }
            }
            else
            {
                if (_speakPlayer != null)
                {
                    _speakPlayer.Stop();
                }

                if (_backloopPlayer != null && _backloopPlayer.clip != null)
                {
                    _backloopPlayer.Stop();
                }
            }
        }

        /// <summary>
        /// 会話を停止する
        /// </summary>
        public void StopSpeak()
        {
            if (_isStoping)
            {
                return;
            }

            PlaySpeakSoundEffect(false);

            // 会話が終了した時の音声を再生
            if (_endPlayer != null && _endPlayer.clip != null)
            {
                _endPlayer.Play();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// SpeechConfigの初期化とキャンセル時のエラーハンドリングを設定
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Initialize()
        {
            // キーと地域の設定
            _speechConfig = SpeechConfig.FromSubscription(_speechKeySetting, _regionSetting);

            // 音声形式を設定
            _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

            // 読み上げ音声を変更
            ChangeReadingVoice(_selectReadingVoiceName);

            // 音声合成がキャンセルされた時のエラーハンドリング
            _speechSynthesizer.SynthesisCanceled += (s, e) =>
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                var message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
                throw new Exception(message);
            };
        }

        private async void Update()
        {
            if (_isStoping)
            {
                StopSpeak();
                _isStoping = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                await PlaySpeakAsync("Hello World", default);
            }
        }

        private void OnDestroy()
        {
            _speechSynthesizer.Dispose();
        }
    }
}
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using System;

namespace Yamadev.YamaStream.Integration
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YamaPlayerAdvancedIntegration : UdonSharpBehaviour
    {
        [Header("YamaPlayer Settings")]
        [SerializeField] private Controller _yamaPlayerController;
        [SerializeField] private VideoPlayerType _defaultVideoPlayerType = VideoPlayerType.AVProVideoPlayer;
        
        [Header("Video Configuration")]
        [SerializeField] private VideoConfig[] _videoConfigs;
        [SerializeField] private int _defaultVideoIndex = 0;
        
        [Header("Playlist Settings")]
        [SerializeField] private VRCUrl _playlistUrl = VRCUrl.Empty;
        [SerializeField] private bool _enablePlaylistMode = false;
        
        [Header("Interaction Settings")]
        [SerializeField] private InteractionMode _interactionMode = InteractionMode.SingleVideo;
        [SerializeField] private bool _cycleThroughVideos = false;
        [SerializeField] private float _interactionCooldown = 1.0f;
        
        [Header("Permission Settings")]
        [SerializeField] private bool _requirePermission = true;
        [SerializeField] private PlayerPermission _minimumPermission = PlayerPermission.Viewer;
        
        [Header("UI Settings")]
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private GameObject _successIndicator;
        [SerializeField] private GameObject _errorIndicator;
        [SerializeField] private float _indicatorDisplayTime = 2.0f;
        
        [Header("Event Listeners")]
        [SerializeField] private UdonSharpBehaviour[] _eventListeners;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        // 内部状態
        private bool _isProcessing = false;
        private int _currentVideoIndex = 0;
        private float _lastInteractionTime = 0f;
        private bool _isInitialized = false;
        
        // 列挙型
        public enum InteractionMode
        {
            SingleVideo,
            MultipleVideos,
            Playlist,
            RandomVideo
        }
        
        [System.Serializable]
        public class VideoConfig
        {
            [Header("Video Info")]
            public string name = "動画";
            public VRCUrl url = VRCUrl.Empty;
            public string title = "";
            public VideoPlayerType playerType = VideoPlayerType.AVProVideoPlayer;
            
            [Header("Playback Settings")]
            public bool addToQueue = false;
            public bool autoPlay = true;
            
            [Header("UI")]
            public string successMessage = "動画を再生しました";
            public string errorMessage = "再生に失敗しました";
        }
        
        void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            if (_yamaPlayerController == null)
            {
                PrintError("YamaPlayerControllerが設定されていません");
                return;
            }
            
            _currentVideoIndex = _defaultVideoIndex;
            
            // インディケーターを初期化
            if (_loadingIndicator != null) _loadingIndicator.SetActive(false);
            if (_successIndicator != null) _successIndicator.SetActive(false);
            if (_errorIndicator != null) _errorIndicator.SetActive(false);
            
            _isInitialized = true;
            PrintLog("YamaPlayerAdvancedIntegration初期化完了");
        }
        
        public override void Interact()
        {
            if (!_isInitialized)
            {
                PrintError("初期化が完了していません");
                return;
            }
            
            if (_isProcessing)
            {
                PrintLog("処理中です。しばらくお待ちください。");
                return;
            }
            
            if (Time.time - _lastInteractionTime < _interactionCooldown)
            {
                PrintLog("クールダウン中です");
                return;
            }
            
            _lastInteractionTime = Time.time;
            
            if (_requirePermission && !CheckPermission())
            {
                ShowError("権限が不足しています");
                return;
            }
            
            switch (_interactionMode)
            {
                case InteractionMode.SingleVideo:
                    PlaySingleVideo();
                    break;
                case InteractionMode.MultipleVideos:
                    PlayMultipleVideos();
                    break;
                case InteractionMode.Playlist:
                    PlayPlaylist();
                    break;
                case InteractionMode.RandomVideo:
                    PlayRandomVideo();
                    break;
            }
        }
        
        private void PlaySingleVideo()
        {
            if (_videoConfigs == null || _videoConfigs.Length == 0)
            {
                ShowError("動画設定がありません");
                return;
            }
            
            VideoConfig config = _videoConfigs[_currentVideoIndex];
            if (!ValidateVideoConfig(config))
            {
                ShowError($"無効な動画設定: {config.name}");
                return;
            }
            
            PlayVideoFromConfig(config);
            
            if (_cycleThroughVideos)
            {
                _currentVideoIndex = (_currentVideoIndex + 1) % _videoConfigs.Length;
            }
        }
        
        private void PlayMultipleVideos()
        {
            if (_videoConfigs == null || _videoConfigs.Length == 0)
            {
                ShowError("動画設定がありません");
                return;
            }
            
            _isProcessing = true;
            ShowLoading("複数動画を処理中...");
            
            // 最初の動画を再生し、残りをキューに追加
            VideoConfig firstConfig = _videoConfigs[0];
            PlayVideoFromConfig(firstConfig, false);
            
            // 残りの動画をキューに追加
            for (int i = 1; i < _videoConfigs.Length; i++)
            {
                VideoConfig config = _videoConfigs[i];
                if (ValidateVideoConfig(config))
                {
                    AddVideoToQueue(config);
                }
            }
            
            CompleteProcessing();
        }
        
        private void PlayPlaylist()
        {
            if (_playlistUrl == null || _playlistUrl.Get() == string.Empty)
            {
                ShowError("プレイリストURLが設定されていません");
                return;
            }
            
            if (!_playlistUrl.Get().IsValidUrl())
            {
                ShowError("無効なプレイリストURLです");
                return;
            }
            
            _isProcessing = true;
            ShowLoading("プレイリストを読み込み中...");
            
            // プレイリストを読み込み
            LoadPlaylist();
        }
        
        private void PlayRandomVideo()
        {
            if (_videoConfigs == null || _videoConfigs.Length == 0)
            {
                ShowError("動画設定がありません");
                return;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, _videoConfigs.Length);
            VideoConfig config = _videoConfigs[randomIndex];
            
            if (!ValidateVideoConfig(config))
            {
                ShowError($"無効な動画設定: {config.name}");
                return;
            }
            
            PlayVideoFromConfig(config);
        }
        
        private bool ValidateVideoConfig(VideoConfig config)
        {
            if (config == null) return false;
            if (config.url == null || config.url.Get() == string.Empty) return false;
            if (!config.url.Get().IsValidUrl()) return false;
            return true;
        }
        
        private void PlayVideoFromConfig(VideoConfig config, bool showSuccess = true)
        {
            _isProcessing = true;
            ShowLoading($"動画を再生中: {config.name}");
            
            try
            {
                // 所有権を取得
                _yamaPlayerController.TakeOwnership();
                
                // Trackオブジェクトを作成
                Track track = Track.New(config.playerType, config.title, config.url);
                
                if (config.addToQueue)
                {
                    AddVideoToQueue(track, config);
                }
                else
                {
                    PlayTrackDirectly(track, config);
                }
                
                if (showSuccess)
                {
                    ShowSuccess(config.successMessage);
                }
                
                // イベントリスナーに通知
                NotifyEventListeners("OnVideoPlaySuccess", config);
                
                PrintLog($"動画再生成功: {config.name} - {config.url.Get()}");
            }
            catch (Exception e)
            {
                PrintError($"動画再生エラー: {e.Message}");
                ShowError(config.errorMessage);
                NotifyEventListeners("OnVideoPlayError", config);
            }
            
            CompleteProcessing();
        }
        
        private void AddVideoToQueue(Track track, VideoConfig config = null)
        {
            if (_yamaPlayerController.Queue == null)
            {
                PrintError("キューが利用できません");
                return;
            }
            
            _yamaPlayerController.Queue.TakeOwnership();
            _yamaPlayerController.Queue.AddTrack(track);
            
            string message = config != null ? 
                $"キューに追加: {config.name}" : 
                $"キューに追加: {track.GetTitle()}";
            
            PrintLog(message);
        }
        
        private void PlayTrackDirectly(Track track, VideoConfig config = null)
        {
            // 現在の動画を停止
            if (!_yamaPlayerController.Stopped)
            {
                _yamaPlayerController.Stopped = true;
            }
            
            // 動画を再生
            _yamaPlayerController.PlayTrack(track);
        }
        
        private void LoadPlaylist()
        {
            if (_yamaPlayerController.Queue == null)
            {
                PrintError("キューが利用できません");
                CompleteProcessing();
                return;
            }
            
            // プレイリストをキューに読み込み
            _yamaPlayerController.Queue.TakeOwnership();
            _yamaPlayerController.Queue.LoadPlaylist(_playlistUrl);
            
            ShowSuccess("プレイリストを読み込みました");
            CompleteProcessing();
        }
        
        private bool CheckPermission()
        {
            if (_yamaPlayerController == null || _yamaPlayerController.Permission == null)
                return true;
                
            PlayerPermission currentPermission = _yamaPlayerController.PlayerPermission;
            return (int)currentPermission >= (int)_minimumPermission;
        }
        
        private void ShowLoading(string message)
        {
            PrintLog($"読み込み中: {message}");
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(true);
        }
        
        private void ShowSuccess(string message)
        {
            PrintLog($"成功: {message}");
            if (_successIndicator != null)
            {
                _successIndicator.SetActive(true);
                SendCustomEventDelayedSeconds(nameof(HideSuccessIndicator), _indicatorDisplayTime);
            }
        }
        
        private void ShowError(string message)
        {
            PrintError($"エラー: {message}");
            if (_errorIndicator != null)
            {
                _errorIndicator.SetActive(true);
                SendCustomEventDelayedSeconds(nameof(HideErrorIndicator), _indicatorDisplayTime);
            }
        }
        
        public void HideSuccessIndicator()
        {
            if (_successIndicator != null)
                _successIndicator.SetActive(false);
        }
        
        public void HideErrorIndicator()
        {
            if (_errorIndicator != null)
                _errorIndicator.SetActive(false);
        }
        
        private void CompleteProcessing()
        {
            _isProcessing = false;
            
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(false);
        }
        
        private void NotifyEventListeners(string eventName, object data = null)
        {
            if (_eventListeners == null) return;
            
            foreach (UdonSharpBehaviour listener in _eventListeners)
            {
                if (listener != null)
                {
                    listener.SendCustomEvent(eventName);
                }
            }
        }
        
        // パブリックメソッド
        public void SetInteractionMode(InteractionMode mode)
        {
            _interactionMode = mode;
            PrintLog($"インタラクションモードを変更: {mode}");
        }
        
        public void SetCurrentVideoIndex(int index)
        {
            if (_videoConfigs != null && index >= 0 && index < _videoConfigs.Length)
            {
                _currentVideoIndex = index;
                PrintLog($"現在の動画インデックスを設定: {index}");
            }
        }
        
        public void SetPlaylistUrl(VRCUrl url)
        {
            _playlistUrl = url;
            PrintLog($"プレイリストURLを設定: {url.Get()}");
        }
        
        public void SetEnablePlaylistMode(bool enable)
        {
            _enablePlaylistMode = enable;
            PrintLog($"プレイリストモードを設定: {enable}");
        }
        
        public void SetCycleThroughVideos(bool cycle)
        {
            _cycleThroughVideos = cycle;
            PrintLog($"動画サイクル設定を変更: {cycle}");
        }
        
        public void SetInteractionCooldown(float cooldown)
        {
            _interactionCooldown = cooldown;
            PrintLog($"インタラクションクールダウンを設定: {cooldown}秒");
        }
        
        // デバッグ用メソッド
        private void PrintLog(object message)
        {
            if (_enableDebugLogs)
                Debug.Log($"[<color=#EF6291>YamaPlayerAdvancedIntegration</color>] {message}");
        }
        
        private void PrintError(object message)
        {
            Debug.LogError($"[<color=#EF6291>YamaPlayerAdvancedIntegration</color>] {message}");
        }
        
        // イベントリスナー用のカスタムイベント
        public void OnVideoPlaySuccess()
        {
            PrintLog("動画再生が正常に開始されました");
        }
        
        public void OnVideoPlayError()
        {
            PrintError("動画再生に失敗しました");
            CompleteProcessing();
        }
        
        public void OnPlaylistLoadSuccess()
        {
            PrintLog("プレイリストの読み込みが完了しました");
        }
        
        public void OnPlaylistLoadError()
        {
            PrintError("プレイリストの読み込みに失敗しました");
            CompleteProcessing();
        }
    }
}
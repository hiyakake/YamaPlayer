using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Yamadev.YamaStream.Integration
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YamaPlayerIntegration : UdonSharpBehaviour
    {
        [Header("YamaPlayer Settings")]
        [SerializeField] private Controller _yamaPlayerController;
        [SerializeField] private VideoPlayerType _videoPlayerType = VideoPlayerType.AVProVideoPlayer;
        
        [Header("Video Settings")]
        [SerializeField] private VRCUrl _videoUrl = VRCUrl.Empty;
        [SerializeField] private string _videoTitle = "";
        [SerializeField] private bool _autoPlay = true;
        [SerializeField] private bool _addToQueue = false;
        
        [Header("Permission Settings")]
        [SerializeField] private bool _requirePermission = true;
        [SerializeField] private PlayerPermission _minimumPermission = PlayerPermission.Viewer;
        
        [Header("UI Feedback")]
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private string _successMessage = "動画を再生しました";
        [SerializeField] private string _errorMessage = "権限が不足しています";
        [SerializeField] private string _invalidUrlMessage = "無効なURLです";
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        private bool _isProcessing = false;
        
        void Start()
        {
            // 初期化時の処理
            if (_yamaPlayerController == null)
            {
                PrintError("YamaPlayerControllerが設定されていません");
                return;
            }
            
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(false);
                
            PrintLog("YamaPlayerIntegration初期化完了");
        }
        
        public override void Interact()
        {
            if (_isProcessing)
            {
                PrintLog("処理中です。しばらくお待ちください。");
                return;
            }
            
            if (!ValidateVideoUrl())
            {
                ShowMessage(_invalidUrlMessage);
                return;
            }
            
            if (_requirePermission && !CheckPermission())
            {
                ShowMessage(_errorMessage);
                return;
            }
            
            PlayVideo();
        }
        
        private bool ValidateVideoUrl()
        {
            if (_videoUrl == null || _videoUrl.Get() == string.Empty)
            {
                PrintError("動画URLが設定されていません");
                return false;
            }
            
            string url = _videoUrl.Get();
            if (!url.IsValidUrl())
            {
                PrintError($"無効なURL形式です: {url}");
                return false;
            }
            
            return true;
        }
        
        private bool CheckPermission()
        {
            if (_yamaPlayerController == null || _yamaPlayerController.Permission == null)
                return true; // 権限システムが設定されていない場合は許可
                
            PlayerPermission currentPermission = _yamaPlayerController.PlayerPermission;
            return (int)currentPermission >= (int)_minimumPermission;
        }
        
        private void PlayVideo()
        {
            _isProcessing = true;
            
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(true);
            
            PrintLog($"動画再生開始: {_videoUrl.Get()}");
            
            // 所有権を取得
            _yamaPlayerController.TakeOwnership();
            
            // Trackオブジェクトを作成
            Track track = Track.New(_videoPlayerType, _videoTitle, _videoUrl);
            
            if (_addToQueue)
            {
                // キューに追加
                AddToQueue(track);
            }
            else
            {
                // 直接再生
                PlayTrackDirectly(track);
            }
        }
        
        private void AddToQueue(Track track)
        {
            if (_yamaPlayerController.Queue == null)
            {
                PrintError("キューが利用できません");
                CompleteProcessing();
                return;
            }
            
            _yamaPlayerController.Queue.TakeOwnership();
            _yamaPlayerController.Queue.AddTrack(track);
            
            ShowMessage($"キューに追加しました: {track.GetTitle()}");
            PrintLog($"キューに追加: {track.GetUrl()}");
            
            CompleteProcessing();
        }
        
        private void PlayTrackDirectly(Track track)
        {
            try
            {
                // 現在の動画を停止してから新しい動画を再生
                if (!_yamaPlayerController.Stopped)
                {
                    _yamaPlayerController.Stopped = true;
                }
                
                // 動画を再生
                _yamaPlayerController.PlayTrack(track);
                
                ShowMessage(_successMessage);
                PrintLog($"動画再生成功: {track.GetUrl()}");
            }
            catch (System.Exception e)
            {
                PrintError($"動画再生エラー: {e.Message}");
                ShowMessage("動画再生に失敗しました");
            }
            
            CompleteProcessing();
        }
        
        private void CompleteProcessing()
        {
            _isProcessing = false;
            
            if (_loadingIndicator != null)
                _loadingIndicator.SetActive(false);
        }
        
        private void ShowMessage(string message)
        {
            // メッセージ表示の実装
            // ここではログ出力のみ行うが、実際の使用ではUI表示を追加
            PrintLog($"メッセージ: {message}");
        }
        
        // 外部から呼び出し可能なメソッド
        public void SetVideoUrl(VRCUrl url)
        {
            _videoUrl = url;
            PrintLog($"動画URLを設定: {url.Get()}");
        }
        
        public void SetVideoTitle(string title)
        {
            _videoTitle = title;
            PrintLog($"動画タイトルを設定: {title}");
        }
        
        public void SetVideoPlayerType(VideoPlayerType playerType)
        {
            _videoPlayerType = playerType;
            PrintLog($"ビデオプレイヤータイプを設定: {playerType}");
        }
        
        public void SetAddToQueue(bool addToQueue)
        {
            _addToQueue = addToQueue;
            PrintLog($"キュー追加設定を変更: {addToQueue}");
        }
        
        public void SetRequirePermission(bool requirePermission)
        {
            _requirePermission = requirePermission;
            PrintLog($"権限要求設定を変更: {requirePermission}");
        }
        
        public void SetMinimumPermission(PlayerPermission permission)
        {
            _minimumPermission = permission;
            PrintLog($"最小権限を設定: {permission}");
        }
        
        // デバッグ用メソッド
        private void PrintLog(object message)
        {
            if (_enableDebugLogs)
                Debug.Log($"[<color=#EF6291>YamaPlayerIntegration</color>] {message}");
        }
        
        private void PrintError(object message)
        {
            Debug.LogError($"[<color=#EF6291>YamaPlayerIntegration</color>] {message}");
        }
        
        // ネットワーク同期用のカスタムイベント
        public void OnVideoPlaySuccess()
        {
            // 動画再生成功時の処理
            PrintLog("動画再生が正常に開始されました");
        }
        
        public void OnVideoPlayError()
        {
            // 動画再生エラー時の処理
            PrintError("動画再生に失敗しました");
            CompleteProcessing();
        }
    }
}
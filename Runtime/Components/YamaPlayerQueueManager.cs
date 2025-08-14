using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components.Video;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YamaPlayerQueueManager : UdonSharpBehaviour
    {
        [Header("YamaPlayer Controller")]
        [SerializeField] Controller _controller;
        
        [Header("Queue Management")]
        [SerializeField] bool _autoAddToQueue = true;
        [SerializeField] bool _notifyOnAdd = true;
        
        [Header("URL Management")]
        [SerializeField] VRCUrl[] _predefinedUrls = new VRCUrl[] { };
        [SerializeField] bool _usePredefinedUrls = false;
        
        [Header("Event Triggers")]
        [SerializeField] bool _addOnVideoEnd = true;
        [SerializeField] bool _addOnVideoError = false;
        [SerializeField] bool _addOnVideoStop = false;
        
        private int _currentUrlIndex = 0;
        private bool _isInitialized = false;
        
        void Start()
        {
            if (_controller == null)
            {
                Debug.LogError("[YamaPlayerQueueManager] Controller is not assigned!");
                return;
            }
            
            // コントローラーにリスナーとして登録
            _controller.AddListener(this);
            _isInitialized = true;
            
            PrintLog("YamaPlayerQueueManager initialized successfully.");
        }
        
        #region Public Methods
        
        /// <summary>
        /// 手動でURLをキュートに追加
        /// </summary>
        /// <param name="url">追加するVRCUrl</param>
        public void AddUrlToQueue(VRCUrl url)
        {
            if (!_isInitialized || _controller == null) return;
            
            if (url.Get().IsValidUrl())
            {
                Track newTrack = Track.New(VideoPlayerType.AVProVideoPlayer, "", url);
                _controller.Queue.AddTrack(newTrack);
                
                if (_notifyOnAdd)
                {
                    PrintLog($"Added URL to queue: {url.Get()}");
                }
            }
            else
            {
                PrintError($"Invalid URL: {url.Get()}");
            }
        }
        
        /// <summary>
        /// 事前定義されたURLをキュートに追加
        /// </summary>
        public void AddPredefinedUrlToQueue()
        {
            if (!_usePredefinedUrls || _predefinedUrls.Length == 0) return;
            
            if (_currentUrlIndex < _predefinedUrls.Length)
            {
                AddUrlToQueue(_predefinedUrls[_currentUrlIndex]);
                _currentUrlIndex++;
            }
            else
            {
                // すべてのURLを追加したら最初から再開
                _currentUrlIndex = 0;
                if (_predefinedUrls.Length > 0)
                {
                    AddUrlToQueue(_predefinedUrls[_currentUrlIndex]);
                    _currentUrlIndex++;
                }
            }
        }
        
        /// <summary>
        /// キュートをクリア
        /// </summary>
        public void ClearQueue()
        {
            if (!_isInitialized || _controller == null) return;
            
            _controller.Queue.Clear();
            PrintLog("Queue cleared.");
        }
        
        /// <summary>
        /// キュートの内容を表示
        /// </summary>
        public void ShowQueueInfo()
        {
            if (!_isInitialized || _controller == null) return;
            
            int queueLength = _controller.Queue.Length;
            PrintLog($"Queue contains {queueLength} tracks.");
            
            for (int i = 0; i < queueLength; i++)
            {
                Track track = _controller.Queue.GetTrack(i);
                string title = track.HasTitle() ? track.GetTitle() : "No Title";
                string url = track.GetUrl();
                PrintLog($"  [{i}] {title} - {url}");
            }
        }
        
        /// <summary>
        /// 自動追加機能の切り替え
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetAutoAddToQueue(bool enabled)
        {
            _autoAddToQueue = enabled;
            PrintLog($"Auto add to queue: {_autoAddToQueue}");
        }
        
        #endregion
        
        #region YamaPlayer Event Handlers
        
        /// <summary>
        /// 動画終了時のイベント
        /// </summary>
        public override void OnVideoEnd()
        {
            if (_addOnVideoEnd && _autoAddToQueue)
            {
                AddPredefinedUrlToQueue();
            }
        }
        
        /// <summary>
        /// 動画エラー時のイベント
        /// </summary>
        /// <param name="videoError">エラー情報</param>
        public override void OnVideoError(VideoError videoError)
        {
            if (_addOnVideoError && _autoAddToQueue)
            {
                PrintLog($"Video error occurred: {videoError}. Adding next URL to queue.");
                AddPredefinedUrlToQueue();
            }
        }
        
        /// <summary>
        /// 動画停止時のイベント
        /// </summary>
        public override void OnVideoStop()
        {
            if (_addOnVideoStop && _autoAddToQueue)
            {
                AddPredefinedUrlToQueue();
            }
        }
        
        /// <summary>
        /// キュート更新時のイベント
        /// </summary>
        public override void OnQueueUpdated()
        {
            if (_notifyOnAdd)
            {
                PrintLog("Queue updated.");
            }
        }
        
        /// <summary>
        /// トラック更新時のイベント
        /// </summary>
        public override void OnTrackUpdated()
        {
            // トラックが更新された時の処理
            if (_notifyOnAdd)
            {
                Track currentTrack = _controller.Track;
                if (currentTrack.HasTitle())
                {
                    PrintLog($"Now playing: {currentTrack.GetTitle()}");
                }
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void PrintLog(object message)
        {
            Debug.Log($"[<color=#EF6291>YamaPlayerQueueManager</color>] {message}");
        }
        
        /// <summary>
        /// エラーログ出力
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        private void PrintError(object message)
        {
            Debug.LogError($"[<color=#EF6291>YamaPlayerQueueManager</color>] {message}");
        }
        
        #endregion
    }
}
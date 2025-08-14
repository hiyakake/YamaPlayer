using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components.Video;
using System;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YamaPlayerDynamicQueueManager : UdonSharpBehaviour
    {
        [Header("YamaPlayer Controller")]
        [SerializeField] Controller _controller;
        
        [Header("Dynamic URL Management")]
        [SerializeField, UdonSynced] VRCUrl[] _dynamicUrls = new VRCUrl[] { };
        [SerializeField] int _maxDynamicUrls = 50;
        
        [Header("Queue Behavior")]
        [SerializeField] bool _autoPlayNext = true;
        [SerializeField] bool _loopQueue = false;
        [SerializeField] bool _shuffleQueue = false;
        
        [Header("Event Settings")]
        [SerializeField] bool _addOnVideoEnd = true;
        [SerializeField] bool _addOnVideoError = true;
        [SerializeField] bool _addOnVideoStop = false;
        [SerializeField] bool _enableNotifications = true;
        
        [Header("Advanced Settings")]
        [SerializeField] float _queueCheckInterval = 1.0f;
        [SerializeField] bool _enableQueueValidation = true;
        
        private int _currentDynamicIndex = 0;
        private bool _isInitialized = false;
        private float _lastQueueCheck = 0f;
        private System.Random _random;
        
        void Start()
        {
            if (_controller == null)
            {
                Debug.LogError("[YamaPlayerDynamicQueueManager] Controller is not assigned!");
                return;
            }
            
            _controller.AddListener(this);
            _random = new System.Random();
            _isInitialized = true;
            
            PrintLog("YamaPlayerDynamicQueueManager initialized successfully.");
        }
        
        void Update()
        {
            if (!_isInitialized || _controller == null) return;
            
            // 定期的にキュートの状態をチェック
            if (Time.time - _lastQueueCheck > _queueCheckInterval)
            {
                CheckQueueStatus();
                _lastQueueCheck = Time.time;
            }
        }
        
        #region Public Methods - URL Management
        
        /// <summary>
        /// 動的URLを追加
        /// </summary>
        /// <param name="url">追加するVRCUrl</param>
        public void AddDynamicUrl(VRCUrl url)
        {
            if (!_isInitialized || !Networking.IsOwner(gameObject)) return;
            
            if (url.Get().IsValidUrl())
            {
                if (_dynamicUrls.Length < _maxDynamicUrls)
                {
                    _dynamicUrls = _dynamicUrls.Add(url);
                    RequestSerialization();
                    
                    if (_enableNotifications)
                    {
                        PrintLog($"Added dynamic URL: {url.Get()}");
                    }
                }
                else
                {
                    PrintError($"Maximum dynamic URLs reached ({_maxDynamicUrls})");
                }
            }
            else
            {
                PrintError($"Invalid URL: {url.Get()}");
            }
        }
        
        /// <summary>
        /// 複数の動的URLを一括追加
        /// </summary>
        /// <param name="urls">追加するVRCUrl配列</param>
        public void AddDynamicUrls(VRCUrl[] urls)
        {
            if (!_isInitialized || !Networking.IsOwner(gameObject)) return;
            
            foreach (VRCUrl url in urls)
            {
                AddDynamicUrl(url);
            }
        }
        
        /// <summary>
        /// 動的URLを削除
        /// </summary>
        /// <param name="index">削除するインデックス</param>
        public void RemoveDynamicUrl(int index)
        {
            if (!_isInitialized || !Networking.IsOwner(gameObject)) return;
            
            if (index >= 0 && index < _dynamicUrls.Length)
            {
                VRCUrl removedUrl = _dynamicUrls[index];
                _dynamicUrls = _dynamicUrls.RemoveAt(index);
                RequestSerialization();
                
                if (_enableNotifications)
                {
                    PrintLog($"Removed dynamic URL: {removedUrl.Get()}");
                }
            }
        }
        
        /// <summary>
        /// 動的URLをクリア
        /// </summary>
        public void ClearDynamicUrls()
        {
            if (!_isInitialized || !Networking.IsOwner(gameObject)) return;
            
            _dynamicUrls = new VRCUrl[] { };
            _currentDynamicIndex = 0;
            RequestSerialization();
            
            if (_enableNotifications)
            {
                PrintLog("All dynamic URLs cleared.");
            }
        }
        
        #endregion
        
        #region Public Methods - Queue Management
        
        /// <summary>
        /// 動的URLをキュートに追加
        /// </summary>
        public void AddDynamicUrlToQueue()
        {
            if (!_isInitialized || _controller == null || _dynamicUrls.Length == 0) return;
            
            if (_currentDynamicIndex < _dynamicUrls.Length)
            {
                VRCUrl url = _dynamicUrls[_currentDynamicIndex];
                AddUrlToQueue(url);
                _currentDynamicIndex++;
            }
            else if (_loopQueue)
            {
                _currentDynamicIndex = 0;
                if (_dynamicUrls.Length > 0)
                {
                    VRCUrl url = _dynamicUrls[_currentDynamicIndex];
                    AddUrlToQueue(url);
                    _currentDynamicIndex++;
                }
            }
        }
        
        /// <summary>
        /// ランダムな動的URLをキュートに追加
        /// </summary>
        public void AddRandomDynamicUrlToQueue()
        {
            if (!_isInitialized || _controller == null || _dynamicUrls.Length == 0) return;
            
            int randomIndex = _random.Next(0, _dynamicUrls.Length);
            VRCUrl url = _dynamicUrls[randomIndex];
            AddUrlToQueue(url);
        }
        
        /// <summary>
        /// URLをキュートに追加
        /// </summary>
        /// <param name="url">追加するVRCUrl</param>
        public void AddUrlToQueue(VRCUrl url)
        {
            if (!_isInitialized || _controller == null) return;
            
            if (url.Get().IsValidUrl())
            {
                Track newTrack = Track.New(VideoPlayerType.AVProVideoPlayer, "", url);
                _controller.Queue.AddTrack(newTrack);
                
                if (_enableNotifications)
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
        /// キュートをクリア
        /// </summary>
        public void ClearQueue()
        {
            if (!_isInitialized || _controller == null) return;
            
            _controller.Queue.Clear();
            
            if (_enableNotifications)
            {
                PrintLog("Queue cleared.");
            }
        }
        
        /// <summary>
        /// キュートの情報を表示
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
        /// 動的URLの情報を表示
        /// </summary>
        public void ShowDynamicUrlsInfo()
        {
            if (!_isInitialized) return;
            
            PrintLog($"Dynamic URLs ({_dynamicUrls.Length}/{_maxDynamicUrls}):");
            for (int i = 0; i < _dynamicUrls.Length; i++)
            {
                string url = _dynamicUrls[i].Get();
                string status = i == _currentDynamicIndex ? " [Current]" : "";
                PrintLog($"  [{i}]{status} {url}");
            }
        }
        
        #endregion
        
        #region Public Methods - Settings
        
        /// <summary>
        /// 自動再生設定を変更
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetAutoPlayNext(bool enabled)
        {
            _autoPlayNext = enabled;
            PrintLog($"Auto play next: {_autoPlayNext}");
        }
        
        /// <summary>
        /// ループ設定を変更
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetLoopQueue(bool enabled)
        {
            _loopQueue = enabled;
            PrintLog($"Loop queue: {_loopQueue}");
        }
        
        /// <summary>
        /// シャッフル設定を変更
        /// </summary>
        /// <param name="enabled">有効にするかどうか</param>
        public void SetShuffleQueue(bool enabled)
        {
            _shuffleQueue = enabled;
            PrintLog($"Shuffle queue: {_shuffleQueue}");
        }
        
        #endregion
        
        #region YamaPlayer Event Handlers
        
        /// <summary>
        /// 動画終了時のイベント
        /// </summary>
        public override void OnVideoEnd()
        {
            if (_addOnVideoEnd)
            {
                if (_shuffleQueue)
                {
                    AddRandomDynamicUrlToQueue();
                }
                else
                {
                    AddDynamicUrlToQueue();
                }
                
                if (_autoPlayNext && _controller.Queue.Length > 0)
                {
                    _controller.Forward();
                }
            }
        }
        
        /// <summary>
        /// 動画エラー時のイベント
        /// </summary>
        /// <param name="videoError">エラー情報</param>
        public override void OnVideoError(VideoError videoError)
        {
            if (_addOnVideoError)
            {
                PrintLog($"Video error occurred: {videoError}. Adding next URL to queue.");
                
                if (_shuffleQueue)
                {
                    AddRandomDynamicUrlToQueue();
                }
                else
                {
                    AddDynamicUrlToQueue();
                }
            }
        }
        
        /// <summary>
        /// 動画停止時のイベント
        /// </summary>
        public override void OnVideoStop()
        {
            if (_addOnVideoStop)
            {
                if (_shuffleQueue)
                {
                    AddRandomDynamicUrlToQueue();
                }
                else
                {
                    AddDynamicUrlToQueue();
                }
            }
        }
        
        /// <summary>
        /// キュート更新時のイベント
        /// </summary>
        public override void OnQueueUpdated()
        {
            if (_enableNotifications)
            {
                PrintLog("Queue updated.");
            }
        }
        
        /// <summary>
        /// トラック更新時のイベント
        /// </summary>
        public override void OnTrackUpdated()
        {
            if (_enableNotifications)
            {
                Track currentTrack = _controller.Track;
                if (currentTrack.HasTitle())
                {
                    PrintLog($"Now playing: {currentTrack.GetTitle()}");
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// キュートの状態をチェック
        /// </summary>
        private void CheckQueueStatus()
        {
            if (!_enableQueueValidation || _controller == null) return;
            
            // キュートが空で、動的URLがある場合は追加
            if (_controller.Queue.Length == 0 && _dynamicUrls.Length > 0)
            {
                if (_shuffleQueue)
                {
                    AddRandomDynamicUrlToQueue();
                }
                else
                {
                    AddDynamicUrlToQueue();
                }
            }
        }
        
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void PrintLog(object message)
        {
            Debug.Log($"[<color=#EF6291>YamaPlayerDynamicQueueManager</color>] {message}");
        }
        
        /// <summary>
        /// エラーログ出力
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        private void PrintError(object message)
        {
            Debug.LogError($"[<color=#EF6291>YamaPlayerDynamicQueueManager</color>] {message}");
        }
        
        #endregion
    }
}
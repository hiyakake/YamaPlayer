using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PredefinedUrlQueueAdder : Listener
    {
        [Header("Player References")]
        [SerializeField] Controller _controller;

        [Header("Predefined URL (set in Inspector)")]
        [SerializeField] VRCUrl _predefinedUrl = default;
        [SerializeField] string _title = "";

        [Header("Player Type")]
        [SerializeField] bool _useCurrentPlayerType = true;
        [SerializeField] VideoPlayerType _playerType = VideoPlayerType.AVProVideoPlayer;

        [Header("Behavior")]
        [SerializeField] bool _enqueueOnStart = false;

        public void Start()
        {
            if (_controller != null) _controller.AddListener(this);
            if (_enqueueOnStart) EnqueuePredefinedUrl();
        }

        public void EnqueuePredefinedUrl()
        {
            if (_controller == null) return;
            if (!_predefinedUrl.IsValid()) return;

            var resolvedPlayerType = _useCurrentPlayerType ? _controller.VideoPlayerType : _playerType;

            _controller.Queue.TakeOwnership();
            _controller.Queue.AddTrack(Track.New(resolvedPlayerType, _title == null ? string.Empty : _title, _predefinedUrl));
        }

        public void SetUrlAndEnqueue(VRCUrl url)
        {
            _predefinedUrl = url;
            EnqueuePredefinedUrl();
        }
    }
}
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Yamadev.YamaStream;

namespace Yamadev.YamaStream.Components
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistVisibilityController : UdonSharpBehaviour
    {
        [Header("Settings")]
        [SerializeField] Controller _controller;
        [SerializeField] string _targetPlaylistName = "";
        [SerializeField] int _targetPlaylistIndex = -1;

        [Header("Events")]
        [SerializeField] bool _enableCustomEvents = true;
        [SerializeField] string _hidePlaylistEventName = "HidePlaylist";
        [SerializeField] string _showPlaylistEventName = "ShowPlaylist";
        [SerializeField] string _togglePlaylistEventName = "TogglePlaylist";
        [SerializeField] string _hideAllPlaylistsEventName = "HideAllPlaylists";
        [SerializeField] string _showAllPlaylistsEventName = "ShowAllPlaylists";

        private void Start()
        {
            if (_controller == null)
            {
                _controller = GetComponentInParent<Controller>();
            }
        }

        /// <summary>
        /// 指定された名前のプレイリストを非表示にします
        /// </summary>
        /// <param name="playlistName">非表示にするプレイリストの名前</param>
        public void HidePlaylist(string playlistName)
        {
            if (_controller != null)
            {
                _controller.HidePlaylist(playlistName);
            }
        }

        /// <summary>
        /// 設定された名前のプレイリストを非表示にします
        /// </summary>
        public void HidePlaylist()
        {
            if (!string.IsNullOrEmpty(_targetPlaylistName))
            {
                HidePlaylist(_targetPlaylistName);
            }
        }

        /// <summary>
        /// 指定された名前のプレイリストを表示します
        /// </summary>
        /// <param name="playlistName">表示するプレイリストの名前</param>
        public void ShowPlaylist(string playlistName)
        {
            if (_controller != null)
            {
                _controller.ShowPlaylist(playlistName);
            }
        }

        /// <summary>
        /// 設定された名前のプレイリストを表示します
        /// </summary>
        public void ShowPlaylist()
        {
            if (!string.IsNullOrEmpty(_targetPlaylistName))
            {
                ShowPlaylist(_targetPlaylistName);
            }
        }

        /// <summary>
        /// 指定された名前のプレイリストの表示状態を切り替えます
        /// </summary>
        /// <param name="playlistName">切り替えるプレイリストの名前</param>
        public void TogglePlaylist(string playlistName)
        {
            if (_controller != null)
            {
                var playlists = _controller.Playlists;
                for (int i = 0; i < playlists.Length; i++)
                {
                    if (playlists[i].PlaylistName == playlistName)
                    {
                        bool currentVisibility = playlists[i].IsVisible;
                        _controller.SetPlaylistVisibility(playlistName, !currentVisibility);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 設定された名前のプレイリストの表示状態を切り替えます
        /// </summary>
        public void TogglePlaylist()
        {
            if (!string.IsNullOrEmpty(_targetPlaylistName))
            {
                TogglePlaylist(_targetPlaylistName);
            }
        }

        /// <summary>
        /// 指定されたインデックスのプレイリストを非表示にします
        /// </summary>
        /// <param name="index">非表示にするプレイリストのインデックス</param>
        public void HidePlaylistByIndex(int index)
        {
            if (_controller != null)
            {
                _controller.SetPlaylistVisibilityByIndex(index, false);
            }
        }

        /// <summary>
        /// 設定されたインデックスのプレイリストを非表示にします
        /// </summary>
        public void HidePlaylistByIndex()
        {
            if (_targetPlaylistIndex >= 0)
            {
                HidePlaylistByIndex(_targetPlaylistIndex);
            }
        }

        /// <summary>
        /// 指定されたインデックスのプレイリストを表示します
        /// </summary>
        /// <param name="index">表示するプレイリストのインデックス</param>
        public void ShowPlaylistByIndex(int index)
        {
            if (_controller != null)
            {
                _controller.SetPlaylistVisibilityByIndex(index, true);
            }
        }

        /// <summary>
        /// 設定されたインデックスのプレイリストを表示します
        /// </summary>
        public void ShowPlaylistByIndex()
        {
            if (_targetPlaylistIndex >= 0)
            {
                ShowPlaylistByIndex(_targetPlaylistIndex);
            }
        }

        /// <summary>
        /// 指定されたインデックスのプレイリストの表示状態を切り替えます
        /// </summary>
        /// <param name="index">切り替えるプレイリストのインデックス</param>
        public void TogglePlaylistByIndex(int index)
        {
            if (_controller != null)
            {
                var playlists = _controller.Playlists;
                if (index >= 0 && index < playlists.Length)
                {
                    bool currentVisibility = playlists[index].IsVisible;
                    _controller.SetPlaylistVisibilityByIndex(index, !currentVisibility);
                }
            }
        }

        /// <summary>
        /// 設定されたインデックスのプレイリストの表示状態を切り替えます
        /// </summary>
        public void TogglePlaylistByIndex()
        {
            if (_targetPlaylistIndex >= 0)
            {
                TogglePlaylistByIndex(_targetPlaylistIndex);
            }
        }

        /// <summary>
        /// すべてのプレイリストを非表示にします
        /// </summary>
        public void HideAllPlaylists()
        {
            if (_controller != null)
            {
                _controller.HideAllPlaylists();
            }
        }

        /// <summary>
        /// すべてのプレイリストを表示します
        /// </summary>
        public void ShowAllPlaylists()
        {
            if (_controller != null)
            {
                _controller.ShowAllPlaylists();
            }
        }

        /// <summary>
        /// カスタムイベント名でプレイリストを非表示にします
        /// </summary>
        public void CustomHidePlaylist()
        {
            if (_enableCustomEvents && !string.IsNullOrEmpty(_hidePlaylistEventName))
            {
                SendCustomEvent(_hidePlaylistEventName);
            }
        }

        /// <summary>
        /// カスタムイベント名でプレイリストを表示します
        /// </summary>
        public void CustomShowPlaylist()
        {
            if (_enableCustomEvents && !string.IsNullOrEmpty(_showPlaylistEventName))
            {
                SendCustomEvent(_showPlaylistEventName);
            }
        }

        /// <summary>
        /// カスタムイベント名でプレイリストの表示状態を切り替えます
        /// </summary>
        public void CustomTogglePlaylist()
        {
            if (_enableCustomEvents && !string.IsNullOrEmpty(_togglePlaylistEventName))
            {
                SendCustomEvent(_togglePlaylistEventName);
            }
        }

        /// <summary>
        /// カスタムイベント名ですべてのプレイリストを非表示にします
        /// </summary>
        public void CustomHideAllPlaylists()
        {
            if (_enableCustomEvents && !string.IsNullOrEmpty(_hideAllPlaylistsEventName))
            {
                SendCustomEvent(_hideAllPlaylistsEventName);
            }
        }

        /// <summary>
        /// カスタムイベント名ですべてのプレイリストを表示します
        /// </summary>
        public void CustomShowAllPlaylists()
        {
            if (_enableCustomEvents && !string.IsNullOrEmpty(_showAllPlaylistsEventName))
            {
                SendCustomEvent(_showAllPlaylistsEventName);
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ValintaMusicStreaming
{
    public class VPlayerController : MonoBehaviour
    {
        public static VPlayerController Instance;

        private VCatalogue m_catalogue;
        private VMusicPlayback m_musicPlayback;

        private List<VPlayerUI> m_registeredPlayerUIs;
        private VPlayerState m_currentState;

        private VPlaylist m_currentPlaylist;
        private VSong m_currentSong;

        private int m_lastPlayedList;

        private int m_songCounter;
        private bool m_playAdNext;

        private int m_timeCounter;
        private float m_playerSessionStartTime;
        private bool m_playerSessionActive;

        void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            m_registeredPlayerUIs = new List<VPlayerUI>();
            m_currentState = new VPlayerState();
            m_songCounter = 0;
            m_playerSessionActive = false;

            m_musicPlayback = gameObject.AddComponent<VMusicPlayback>();
        }

        void Start()
        {
            AddMusicPlaybackListeners();

            ShowPlayerInitializingInUI();

            m_timeCounter = 0;
        }

        void Update()
        {
            if (!m_playerSessionActive) return;
            if (VSettings.DataBundleFrequency == 0) return;

            if((Time.realtimeSinceStartup - m_playerSessionStartTime) >= VSettings.DataBundleFrequency)
            {
                VAnalytics.Instance.SendSessionDuration(++m_timeCounter);
                m_playerSessionStartTime = Time.realtimeSinceStartup;
            }
        }

        // Called when catalogue is downloaded and parsed
        public void CatalogueReady()
        {
            m_currentState.CatalogueReady = true;
            m_catalogue = ValintaPlayer.Instance.GetCatalogueInstance();
            LoadLastPlayedPlaylist();

            UpdatePlayerUI();
        }

        #region Attach/Detach listeners

        private void AddMusicPlaybackListeners()
        {
            m_musicPlayback.OnPlaybackError += OnPlaybackError;
            m_musicPlayback.OnPlaybackLoading += OnPlaybackLoading;
            m_musicPlayback.OnPlaybackStopped += OnPlaybackStopped;
            m_musicPlayback.OnPlaybackTimedOut += OnPlaybackTimedOut;
            m_musicPlayback.OnPlaybackStarted += OnPlaybackStarted;
            m_musicPlayback.OnPlaybackGetNext += OnPlaybackGetNext;
        }

        private void RemoveMusicPlaybackListeners()
        {
            m_musicPlayback.OnPlaybackError -= OnPlaybackError;
            m_musicPlayback.OnPlaybackLoading -= OnPlaybackLoading;
            m_musicPlayback.OnPlaybackStopped -= OnPlaybackStopped;
            m_musicPlayback.OnPlaybackTimedOut -= OnPlaybackTimedOut;
            m_musicPlayback.OnPlaybackStarted -= OnPlaybackStarted;
            m_musicPlayback.OnPlaybackGetNext -= OnPlaybackGetNext;
        }

        #endregion


        #region UI handling

        /// <summary>
        /// Add UI script to registered players.
        /// </summary>
        /// <param name="player"></param>
        public void RegisterPlayerUI(VPlayerUI player)
        {
            if (m_registeredPlayerUIs.Contains(player)) return;

            m_registeredPlayerUIs.Add(player);

            UpdatePlayerUI();
        }

        /// <summary>
        /// Remove UI script from registered players.
        /// </summary>
        /// <param name="player"></param>
        public void DeregisterPlayerUI(VPlayerUI player)
        {
            if (!m_registeredPlayerUIs.Contains(player)) return;

            m_registeredPlayerUIs.Remove(player);
        }

        /// <summary>
        /// Send updates to all registered UIs
        /// </summary>
        public void UpdatePlayerUI()
        {
            foreach (VPlayerUI p in m_registeredPlayerUIs)
            {
                p.UpdateState(m_currentState);
            }
        }

        /// <summary>
        /// Show error situations in UI
        /// </summary>
        /// <param name="s"></param>
        /// <param name="retryable"></param>
        public void ShowErrorInUI(string s, bool retryable)
        {
            m_currentState.PlayerError(s, retryable);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Announce that player is ready to use
        /// </summary>
        public void ShowPlayerReadyInUI()
        {
            m_currentState.PlayerReady(VStrings.ValintaPlayer);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Announce that player is starting up
        /// </summary>
        private void ShowPlayerInitializingInUI()
        {
            m_currentState.PlayerInitializing(VStrings.PlayerInitializing);
            UpdatePlayerUI();
        }

        #endregion

       
        #region Saving/Loading playlist to/from PlayerPrefs

        /// <summary>
        /// Save user's playlist selection to Player Prefs.
        /// </summary>
        /// <param name="playlist"></param>
        private void SaveLastPlayedPlaylist(VPlaylist playlist)
        {
            PlayerPrefs.SetInt(VStrings.LAST_PLAYED, playlist.Id);
        }

        /// <summary>
        /// Load playlist from Player Prefs. NOTE: Playlist order may change in backend so this might not be accurate. 
        /// </summary>
        private void LoadLastPlayedPlaylist()
        {
            int id = PlayerPrefs.GetInt(VStrings.LAST_PLAYED);

            foreach (VPlaylist v in m_catalogue.GetAllPlaylists())
            {
                if (v.Id == id)
                {
                    m_currentPlaylist = v;
                    return;
                }
            }

            // If not found, get first
            m_currentPlaylist = m_catalogue.GetPlaylistByIndex(0);
        }

        #endregion


        #region Playback handling

        /// <summary>
        /// Handle play logic
        /// </summary>
        public void Play()
        {
            // If player is in error state, try to authorize the player again
            if (m_currentState.IsError)
            {
                TryRelogin();
                return;
            }

            // No network connection, retryable error
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ShowErrorInUI(VStrings.InternetConnectionError, true);
                return;
            }

            if (!m_playerSessionActive)
            {
                m_playerSessionStartTime = Time.realtimeSinceStartup;
                m_playerSessionActive = true;
            }

            // Save the playlist
            if (m_currentPlaylist != null)
            {
                SaveLastPlayedPlaylist(m_currentPlaylist);
            }

            // If there are no catalogue or playlists, show error. Retryable.
            if (m_catalogue == null || m_catalogue.GetAllPlaylists().Count <= 0)
            {
                ShowErrorInUI(VStrings.ErrorPlaylists, true);
                return;
            }

            
            if (m_currentState.IsPaused &&
                m_currentSong != null &&
                m_currentSong.Playlist == m_currentPlaylist.Id)
            {
                // There was a song playing, resume playback
                m_musicPlayback.Resume();
            }
            else
            {
                // No playlist specified (play pressed when player is in stand-by)
                if (m_currentPlaylist == null)
                {
                    if (m_catalogue.GetAllPlaylists().Count > 0)
                    {
                        m_currentPlaylist = m_catalogue.GetPlaylistByIndex(0);
                    }
                    else
                    {
                        // Should not go here but handling error just in case
                        ShowErrorInUI(VStrings.ErrorPlaylists, true);
                        return;
                    }
                }

                // Get next song and start playback
                m_currentSong = m_currentPlaylist.GetNextSong();
                m_musicPlayback.Play(m_currentSong);

                // Keep track on played songs to get ads played between songs
                m_songCounter++;
                // If ad frequency is not set, it won't go here at all = mid roll ads are not played
                if (m_songCounter >= VSettings.AdFrequency && VSettings.AdFrequency > 0)
                {
                    m_playAdNext = true;
                }
            }
        }

        /// <summary>
        /// Start playing the selected playlist.
        /// </summary>
        /// <param name="playlist"></param>
        public void Play(VPlaylist playlist)
        {
            m_currentPlaylist = playlist;

            Play();
        }

        /// <summary>
        /// Pause playback.
        /// </summary>
        public void Pause()
        {
            m_musicPlayback.Pause();
            m_currentState.MusicPlaybackPause();
            UpdatePlayerUI();
        }

        /// <summary>
        /// Stop playback. 
        /// </summary>
        public void Stop()
        {
            SetAdBannerActive(false);
            m_musicPlayback.StopPlayback();

            m_currentState.MusicPlaybackStop(VStrings.ValintaPlayer);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Opens URL in default browser.
        /// </summary>
        public void OpenURL()
        {
            string site = string.Empty;

            // Open music provider's landing page
            if (m_currentSong != null)
            {
                site = m_currentSong.Site;
            }

            // If player is in stand-by and shows valinta player text
            if (m_currentState.StatusText.Equals(VStrings.ValintaPlayer))
            {
                site = "http://www.zemeho.com";
            }

            if (!string.IsNullOrEmpty(site))
                Application.OpenURL(site);
        }

        private void TryRelogin()
        {
            ValintaPlayer.Instance.RetryLogin();
            m_currentState.RetryingLogin(VStrings.Retrying);
        }

        #endregion


        #region Music playback events

        /// <summary>
        /// Playback has started. Update UI with song info.
        /// </summary>
        private void OnPlaybackStarted()
        {
            m_currentState.PlayingMusic(string.Concat(m_currentPlaylist.Name, ": ", m_currentSong.Title, " by ", m_currentSong.Artist));
            UpdatePlayerUI();
        }

        /// <summary>
        /// Song has ended. Start next song.
        /// </summary>
        private void OnPlaybackGetNext()
        {
            VAnalytics.Instance.SendSongState(m_currentSong, false);
            Play();
        }

        /// <summary>
        /// Music playback has stopped.
        /// </summary>
        private void OnPlaybackStopped()
        {
            m_currentState.MusicPlaybackStop(string.Empty);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Loading song.
        /// </summary>
        private void OnPlaybackLoading()
        {
            m_currentState.PlaybackLoading(VStrings.Loading);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Error has occurred.
        /// </summary>
        /// <param name="s"></param>
        private void OnPlaybackError(string s)
        {
            m_currentState.PlaybackError(VStrings.InternetConnectionError + ": " + s);
            UpdatePlayerUI();
        }

        /// <summary>
        /// Loading has timed out.
        /// </summary>
        private void OnPlaybackTimedOut()
        {
            m_currentState.PlaybackError(VStrings.TimedOut);
            UpdatePlayerUI();
        }

        #endregion

        #region Ad banner

        /// <summary>
        /// Set texture for ad banner.
        /// </summary>
        /// <param name="tex"></param>
        public void SetTextureForAdBanner(Texture2D tex)
        {
            foreach (VPlayerUI p in m_registeredPlayerUIs)
            {
                p.AssignTextureToBanner(tex);
            }
        }

        /// <summary>
        /// Set URL which is opened when ad banner is clicked.
        /// </summary>
        /// <param name="url"></param>
        public void SetURLForBannerClick(string url)
        {
            foreach (VPlayerUI p in m_registeredPlayerUIs)
            {
                p.SetBannerClickUrl(url);
            }
        }

        /// <summary>
        /// Activate/Deactivate banner object.
        /// </summary>
        /// <param name="enable"></param>
        public void SetAdBannerActive(bool enable)
        {
            foreach (VPlayerUI p in m_registeredPlayerUIs)
            {
                p.ShowAdBanner(enable);
            }
        }

        public void SetSplashActive(bool enable)
        {
            foreach (VPlayerUI p in m_registeredPlayerUIs)
            {
                p.ShowSplashWindow();
            }
        }

        #endregion

        // If player states are needed in some other scripts, examples below.

        public bool IsPaused()
        {
            return m_currentState.IsPaused;
        }

        public bool IsStopped()
        {
            return m_currentState.IsStopped;
        }

        public bool IsInteractable()
        {
            return m_currentState.ButtonsEnabled;
        }
    }
}

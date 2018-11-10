using UnityEngine;
using System.Collections;

namespace ValintaMusicStreaming
{
    public class VMusicPlayback : MonoBehaviour
    {
        public delegate void PlaybackError(string s);
        public event PlaybackError OnPlaybackError;

        public delegate void PlaybackStarted();
        public event PlaybackStarted OnPlaybackStarted;

        public delegate void PlaybackGetNext();
        public event PlaybackStarted OnPlaybackGetNext;

        public delegate void PlaybackStopped();
        public event PlaybackStopped OnPlaybackStopped;

        public delegate void PlaybackLoading();
        public event PlaybackLoading OnPlaybackLoading;

        public delegate void PlaybackTimedOut();
        public event PlaybackTimedOut OnPlaybackTimedOut;

        private AudioSource m_audioSource;
        private VSong m_currentSong;

        private bool m_isSongSourceLoaded = false;

        private enum PlayerState
        {
            Standby,
            Playing
        }
        private PlayerState m_currentState;

        void Start()
        {
            if (GetComponent<AudioSource>() == null)
            {
                m_audioSource = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                m_audioSource = GetComponent<AudioSource>();
            }

            ChangeState(PlayerState.Standby);
        }

        void Update()
        {
            if (m_currentState == PlayerState.Playing)
            {
                if (!m_audioSource.isPlaying)
                {
                    OnGetNext();
                }
            }
        }

        private void ChangeState(PlayerState newState)
        {
            m_currentState = newState;
        }


        #region Playback preparations

        private IEnumerator PrepareSong()
        {
            if(m_currentSong == null)
            {
                OnError("No song specified");
                yield break;
            }

            OnLoading();

            DisposeCurrentAudio();

            // m_audioSource.clip = clip;
            m_audioSource.Play();

            OnStarted();
        }

        /// <summary>
        /// Dispose previous audio
        /// </summary>
        private void DisposeCurrentAudio()
        {
            if (m_audioSource.clip != null)
            {
                m_audioSource.Stop();
                Destroy(m_audioSource.clip);
                m_audioSource.clip = null;
            }
        }

        #endregion


        #region Playback controls

        /// <summary>
        /// Get song from player controller, prepare and play it.
        /// </summary>
        /// <param name="song"></param>
        public void Play(VSong song)
        {
            StopPlayback();

            m_currentSong = song;
            StartCoroutine(PrepareSong());
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            if (!m_audioSource.isPlaying) return;

            ChangeState(PlayerState.Standby);
            m_audioSource.Pause();
        }

        /// <summary>
        /// Resume playback
        /// </summary>
        public void Resume()
        {
            if (m_currentSong == null || m_audioSource.clip == null) return;
            if (m_audioSource.isPlaying) return;

            ChangeState(PlayerState.Playing);
            m_audioSource.Play();
            OnStarted();
        }

        /// <summary>
        /// Stop playback
        /// </summary>
        public void StopPlayback()
        {
            OnStopped();
            m_audioSource.Stop();
        }

        #endregion


        #region Event handling

        /// <summary>
        /// Playback started
        /// </summary>
        private void OnStarted()
        {
            if (OnPlaybackStarted != null)
                OnPlaybackStarted();

            ChangeState(PlayerState.Playing);
        }

        /// <summary>
        /// Playback stopped, get next automatically
        /// </summary>
        private void OnGetNext()
        {
            if (OnPlaybackGetNext != null)
                OnPlaybackGetNext();

            ChangeState(PlayerState.Standby);
        }

        /// <summary>
        /// Playback loading
        /// </summary>
        private void OnLoading()
        {
            if (OnPlaybackLoading != null)
                OnPlaybackLoading();

            ChangeState(PlayerState.Standby);
        }

        /// <summary>
        /// Playback stopped
        /// </summary>
        private void OnStopped()
        {
            if (OnPlaybackStopped != null)
                OnPlaybackStopped();

            ChangeState(PlayerState.Standby);
        }

        /// <summary>
        /// Error occurred
        /// </summary>
        /// <param name="s"></param>
        private void OnError(string s)
        {
            if (OnPlaybackError != null)
                OnPlaybackError(s);

            ChangeState(PlayerState.Standby);
        }

        /// <summary>
        /// Loading timed out
        /// </summary>
        private void OnTimedOut()
        {
            if (OnPlaybackTimedOut != null)
                OnPlaybackTimedOut();

            ChangeState(PlayerState.Standby);
        }

        #endregion
    }
}

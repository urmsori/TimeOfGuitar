using UnityEngine;
using System.Collections;
using System;

namespace ValintaMusicStreaming
{
    /// <summary>
    /// Valinta Player starting point
    /// </summary>
    public class ValintaPlayer : MonoBehaviour
    {
        public static ValintaPlayer Instance;

        [SerializeField]
        public string ValintaApplicationID;

        [SerializeField]
        public bool UseWWWForAudioDownload;

        private VCatalogue m_catalogue;

        void Awake()
        {
            // Prevents duplicate object instantiation when scene is changed
            if (FindObjectsOfType<ValintaPlayer>().Length > 1)
            {
                Destroy(gameObject);
            }

            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            if (Instance != null)
                return;

            Instance = this;

            InitComponents();
        }

        /// <summary>
        /// Add basic player components; network, catalogue, session, client and analytics
        /// </summary>
        void InitComponents()
        {
            gameObject.AddComponent<VPlayerController>();
            if (m_catalogue == null) m_catalogue = gameObject.AddComponent<VCatalogue>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="error"></param>
        private void OnSessionReady(bool isSuccess, string error)
        {
            VPlayerController.Instance.ShowPlayerReadyInUI();
        }

        /// <summary>
        /// Get instance of catalogue component
        /// </summary>
        /// <returns>VCatalogue instance</returns>
        public VCatalogue GetCatalogueInstance()
        {
            return m_catalogue;
        }
    }
}

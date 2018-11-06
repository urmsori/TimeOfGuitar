using UnityEngine;
using System.Collections;

namespace ValintaMusicStreaming
{
    public static class VStrings
    {
        public const string APIV3 = "https://valinta.zemeho.com/api/v3/";
        public const string CATALOGUE = APIV3 + "catalog";
        public const string ANALYTICS_SESSION = APIV3 + "analytics/sessions";
        public const string ANALYTICS_PLAYBACK = APIV3 + "analytics/songs/playback";
        public const string APIV2 = "https://valinta.zemeho.com/api/v2/";

        public const string ValintaPlayer = "Valinta music streaming";
        public const string ChooseGenre = "Choose playlist or press play";

        public const string PlayerInitializing = "Player initializing";

        public const string Authorizing = "Authorizing player";
        public const string AuthorizationFailed = "Authorization failed";
        public const string AuthorizationSuccess = "Authorization success";

        public const string Loading = "Loading...";
        public const string Paused = "Paused";
        public const string TimedOut = "Timed out";
        public const string InternetConnectionError = "No internet connection";
        public const string ErrorPlaylists = "Playlists could not be fetched";
        public const string Retrying = "Retrying...";

        public static readonly string LAST_PLAYED = "VALINTA_LAST_PLAYED";
    }
}

using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace BabysFirst
{
    /// <summary>
    /// Player identity, invisible to the player. Initializes Unity Gaming Services and signs in
    /// anonymously at boot, without blocking anything. Anonymous identity is device-local:
    /// it persists on this device while the player stays signed in.
    /// </summary>
    public static class ServicesBootstrap
    {
        public static bool IsReady =>
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        public static string PlayerId => IsReady ? AuthenticationService.Instance.PlayerId : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Kickoff()
        {
            _ = InitAsync();
        }

        static async Task InitAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[Services] Signed in anonymously as {AuthenticationService.Instance.PlayerId}");
            }
            catch (System.Exception e)
            {
                // Offline or not linked: the game still runs, saves stay local.
                Debug.LogWarning($"[Services] Running without cloud identity: {e.Message}");
            }
        }
    }
}

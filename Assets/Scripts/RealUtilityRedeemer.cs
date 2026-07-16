using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Singleton bridge between the Unity game and the real-world energy accounting
/// layer (HulosChain / private utility API).
///
/// RedeemPowerCredits() is called by LumiReactorNFT after every generation
/// cycle. In production it will POST to the secured Express endpoint
/// POST /api/reactor/:telegramId/redeem-credits — the same server that runs
/// the MagiPlant Powered Telegram Mini App — so all credit records live in
/// one place.
///
/// The wallet parameter maps to the player's telegramId (not a raw crypto
/// address) so the web app can match the record to an existing player row.
/// </summary>
public class RealUtilityRedeemer : MonoBehaviour
{
    public static RealUtilityRedeemer Instance { get; private set; }

    [Header("API Configuration")]
    [Tooltip("Base URL of the MagiPlant Express API. Populated at build time via a ScriptableObject or env injection.")]
    public string apiBaseUrl = "https://api.magiminigames.com"; // override in Inspector / build pipeline

    [Tooltip("Shared secret injected at build time — never commit a real value here.")]
    public string apiSecret  = "REPLACE_AT_BUILD_TIME";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public surface ────────────────────────────────────────────────────────

    /// <summary>
    /// Record <paramref name="kwh"/> real power credits for <paramref name="wallet"/>
    /// (telegramId). Fire-and-forget coroutine so the game loop is never blocked.
    /// </summary>
    public void RedeemPowerCredits(float kwh, string wallet)
    {
        if (kwh <= 0f || string.IsNullOrEmpty(wallet)) return;

        Debug.Log($"[RealUtilityRedeemer] Queuing {kwh:F3} kWh credit for wallet {wallet}");
        StartCoroutine(PostCreditToApi(kwh, wallet));
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private IEnumerator PostCreditToApi(float kwh, string telegramId)
    {
        string url  = $"{apiBaseUrl.TrimEnd('/')}/api/reactor/{telegramId}/redeem-credits";
        string body = $"{{\"kwhEarned\":{kwh:F4}}}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("X-Api-Secret", apiSecret); // rotated per environment

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[RealUtilityRedeemer] {kwh:F3} kWh credited. Response: {req.downloadHandler.text}");
        }
        else
        {
            Debug.LogWarning($"[RealUtilityRedeemer] Credit POST failed ({req.responseCode}): {req.error}");
            // TODO: queue for retry via local PlayerPrefs cache
        }
    }
}

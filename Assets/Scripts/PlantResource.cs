using UnityEngine;

/// <summary>
/// Represents a single bioluminescent plant in the MagiPlant ecosystem.
/// Growth is health-weighted; harvest resets the plant for the next cycle.
/// Corresponds to the web app's flora / plants tables and their tend/harvest routes.
/// </summary>
public class PlantResource : MonoBehaviour
{
    [Header("Plant Identity")]
    public string plantType = "Glowleaf"; // Glowleaf | Bloomveil | Monolith

    [Header("Growth State")]
    public float growthProgress = 0f;    // 0–100
    public bool  isGrown       = false;

    [Header("Vitals")]
    public float energyYield = 5f;       // base kWh-equivalent per harvest
    public bool  isHealthy   = true;     // unhealthy plants grow and yield at 50 / 80 %

    // ── Web-app parity fields ─────────────────────────────────────────────────
    // These mirror the columns in magicalFloraTable so Unity state can be
    // serialised / synced to the Express API without a schema change.
    [HideInInspector] public string telegramId   = "";   // owning player
    [HideInInspector] public int    floraId      = -1;   // DB row id
    [HideInInspector] public int    growthStage  = 1;    // 1–5 (maps to growthProgress bands)
    [HideInInspector] public float  health       = 100f; // 0–100

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Advance growth by <paramref name="amount"/> points.
    /// Unhealthy plants receive only half the benefit.
    /// </summary>
    public void Grow(float amount)
    {
        growthProgress += amount * (isHealthy ? 1f : 0.5f);
        if (growthProgress >= 100f)
        {
            growthProgress = 100f;
            isGrown = true;
        }

        // Keep growthStage in sync (bands: 0–19 → 1, 20–39 → 2, …, 80–100 → 5)
        growthStage = Mathf.Clamp(Mathf.FloorToInt(growthProgress / 20f) + 1, 1, 5);
    }

    /// <summary>
    /// Harvest the plant and return the energy yield.
    /// Returns 0 if not yet fully grown.
    /// </summary>
    public float Harvest()
    {
        if (!isGrown) return 0f;

        float yield = energyYield * (isHealthy ? 1.2f : 0.8f);

        // Reset for next cycle (mirrors web-app harvest reset logic)
        growthProgress = 0f;
        isGrown        = false;
        growthStage    = 1;
        health         = Mathf.Max(60f, health * 0.9f); // mild health decay on harvest

        return yield;
    }
}

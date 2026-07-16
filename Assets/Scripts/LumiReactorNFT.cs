using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NFT-tier power reactor that harvests connected PlantResource objects and
/// converts their yield into both in-game LUMI energy and real-world utility
/// credits (kWh).
///
/// Tier multipliers
///   Glowleaf  — base (1×)
///   Bloomveil — standard (1×, default)
///   Monolith  — premium real-power multiplier (2.5×)
///
/// Corresponds to the web app's powerPlantsTable and
/// POST /api/power-plant/:telegramId/produce + /convert routes.
/// </summary>
public class LumiReactorNFT : MonoBehaviour
{
    [Header("Reactor Identity")]
    public string reactorTier = "Bloomveil"; // Glowleaf | Bloomveil | Monolith

    [Header("Connected Plants")]
    public List<PlantResource> connectedPlants = new List<PlantResource>();

    [Header("Energy State")]
    public float currentEnergy          = 0f;   // accumulated in-game LUMI energy
    public float realPowerOutputPerDay  = 50f;  // real kWh ceiling per 24 h cycle
    public bool  isActive               = true;

    [Header("Equity")]
    [Tooltip("Fairness weighting applied to each plant's harvest yield (1.0 = neutral).")]
    public float fairnessEquityFactor = 1.0f;

    // ── Web-app parity fields ─────────────────────────────────────────────────
    [HideInInspector] public string telegramId     = "";    // owning player
    [HideInInspector] public int    reactorLevel   = 1;
    [HideInInspector] public float  efficiency     = 0.85f;
    [HideInInspector] public float  magiaBoost     = 1.0f;
    [HideInInspector] public float  totalConverted = 0f;
    // ─────────────────────────────────────────────────────────────────────────

    // LUMINARA base energy multiplier (mirrors web-app 1.5× magia default)
    private const float LuminaraMultiplier = 1.5f;

    /// <summary>
    /// Attach a plant to this reactor. Idempotent.
    /// </summary>
    public void AddPlant(PlantResource plant)
    {
        if (!connectedPlants.Contains(plant))
            connectedPlants.Add(plant);
    }

    /// <summary>
    /// Harvest all grown plants, accumulate LUMI energy, and trigger the
    /// real-world utility credit redemption.
    /// Returns the updated <see cref="currentEnergy"/> value.
    /// </summary>
    public float GenerateEnergyAndRealPower()
    {
        if (!isActive) return 0f;

        float harvestTotal = 0f;
        foreach (var plant in connectedPlants)
        {
            if (plant != null && plant.isGrown)
                harvestTotal += plant.Harvest() * fairnessEquityFactor;
        }

        // Apply LUMINARA multiplier → in-game energy
        currentEnergy += harvestTotal * LuminaraMultiplier;
        totalConverted += harvestTotal;

        // Real-world power calculation — Monolith tier gets a 2.5× premium
        float tierMultiplier = reactorTier == "Monolith" ? 2.5f : 1f;
        float realPower = Mathf.Min(
            currentEnergy * tierMultiplier * 0.1f,
            realPowerOutputPerDay
        );

        // Hand off to the utility bridge (telegramId used as wallet identifier)
        if (realPower > 0f)
            RealUtilityRedeemer.Instance.RedeemPowerCredits(realPower, telegramId);

        return currentEnergy;
    }

    /// <summary>
    /// Convert stored energy to LUMI tokens at the reactor's efficiency rate.
    /// Returns LUMI gained; deducts from currentEnergy.
    /// Mirrors POST /api/power-plant/:telegramId/convert.
    /// </summary>
    public float ConvertEnergyToLumi(float amount)
    {
        float toConvert = Mathf.Min(amount, currentEnergy);
        if (toConvert <= 0f) return 0f;

        float lumiGained = toConvert * efficiency;
        currentEnergy -= toConvert;
        return lumiGained;
    }
}

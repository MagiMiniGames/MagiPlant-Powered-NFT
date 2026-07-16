using UnityEngine;
using System.Collections;

/// <summary>
/// Scene-level orchestrator that ties together one or more LumiReactorNFT
/// objects and drives the production loop.
///
/// Drop this on a manager GameObject in your Unity scene. Assign reactors in
/// the Inspector. The manager calls GenerateEnergyAndRealPower() on a fixed
/// interval (default: once per in-game "day" = 300 real seconds in dev mode).
/// </summary>
public class ReactorManager : MonoBehaviour
{
    [Header("Reactors")]
    public LumiReactorNFT[] reactors;

    [Header("Cycle Timing")]
    [Tooltip("Seconds between production cycles. 300 s in dev; set to 86400 for a real 24 h cycle.")]
    public float cycleIntervalSeconds = 300f;

    [Header("Player")]
    public string telegramId = ""; // set from Telegram WebApp init data or login flow

    void Start()
    {
        // Propagate telegramId to all reactors so the utility bridge knows the owner
        foreach (var reactor in reactors)
            if (reactor != null) reactor.telegramId = telegramId;

        StartCoroutine(ProductionLoop());
    }

    private IEnumerator ProductionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(cycleIntervalSeconds);
            RunProductionCycle();
        }
    }

    /// <summary>
    /// Run one full production cycle across all active reactors.
    /// Can also be called directly for testing or from a UI button.
    /// </summary>
    public void RunProductionCycle()
    {
        foreach (var reactor in reactors)
        {
            if (reactor == null || !reactor.isActive) continue;
            float energy = reactor.GenerateEnergyAndRealPower();
            Debug.Log($"[ReactorManager] Reactor '{reactor.reactorTier}' produced. Energy now: {energy:F2}");
        }
    }
}

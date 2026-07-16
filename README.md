# MagiPlant Powered — NFT Unity Scripts

Real-world energy bridge for the **MagiPlant Powered** bioluminescent Play-to-Earn ecosystem.

This repository holds the Unity C# scripts that model the physical-world payoff layer of the game — the link between in-game plant tending and real utility-credit accounting.

---

## Repository structure

```
Assets/Scripts/
├── PlantResource.cs        — Individual plant: growth, health, harvest yield
├── LumiReactorNFT.cs       — NFT-tier reactor: harvests plants → LUMI + real kWh
├── RealUtilityRedeemer.cs  — Singleton API bridge → POST credits to Express server
└── ReactorManager.cs       — Scene orchestrator: runs the production loop
```

---

## How it fits the wider project

| Unity script | Web-app equivalent | API route |
|---|---|---|
| `PlantResource` | `magicalFloraTable` rows | `POST /api/flora/:id/tend`, `/harvest` |
| `LumiReactorNFT` | `powerPlantsTable` row | `POST /api/power-plant/:id/produce`, `/convert` |
| `RealUtilityRedeemer` | Server-side credit service (TODO) | `POST /api/reactor/:telegramId/redeem-credits` |
| `ReactorManager` | — | Called client-side in the Telegram Mini App |

The Telegram Mini App (`MagiPlant-Powered` repo) is the primary player surface. These Unity scripts are the reference implementation and a potential WebGL embed for richer 3D scenes.

---

## Reactor tiers

| Tier | Real-power multiplier | Notes |
|---|---|---|
| Glowleaf | 1× | Starter tier |
| Bloomveil | 1× | Default |
| Monolith | 2.5× | Premium NFT tier |

---

## Player wallet

The player identifier used throughout is the **Telegram user ID** (`telegramId`), not a raw crypto wallet address. This matches the web app's primary key and lets the Express server correlate credit records with existing player rows.

Set `ReactorManager.telegramId` from your Telegram WebApp init data flow:

```csharp
reactorManager.telegramId = TelegramWebApp.InitData.user.id.ToString();
```

---

## RealUtilityRedeemer — wiring to production

`RedeemPowerCredits(float kwh, string telegramId)` currently POSTs to:

```
POST {apiBaseUrl}/api/reactor/{telegramId}/redeem-credits
Body: { "kwhEarned": 0.0000 }
Header: X-Api-Secret: <rotated secret>
```

That endpoint does not yet exist in the Express server — see the TODO in  
`artifacts/api-server/src/routes/` in the main `MagiPlant-Powered` repo.  
The `apiBaseUrl` and `apiSecret` fields on `RealUtilityRedeemer` are set at Unity build time via Inspector values or a CI-injected `ScriptableObject` — **never commit real secrets here**.

---

## Related repositories

| Repo | Purpose |
|---|---|
| [MagiPlant-Powered](https://github.com/MagiMiniGames/MagiPlant-Powered) | React/Vite Telegram Mini App + Express API server |
| **MagiPlant-Powered-NFT** (this repo) | Unity C# energy bridge scripts |

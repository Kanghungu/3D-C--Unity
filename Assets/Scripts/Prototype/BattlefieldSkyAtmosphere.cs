using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Creates and animates lightweight sky-layer atmosphere for the prototype battlefield.
    /// Uses primitive strips and motes to make the map feel larger without authored VFX.
    /// </summary>
    public class BattlefieldSkyAtmosphere : MonoBehaviour
    {
        private struct DriftLayer
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct HorizonSilhouette
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SkyTrail
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct PatrolFormation
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 BaseScale;
            public float Phase;
            public bool StartsFromWest;
            public float BaseHeight;
            public float LaneT;
        }

        private struct OrbitalLance
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FlakBurst
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FallingWreckage
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 BaseScale;
            public float Phase;
            public float ResetHeight;
        }

        private struct ShieldImpact
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SiegeVolley
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float ApexHeight;
            public float Phase;
        }

        private struct LaunchStreak
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct BatteryFlash
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct BarrageImpact
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct TargetDesignator
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct CounterScan
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public Quaternion BaseRotation;
            public float Phase;
        }

        private struct RelayPulse
        {
            public Transform Transform;
            public Transform PulseNode;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public Vector3 PulseBaseLocalPosition;
            public float Phase;
        }

        private struct ResponseArc
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float ApexHeight;
            public float Phase;
        }

        private struct CommandEcho
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct OrderRipple
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct AdvanceChevron
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FrontlineAcknowledge
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct RallyStream
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct BattlelineHandoff
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ClashPulse
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ShockfrontTrace
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct PressureFlare
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct BraceSweep
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct HoldBeacon
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct BulwarkLink
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ReserveRelay
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ReserveAnchor
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ReserveSurge
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct CommitBeacon
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ForwardSpill
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct EdgeClash
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ReboundTrace
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FallbackBeacon
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FallbackSweep
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct RecoveryLattice
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct StabilityPulse
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SentinelEcho
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SentinelReturn
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct CircuitSeal
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SealRipple
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SealAfterglow
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct AfterglowDrift
        {
            public Transform Transform;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct DormantVeil
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct QuietResidue
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ResidualBlink
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct LastEmber
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FinalHush
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct StillTrace
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SettledSpeck
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SilentGrain
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct MuteDust
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct StillAsh
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ColdSpeck
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FrostMote
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct RimeSeed
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct IcePin
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ChillNail
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FrostTack
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct GlazeDot
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct HoarBead
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct PaleDew
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FaintPearl
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct WanDroplet
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct HushMoisture
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct DimCondensate
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct StillFilm
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct QuietSheen
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct LastLustre
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct ThinGlint
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FadingGleam
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SoftTrace
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FaintVeil
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct GhostSheen
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FinalTint
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct MuteHue
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct HushedTint
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FadedCast
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SpentShade
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct DryStain
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct WornMark
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct FaintScuff
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct TraceNick
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct PinPrick
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct NeedleDot
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct DustSpeck
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct AshMote
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private struct SootFleck
        {
            public Transform Transform;
            public Vector3 BasePosition;
            public Vector3 BaseScale;
            public float Phase;
        }

        private readonly List<DriftLayer> driftLayers = new();
        private readonly List<HorizonSilhouette> horizonSilhouettes = new();
        private readonly List<SkyTrail> skyTrails = new();
        private readonly List<Renderer> skyTrailRenderers = new();
        private readonly List<Color> skyTrailBaseColors = new();
        private readonly List<Renderer> skyTrailTipRenderers = new();
        private readonly List<Color> skyTrailTipBaseColors = new();
        private readonly List<PatrolFormation> patrolFormations = new();
        private readonly List<Renderer> patrolGlowRenderers = new();
        private readonly List<Color> patrolGlowBaseColors = new();
        private readonly List<OrbitalLance> orbitalLances = new();
        private readonly List<Renderer> orbitalLanceRenderers = new();
        private readonly List<Color> orbitalLanceBaseColors = new();
        private readonly List<FlakBurst> flakBursts = new();
        private readonly List<Renderer> flakBurstRenderers = new();
        private readonly List<Color> flakBurstBaseColors = new();
        private readonly List<FallingWreckage> fallingWreckages = new();
        private readonly List<Renderer> wreckageGlowRenderers = new();
        private readonly List<Color> wreckageGlowBaseColors = new();
        private readonly List<ShieldImpact> shieldImpacts = new();
        private readonly List<Renderer> shieldImpactRenderers = new();
        private readonly List<Color> shieldImpactBaseColors = new();
        private readonly List<SiegeVolley> siegeVolleys = new();
        private readonly List<Renderer> siegeVolleyRenderers = new();
        private readonly List<Color> siegeVolleyBaseColors = new();
        private readonly List<LaunchStreak> launchStreaks = new();
        private readonly List<Renderer> launchStreakRenderers = new();
        private readonly List<Color> launchStreakBaseColors = new();
        private readonly List<BatteryFlash> batteryFlashes = new();
        private readonly List<Renderer> batteryFlashRenderers = new();
        private readonly List<Color> batteryFlashBaseColors = new();
        private readonly List<BarrageImpact> barrageImpacts = new();
        private readonly List<Renderer> barrageImpactRenderers = new();
        private readonly List<Color> barrageImpactBaseColors = new();
        private readonly List<TargetDesignator> targetDesignators = new();
        private readonly List<Renderer> targetDesignatorRenderers = new();
        private readonly List<Color> targetDesignatorBaseColors = new();
        private readonly List<CounterScan> counterScans = new();
        private readonly List<Renderer> counterScanRenderers = new();
        private readonly List<Color> counterScanBaseColors = new();
        private readonly List<RelayPulse> relayPulses = new();
        private readonly List<Renderer> relayPulseRenderers = new();
        private readonly List<Color> relayPulseBaseColors = new();
        private readonly List<ResponseArc> responseArcs = new();
        private readonly List<Renderer> responseArcRenderers = new();
        private readonly List<Color> responseArcBaseColors = new();
        private readonly List<CommandEcho> commandEchos = new();
        private readonly List<Renderer> commandEchoRenderers = new();
        private readonly List<Color> commandEchoBaseColors = new();
        private readonly List<OrderRipple> orderRipples = new();
        private readonly List<Renderer> orderRippleRenderers = new();
        private readonly List<Color> orderRippleBaseColors = new();
        private readonly List<AdvanceChevron> advanceChevrons = new();
        private readonly List<Renderer> advanceChevronRenderers = new();
        private readonly List<Color> advanceChevronBaseColors = new();
        private readonly List<FrontlineAcknowledge> frontlineAcknowledges = new();
        private readonly List<Renderer> frontlineAcknowledgeRenderers = new();
        private readonly List<Color> frontlineAcknowledgeBaseColors = new();
        private readonly List<RallyStream> rallyStreams = new();
        private readonly List<Renderer> rallyStreamRenderers = new();
        private readonly List<Color> rallyStreamBaseColors = new();
        private readonly List<BattlelineHandoff> battlelineHandoffs = new();
        private readonly List<Renderer> battlelineHandoffRenderers = new();
        private readonly List<Color> battlelineHandoffBaseColors = new();
        private readonly List<ClashPulse> clashPulses = new();
        private readonly List<Renderer> clashPulseRenderers = new();
        private readonly List<Color> clashPulseBaseColors = new();
        private readonly List<ShockfrontTrace> shockfrontTraces = new();
        private readonly List<Renderer> shockfrontTraceRenderers = new();
        private readonly List<Color> shockfrontTraceBaseColors = new();
        private readonly List<PressureFlare> pressureFlares = new();
        private readonly List<Renderer> pressureFlareRenderers = new();
        private readonly List<Color> pressureFlareBaseColors = new();
        private readonly List<BraceSweep> braceSweeps = new();
        private readonly List<Renderer> braceSweepRenderers = new();
        private readonly List<Color> braceSweepBaseColors = new();
        private readonly List<HoldBeacon> holdBeacons = new();
        private readonly List<Renderer> holdBeaconRenderers = new();
        private readonly List<Color> holdBeaconBaseColors = new();
        private readonly List<BulwarkLink> bulwarkLinks = new();
        private readonly List<Renderer> bulwarkLinkRenderers = new();
        private readonly List<Color> bulwarkLinkBaseColors = new();
        private readonly List<ReserveRelay> reserveRelays = new();
        private readonly List<Renderer> reserveRelayRenderers = new();
        private readonly List<Color> reserveRelayBaseColors = new();
        private readonly List<ReserveAnchor> reserveAnchors = new();
        private readonly List<Renderer> reserveAnchorRenderers = new();
        private readonly List<Color> reserveAnchorBaseColors = new();
        private readonly List<ReserveSurge> reserveSurges = new();
        private readonly List<Renderer> reserveSurgeRenderers = new();
        private readonly List<Color> reserveSurgeBaseColors = new();
        private readonly List<CommitBeacon> commitBeacons = new();
        private readonly List<Renderer> commitBeaconRenderers = new();
        private readonly List<Color> commitBeaconBaseColors = new();
        private readonly List<ForwardSpill> forwardSpills = new();
        private readonly List<Renderer> forwardSpillRenderers = new();
        private readonly List<Color> forwardSpillBaseColors = new();
        private readonly List<EdgeClash> edgeClashes = new();
        private readonly List<Renderer> edgeClashRenderers = new();
        private readonly List<Color> edgeClashBaseColors = new();
        private readonly List<ReboundTrace> reboundTraces = new();
        private readonly List<Renderer> reboundTraceRenderers = new();
        private readonly List<Color> reboundTraceBaseColors = new();
        private readonly List<FallbackBeacon> fallbackBeacons = new();
        private readonly List<Renderer> fallbackBeaconRenderers = new();
        private readonly List<Color> fallbackBeaconBaseColors = new();
        private readonly List<FallbackSweep> fallbackSweeps = new();
        private readonly List<Renderer> fallbackSweepRenderers = new();
        private readonly List<Color> fallbackSweepBaseColors = new();
        private readonly List<RecoveryLattice> recoveryLattices = new();
        private readonly List<Renderer> recoveryLatticeRenderers = new();
        private readonly List<Color> recoveryLatticeBaseColors = new();
        private readonly List<StabilityPulse> stabilityPulses = new();
        private readonly List<Renderer> stabilityPulseRenderers = new();
        private readonly List<Color> stabilityPulseBaseColors = new();
        private readonly List<SentinelEcho> sentinelEchos = new();
        private readonly List<Renderer> sentinelEchoRenderers = new();
        private readonly List<Color> sentinelEchoBaseColors = new();
        private readonly List<SentinelReturn> sentinelReturns = new();
        private readonly List<Renderer> sentinelReturnRenderers = new();
        private readonly List<Color> sentinelReturnBaseColors = new();
        private readonly List<CircuitSeal> circuitSeals = new();
        private readonly List<Renderer> circuitSealRenderers = new();
        private readonly List<Color> circuitSealBaseColors = new();
        private readonly List<SealRipple> sealRipples = new();
        private readonly List<Renderer> sealRippleRenderers = new();
        private readonly List<Color> sealRippleBaseColors = new();
        private readonly List<SealAfterglow> sealAfterglows = new();
        private readonly List<Renderer> sealAfterglowRenderers = new();
        private readonly List<Color> sealAfterglowBaseColors = new();
        private readonly List<AfterglowDrift> afterglowDrifts = new();
        private readonly List<Renderer> afterglowDriftRenderers = new();
        private readonly List<Color> afterglowDriftBaseColors = new();
        private readonly List<DormantVeil> dormantVeils = new();
        private readonly List<Renderer> dormantVeilRenderers = new();
        private readonly List<Color> dormantVeilBaseColors = new();
        private readonly List<QuietResidue> quietResidues = new();
        private readonly List<Renderer> quietResidueRenderers = new();
        private readonly List<Color> quietResidueBaseColors = new();
        private readonly List<ResidualBlink> residualBlinks = new();
        private readonly List<Renderer> residualBlinkRenderers = new();
        private readonly List<Color> residualBlinkBaseColors = new();
        private readonly List<LastEmber> lastEmbers = new();
        private readonly List<Renderer> lastEmberRenderers = new();
        private readonly List<Color> lastEmberBaseColors = new();
        private readonly List<FinalHush> finalHushes = new();
        private readonly List<Renderer> finalHushRenderers = new();
        private readonly List<Color> finalHushBaseColors = new();
        private readonly List<StillTrace> stillTraces = new();
        private readonly List<Renderer> stillTraceRenderers = new();
        private readonly List<Color> stillTraceBaseColors = new();
        private readonly List<SettledSpeck> settledSpecks = new();
        private readonly List<Renderer> settledSpeckRenderers = new();
        private readonly List<Color> settledSpeckBaseColors = new();
        private readonly List<SilentGrain> silentGrains = new();
        private readonly List<Renderer> silentGrainRenderers = new();
        private readonly List<Color> silentGrainBaseColors = new();
        private readonly List<MuteDust> muteDusts = new();
        private readonly List<Renderer> muteDustRenderers = new();
        private readonly List<Color> muteDustBaseColors = new();
        private readonly List<StillAsh> stillAshes = new();
        private readonly List<Renderer> stillAshRenderers = new();
        private readonly List<Color> stillAshBaseColors = new();
        private readonly List<ColdSpeck> coldSpecks = new();
        private readonly List<Renderer> coldSpeckRenderers = new();
        private readonly List<Color> coldSpeckBaseColors = new();
        private readonly List<FrostMote> frostMotes = new();
        private readonly List<Renderer> frostMoteRenderers = new();
        private readonly List<Color> frostMoteBaseColors = new();
        private readonly List<RimeSeed> rimeSeeds = new();
        private readonly List<Renderer> rimeSeedRenderers = new();
        private readonly List<Color> rimeSeedBaseColors = new();
        private readonly List<IcePin> icePins = new();
        private readonly List<Renderer> icePinRenderers = new();
        private readonly List<Color> icePinBaseColors = new();
        private readonly List<ChillNail> chillNails = new();
        private readonly List<Renderer> chillNailRenderers = new();
        private readonly List<Color> chillNailBaseColors = new();
        private readonly List<FrostTack> frostTacks = new();
        private readonly List<Renderer> frostTackRenderers = new();
        private readonly List<Color> frostTackBaseColors = new();
        private readonly List<GlazeDot> glazeDots = new();
        private readonly List<Renderer> glazeDotRenderers = new();
        private readonly List<Color> glazeDotBaseColors = new();
        private readonly List<HoarBead> hoarBeads = new();
        private readonly List<Renderer> hoarBeadRenderers = new();
        private readonly List<Color> hoarBeadBaseColors = new();
        private readonly List<PaleDew> paleDews = new();
        private readonly List<Renderer> paleDewRenderers = new();
        private readonly List<Color> paleDewBaseColors = new();
        private readonly List<FaintPearl> faintPearls = new();
        private readonly List<Renderer> faintPearlRenderers = new();
        private readonly List<Color> faintPearlBaseColors = new();
        private readonly List<WanDroplet> wanDroplets = new();
        private readonly List<Renderer> wanDropletRenderers = new();
        private readonly List<Color> wanDropletBaseColors = new();
        private readonly List<HushMoisture> hushMoistures = new();
        private readonly List<Renderer> hushMoistureRenderers = new();
        private readonly List<Color> hushMoistureBaseColors = new();
        private readonly List<DimCondensate> dimCondensates = new();
        private readonly List<Renderer> dimCondensateRenderers = new();
        private readonly List<Color> dimCondensateBaseColors = new();
        private readonly List<StillFilm> stillFilms = new();
        private readonly List<Renderer> stillFilmRenderers = new();
        private readonly List<Color> stillFilmBaseColors = new();
        private readonly List<QuietSheen> quietSheens = new();
        private readonly List<Renderer> quietSheenRenderers = new();
        private readonly List<Color> quietSheenBaseColors = new();
        private readonly List<LastLustre> lastLustres = new();
        private readonly List<Renderer> lastLustreRenderers = new();
        private readonly List<Color> lastLustreBaseColors = new();
        private readonly List<ThinGlint> thinGlints = new();
        private readonly List<Renderer> thinGlintRenderers = new();
        private readonly List<Color> thinGlintBaseColors = new();
        private readonly List<FadingGleam> fadingGleams = new();
        private readonly List<Renderer> fadingGleamRenderers = new();
        private readonly List<Color> fadingGleamBaseColors = new();
        private readonly List<SoftTrace> softTraces = new();
        private readonly List<Renderer> softTraceRenderers = new();
        private readonly List<Color> softTraceBaseColors = new();
        private readonly List<FaintVeil> faintVeils = new();
        private readonly List<Renderer> faintVeilRenderers = new();
        private readonly List<Color> faintVeilBaseColors = new();
        private readonly List<GhostSheen> ghostSheens = new();
        private readonly List<Renderer> ghostSheenRenderers = new();
        private readonly List<Color> ghostSheenBaseColors = new();
        private readonly List<FinalTint> finalTints = new();
        private readonly List<Renderer> finalTintRenderers = new();
        private readonly List<Color> finalTintBaseColors = new();
        private readonly List<MuteHue> muteHues = new();
        private readonly List<Renderer> muteHueRenderers = new();
        private readonly List<Color> muteHueBaseColors = new();
        private readonly List<HushedTint> hushedTints = new();
        private readonly List<Renderer> hushedTintRenderers = new();
        private readonly List<Color> hushedTintBaseColors = new();
        private readonly List<FadedCast> fadedCasts = new();
        private readonly List<Renderer> fadedCastRenderers = new();
        private readonly List<Color> fadedCastBaseColors = new();
        private readonly List<SpentShade> spentShades = new();
        private readonly List<Renderer> spentShadeRenderers = new();
        private readonly List<Color> spentShadeBaseColors = new();
        private readonly List<DryStain> dryStains = new();
        private readonly List<Renderer> dryStainRenderers = new();
        private readonly List<Color> dryStainBaseColors = new();
        private readonly List<WornMark> wornMarks = new();
        private readonly List<Renderer> wornMarkRenderers = new();
        private readonly List<Color> wornMarkBaseColors = new();
        private readonly List<FaintScuff> faintScuffs = new();
        private readonly List<Renderer> faintScuffRenderers = new();
        private readonly List<Color> faintScuffBaseColors = new();
        private readonly List<TraceNick> traceNicks = new();
        private readonly List<Renderer> traceNickRenderers = new();
        private readonly List<Color> traceNickBaseColors = new();
        private readonly List<PinPrick> pinPricks = new();
        private readonly List<Renderer> pinPrickRenderers = new();
        private readonly List<Color> pinPrickBaseColors = new();
        private readonly List<NeedleDot> needleDots = new();
        private readonly List<Renderer> needleDotRenderers = new();
        private readonly List<Color> needleDotBaseColors = new();
        private readonly List<DustSpeck> dustSpecks = new();
        private readonly List<Renderer> dustSpeckRenderers = new();
        private readonly List<Color> dustSpeckBaseColors = new();
        private readonly List<AshMote> ashMotes = new();
        private readonly List<Renderer> ashMoteRenderers = new();
        private readonly List<Color> ashMoteBaseColors = new();
        private readonly List<SootFleck> sootFlecks = new();
        private readonly List<Renderer> sootFleckRenderers = new();
        private readonly List<Color> sootFleckBaseColors = new();
        private readonly List<Transform> shrineMotes = new();
        private readonly List<Vector3> shrineMoteCenters = new();
        private readonly List<float> shrineMotePhases = new();
        [SerializeField]
        private bool usePrototypeMinimalSky = true;

        private BattlefieldMapProfile mapProfile;
        private BattlefieldTheme theme;
        private Transform root;

        public void Configure(BattlefieldTheme battlefieldTheme, BattlefieldMapProfile profile, Transform parent)
        {
            theme = battlefieldTheme;
            mapProfile = profile;
            driftLayers.Clear();
            horizonSilhouettes.Clear();
            skyTrails.Clear();
            skyTrailRenderers.Clear();
            skyTrailBaseColors.Clear();
            skyTrailTipRenderers.Clear();
            skyTrailTipBaseColors.Clear();
            patrolFormations.Clear();
            patrolGlowRenderers.Clear();
            patrolGlowBaseColors.Clear();
            orbitalLances.Clear();
            orbitalLanceRenderers.Clear();
            orbitalLanceBaseColors.Clear();
            flakBursts.Clear();
            flakBurstRenderers.Clear();
            flakBurstBaseColors.Clear();
            fallingWreckages.Clear();
            wreckageGlowRenderers.Clear();
            wreckageGlowBaseColors.Clear();
            shieldImpacts.Clear();
            shieldImpactRenderers.Clear();
            shieldImpactBaseColors.Clear();
            siegeVolleys.Clear();
            siegeVolleyRenderers.Clear();
            siegeVolleyBaseColors.Clear();
            launchStreaks.Clear();
            launchStreakRenderers.Clear();
            launchStreakBaseColors.Clear();
            batteryFlashes.Clear();
            batteryFlashRenderers.Clear();
            batteryFlashBaseColors.Clear();
            barrageImpacts.Clear();
            barrageImpactRenderers.Clear();
            barrageImpactBaseColors.Clear();
            targetDesignators.Clear();
            targetDesignatorRenderers.Clear();
            targetDesignatorBaseColors.Clear();
            counterScans.Clear();
            counterScanRenderers.Clear();
            counterScanBaseColors.Clear();
            relayPulses.Clear();
            relayPulseRenderers.Clear();
            relayPulseBaseColors.Clear();
            responseArcs.Clear();
            responseArcRenderers.Clear();
            responseArcBaseColors.Clear();
            commandEchos.Clear();
            commandEchoRenderers.Clear();
            commandEchoBaseColors.Clear();
            orderRipples.Clear();
            orderRippleRenderers.Clear();
            orderRippleBaseColors.Clear();
            advanceChevrons.Clear();
            advanceChevronRenderers.Clear();
            advanceChevronBaseColors.Clear();
            frontlineAcknowledges.Clear();
            frontlineAcknowledgeRenderers.Clear();
            frontlineAcknowledgeBaseColors.Clear();
            rallyStreams.Clear();
            rallyStreamRenderers.Clear();
            rallyStreamBaseColors.Clear();
            battlelineHandoffs.Clear();
            battlelineHandoffRenderers.Clear();
            battlelineHandoffBaseColors.Clear();
            clashPulses.Clear();
            clashPulseRenderers.Clear();
            clashPulseBaseColors.Clear();
            shockfrontTraces.Clear();
            shockfrontTraceRenderers.Clear();
            shockfrontTraceBaseColors.Clear();
            pressureFlares.Clear();
            pressureFlareRenderers.Clear();
            pressureFlareBaseColors.Clear();
            braceSweeps.Clear();
            braceSweepRenderers.Clear();
            braceSweepBaseColors.Clear();
            holdBeacons.Clear();
            holdBeaconRenderers.Clear();
            holdBeaconBaseColors.Clear();
            bulwarkLinks.Clear();
            bulwarkLinkRenderers.Clear();
            bulwarkLinkBaseColors.Clear();
            reserveRelays.Clear();
            reserveRelayRenderers.Clear();
            reserveRelayBaseColors.Clear();
            reserveAnchors.Clear();
            reserveAnchorRenderers.Clear();
            reserveAnchorBaseColors.Clear();
            reserveSurges.Clear();
            reserveSurgeRenderers.Clear();
            reserveSurgeBaseColors.Clear();
            commitBeacons.Clear();
            commitBeaconRenderers.Clear();
            commitBeaconBaseColors.Clear();
            forwardSpills.Clear();
            forwardSpillRenderers.Clear();
            forwardSpillBaseColors.Clear();
            edgeClashes.Clear();
            edgeClashRenderers.Clear();
            edgeClashBaseColors.Clear();
            reboundTraces.Clear();
            reboundTraceRenderers.Clear();
            reboundTraceBaseColors.Clear();
            fallbackBeacons.Clear();
            fallbackBeaconRenderers.Clear();
            fallbackBeaconBaseColors.Clear();
            fallbackSweeps.Clear();
            fallbackSweepRenderers.Clear();
            fallbackSweepBaseColors.Clear();
            recoveryLattices.Clear();
            recoveryLatticeRenderers.Clear();
            recoveryLatticeBaseColors.Clear();
            stabilityPulses.Clear();
            stabilityPulseRenderers.Clear();
            stabilityPulseBaseColors.Clear();
            sentinelEchos.Clear();
            sentinelEchoRenderers.Clear();
            sentinelEchoBaseColors.Clear();
            sentinelReturns.Clear();
            sentinelReturnRenderers.Clear();
            sentinelReturnBaseColors.Clear();
            circuitSeals.Clear();
            circuitSealRenderers.Clear();
            circuitSealBaseColors.Clear();
            sealRipples.Clear();
            sealRippleRenderers.Clear();
            sealRippleBaseColors.Clear();
            sealAfterglows.Clear();
            sealAfterglowRenderers.Clear();
            sealAfterglowBaseColors.Clear();
            afterglowDrifts.Clear();
            afterglowDriftRenderers.Clear();
            afterglowDriftBaseColors.Clear();
            dormantVeils.Clear();
            dormantVeilRenderers.Clear();
            dormantVeilBaseColors.Clear();
            quietResidues.Clear();
            quietResidueRenderers.Clear();
            quietResidueBaseColors.Clear();
            residualBlinks.Clear();
            residualBlinkRenderers.Clear();
            residualBlinkBaseColors.Clear();
            lastEmbers.Clear();
            lastEmberRenderers.Clear();
            lastEmberBaseColors.Clear();
            finalHushes.Clear();
            finalHushRenderers.Clear();
            finalHushBaseColors.Clear();
            stillTraces.Clear();
            stillTraceRenderers.Clear();
            stillTraceBaseColors.Clear();
            settledSpecks.Clear();
            settledSpeckRenderers.Clear();
            settledSpeckBaseColors.Clear();
            silentGrains.Clear();
            silentGrainRenderers.Clear();
            silentGrainBaseColors.Clear();
            muteDusts.Clear();
            muteDustRenderers.Clear();
            muteDustBaseColors.Clear();
            stillAshes.Clear();
            stillAshRenderers.Clear();
            stillAshBaseColors.Clear();
            coldSpecks.Clear();
            coldSpeckRenderers.Clear();
            coldSpeckBaseColors.Clear();
            frostMotes.Clear();
            frostMoteRenderers.Clear();
            frostMoteBaseColors.Clear();
            rimeSeeds.Clear();
            rimeSeedRenderers.Clear();
            rimeSeedBaseColors.Clear();
            icePins.Clear();
            icePinRenderers.Clear();
            icePinBaseColors.Clear();
            chillNails.Clear();
            chillNailRenderers.Clear();
            chillNailBaseColors.Clear();
            frostTacks.Clear();
            frostTackRenderers.Clear();
            frostTackBaseColors.Clear();
            glazeDots.Clear();
            glazeDotRenderers.Clear();
            glazeDotBaseColors.Clear();
            hoarBeads.Clear();
            hoarBeadRenderers.Clear();
            hoarBeadBaseColors.Clear();
            paleDews.Clear();
            paleDewRenderers.Clear();
            paleDewBaseColors.Clear();
            faintPearls.Clear();
            faintPearlRenderers.Clear();
            faintPearlBaseColors.Clear();
            wanDroplets.Clear();
            wanDropletRenderers.Clear();
            wanDropletBaseColors.Clear();
            hushMoistures.Clear();
            hushMoistureRenderers.Clear();
            hushMoistureBaseColors.Clear();
            dimCondensates.Clear();
            dimCondensateRenderers.Clear();
            dimCondensateBaseColors.Clear();
            stillFilms.Clear();
            stillFilmRenderers.Clear();
            stillFilmBaseColors.Clear();
            quietSheens.Clear();
            quietSheenRenderers.Clear();
            quietSheenBaseColors.Clear();
            lastLustres.Clear();
            lastLustreRenderers.Clear();
            lastLustreBaseColors.Clear();
            thinGlints.Clear();
            thinGlintRenderers.Clear();
            thinGlintBaseColors.Clear();
            fadingGleams.Clear();
            fadingGleamRenderers.Clear();
            fadingGleamBaseColors.Clear();
            softTraces.Clear();
            softTraceRenderers.Clear();
            softTraceBaseColors.Clear();
            faintVeils.Clear();
            faintVeilRenderers.Clear();
            faintVeilBaseColors.Clear();
            ghostSheens.Clear();
            ghostSheenRenderers.Clear();
            ghostSheenBaseColors.Clear();
            finalTints.Clear();
            finalTintRenderers.Clear();
            finalTintBaseColors.Clear();
            muteHues.Clear();
            muteHueRenderers.Clear();
            muteHueBaseColors.Clear();
            hushedTints.Clear();
            hushedTintRenderers.Clear();
            hushedTintBaseColors.Clear();
            fadedCasts.Clear();
            fadedCastRenderers.Clear();
            fadedCastBaseColors.Clear();
            spentShades.Clear();
            spentShadeRenderers.Clear();
            spentShadeBaseColors.Clear();
            dryStains.Clear();
            dryStainRenderers.Clear();
            dryStainBaseColors.Clear();
            wornMarks.Clear();
            wornMarkRenderers.Clear();
            wornMarkBaseColors.Clear();
            faintScuffs.Clear();
            faintScuffRenderers.Clear();
            faintScuffBaseColors.Clear();
            traceNicks.Clear();
            traceNickRenderers.Clear();
            traceNickBaseColors.Clear();
            pinPricks.Clear();
            pinPrickRenderers.Clear();
            pinPrickBaseColors.Clear();
            needleDots.Clear();
            needleDotRenderers.Clear();
            needleDotBaseColors.Clear();
            dustSpecks.Clear();
            dustSpeckRenderers.Clear();
            dustSpeckBaseColors.Clear();
            ashMotes.Clear();
            ashMoteRenderers.Clear();
            ashMoteBaseColors.Clear();
            sootFlecks.Clear();
            sootFleckRenderers.Clear();
            sootFleckBaseColors.Clear();
            shrineMotes.Clear();
            shrineMoteCenters.Clear();
            shrineMotePhases.Clear();

            if (root != null)
            {
                Destroy(root.gameObject);
            }

            root = new GameObject("Sky Atmosphere").transform;
            root.SetParent(parent);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;

            // Sky atmosphere disabled: blocking combat view. Re-enable after prototype validation.
            return;

#pragma warning disable CS0162
            CreateDriftLayers();
            CreateHorizonSilhouettes();
            CreateSkyTrails();
            CreatePatrolFormations();
            CreateOrbitalLances();
            CreateFlakBursts();
            CreateFallingWreckage();
            CreateShieldImpacts();
            CreateSiegeVolleys();
            CreateLaunchStreaks();
            CreateBatteryFlashes();
            CreateBarrageImpacts();
            CreateTargetDesignators();
            CreateCounterScans();
            CreateRelayPulses();
            CreateResponseArcs();
            CreateCommandEchos();
            CreateOrderRipples();
            CreateAdvanceChevrons();
            CreateFrontlineAcknowledges();
            CreateRallyStreams();
            CreateBattlelineHandoffs();
            CreateClashPulses();
            CreateShockfrontTraces();
            CreatePressureFlares();
            CreateBraceSweeps();
            CreateHoldBeacons();
            CreateBulwarkLinks();
            CreateReserveRelays();
            CreateReserveAnchors();
            CreateReserveSurges();
            CreateCommitBeacons();
            CreateForwardSpills();
            CreateEdgeClashes();
            CreateReboundTraces();
            CreateFallbackBeacons();
            CreateFallbackSweeps();
            CreateRecoveryLattices();
            CreateStabilityPulses();
            CreateSentinelEchos();
            CreateSentinelReturns();
            CreateCircuitSeals();
            CreateSealRipples();
            CreateSealAfterglows();
            if (usePrototypeMinimalSky)
            {
                // Keep the prototype sky focused on readable frontline cues instead of ultra-fine residue layers.
                CreateShrineMotes();
                return;
            }

            CreateAfterglowDrifts();
            CreateDormantVeils();
            CreateQuietResidues();
            CreateResidualBlinks();
            CreateLastEmbers();
            CreateFinalHushes();
            CreateStillTraces();
            CreateSettledSpecks();
            CreateSilentGrains();
            CreateMuteDusts();
            CreateStillAshes();
            CreateColdSpecks();
            CreateFrostMotes();
            CreateRimeSeeds();
            CreateIcePins();
            CreateChillNails();
            CreateFrostTacks();
            CreateGlazeDots();
            CreateHoarBeads();
            CreatePaleDews();
            CreateFaintPearls();
            CreateWanDroplets();
            CreateHushMoistures();
            CreateDimCondensates();
            CreateStillFilms();
            CreateQuietSheens();
            CreateLastLustres();
            CreateThinGlints();
            CreateFadingGleams();
            CreateSoftTraces();
            CreateFaintVeils();
            CreateGhostSheens();
            CreateFinalTints();
            CreateMuteHues();
            CreateHushedTints();
            CreateFadedCasts();
            CreateSpentShades();
            CreateDryStains();
            CreateWornMarks();
            CreateFaintScuffs();
            CreateTraceNicks();
            CreatePinPricks();
            CreateNeedleDots();
            CreateDustSpecks();
            CreateAshMotes();
            CreateSootFleckLayer();
            CreateShrineMotes();
#pragma warning restore CS0162
        }

        private void Update()
        {
            // Sky atmosphere disabled. Nothing to animate.
            if (root == null || root.childCount == 0) return;

            AnimateDriftLayers();
            AnimateHorizonSilhouettes();
            AnimateSkyTrails();
            AnimatePatrolFormations();
            AnimateOrbitalLances();
            AnimateFlakBursts();
            AnimateFallingWreckage();
            AnimateShieldImpacts();
            AnimateSiegeVolleys();
            AnimateLaunchStreaks();
            AnimateBatteryFlashes();
            AnimateBarrageImpacts();
            AnimateTargetDesignators();
            AnimateCounterScans();
            AnimateRelayPulses();
            AnimateResponseArcs();
            AnimateCommandEchos();
            AnimateOrderRipples();
            AnimateAdvanceChevrons();
            AnimateFrontlineAcknowledges();
            AnimateRallyStreams();
            AnimateBattlelineHandoffs();
            AnimateClashPulses();
            AnimateShockfrontTraces();
            AnimatePressureFlares();
            AnimateBraceSweeps();
            AnimateHoldBeacons();
            AnimateBulwarkLinks();
            AnimateReserveRelays();
            AnimateReserveAnchors();
            AnimateReserveSurges();
            AnimateCommitBeacons();
            AnimateForwardSpills();
            AnimateEdgeClashes();
            AnimateReboundTraces();
            AnimateFallbackBeacons();
            AnimateFallbackSweeps();
            AnimateRecoveryLattices();
            AnimateStabilityPulses();
            AnimateSentinelEchos();
            AnimateSentinelReturns();
            AnimateCircuitSeals();
            AnimateSealRipples();
            AnimateSealAfterglows();
            AnimateAfterglowDrifts();
            AnimateDormantVeils();
            AnimateQuietResidues();
            AnimateResidualBlinks();
            AnimateLastEmbers();
            AnimateFinalHushes();
            AnimateStillTraces();
            AnimateSettledSpecks();
            AnimateSilentGrains();
            AnimateMuteDusts();
            AnimateStillAshes();
            AnimateColdSpecks();
            AnimateFrostMotes();
            AnimateRimeSeeds();
            AnimateIcePins();
            AnimateChillNails();
            AnimateFrostTacks();
            AnimateGlazeDots();
            AnimateHoarBeads();
            AnimatePaleDews();
            AnimateFaintPearls();
            AnimateWanDroplets();
            AnimateHushMoistures();
            AnimateDimCondensates();
            AnimateStillFilms();
            AnimateQuietSheens();
            AnimateLastLustres();
            AnimateThinGlints();
            AnimateFadingGleams();
            AnimateSoftTraces();
            AnimateFaintVeils();
            AnimateGhostSheens();
            AnimateFinalTints();
            AnimateMuteHues();
            AnimateHushedTints();
            AnimateFadedCasts();
            AnimateSpentShades();
            AnimateDryStains();
            AnimateWornMarks();
            AnimateFaintScuffs();
            AnimateTraceNicks();
            AnimatePinPricks();
            AnimateNeedleDots();
            AnimateDustSpecks();
            AnimateAshMotes();
            AnimateSootFleckLayer();
            AnimateShrineMotes();
        }

        private void CreateDriftLayers()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color layerColorA = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.52f, 0.22f, 0.18f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.74f, 0.76f, 0.82f, 0.64f),
                _ => new Color(0.72f, 0.62f, 0.44f, 0.66f)
            };
            Color layerColorB = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.78f, 0.32f, 0.2f, 0.56f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.82f, 0.88f, 0.96f, 0.52f),
                _ => new Color(0.9f, 0.78f, 0.56f, 0.5f)
            };

            int layerCount = 8;
            for (int index = 0; index < layerCount; index++)
            {
                GameObject layerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                layerObject.name = $"Sky Drift {index + 1}";
                layerObject.transform.SetParent(root);
                layerObject.transform.position = new Vector3(
                    Random.Range(mapProfile.MinX, mapProfile.MaxX),
                    42f + index * 3.8f,
                    Random.Range(mapProfile.MinZ, mapProfile.MaxZ));
                layerObject.transform.localScale = new Vector3(
                    Random.Range(220f, 420f),
                    Random.Range(0.8f, 1.8f),
                    Random.Range(54f, 96f));
                layerObject.transform.rotation = Quaternion.Euler(0f, Random.Range(-22f, 22f), Random.Range(-4f, 4f));

                Collider colliderComponent = layerObject.GetComponent<Collider>();
                if (colliderComponent != null)
                {
                    colliderComponent.enabled = false;
                }

                Renderer rendererComponent = layerObject.GetComponent<Renderer>();
                if (rendererComponent != null)
                {
                    rendererComponent.material.color = Color.Lerp(layerColorA, layerColorB, index / (float)Mathf.Max(1, layerCount - 1));
                }

                PrototypeTerrainPrimitiveFactory.EnsureFogObject(layerObject, BattlefieldFogRequirement.Explored);

                driftLayers.Add(new DriftLayer
                {
                    Transform = layerObject.transform,
                    Velocity = new Vector3(18f + index * 2.8f, 0f, 8f + index * 1.6f),
                    BaseScale = layerObject.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateHorizonSilhouettes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color silhouetteColor = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.22f, 0.08f, 0.08f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.32f, 0.34f, 0.4f, 0.64f),
                _ => new Color(0.18f, 0.14f, 0.12f, 0.68f)
            };

            int silhouetteCount = 4;
            for (int index = 0; index < silhouetteCount; index++)
            {
                GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hull.name = $"Horizon Silhouette {index + 1}";
                hull.transform.SetParent(root);
                hull.transform.position = new Vector3(
                    mapProfile.MinX - 900f - index * 520f,
                    18f + index * 4.5f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.18f + index * 0.2f));
                hull.transform.localScale = new Vector3(180f + index * 34f, 22f + index * 5f, 54f + index * 10f);
                hull.transform.rotation = Quaternion.Euler(0f, 14f - index * 4f, 0f);

                Collider hullCollider = hull.GetComponent<Collider>();
                if (hullCollider != null)
                {
                    hullCollider.enabled = false;
                }

                Renderer hullRenderer = hull.GetComponent<Renderer>();
                if (hullRenderer != null)
                {
                    hullRenderer.material.color = silhouetteColor;
                }

                PrototypeTerrainPrimitiveFactory.EnsureFogObject(hull, BattlefieldFogRequirement.Explored);

                CreateSilhouetteSpine(hull.transform, silhouetteColor, 0.32f + index * 0.06f);
                CreateSilhouetteTower(hull.transform, silhouetteColor, new Vector3(-0.22f, 0.7f, -0.08f), new Vector3(0.16f, 0.64f, 0.16f));
                CreateSilhouetteTower(hull.transform, silhouetteColor, new Vector3(0.18f, 0.88f, 0.12f), new Vector3(0.12f, 0.76f, 0.12f));

                horizonSilhouettes.Add(new HorizonSilhouette
                {
                    Transform = hull.transform,
                    Velocity = new Vector3(14f + index * 2f, 0f, 1.6f + index * 0.5f),
                    BaseScale = hull.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSkyTrails()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color trailColorA = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.46f, 0.24f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.74f, 0.9f, 1f, 0.68f),
                _ => new Color(0.5f, 0.9f, 1f, 0.7f)
            };
            Color trailColorB = new Color(1f, 0.82f, 0.56f, 0.64f);

            int trailCount = 6;
            for (int index = 0; index < trailCount; index++)
            {
                GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trail.name = $"Sky Trail {index + 1}";
                trail.transform.SetParent(root);
                trail.transform.position = new Vector3(
                    Random.Range(mapProfile.MinX, mapProfile.MaxX),
                    78f + index * 4.5f,
                    Random.Range(mapProfile.MinZ, mapProfile.MaxZ));
                trail.transform.localScale = new Vector3(1.6f, 0.28f, 54f + index * 8f);
                trail.transform.rotation = Quaternion.Euler(0f, -58f + index * 8f, 0f);

                Collider trailCollider = trail.GetComponent<Collider>();
                if (trailCollider != null)
                {
                    trailCollider.enabled = false;
                }

                Renderer trailRenderer = trail.GetComponent<Renderer>();
                if (trailRenderer != null)
                {
                    Color trailColor = Color.Lerp(trailColorA, trailColorB, index / (float)Mathf.Max(1, trailCount - 1));
                    trailRenderer.material.color = trailColor;
                    skyTrailRenderers.Add(trailRenderer);
                    skyTrailBaseColors.Add(trailColor);
                }

                PrototypeTerrainPrimitiveFactory.EnsureFogObject(trail, BattlefieldFogRequirement.Explored);

                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = $"Sky Trail Tip {index + 1}";
                tip.transform.SetParent(trail.transform);
                tip.transform.localPosition = new Vector3(0f, 0f, trail.transform.localScale.z * 0.5f);
                tip.transform.localScale = Vector3.one * 2.4f;
                Collider tipCollider = tip.GetComponent<Collider>();
                if (tipCollider != null)
                {
                    tipCollider.enabled = false;
                }

                Renderer tipRenderer = tip.GetComponent<Renderer>();
                if (tipRenderer != null)
                {
                    Color tipColor = Color.Lerp(trailColorB, Color.white, 0.16f);
                    tipRenderer.material.color = tipColor;
                    skyTrailTipRenderers.Add(tipRenderer);
                    skyTrailTipBaseColors.Add(tipColor);
                }

                skyTrails.Add(new SkyTrail
                {
                    Transform = trail.transform,
                    Velocity = new Vector3(120f + index * 12f, 0f, -64f - index * 8f),
                    BaseScale = trail.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateShrineMotes()
        {
            if (root == null)
            {
                return;
            }

            Color moteColor = theme == BattlefieldTheme.CrimsonBasin
                ? new Color(1f, 0.54f, 0.28f, 0.9f)
                : new Color(0.88f, 0.84f, 0.66f, 0.88f);

            int moteCount = 14;
            Vector3 center = new Vector3(0f, 18f, 0f);
            for (int index = 0; index < moteCount; index++)
            {
                GameObject mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = $"Shrine Mote {index + 1}";
                mote.transform.SetParent(root);
                mote.transform.localScale = Vector3.one * Random.Range(0.5f, 1.4f);

                Collider colliderComponent = mote.GetComponent<Collider>();
                if (colliderComponent != null)
                {
                    colliderComponent.enabled = false;
                }

                Renderer rendererComponent = mote.GetComponent<Renderer>();
                if (rendererComponent != null)
                {
                    rendererComponent.material.color = moteColor;
                }

                float phase = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(18f, 42f);
                Vector3 position = center + new Vector3(Mathf.Cos(phase) * radius, Random.Range(0f, 18f), Mathf.Sin(phase) * radius);
                mote.transform.position = position;
                PrototypeTerrainPrimitiveFactory.EnsureFogObject(mote, BattlefieldFogRequirement.Explored);

                shrineMotes.Add(mote.transform);
                shrineMoteCenters.Add(center);
                shrineMotePhases.Add(phase);
            }
        }

        private void CreatePatrolFormations()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerHull = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.6f, 0.72f, 0.82f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.72f, 0.84f, 0.92f, 0.76f),
                _ => new Color(0.58f, 0.74f, 0.86f, 0.78f)
            };
            Color enemyHull = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.82f, 0.38f, 0.26f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.78f, 0.48f, 0.34f, 0.76f),
                _ => new Color(0.86f, 0.46f, 0.28f, 0.78f)
            };

            for (int index = 0; index < 4; index++)
            {
                bool startsFromWest = index < 2;
                int laneIndex = startsFromWest ? index : index - 2;
                float laneT = startsFromWest
                    ? 0.22f + laneIndex * 0.16f
                    : 0.78f - laneIndex * 0.16f;
                float baseHeight = 58f + index * 7f;
                Vector3 startPosition = new Vector3(
                    startsFromWest ? mapProfile.MinX - 340f - laneIndex * 90f : mapProfile.MaxX + 340f + laneIndex * 90f,
                    baseHeight,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 velocity = startsFromWest
                    ? new Vector3(48f + laneIndex * 8f, 0f, 10f + laneIndex * 3f)
                    : new Vector3(-52f - laneIndex * 8f, 0f, -10f - laneIndex * 3f);

                GameObject formation = new($"Sky Patrol Formation {index + 1}");
                formation.transform.SetParent(root);
                formation.transform.position = startPosition;
                formation.transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                formation.transform.localScale = Vector3.one * (1f + index * 0.06f);

                Color hullColor = startsFromWest ? playerHull : enemyHull;
                Color glowColor = Color.Lerp(hullColor, Color.white, 0.18f);

                CreatePatrolCraft(formation.transform, hullColor, glowColor, 0f, 0f, 0f, 1.2f);
                CreatePatrolCraft(formation.transform, hullColor, glowColor, -7.2f, -0.6f, -5.8f, 0.86f);
                CreatePatrolCraft(formation.transform, hullColor, glowColor, 7.2f, -0.6f, -5.8f, 0.86f);

                patrolFormations.Add(new PatrolFormation
                {
                    Transform = formation.transform,
                    Velocity = velocity,
                    BaseScale = formation.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f,
                    StartsFromWest = startsFromWest,
                    BaseHeight = baseHeight,
                    LaneT = laneT
                });
            }
        }

        private void CreateOrbitalLances()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerLance = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.7f, 0.84f, 1f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.78f, 0.94f, 1f, 0.84f),
                _ => new Color(0.64f, 0.9f, 1f, 0.86f)
            };
            Color enemyLance = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.46f, 0.28f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.96f, 0.62f, 0.34f, 0.84f),
                _ => new Color(1f, 0.56f, 0.3f, 0.86f)
            };

            for (int index = 0; index < 6; index++)
            {
                bool playerSide = index % 2 == 0;
                float zT = 0.16f + index * 0.13f;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MinX - 220f : mapProfile.MaxX + 220f,
                    10f + index * 1.5f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, zT));

                GameObject lance = new($"Orbital Lance {index + 1}");
                lance.transform.SetParent(root);
                lance.transform.position = basePosition;
                lance.transform.rotation = Quaternion.identity;
                lance.transform.localScale = Vector3.one;

                Color lanceColor = playerSide ? playerLance : enemyLance;
                CreateOrbitalLanceElement(
                    lance.transform,
                    "Orbital Lance Beam",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 18f, 0f),
                    new Vector3(1.8f, 18f, 1.8f),
                    lanceColor);
                CreateOrbitalLanceElement(
                    lance.transform,
                    "Orbital Lance Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 38f, 0f),
                    Vector3.one * 4.4f,
                    Color.Lerp(lanceColor, Color.white, 0.22f));
                CreateOrbitalLanceElement(
                    lance.transform,
                    "Orbital Lance Halo",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 0.2f, 0f),
                    new Vector3(8.8f, 0.05f, 8.8f),
                    Color.Lerp(lanceColor, Color.white, 0.08f));

                orbitalLances.Add(new OrbitalLance
                {
                    Transform = lance.transform,
                    BasePosition = basePosition,
                    BaseScale = lance.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFlakBursts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color flakColorA = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.72f, 0.4f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.86f, 0.96f, 1f, 0.82f),
                _ => new Color(1f, 0.86f, 0.58f, 0.84f)
            };
            Color flakColorB = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.42f, 0.22f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.68f, 0.88f, 1f, 0.76f),
                _ => new Color(0.82f, 0.92f, 1f, 0.8f)
            };

            int burstCount = 10;
            for (int index = 0; index < burstCount; index++)
            {
                Vector3 basePosition = new Vector3(
                    Mathf.Lerp(mapProfile.MinX, mapProfile.MaxX, 0.12f + (index % 5) * 0.18f),
                    44f + (index % 3) * 8f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.16f + (index / 5f) * 0.34f));

                GameObject burst = new($"Flak Burst {index + 1}");
                burst.transform.SetParent(root);
                burst.transform.position = basePosition;
                burst.transform.rotation = Quaternion.identity;
                burst.transform.localScale = Vector3.one;

                Color burstColor = Color.Lerp(flakColorA, flakColorB, index / (float)Mathf.Max(1, burstCount - 1));
                CreateFlakBurstElement(
                    burst.transform,
                    "Flak Burst Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 2.2f,
                    burstColor);
                CreateFlakBurstElement(
                    burst.transform,
                    "Flak Burst Halo",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, -0.2f, 0f),
                    new Vector3(3.8f, 0.05f, 3.8f),
                    Color.Lerp(burstColor, Color.white, 0.18f));

                flakBursts.Add(new FlakBurst
                {
                    Transform = burst.transform,
                    BasePosition = basePosition,
                    BaseScale = burst.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFallingWreckage()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color hullColor = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.34f, 0.22f, 0.2f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.46f, 0.5f, 0.56f, 0.8f),
                _ => new Color(0.38f, 0.32f, 0.28f, 0.82f)
            };
            Color glowColor = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.52f, 0.24f, 0.88f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.82f, 0.92f, 1f, 0.84f),
                _ => new Color(1f, 0.72f, 0.42f, 0.88f)
            };

            int wreckageCount = 8;
            for (int index = 0; index < wreckageCount; index++)
            {
                Vector3 position = new Vector3(
                    Mathf.Lerp(mapProfile.MinX, mapProfile.MaxX, 0.1f + (index % 4) * 0.22f),
                    92f + index * 5f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.18f + (index / 4f) * 0.28f));

                GameObject wreckage = new($"Falling Wreckage {index + 1}");
                wreckage.transform.SetParent(root);
                wreckage.transform.position = position;
                wreckage.transform.rotation = Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(0f, 360f), Random.Range(-30f, 30f));
                wreckage.transform.localScale = Vector3.one * (0.8f + index * 0.04f);

                CreateWreckageElement(
                    wreckage.transform,
                    "Wreckage Body",
                    PrimitiveType.Cube,
                    Vector3.zero,
                    new Vector3(3.6f, 1.1f, 6.2f),
                    hullColor);
                CreateWreckageElement(
                    wreckage.transform,
                    "Wreckage Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.4f, -6.8f),
                    new Vector3(0.7f, 0.7f, 9.4f),
                    new Color(glowColor.r, glowColor.g, glowColor.b, 0.56f));
                CreateWreckageElement(
                    wreckage.transform,
                    "Wreckage Ember",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.2f, 2.8f),
                    Vector3.one * 1.8f,
                    glowColor);

                fallingWreckages.Add(new FallingWreckage
                {
                    Transform = wreckage.transform,
                    Velocity = new Vector3(8f + index * 1.8f, -26f - index * 1.4f, -18f - index * 2.2f),
                    BaseScale = wreckage.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f,
                    ResetHeight = 92f + index * 5f
                });
            }
        }

        private void CreateShieldImpacts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerShield = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.54f, 0.82f, 1f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.74f, 0.94f, 1f, 0.82f),
                _ => new Color(0.58f, 0.88f, 1f, 0.84f)
            };
            Color enemyShield = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.52f, 0.32f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.7f, 0.42f, 0.82f),
                _ => new Color(1f, 0.62f, 0.36f, 0.84f)
            };

            int impactCount = 6;
            for (int index = 0; index < impactCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MinX - 180f : mapProfile.MaxX + 180f,
                    34f + (index % 3) * 5f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.18f + index * 0.11f));

                GameObject impact = new($"Shield Impact {index + 1}");
                impact.transform.SetParent(root);
                impact.transform.position = basePosition;
                impact.transform.rotation = Quaternion.Euler(0f, playerSide ? 90f : -90f, 0f);
                impact.transform.localScale = Vector3.one;

                Color impactColor = playerSide ? playerShield : enemyShield;
                CreateShieldImpactElement(
                    impact.transform,
                    "Shield Impact Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(6.8f, 0.05f, 6.8f),
                    impactColor);
                CreateShieldImpactElement(
                    impact.transform,
                    "Shield Impact Flash",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0f, 0f),
                    Vector3.one * 3.2f,
                    Color.Lerp(impactColor, Color.white, 0.28f));
                CreateShieldImpactElement(
                    impact.transform,
                    "Shield Impact Arc",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 1.8f, 0f),
                    new Vector3(3.4f, 0.05f, 3.4f),
                    Color.Lerp(impactColor, Color.white, 0.12f));

                shieldImpacts.Add(new ShieldImpact
                {
                    Transform = impact.transform,
                    BasePosition = basePosition,
                    BaseScale = impact.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSiegeVolleys()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerVolley = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.72f, 0.9f, 1f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.84f, 0.96f, 1f, 0.82f),
                _ => new Color(0.7f, 0.92f, 1f, 0.84f)
            };
            Color enemyVolley = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.62f, 0.34f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.78f, 0.46f, 0.82f),
                _ => new Color(1f, 0.68f, 0.4f, 0.84f)
            };

            int volleyCount = 8;
            for (int index = 0; index < volleyCount; index++)
            {
                bool playerSide = index % 2 == 0;
                float laneT = 0.14f + index * 0.09f;
                Vector3 startPosition = new Vector3(
                    playerSide ? mapProfile.MinX - 140f : mapProfile.MaxX + 140f,
                    20f + (index % 3) * 3f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 140f : mapProfile.MinX - 140f,
                    18f + (index % 3) * 2f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.2f + ((volleyCount - index - 1) * 0.08f)));

                GameObject volley = new($"Siege Volley {index + 1}");
                volley.transform.SetParent(root);
                volley.transform.position = startPosition;
                volley.transform.rotation = Quaternion.identity;
                volley.transform.localScale = Vector3.one;

                Color volleyColor = playerSide ? playerVolley : enemyVolley;
                CreateSiegeVolleyElement(
                    volley.transform,
                    "Siege Volley Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -4.8f),
                    new Vector3(0.54f, 0.54f, 8.8f),
                    new Color(volleyColor.r, volleyColor.g, volleyColor.b, 0.52f));
                CreateSiegeVolleyElement(
                    volley.transform,
                    "Siege Volley Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 1.7f,
                    volleyColor);

                siegeVolleys.Add(new SiegeVolley
                {
                    Transform = volley.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = volley.transform.localScale,
                    ApexHeight = 22f + (index % 4) * 4f,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateLaunchStreaks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerLaunch = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.7f, 0.9f, 1f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.86f, 0.98f, 1f, 0.84f),
                _ => new Color(0.76f, 0.94f, 1f, 0.86f)
            };
            Color enemyLaunch = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.64f, 0.36f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.8f, 0.48f, 0.84f),
                _ => new Color(1f, 0.72f, 0.42f, 0.86f)
            };

            int streakCount = 8;
            for (int index = 0; index < streakCount; index++)
            {
                bool playerSide = index % 2 == 0;
                float laneT = 0.16f + index * 0.08f;
                Vector3 startPosition = new Vector3(
                    playerSide ? mapProfile.MinX - 120f : mapProfile.MaxX + 120f,
                    14f + (index % 3) * 2.2f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerSide ? mapProfile.MinX + 220f + (index % 2) * 80f : mapProfile.MaxX - 220f - (index % 2) * 80f,
                    104f + (index % 4) * 10f,
                    startPosition.z + (playerSide ? -34f : 34f));

                GameObject streak = new($"Launch Streak {index + 1}");
                streak.transform.SetParent(root);
                streak.transform.position = startPosition;
                streak.transform.rotation = Quaternion.identity;
                streak.transform.localScale = Vector3.one;

                Color streakColor = playerSide ? playerLaunch : enemyLaunch;
                CreateLaunchStreakElement(
                    streak.transform,
                    "Launch Streak Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -4.6f),
                    new Vector3(0.48f, 0.48f, 8.4f),
                    new Color(streakColor.r, streakColor.g, streakColor.b, 0.52f));
                CreateLaunchStreakElement(
                    streak.transform,
                    "Launch Streak Tip",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 1.5f,
                    streakColor);

                launchStreaks.Add(new LaunchStreak
                {
                    Transform = streak.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = streak.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateBatteryFlashes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerFlash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.84f, 0.94f, 1f, 0.88f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.9f, 0.98f, 1f, 0.86f),
                _ => new Color(0.86f, 0.96f, 1f, 0.88f)
            };
            Color enemyFlash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.76f, 0.42f, 0.88f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.84f, 0.5f, 0.86f),
                _ => new Color(1f, 0.8f, 0.46f, 0.88f)
            };

            int flashCount = 8;
            for (int index = 0; index < flashCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MinX - 150f : mapProfile.MaxX + 150f,
                    11f + (index % 3) * 1.6f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.18f + index * 0.08f));

                GameObject flash = new($"Battery Flash {index + 1}");
                flash.transform.SetParent(root);
                flash.transform.position = basePosition;
                flash.transform.rotation = Quaternion.identity;
                flash.transform.localScale = Vector3.one;

                Color flashColor = playerSide ? playerFlash : enemyFlash;
                CreateBatteryFlashElement(
                    flash.transform,
                    "Battery Flash Halo",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(5.2f, 0.05f, 5.2f),
                    new Color(flashColor.r, flashColor.g, flashColor.b, 0.58f));
                CreateBatteryFlashElement(
                    flash.transform,
                    "Battery Flash Burst",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1.4f, 0f),
                    Vector3.one * 2.6f,
                    flashColor);

                batteryFlashes.Add(new BatteryFlash
                {
                    Transform = flash.transform,
                    BasePosition = basePosition,
                    BaseScale = flash.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateBarrageImpacts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerImpact = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.88f, 0.96f, 1f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.94f, 0.99f, 1f, 0.84f),
                _ => new Color(0.9f, 0.97f, 1f, 0.86f)
            };
            Color enemyImpact = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.82f, 0.48f, 0.86f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.88f, 0.58f, 0.84f),
                _ => new Color(1f, 0.84f, 0.52f, 0.86f)
            };

            int impactCount = 8;
            for (int index = 0; index < impactCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 170f : mapProfile.MinX - 170f,
                    10f + (index % 3) * 1.4f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.16f + index * 0.085f));

                GameObject impact = new($"Barrage Impact {index + 1}");
                impact.transform.SetParent(root);
                impact.transform.position = basePosition;
                impact.transform.rotation = Quaternion.identity;
                impact.transform.localScale = Vector3.one;

                Color impactColor = playerSide ? playerImpact : enemyImpact;
                CreateBarrageImpactElement(
                    impact.transform,
                    "Barrage Impact Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(7.4f, 0.05f, 7.4f),
                    new Color(impactColor.r, impactColor.g, impactColor.b, 0.58f));
                CreateBarrageImpactElement(
                    impact.transform,
                    "Barrage Impact Bloom",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1.8f, 0f),
                    Vector3.one * 3.6f,
                    impactColor);
                CreateBarrageImpactElement(
                    impact.transform,
                    "Barrage Impact Smoke",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 5f, 0f),
                    new Vector3(2.8f, 4.2f, 2.8f),
                    new Color(impactColor.r * 0.72f, impactColor.g * 0.72f, impactColor.b * 0.72f, 0.44f));

                barrageImpacts.Add(new BarrageImpact
                {
                    Transform = impact.transform,
                    BasePosition = basePosition,
                    BaseScale = impact.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateTargetDesignators()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDesignator = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.72f, 0.92f, 1f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.86f, 0.98f, 1f, 0.8f),
                _ => new Color(0.78f, 0.94f, 1f, 0.82f)
            };
            Color enemyDesignator = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.74f, 0.42f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.86f, 0.54f, 0.8f),
                _ => new Color(1f, 0.78f, 0.48f, 0.82f)
            };

            int designatorCount = 8;
            for (int index = 0; index < designatorCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 150f : mapProfile.MinX - 150f,
                    8f + (index % 2) * 0.8f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.18f + index * 0.08f));

                GameObject designator = new($"Target Designator {index + 1}");
                designator.transform.SetParent(root);
                designator.transform.position = basePosition;
                designator.transform.rotation = Quaternion.identity;
                designator.transform.localScale = Vector3.one;

                Color designatorColor = playerSide ? playerDesignator : enemyDesignator;
                CreateTargetDesignatorElement(
                    designator.transform,
                    "Target Designator Beam",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 8f, 0f),
                    new Vector3(0.7f, 8f, 0.7f),
                    new Color(designatorColor.r, designatorColor.g, designatorColor.b, 0.42f));
                CreateTargetDesignatorElement(
                    designator.transform,
                    "Target Designator Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 16.2f, 0f),
                    Vector3.one * 2.2f,
                    designatorColor);
                CreateTargetDesignatorElement(
                    designator.transform,
                    "Target Designator Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(4.8f, 0.04f, 4.8f),
                    new Color(designatorColor.r, designatorColor.g, designatorColor.b, 0.52f));

                targetDesignators.Add(new TargetDesignator
                {
                    Transform = designator.transform,
                    BasePosition = basePosition,
                    BaseScale = designator.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateCounterScans()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerScan = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.62f, 0.88f, 1f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.82f, 0.98f, 1f, 0.78f),
                _ => new Color(0.68f, 0.92f, 1f, 0.8f)
            };
            Color enemyScan = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.7f, 0.4f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.84f, 0.56f, 0.78f),
                _ => new Color(1f, 0.76f, 0.46f, 0.8f)
            };

            int scanCount = 6;
            for (int index = 0; index < scanCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 130f : mapProfile.MinX - 130f,
                    14f + (index % 3) * 2f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.22f + index * 0.1f));
                Quaternion baseRotation = Quaternion.Euler(0f, playerSide ? -90f : 90f, 0f);

                GameObject scan = new($"Counter Scan {index + 1}");
                scan.transform.SetParent(root);
                scan.transform.position = basePosition;
                scan.transform.rotation = baseRotation;
                scan.transform.localScale = Vector3.one;

                Color scanColor = playerSide ? playerScan : enemyScan;
                CreateCounterScanElement(
                    scan.transform,
                    "Counter Scan Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 18f),
                    new Vector3(1.2f, 0.34f, 36f),
                    new Color(scanColor.r, scanColor.g, scanColor.b, 0.34f));
                CreateCounterScanElement(
                    scan.transform,
                    "Counter Scan Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0f, 0f),
                    Vector3.one * 1.8f,
                    scanColor);
                CreateCounterScanElement(
                    scan.transform,
                    "Counter Scan Halo",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, -0.2f, 0f),
                    new Vector3(3.8f, 0.04f, 3.8f),
                    new Color(scanColor.r, scanColor.g, scanColor.b, 0.48f));

                counterScans.Add(new CounterScan
                {
                    Transform = scan.transform,
                    BasePosition = basePosition,
                    BaseScale = scan.transform.localScale,
                    BaseRotation = baseRotation,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateRelayPulses()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerRelay = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.7f, 0.92f, 1f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.88f, 0.99f, 1f, 0.8f),
                _ => new Color(0.76f, 0.95f, 1f, 0.82f)
            };
            Color enemyRelay = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.76f, 0.44f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.88f, 0.58f, 0.8f),
                _ => new Color(1f, 0.82f, 0.5f, 0.82f)
            };

            int relayCount = 6;
            for (int index = 0; index < relayCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 120f : mapProfile.MinX - 120f,
                    10f + (index % 3) * 1.8f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.2f + index * 0.1f));

                GameObject relay = new($"Relay Pulse {index + 1}");
                relay.transform.SetParent(root);
                relay.transform.position = basePosition;
                relay.transform.rotation = Quaternion.identity;
                relay.transform.localScale = Vector3.one;

                Color relayColor = playerSide ? playerRelay : enemyRelay;
                CreateRelayPulseElement(
                    relay.transform,
                    "Relay Pulse Mast",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 5f, 0f),
                    new Vector3(0.5f, 5f, 0.5f),
                    new Color(relayColor.r, relayColor.g, relayColor.b, 0.34f));
                Transform pulseNode = CreateRelayPulseElement(
                    relay.transform,
                    "Relay Pulse Node",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1f, 0f),
                    Vector3.one * 1.4f,
                    relayColor);
                CreateRelayPulseElement(
                    relay.transform,
                    "Relay Pulse Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 10.8f, 0f),
                    Vector3.one * 2f,
                    Color.Lerp(relayColor, Color.white, 0.18f));

                relayPulses.Add(new RelayPulse
                {
                    Transform = relay.transform,
                    PulseNode = pulseNode,
                    BasePosition = basePosition,
                    BaseScale = relay.transform.localScale,
                    PulseBaseLocalPosition = pulseNode != null ? pulseNode.localPosition : new Vector3(0f, 1f, 0f),
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateResponseArcs()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerArc = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.74f, 0.94f, 1f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.9f, 1f, 1f, 0.8f),
                _ => new Color(0.8f, 0.96f, 1f, 0.82f)
            };
            Color enemyArc = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.78f, 0.46f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.9f, 0.62f, 0.8f),
                _ => new Color(1f, 0.84f, 0.54f, 0.82f)
            };

            int arcCount = 6;
            for (int index = 0; index < arcCount; index++)
            {
                bool playerSide = index % 2 == 0;
                float laneT = 0.18f + index * 0.1f;
                Vector3 startPosition = new Vector3(
                    playerSide ? mapProfile.MaxX + 110f : mapProfile.MinX - 110f,
                    22f + (index % 3) * 3f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 260f - (index % 2) * 90f : mapProfile.MinX + 260f + (index % 2) * 90f,
                    54f + (index % 2) * 8f,
                    startPosition.z + (playerSide ? -26f : 26f));

                GameObject arc = new($"Response Arc {index + 1}");
                arc.transform.SetParent(root);
                arc.transform.position = startPosition;
                arc.transform.rotation = Quaternion.identity;
                arc.transform.localScale = Vector3.one;

                Color arcColor = playerSide ? playerArc : enemyArc;
                CreateResponseArcElement(
                    arc.transform,
                    "Response Arc Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -4.2f),
                    new Vector3(0.42f, 0.42f, 7.8f),
                    new Color(arcColor.r, arcColor.g, arcColor.b, 0.48f));
                CreateResponseArcElement(
                    arc.transform,
                    "Response Arc Tip",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 1.4f,
                    arcColor);

                responseArcs.Add(new ResponseArc
                {
                    Transform = arc.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = arc.transform.localScale,
                    ApexHeight = 12f + (index % 3) * 3f,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateCommandEchos()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerEcho = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.78f, 0.96f, 1f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.92f, 1f, 1f, 0.82f),
                _ => new Color(0.84f, 0.97f, 1f, 0.84f)
            };
            Color enemyEcho = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.82f, 0.5f, 0.84f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.92f, 0.66f, 0.82f),
                _ => new Color(1f, 0.86f, 0.58f, 0.84f)
            };

            int echoCount = 6;
            for (int index = 0; index < echoCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 320f - (index % 2) * 80f : mapProfile.MinX + 320f + (index % 2) * 80f,
                    18f + (index % 3) * 2.2f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.2f + index * 0.1f));

                GameObject echo = new($"Command Echo {index + 1}");
                echo.transform.SetParent(root);
                echo.transform.position = basePosition;
                echo.transform.rotation = Quaternion.identity;
                echo.transform.localScale = Vector3.one;

                Color echoColor = playerSide ? playerEcho : enemyEcho;
                CreateCommandEchoElement(
                    echo.transform,
                    "Command Echo Beam",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 8f, 0f),
                    new Vector3(0.72f, 8f, 0.72f),
                    new Color(echoColor.r, echoColor.g, echoColor.b, 0.4f));
                CreateCommandEchoElement(
                    echo.transform,
                    "Command Echo Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 16.6f, 0f),
                    Vector3.one * 2.4f,
                    echoColor);
                CreateCommandEchoElement(
                    echo.transform,
                    "Command Echo Halo",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(5.6f, 0.04f, 5.6f),
                    new Color(echoColor.r, echoColor.g, echoColor.b, 0.5f));

                commandEchos.Add(new CommandEcho
                {
                    Transform = echo.transform,
                    BasePosition = basePosition,
                    BaseScale = echo.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateOrderRipples()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerRipple = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.72f, 0.94f, 1f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.88f, 1f, 1f, 0.7f),
                _ => new Color(0.8f, 0.96f, 1f, 0.72f)
            };
            Color enemyRipple = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.78f, 0.48f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.9f, 0.64f, 0.7f),
                _ => new Color(1f, 0.84f, 0.56f, 0.72f)
            };

            int rippleCount = 6;
            for (int index = 0; index < rippleCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 440f - (index % 2) * 120f : mapProfile.MinX + 440f + (index % 2) * 120f,
                    6f + (index % 2) * 0.6f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.24f + index * 0.1f));

                GameObject ripple = new($"Order Ripple {index + 1}");
                ripple.transform.SetParent(root);
                ripple.transform.position = basePosition;
                ripple.transform.rotation = Quaternion.identity;
                ripple.transform.localScale = Vector3.one;

                Color rippleColor = playerSide ? playerRipple : enemyRipple;
                CreateOrderRippleElement(
                    ripple.transform,
                    "Order Ripple Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(6.4f, 0.03f, 6.4f),
                    new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0.54f));
                CreateOrderRippleElement(
                    ripple.transform,
                    "Order Ripple Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.9f, 0f),
                    Vector3.one * 1.5f,
                    rippleColor);

                orderRipples.Add(new OrderRipple
                {
                    Transform = ripple.transform,
                    BasePosition = basePosition,
                    BaseScale = ripple.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateAdvanceChevrons()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerChevron = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.78f, 0.96f, 1f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.92f, 1f, 1f, 0.76f),
                _ => new Color(0.84f, 0.98f, 1f, 0.78f)
            };
            Color enemyChevron = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.82f, 0.52f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.92f, 0.68f, 0.76f),
                _ => new Color(1f, 0.86f, 0.6f, 0.78f)
            };

            int chevronCount = 8;
            for (int index = 0; index < chevronCount; index++)
            {
                bool playerSide = index % 2 == 0;
                float laneT = 0.22f + index * 0.07f;
                Vector3 startPosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 520f - (index % 2) * 110f : mapProfile.MinX + 520f + (index % 2) * 110f,
                    7.4f + (index % 2) * 0.5f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 120f : mapProfile.MinX + 120f,
                    8.2f + (index % 2) * 0.5f,
                    startPosition.z + (playerSide ? -18f : 18f));

                GameObject chevron = new($"Advance Chevron {index + 1}");
                chevron.transform.SetParent(root);
                chevron.transform.position = startPosition;
                chevron.transform.rotation = Quaternion.identity;
                chevron.transform.localScale = Vector3.one;

                Color chevronColor = playerSide ? playerChevron : enemyChevron;
                CreateAdvanceChevronElement(
                    chevron.transform,
                    "Advance Chevron Body",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0.8f, 0.18f, 3.2f),
                    new Color(chevronColor.r, chevronColor.g, chevronColor.b, 0.56f));
                CreateAdvanceChevronElement(
                    chevron.transform,
                    "Advance Chevron Head",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.18f, 1.8f),
                    Vector3.one * 1.2f,
                    chevronColor);

                advanceChevrons.Add(new AdvanceChevron
                {
                    Transform = chevron.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = chevron.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFrontlineAcknowledges()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerAck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.82f, 0.98f, 1f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.94f, 1f, 1f, 0.8f),
                _ => new Color(0.88f, 0.99f, 1f, 0.82f)
            };
            Color enemyAck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.86f, 0.58f, 0.82f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.96f, 0.72f, 0.8f),
                _ => new Color(1f, 0.9f, 0.64f, 0.82f)
            };

            int acknowledgeCount = 8;
            for (int index = 0; index < acknowledgeCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 40f : mapProfile.MinX + 40f,
                    8.8f + (index % 2) * 0.4f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.24f + index * 0.07f));

                GameObject acknowledge = new($"Frontline Acknowledge {index + 1}");
                acknowledge.transform.SetParent(root);
                acknowledge.transform.position = basePosition;
                acknowledge.transform.rotation = Quaternion.identity;
                acknowledge.transform.localScale = Vector3.one;

                Color acknowledgeColor = playerSide ? playerAck : enemyAck;
                CreateFrontlineAcknowledgeElement(
                    acknowledge.transform,
                    "Frontline Acknowledge Beam",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 3.4f, 0f),
                    new Vector3(0.54f, 3.4f, 0.54f),
                    new Color(acknowledgeColor.r, acknowledgeColor.g, acknowledgeColor.b, 0.42f));
                CreateFrontlineAcknowledgeElement(
                    acknowledge.transform,
                    "Frontline Acknowledge Crown",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 7.2f, 0f),
                    Vector3.one * 1.7f,
                    acknowledgeColor);

                frontlineAcknowledges.Add(new FrontlineAcknowledge
                {
                    Transform = acknowledge.transform,
                    BasePosition = basePosition,
                    BaseScale = acknowledge.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateRallyStreams()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerStream = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.84f, 0.98f, 1f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.96f, 1f, 1f, 0.78f),
                _ => new Color(0.9f, 0.99f, 1f, 0.8f)
            };
            Color enemyStream = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.88f, 0.62f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.76f, 0.78f),
                _ => new Color(1f, 0.92f, 0.68f, 0.8f)
            };

            int streamCount = 8;
            for (int index = 0; index < streamCount; index++)
            {
                bool playerSide = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 startPosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 70f : mapProfile.MinX + 70f,
                    7.2f + (index % 2) * 0.4f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 520f - (index % 2) * 90f : mapProfile.MinX + 520f + (index % 2) * 90f,
                    7.6f + (index % 2) * 0.4f,
                    startPosition.z + (playerSide ? 12f : -12f));

                GameObject stream = new($"Rally Stream {index + 1}");
                stream.transform.SetParent(root);
                stream.transform.position = startPosition;
                stream.transform.rotation = Quaternion.identity;
                stream.transform.localScale = Vector3.one;

                Color streamColor = playerSide ? playerStream : enemyStream;
                CreateRallyStreamElement(
                    stream.transform,
                    "Rally Stream Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -3.8f),
                    new Vector3(0.36f, 0.16f, 6.4f),
                    new Color(streamColor.r, streamColor.g, streamColor.b, 0.48f));
                CreateRallyStreamElement(
                    stream.transform,
                    "Rally Stream Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 1.02f,
                    streamColor);

                rallyStreams.Add(new RallyStream
                {
                    Transform = stream.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = stream.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateBattlelineHandoffs()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerHandoff = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.88f, 0.99f, 1f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.96f, 1f, 1f, 0.78f),
                _ => new Color(0.92f, 1f, 1f, 0.8f)
            };
            Color enemyHandoff = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.9f, 0.66f, 0.8f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.8f, 0.78f),
                _ => new Color(1f, 0.94f, 0.72f, 0.8f)
            };

            int handoffCount = 6;
            for (int index = 0; index < handoffCount; index++)
            {
                bool playerSide = index % 2 == 0;
                Vector3 basePosition = new Vector3(
                    playerSide ? mapProfile.MaxX - 720f - (index % 2) * 120f : mapProfile.MinX + 720f + (index % 2) * 120f,
                    8.2f + (index % 2) * 0.4f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, 0.26f + index * 0.09f));

                GameObject handoff = new($"Battleline Handoff {index + 1}");
                handoff.transform.SetParent(root);
                handoff.transform.position = basePosition;
                handoff.transform.rotation = Quaternion.identity;
                handoff.transform.localScale = Vector3.one;

                Color handoffColor = playerSide ? playerHandoff : enemyHandoff;
                CreateBattlelineHandoffElement(
                    handoff.transform,
                    "Battleline Handoff Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(5.4f, 0.03f, 5.4f),
                    new Color(handoffColor.r, handoffColor.g, handoffColor.b, 0.52f));
                CreateBattlelineHandoffElement(
                    handoff.transform,
                    "Battleline Handoff Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1f, 0f),
                    Vector3.one * 1.3f,
                    handoffColor);
                CreateBattlelineHandoffElement(
                    handoff.transform,
                    "Battleline Handoff Beam",
                    PrimitiveType.Cylinder,
                    new Vector3(0f, 3.2f, 0f),
                    new Vector3(0.44f, 3.2f, 0.44f),
                    new Color(handoffColor.r, handoffColor.g, handoffColor.b, 0.38f));

                battlelineHandoffs.Add(new BattlelineHandoff
                {
                    Transform = handoff.transform,
                    BasePosition = basePosition,
                    BaseScale = handoff.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateClashPulses()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerClash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.98f, 1f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.98f, 1f, 1f, 0.76f),
                _ => new Color(0.94f, 1f, 1f, 0.78f)
            };
            Color enemyClash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.92f, 0.7f, 0.78f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.82f, 0.76f),
                _ => new Color(1f, 0.95f, 0.76f, 0.78f)
            };

            int pulseCount = 8;
            for (int index = 0; index < pulseCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -120f - (index % 2) * 70f : 120f + (index % 2) * 70f,
                    7.6f + (index % 2) * 0.3f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));

                GameObject pulse = new($"Clash Pulse {index + 1}");
                pulse.transform.SetParent(root);
                pulse.transform.position = basePosition;
                pulse.transform.rotation = Quaternion.identity;
                pulse.transform.localScale = Vector3.one;

                Color pulseColor = playerBias ? playerClash : enemyClash;
                CreateClashPulseElement(
                    pulse.transform,
                    "Clash Pulse Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(6.2f, 0.03f, 6.2f),
                    new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0.5f));
                CreateClashPulseElement(
                    pulse.transform,
                    "Clash Pulse Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1f, 0f),
                    Vector3.one * 1.2f,
                    pulseColor);
                CreateClashPulseElement(
                    pulse.transform,
                    "Clash Pulse Spark",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 2.4f, 0f),
                    Vector3.one * 0.84f,
                    Color.Lerp(pulseColor, Color.white, 0.16f));

                clashPulses.Add(new ClashPulse
                {
                    Transform = pulse.transform,
                    BasePosition = basePosition,
                    BaseScale = pulse.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateShockfrontTraces()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.92f, 0.99f, 1f, 0.74f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.98f, 1f, 1f, 0.72f),
                _ => new Color(0.96f, 1f, 1f, 0.74f)
            };
            Color enemyTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.94f, 0.74f, 0.74f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.86f, 0.72f),
                _ => new Color(1f, 0.97f, 0.8f, 0.74f)
            };

            int traceCount = 8;
            for (int index = 0; index < traceCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.26f + index * 0.06f;
                Vector3 startPosition = new Vector3(
                    playerBias ? -90f : 90f,
                    7.4f + (index % 2) * 0.25f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));
                Vector3 endPosition = new Vector3(
                    playerBias ? -360f - (index % 2) * 90f : 360f + (index % 2) * 90f,
                    7.8f + (index % 2) * 0.25f,
                    startPosition.z + (playerBias ? 10f : -10f));

                GameObject trace = new($"Shockfront Trace {index + 1}");
                trace.transform.SetParent(root);
                trace.transform.position = startPosition;
                trace.transform.rotation = Quaternion.identity;
                trace.transform.localScale = Vector3.one;

                Color traceColor = playerBias ? playerTrace : enemyTrace;
                CreateShockfrontTraceElement(
                    trace.transform,
                    "Shockfront Trace Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -3.6f),
                    new Vector3(0.28f, 0.12f, 6f),
                    new Color(traceColor.r, traceColor.g, traceColor.b, 0.44f));
                CreateShockfrontTraceElement(
                    trace.transform,
                    "Shockfront Trace Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.94f,
                    traceColor);

                shockfrontTraces.Add(new ShockfrontTrace
                {
                    Transform = trace.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = trace.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreatePressureFlares()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerFlare = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.98f, 1f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.98f, 1f, 1f, 0.72f),
                _ => new Color(0.96f, 1f, 1f, 0.72f)
            };
            Color enemyFlare = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.92f, 0.76f, 0.72f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.88f, 0.7f),
                _ => new Color(1f, 0.96f, 0.82f, 0.72f)
            };

            int flareCount = 8;
            for (int index = 0; index < flareCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -255f - (index % 3) * 28f : 255f + (index % 3) * 28f,
                    7.2f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 6f : -6f));

                GameObject flare = new($"Pressure Flare {index + 1}");
                flare.transform.SetParent(root);
                flare.transform.position = basePosition;
                flare.transform.rotation = Quaternion.identity;
                flare.transform.localScale = Vector3.one;

                Color flareColor = playerBias ? playerFlare : enemyFlare;
                CreatePressureFlareElement(
                    flare.transform,
                    "Pressure Flare Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(4.8f, 0.025f, 4.8f),
                    new Color(flareColor.r, flareColor.g, flareColor.b, 0.46f));
                CreatePressureFlareElement(
                    flare.transform,
                    "Pressure Flare Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.92f, 0f),
                    Vector3.one * 0.9f,
                    flareColor);
                CreatePressureFlareElement(
                    flare.transform,
                    "Pressure Flare Column",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.8f, 0f),
                    new Vector3(0.18f, 1.9f, 0.18f),
                    new Color(flareColor.r, flareColor.g, flareColor.b, 0.52f));

                pressureFlares.Add(new PressureFlare
                {
                    Transform = flare.transform,
                    BasePosition = basePosition,
                    BaseScale = flare.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateBraceSweeps()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSweep = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.88f, 0.98f, 1f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.98f, 1f, 1f, 0.68f),
                _ => new Color(0.94f, 1f, 1f, 0.68f)
            };
            Color enemySweep = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.93f, 0.78f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.9f, 0.68f),
                _ => new Color(1f, 0.96f, 0.84f, 0.68f)
            };

            int sweepCount = 8;
            for (int index = 0; index < sweepCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.22f + index * 0.07f;
                float anchorX = playerBias ? -292f : 292f;
                float startZ = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT);
                Vector3 startPosition = new Vector3(anchorX, 7.45f, startZ - 26f);
                Vector3 endPosition = new Vector3(anchorX, 7.45f, startZ + 26f);

                GameObject sweep = new($"Brace Sweep {index + 1}");
                sweep.transform.SetParent(root);
                sweep.transform.position = startPosition;
                sweep.transform.rotation = Quaternion.identity;
                sweep.transform.localScale = Vector3.one;

                Color sweepColor = playerBias ? playerSweep : enemySweep;
                CreateBraceSweepElement(
                    sweep.transform,
                    "Brace Sweep Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -3.1f),
                    new Vector3(0.24f, 0.11f, 5.4f),
                    new Color(sweepColor.r, sweepColor.g, sweepColor.b, 0.42f));
                CreateBraceSweepElement(
                    sweep.transform,
                    "Brace Sweep Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.82f,
                    sweepColor);

                braceSweeps.Add(new BraceSweep
                {
                    Transform = sweep.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = sweep.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateHoldBeacons()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.99f, 1f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.68f),
                _ => new Color(0.96f, 1f, 1f, 0.7f)
            };
            Color enemyBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.93f, 0.8f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.9f, 0.68f),
                _ => new Color(1f, 0.97f, 0.86f, 0.7f)
            };

            int beaconCount = 8;
            for (int index = 0; index < beaconCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.23f + index * 0.067f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -292f : 292f,
                    7.25f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT));

                GameObject beacon = new($"Hold Beacon {index + 1}");
                beacon.transform.SetParent(root);
                beacon.transform.position = basePosition;
                beacon.transform.rotation = Quaternion.identity;
                beacon.transform.localScale = Vector3.one;

                Color beaconColor = playerBias ? playerBeacon : enemyBeacon;
                CreateHoldBeaconElement(
                    beacon.transform,
                    "Hold Beacon Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(3.8f, 0.02f, 3.8f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.42f));
                CreateHoldBeaconElement(
                    beacon.transform,
                    "Hold Beacon Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.84f, 0f),
                    Vector3.one * 0.74f,
                    beaconColor);
                CreateHoldBeaconElement(
                    beacon.transform,
                    "Hold Beacon Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.85f, 0f),
                    new Vector3(0.14f, 2.1f, 0.14f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.5f));

                holdBeacons.Add(new HoldBeacon
                {
                    Transform = beacon.transform,
                    BasePosition = basePosition,
                    BaseScale = beacon.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateBulwarkLinks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerLink = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.88f, 0.98f, 1f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.98f, 1f, 1f, 0.64f),
                _ => new Color(0.94f, 1f, 1f, 0.64f)
            };
            Color enemyLink = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.93f, 0.8f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.98f, 0.9f, 0.64f),
                _ => new Color(1f, 0.96f, 0.86f, 0.64f)
            };

            int linkCount = 8;
            for (int index = 0; index < linkCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.23f + index * 0.067f;
                float edgeX = playerBias ? -256f : 256f;
                float innerX = playerBias ? -188f : 188f;
                float z = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT);
                Vector3 basePosition = new Vector3((edgeX + innerX) * 0.5f, 7.36f, z);

                GameObject link = new($"Bulwark Link {index + 1}");
                link.transform.SetParent(root);
                link.transform.position = basePosition;
                link.transform.rotation = Quaternion.identity;
                link.transform.localScale = Vector3.one;

                Color linkColor = playerBias ? playerLink : enemyLink;
                CreateBulwarkLinkElement(
                    link.transform,
                    "Bulwark Link Beam",
                    PrimitiveType.Cube,
                    Vector3.zero,
                    new Vector3(Mathf.Abs(edgeX - innerX), 0.08f, 0.28f),
                    new Color(linkColor.r, linkColor.g, linkColor.b, 0.34f));
                CreateBulwarkLinkElement(
                    link.transform,
                    "Bulwark Link Core",
                    PrimitiveType.Sphere,
                    new Vector3(playerBias ? -34f : 34f, 0.18f, 0f),
                    Vector3.one * 0.56f,
                    linkColor);
                CreateBulwarkLinkElement(
                    link.transform,
                    "Bulwark Link Tip",
                    PrimitiveType.Sphere,
                    new Vector3(playerBias ? 34f : -34f, 0.18f, 0f),
                    Vector3.one * 0.42f,
                    new Color(linkColor.r, linkColor.g, linkColor.b, 0.78f));

                bulwarkLinks.Add(new BulwarkLink
                {
                    Transform = link.transform,
                    BasePosition = basePosition,
                    BaseScale = link.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateReserveRelays()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerRelay = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.99f, 1f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.68f),
                _ => new Color(0.96f, 1f, 1f, 0.68f)
            };
            Color enemyRelay = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.94f, 0.82f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.92f, 0.68f),
                _ => new Color(1f, 0.97f, 0.88f, 0.68f)
            };

            int relayCount = 8;
            for (int index = 0; index < relayCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                float outerX = playerBias ? -188f : 188f;
                float innerX = playerBias ? -112f : 112f;
                float z = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT);
                Vector3 startPosition = new Vector3(outerX, 7.52f, z);
                Vector3 endPosition = new Vector3(innerX, 7.76f, z + (playerBias ? 8f : -8f));

                GameObject relay = new($"Reserve Relay {index + 1}");
                relay.transform.SetParent(root);
                relay.transform.position = startPosition;
                relay.transform.rotation = Quaternion.identity;
                relay.transform.localScale = Vector3.one;

                Color relayColor = playerBias ? playerRelay : enemyRelay;
                CreateReserveRelayElement(
                    relay.transform,
                    "Reserve Relay Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -2.8f),
                    new Vector3(0.22f, 0.1f, 4.9f),
                    new Color(relayColor.r, relayColor.g, relayColor.b, 0.4f));
                CreateReserveRelayElement(
                    relay.transform,
                    "Reserve Relay Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.72f,
                    relayColor);

                reserveRelays.Add(new ReserveRelay
                {
                    Transform = relay.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = relay.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateReserveAnchors()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerAnchor = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.92f, 0.99f, 1f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.7f),
                _ => new Color(0.97f, 1f, 1f, 0.7f)
            };
            Color enemyAnchor = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.95f, 0.84f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.93f, 0.7f),
                _ => new Color(1f, 0.98f, 0.9f, 0.7f)
            };

            int anchorCount = 8;
            for (int index = 0; index < anchorCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -112f : 112f,
                    7.58f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 8f : -8f));

                GameObject anchor = new($"Reserve Anchor {index + 1}");
                anchor.transform.SetParent(root);
                anchor.transform.position = basePosition;
                anchor.transform.rotation = Quaternion.identity;
                anchor.transform.localScale = Vector3.one;

                Color anchorColor = playerBias ? playerAnchor : enemyAnchor;
                CreateReserveAnchorElement(
                    anchor.transform,
                    "Reserve Anchor Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(3.2f, 0.02f, 3.2f),
                    new Color(anchorColor.r, anchorColor.g, anchorColor.b, 0.4f));
                CreateReserveAnchorElement(
                    anchor.transform,
                    "Reserve Anchor Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.76f, 0f),
                    Vector3.one * 0.68f,
                    anchorColor);
                CreateReserveAnchorElement(
                    anchor.transform,
                    "Reserve Anchor Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.6f, 0f),
                    new Vector3(0.12f, 1.7f, 0.12f),
                    new Color(anchorColor.r, anchorColor.g, anchorColor.b, 0.48f));

                reserveAnchors.Add(new ReserveAnchor
                {
                    Transform = anchor.transform,
                    BasePosition = basePosition,
                    BaseScale = anchor.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateReserveSurges()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSurge = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.99f, 1f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.68f),
                _ => new Color(0.96f, 1f, 1f, 0.68f)
            };
            Color enemySurge = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.95f, 0.84f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.93f, 0.68f),
                _ => new Color(1f, 0.98f, 0.9f, 0.68f)
            };

            int surgeCount = 8;
            for (int index = 0; index < surgeCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 startPosition = new Vector3(
                    playerBias ? -112f : 112f,
                    7.7f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 8f : -8f));
                Vector3 endPosition = new Vector3(
                    playerBias ? -48f : 48f,
                    7.92f,
                    startPosition.z + (playerBias ? 10f : -10f));

                GameObject surge = new($"Reserve Surge {index + 1}");
                surge.transform.SetParent(root);
                surge.transform.position = startPosition;
                surge.transform.rotation = Quaternion.identity;
                surge.transform.localScale = Vector3.one;

                Color surgeColor = playerBias ? playerSurge : enemySurge;
                CreateReserveSurgeElement(
                    surge.transform,
                    "Reserve Surge Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -2.9f),
                    new Vector3(0.22f, 0.1f, 5f),
                    new Color(surgeColor.r, surgeColor.g, surgeColor.b, 0.4f));
                CreateReserveSurgeElement(
                    surge.transform,
                    "Reserve Surge Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.7f,
                    surgeColor);

                reserveSurges.Add(new ReserveSurge
                {
                    Transform = surge.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = surge.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateCommitBeacons()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.92f, 0.99f, 1f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.7f),
                _ => new Color(0.97f, 1f, 1f, 0.7f)
            };
            Color enemyBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.95f, 0.84f, 0.7f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.93f, 0.7f),
                _ => new Color(1f, 0.98f, 0.9f, 0.7f)
            };

            int beaconCount = 8;
            for (int index = 0; index < beaconCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -48f : 48f,
                    7.86f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 18f : -18f));

                GameObject beacon = new($"Commit Beacon {index + 1}");
                beacon.transform.SetParent(root);
                beacon.transform.position = basePosition;
                beacon.transform.rotation = Quaternion.identity;
                beacon.transform.localScale = Vector3.one;

                Color beaconColor = playerBias ? playerBeacon : enemyBeacon;
                CreateCommitBeaconElement(
                    beacon.transform,
                    "Commit Beacon Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(2.9f, 0.02f, 2.9f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.4f));
                CreateCommitBeaconElement(
                    beacon.transform,
                    "Commit Beacon Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.68f, 0f),
                    Vector3.one * 0.62f,
                    beaconColor);
                CreateCommitBeaconElement(
                    beacon.transform,
                    "Commit Beacon Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.45f, 0f),
                    new Vector3(0.1f, 1.5f, 0.1f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.46f));

                commitBeacons.Add(new CommitBeacon
                {
                    Transform = beacon.transform,
                    BasePosition = basePosition,
                    BaseScale = beacon.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateForwardSpills()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSpill = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.92f, 0.99f, 1f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.66f),
                _ => new Color(0.97f, 1f, 1f, 0.66f)
            };
            Color enemySpill = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.95f, 0.84f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.93f, 0.66f),
                _ => new Color(1f, 0.98f, 0.9f, 0.66f)
            };

            int spillCount = 8;
            for (int index = 0; index < spillCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 startPosition = new Vector3(
                    playerBias ? -48f : 48f,
                    7.92f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 18f : -18f));
                Vector3 endPosition = new Vector3(
                    playerBias ? -18f : 18f,
                    8.08f,
                    startPosition.z + (playerBias ? 8f : -8f));

                GameObject spill = new($"Forward Spill {index + 1}");
                spill.transform.SetParent(root);
                spill.transform.position = startPosition;
                spill.transform.rotation = Quaternion.identity;
                spill.transform.localScale = Vector3.one;

                Color spillColor = playerBias ? playerSpill : enemySpill;
                CreateForwardSpillElement(
                    spill.transform,
                    "Forward Spill Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -2.4f),
                    new Vector3(0.2f, 0.09f, 4.2f),
                    new Color(spillColor.r, spillColor.g, spillColor.b, 0.38f));
                CreateForwardSpillElement(
                    spill.transform,
                    "Forward Spill Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.62f,
                    spillColor);

                forwardSpills.Add(new ForwardSpill
                {
                    Transform = spill.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = spill.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateEdgeClashes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerClash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.94f, 0.99f, 1f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.68f),
                _ => new Color(0.98f, 1f, 1f, 0.68f)
            };
            Color enemyClash = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.96f, 0.86f, 0.68f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.95f, 0.68f),
                _ => new Color(1f, 0.98f, 0.92f, 0.68f)
            };

            int clashCount = 8;
            for (int index = 0; index < clashCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -18f : 18f,
                    8.02f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 26f : -26f));

                GameObject clash = new($"Edge Clash {index + 1}");
                clash.transform.SetParent(root);
                clash.transform.position = basePosition;
                clash.transform.rotation = Quaternion.identity;
                clash.transform.localScale = Vector3.one;

                Color clashColor = playerBias ? playerClash : enemyClash;
                CreateEdgeClashElement(
                    clash.transform,
                    "Edge Clash Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(2.4f, 0.02f, 2.4f),
                    new Color(clashColor.r, clashColor.g, clashColor.b, 0.38f));
                CreateEdgeClashElement(
                    clash.transform,
                    "Edge Clash Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.54f, 0f),
                    Vector3.one * 0.52f,
                    clashColor);
                CreateEdgeClashElement(
                    clash.transform,
                    "Edge Clash Spark",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 1.18f, 0f),
                    Vector3.one * 0.3f,
                    new Color(clashColor.r, clashColor.g, clashColor.b, 0.82f));

                edgeClashes.Add(new EdgeClash
                {
                    Transform = clash.transform,
                    BasePosition = basePosition,
                    BaseScale = clash.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateReboundTraces()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.99f, 1f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.64f),
                _ => new Color(0.96f, 1f, 1f, 0.64f)
            };
            Color enemyTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.95f, 0.86f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.94f, 0.64f),
                _ => new Color(1f, 0.98f, 0.91f, 0.64f)
            };

            int traceCount = 8;
            for (int index = 0; index < traceCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 startPosition = new Vector3(
                    playerBias ? -18f : 18f,
                    8.06f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 26f : -26f));
                Vector3 endPosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.96f,
                    startPosition.z + (playerBias ? -6f : 6f));

                GameObject trace = new($"Rebound Trace {index + 1}");
                trace.transform.SetParent(root);
                trace.transform.position = startPosition;
                trace.transform.rotation = Quaternion.identity;
                trace.transform.localScale = Vector3.one;

                Color traceColor = playerBias ? playerTrace : enemyTrace;
                CreateReboundTraceElement(
                    trace.transform,
                    "Rebound Trace Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -2.5f),
                    new Vector3(0.18f, 0.08f, 4.4f),
                    new Color(traceColor.r, traceColor.g, traceColor.b, 0.36f));
                CreateReboundTraceElement(
                    trace.transform,
                    "Rebound Trace Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.56f,
                    traceColor);

                reboundTraces.Add(new ReboundTrace
                {
                    Transform = trace.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = trace.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFallbackBeacons()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.91f, 0.99f, 1f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.66f),
                _ => new Color(0.97f, 1f, 1f, 0.66f)
            };
            Color enemyBeacon = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.96f, 0.88f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.95f, 0.66f),
                _ => new Color(1f, 0.98f, 0.92f, 0.66f)
            };

            int beaconCount = 8;
            for (int index = 0; index < beaconCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.9f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f));

                GameObject beacon = new($"Fallback Beacon {index + 1}");
                beacon.transform.SetParent(root);
                beacon.transform.position = basePosition;
                beacon.transform.rotation = Quaternion.identity;
                beacon.transform.localScale = Vector3.one;

                Color beaconColor = playerBias ? playerBeacon : enemyBeacon;
                CreateFallbackBeaconElement(
                    beacon.transform,
                    "Fallback Beacon Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(2.6f, 0.02f, 2.6f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.36f));
                CreateFallbackBeaconElement(
                    beacon.transform,
                    "Fallback Beacon Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.58f, 0f),
                    Vector3.one * 0.5f,
                    beaconColor);
                CreateFallbackBeaconElement(
                    beacon.transform,
                    "Fallback Beacon Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.22f, 0f),
                    new Vector3(0.08f, 1.22f, 0.08f),
                    new Color(beaconColor.r, beaconColor.g, beaconColor.b, 0.42f));

                fallbackBeacons.Add(new FallbackBeacon
                {
                    Transform = beacon.transform,
                    BasePosition = basePosition,
                    BaseScale = beacon.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFallbackSweeps()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSweep = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.91f, 0.99f, 1f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.64f),
                _ => new Color(0.97f, 1f, 1f, 0.64f)
            };
            Color enemySweep = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.96f, 0.88f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.95f, 0.64f),
                _ => new Color(1f, 0.98f, 0.92f, 0.64f)
            };

            int sweepCount = 8;
            for (int index = 0; index < sweepCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                float anchorX = playerBias ? -62f : 62f;
                float baseZ = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f);
                Vector3 startPosition = new Vector3(anchorX, 7.96f, baseZ - 18f);
                Vector3 endPosition = new Vector3(anchorX, 7.96f, baseZ + 18f);

                GameObject sweep = new($"Fallback Sweep {index + 1}");
                sweep.transform.SetParent(root);
                sweep.transform.position = startPosition;
                sweep.transform.rotation = Quaternion.identity;
                sweep.transform.localScale = Vector3.one;

                Color sweepColor = playerBias ? playerSweep : enemySweep;
                CreateFallbackSweepElement(
                    sweep.transform,
                    "Fallback Sweep Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -2.3f),
                    new Vector3(0.18f, 0.08f, 4f),
                    new Color(sweepColor.r, sweepColor.g, sweepColor.b, 0.36f));
                CreateFallbackSweepElement(
                    sweep.transform,
                    "Fallback Sweep Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.54f,
                    sweepColor);

                fallbackSweeps.Add(new FallbackSweep
                {
                    Transform = sweep.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = sweep.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateRecoveryLattices()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerLattice = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.9f, 0.99f, 1f, 0.62f),
                BattlefieldTheme.PaleSaltFlats => new Color(0.99f, 1f, 1f, 0.62f),
                _ => new Color(0.97f, 1f, 1f, 0.62f)
            };
            Color enemyLattice = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.96f, 0.9f, 0.62f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 0.99f, 0.96f, 0.62f),
                _ => new Color(1f, 0.98f, 0.93f, 0.62f)
            };

            int latticeCount = 8;
            for (int index = 0; index < latticeCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.92f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f));

                GameObject lattice = new($"Recovery Lattice {index + 1}");
                lattice.transform.SetParent(root);
                lattice.transform.position = basePosition;
                lattice.transform.rotation = Quaternion.identity;
                lattice.transform.localScale = Vector3.one;

                Color latticeColor = playerBias ? playerLattice : enemyLattice;
                CreateRecoveryLatticeElement(
                    lattice.transform,
                    "Recovery Lattice Beam",
                    PrimitiveType.Cube,
                    Vector3.zero,
                    new Vector3(0.1f, 0.07f, 11.5f),
                    new Color(latticeColor.r, latticeColor.g, latticeColor.b, 0.28f));
                CreateRecoveryLatticeElement(
                    lattice.transform,
                    "Recovery Lattice Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.16f, 0f),
                    Vector3.one * 0.44f,
                    latticeColor);
                CreateRecoveryLatticeElement(
                    lattice.transform,
                    "Recovery Lattice Tip A",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.12f, -5.7f),
                    Vector3.one * 0.26f,
                    new Color(latticeColor.r, latticeColor.g, latticeColor.b, 0.78f));
                CreateRecoveryLatticeElement(
                    lattice.transform,
                    "Recovery Lattice Tip B",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.12f, 5.7f),
                    Vector3.one * 0.26f,
                    new Color(latticeColor.r, latticeColor.g, latticeColor.b, 0.78f));

                recoveryLattices.Add(new RecoveryLattice
                {
                    Transform = lattice.transform,
                    BasePosition = basePosition,
                    BaseScale = lattice.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateStabilityPulses()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerPulse = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.92f, 0.99f, 1f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.66f),
                _ => new Color(0.98f, 1f, 1f, 0.66f)
            };
            Color enemyPulse = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.97f, 0.9f, 0.66f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.96f, 0.66f),
                _ => new Color(1f, 0.99f, 0.94f, 0.66f)
            };

            int pulseCount = 8;
            for (int index = 0; index < pulseCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                float x = playerBias ? -62f : 62f;
                float centerZ = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f);
                Vector3 startPosition = new Vector3(x, 8.02f, centerZ - 6.4f);
                Vector3 endPosition = new Vector3(x, 8.02f, centerZ + 6.4f);

                GameObject pulse = new($"Stability Pulse {index + 1}");
                pulse.transform.SetParent(root);
                pulse.transform.position = startPosition;
                pulse.transform.rotation = Quaternion.identity;
                pulse.transform.localScale = Vector3.one;

                Color pulseColor = playerBias ? playerPulse : enemyPulse;
                CreateStabilityPulseElement(
                    pulse.transform,
                    "Stability Pulse Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -1.9f),
                    new Vector3(0.14f, 0.07f, 3.3f),
                    new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0.34f));
                CreateStabilityPulseElement(
                    pulse.transform,
                    "Stability Pulse Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.42f,
                    pulseColor);

                stabilityPulses.Add(new StabilityPulse
                {
                    Transform = pulse.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = pulse.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSentinelEchos()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerEcho = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.94f, 1f, 1f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.64f),
                _ => new Color(0.99f, 1f, 1f, 0.64f)
            };
            Color enemyEcho = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.98f, 0.92f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.97f, 0.64f),
                _ => new Color(1f, 0.99f, 0.95f, 0.64f)
            };

            int echoCount = 8;
            for (int index = 0; index < echoCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -74f : 74f,
                    8.04f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f));

                GameObject echo = new($"Sentinel Echo {index + 1}");
                echo.transform.SetParent(root);
                echo.transform.position = basePosition;
                echo.transform.rotation = Quaternion.identity;
                echo.transform.localScale = Vector3.one;

                Color echoColor = playerBias ? playerEcho : enemyEcho;
                CreateSentinelEchoElement(
                    echo.transform,
                    "Sentinel Echo Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(2.1f, 0.02f, 2.1f),
                    new Color(echoColor.r, echoColor.g, echoColor.b, 0.34f));
                CreateSentinelEchoElement(
                    echo.transform,
                    "Sentinel Echo Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.46f, 0f),
                    Vector3.one * 0.38f,
                    echoColor);
                CreateSentinelEchoElement(
                    echo.transform,
                    "Sentinel Echo Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 1.02f, 0f),
                    new Vector3(0.06f, 0.92f, 0.06f),
                    new Color(echoColor.r, echoColor.g, echoColor.b, 0.4f));

                sentinelEchos.Add(new SentinelEcho
                {
                    Transform = echo.transform,
                    BasePosition = basePosition,
                    BaseScale = echo.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSentinelReturns()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerReturn = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.93f, 1f, 1f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.64f),
                _ => new Color(0.99f, 1f, 1f, 0.64f)
            };
            Color enemyReturn = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.98f, 0.92f, 0.64f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.97f, 0.64f),
                _ => new Color(1f, 0.99f, 0.95f, 0.64f)
            };

            int returnCount = 8;
            for (int index = 0; index < returnCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 startPosition = new Vector3(
                    playerBias ? -74f : 74f,
                    8.04f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 20f : -20f));
                Vector3 endPosition = new Vector3(
                    playerBias ? -62f : 62f,
                    8.01f,
                    startPosition.z + (playerBias ? -4f : 4f));

                GameObject signalReturn = new($"Sentinel Return {index + 1}");
                signalReturn.transform.SetParent(root);
                signalReturn.transform.position = startPosition;
                signalReturn.transform.rotation = Quaternion.identity;
                signalReturn.transform.localScale = Vector3.one;

                Color returnColor = playerBias ? playerReturn : enemyReturn;
                CreateSentinelReturnElement(
                    signalReturn.transform,
                    "Sentinel Return Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -1.7f),
                    new Vector3(0.12f, 0.06f, 3f),
                    new Color(returnColor.r, returnColor.g, returnColor.b, 0.32f));
                CreateSentinelReturnElement(
                    signalReturn.transform,
                    "Sentinel Return Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.36f,
                    returnColor);

                sentinelReturns.Add(new SentinelReturn
                {
                    Transform = signalReturn.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = signalReturn.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateCircuitSeals()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSeal = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.94f, 1f, 1f, 0.62f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.62f),
                _ => new Color(0.99f, 1f, 1f, 0.62f)
            };
            Color enemySeal = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.98f, 0.93f, 0.62f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.98f, 0.62f),
                _ => new Color(1f, 0.99f, 0.96f, 0.62f)
            };

            int sealCount = 8;
            for (int index = 0; index < sealCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    8.02f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject seal = new($"Circuit Seal {index + 1}");
                seal.transform.SetParent(root);
                seal.transform.position = basePosition;
                seal.transform.rotation = Quaternion.identity;
                seal.transform.localScale = Vector3.one;

                Color sealColor = playerBias ? playerSeal : enemySeal;
                CreateCircuitSealElement(
                    seal.transform,
                    "Circuit Seal Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(1.8f, 0.02f, 1.8f),
                    new Color(sealColor.r, sealColor.g, sealColor.b, 0.32f));
                CreateCircuitSealElement(
                    seal.transform,
                    "Circuit Seal Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.38f, 0f),
                    Vector3.one * 0.34f,
                    sealColor);
                CreateCircuitSealElement(
                    seal.transform,
                    "Circuit Seal Beam",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0.82f, 0f),
                    new Vector3(0.05f, 0.74f, 0.05f),
                    new Color(sealColor.r, sealColor.g, sealColor.b, 0.36f));

                circuitSeals.Add(new CircuitSeal
                {
                    Transform = seal.transform,
                    BasePosition = basePosition,
                    BaseScale = seal.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSealRipples()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerRipple = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.95f, 1f, 1f, 0.58f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.58f),
                _ => new Color(1f, 1f, 1f, 0.58f)
            };
            Color enemyRipple = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.99f, 0.94f, 0.58f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.99f, 0.58f),
                _ => new Color(1f, 1f, 0.97f, 0.58f)
            };

            int rippleCount = 8;
            for (int index = 0; index < rippleCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    8f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject ripple = new($"Seal Ripple {index + 1}");
                ripple.transform.SetParent(root);
                ripple.transform.position = basePosition;
                ripple.transform.rotation = Quaternion.identity;
                ripple.transform.localScale = Vector3.one;

                Color rippleColor = playerBias ? playerRipple : enemyRipple;
                CreateSealRippleElement(
                    ripple.transform,
                    "Seal Ripple Ring",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(1.4f, 0.015f, 1.4f),
                    new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0.3f));
                CreateSealRippleElement(
                    ripple.transform,
                    "Seal Ripple Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.18f, 0f),
                    Vector3.one * 0.22f,
                    rippleColor);

                sealRipples.Add(new SealRipple
                {
                    Transform = ripple.transform,
                    BasePosition = basePosition,
                    BaseScale = ripple.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSealAfterglows()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerGlow = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.95f, 1f, 1f, 0.52f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.52f),
                _ => new Color(1f, 1f, 1f, 0.52f)
            };
            Color enemyGlow = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.99f, 0.95f, 0.52f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.99f, 0.52f),
                _ => new Color(1f, 1f, 0.98f, 0.52f)
            };

            int glowCount = 8;
            for (int index = 0; index < glowCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    8.01f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject glow = new($"Seal Afterglow {index + 1}");
                glow.transform.SetParent(root);
                glow.transform.position = basePosition;
                glow.transform.rotation = Quaternion.identity;
                glow.transform.localScale = Vector3.one;

                Color glowColor = playerBias ? playerGlow : enemyGlow;
                CreateSealAfterglowElement(
                    glow.transform,
                    "Seal Afterglow Disc",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(1.1f, 0.012f, 1.1f),
                    new Color(glowColor.r, glowColor.g, glowColor.b, 0.24f));
                CreateSealAfterglowElement(
                    glow.transform,
                    "Seal Afterglow Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.08f, 0f),
                    Vector3.one * 0.16f,
                    glowColor);

                sealAfterglows.Add(new SealAfterglow
                {
                    Transform = glow.transform,
                    BasePosition = basePosition,
                    BaseScale = glow.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateAfterglowDrifts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDrift = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.96f, 1f, 1f, 0.5f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.5f),
                _ => new Color(1f, 1f, 1f, 0.5f)
            };
            Color enemyDrift = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.99f, 0.96f, 0.5f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.99f, 0.5f),
                _ => new Color(1f, 1f, 0.98f, 0.5f)
            };

            int driftCount = 8;
            for (int index = 0; index < driftCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                float x = playerBias ? -62f : 62f;
                float centerZ = Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f);
                Vector3 startPosition = new Vector3(x, 8.02f, centerZ - 2.2f);
                Vector3 endPosition = new Vector3(x, 8.02f, centerZ + 2.2f);

                GameObject drift = new($"Afterglow Drift {index + 1}");
                drift.transform.SetParent(root);
                drift.transform.position = startPosition;
                drift.transform.rotation = Quaternion.identity;
                drift.transform.localScale = Vector3.one;

                Color driftColor = playerBias ? playerDrift : enemyDrift;
                CreateAfterglowDriftElement(
                    drift.transform,
                    "Afterglow Drift Trail",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, -1.2f),
                    new Vector3(0.08f, 0.05f, 2f),
                    new Color(driftColor.r, driftColor.g, driftColor.b, 0.26f));
                CreateAfterglowDriftElement(
                    drift.transform,
                    "Afterglow Drift Core",
                    PrimitiveType.Sphere,
                    Vector3.zero,
                    Vector3.one * 0.2f,
                    driftColor);

                afterglowDrifts.Add(new AfterglowDrift
                {
                    Transform = drift.transform,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BaseScale = drift.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateDormantVeils()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerVeil = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.97f, 1f, 1f, 0.46f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.46f),
                _ => new Color(1f, 1f, 1f, 0.46f)
            };
            Color enemyVeil = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.995f, 0.97f, 0.46f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.995f, 0.46f),
                _ => new Color(1f, 1f, 0.99f, 0.46f)
            };

            int veilCount = 8;
            for (int index = 0; index < veilCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    8f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject veil = new($"Dormant Veil {index + 1}");
                veil.transform.SetParent(root);
                veil.transform.position = basePosition;
                veil.transform.rotation = Quaternion.identity;
                veil.transform.localScale = Vector3.one;

                Color veilColor = playerBias ? playerVeil : enemyVeil;
                CreateDormantVeilElement(
                    veil.transform,
                    "Dormant Veil Disc",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.78f, 0.01f, 0.78f),
                    new Color(veilColor.r, veilColor.g, veilColor.b, 0.18f));
                CreateDormantVeilElement(
                    veil.transform,
                    "Dormant Veil Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.04f, 0f),
                    Vector3.one * 0.1f,
                    veilColor);

                dormantVeils.Add(new DormantVeil
                {
                    Transform = veil.transform,
                    BasePosition = basePosition,
                    BaseScale = veil.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateQuietResidues()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerResidue = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(0.98f, 1f, 1f, 0.42f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.42f),
                _ => new Color(1f, 1f, 1f, 0.42f)
            };
            Color enemyResidue = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 0.998f, 0.98f, 0.42f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 0.998f, 0.42f),
                _ => new Color(1f, 1f, 0.992f, 0.42f)
            };

            int residueCount = 8;
            for (int index = 0; index < residueCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.995f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject residue = new($"Quiet Residue {index + 1}");
                residue.transform.SetParent(root);
                residue.transform.position = basePosition;
                residue.transform.rotation = Quaternion.identity;
                residue.transform.localScale = Vector3.one;

                Color residueColor = playerBias ? playerResidue : enemyResidue;
                CreateQuietResidueElement(
                    residue.transform,
                    "Quiet Residue Disc",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.52f, 0.008f, 0.52f),
                    new Color(residueColor.r, residueColor.g, residueColor.b, 0.14f));
                CreateQuietResidueElement(
                    residue.transform,
                    "Quiet Residue Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.025f, 0f),
                    Vector3.one * 0.06f,
                    residueColor);

                quietResidues.Add(new QuietResidue
                {
                    Transform = residue.transform,
                    BasePosition = basePosition,
                    BaseScale = residue.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateResidualBlinks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerBlink = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.38f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.38f),
                _ => new Color(1f, 1f, 1f, 0.38f)
            };
            Color enemyBlink = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.99f, 0.38f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.38f),
                _ => new Color(1f, 1f, 0.995f, 0.38f)
            };

            int blinkCount = 8;
            for (int index = 0; index < blinkCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.996f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject blink = new($"Residual Blink {index + 1}");
                blink.transform.SetParent(root);
                blink.transform.position = basePosition;
                blink.transform.rotation = Quaternion.identity;
                blink.transform.localScale = Vector3.one;

                Color blinkColor = playerBias ? playerBlink : enemyBlink;
                CreateResidualBlinkElement(
                    blink.transform,
                    "Residual Blink Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.014f, 0f),
                    Vector3.one * 0.038f,
                    blinkColor);

                residualBlinks.Add(new ResidualBlink
                {
                    Transform = blink.transform,
                    BasePosition = basePosition,
                    BaseScale = blink.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateLastEmbers()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerEmber = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.3f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.3f),
                _ => new Color(1f, 1f, 1f, 0.3f)
            };
            Color enemyEmber = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.995f, 0.3f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.3f),
                _ => new Color(1f, 1f, 0.997f, 0.3f)
            };

            int emberCount = 8;
            for (int index = 0; index < emberCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.995f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject ember = new($"Last Ember {index + 1}");
                ember.transform.SetParent(root);
                ember.transform.position = basePosition;
                ember.transform.rotation = Quaternion.identity;
                ember.transform.localScale = Vector3.one;

                Color emberColor = playerBias ? playerEmber : enemyEmber;
                CreateLastEmberElement(
                    ember.transform,
                    "Last Ember Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.008f, 0f),
                    Vector3.one * 0.022f,
                    emberColor);

                lastEmbers.Add(new LastEmber
                {
                    Transform = ember.transform,
                    BasePosition = basePosition,
                    BaseScale = ember.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFinalHushes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerHush = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.24f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.24f),
                _ => new Color(1f, 1f, 1f, 0.24f)
            };
            Color enemyHush = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.998f, 0.24f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.24f),
                _ => new Color(1f, 1f, 0.999f, 0.24f)
            };

            int hushCount = 8;
            for (int index = 0; index < hushCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.994f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject hush = new($"Final Hush {index + 1}");
                hush.transform.SetParent(root);
                hush.transform.position = basePosition;
                hush.transform.rotation = Quaternion.identity;
                hush.transform.localScale = Vector3.one;

                Color hushColor = playerBias ? playerHush : enemyHush;
                CreateFinalHushElement(
                    hush.transform,
                    "Final Hush Disc",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.34f, 0.006f, 0.34f),
                    new Color(hushColor.r, hushColor.g, hushColor.b, 0.1f));
                CreateFinalHushElement(
                    hush.transform,
                    "Final Hush Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.006f, 0f),
                    Vector3.one * 0.028f,
                    hushColor);

                finalHushes.Add(new FinalHush
                {
                    Transform = hush.transform,
                    BasePosition = basePosition,
                    BaseScale = hush.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateStillTraces()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.18f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.18f),
                _ => new Color(1f, 1f, 1f, 0.18f)
            };
            Color enemyTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.18f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.18f),
                _ => new Color(1f, 1f, 0.999f, 0.18f)
            };

            int traceCount = 8;
            for (int index = 0; index < traceCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.993f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject trace = new($"Still Trace {index + 1}");
                trace.transform.SetParent(root);
                trace.transform.position = basePosition;
                trace.transform.rotation = Quaternion.identity;
                trace.transform.localScale = Vector3.one;

                Color traceColor = playerBias ? playerTrace : enemyTrace;
                CreateStillTraceElement(
                    trace.transform,
                    "Still Trace Disc",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.22f, 0.004f, 0.22f),
                    new Color(traceColor.r, traceColor.g, traceColor.b, 0.06f));
                CreateStillTraceElement(
                    trace.transform,
                    "Still Trace Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.003f, 0f),
                    Vector3.one * 0.014f,
                    traceColor);

                stillTraces.Add(new StillTrace
                {
                    Transform = trace.transform,
                    BasePosition = basePosition,
                    BaseScale = trace.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSettledSpecks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.12f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.12f),
                _ => new Color(1f, 1f, 1f, 0.12f)
            };
            Color enemySpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.12f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.12f),
                _ => new Color(1f, 1f, 0.999f, 0.12f)
            };

            int speckCount = 8;
            for (int index = 0; index < speckCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.9925f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject speck = new($"Settled Speck {index + 1}");
                speck.transform.SetParent(root);
                speck.transform.position = basePosition;
                speck.transform.rotation = Quaternion.identity;
                speck.transform.localScale = Vector3.one;

                Color speckColor = playerBias ? playerSpeck : enemySpeck;
                CreateSettledSpeckElement(
                    speck.transform,
                    "Settled Speck Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.002f, 0f),
                    Vector3.one * 0.01f,
                    speckColor);

                settledSpecks.Add(new SettledSpeck
                {
                    Transform = speck.transform,
                    BasePosition = basePosition,
                    BaseScale = speck.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSilentGrains()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerGrain = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.08f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.08f),
                _ => new Color(1f, 1f, 1f, 0.08f)
            };
            Color enemyGrain = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.08f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.08f),
                _ => new Color(1f, 1f, 0.999f, 0.08f)
            };

            int grainCount = 8;
            for (int index = 0; index < grainCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.992f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject grain = new($"Silent Grain {index + 1}");
                grain.transform.SetParent(root);
                grain.transform.position = basePosition;
                grain.transform.rotation = Quaternion.identity;
                grain.transform.localScale = Vector3.one;

                Color grainColor = playerBias ? playerGrain : enemyGrain;
                CreateSilentGrainElement(
                    grain.transform,
                    "Silent Grain Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0015f, 0f),
                    Vector3.one * 0.008f,
                    grainColor);

                silentGrains.Add(new SilentGrain
                {
                    Transform = grain.transform,
                    BasePosition = basePosition,
                    BaseScale = grain.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateMuteDusts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDust = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.05f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.05f),
                _ => new Color(1f, 1f, 1f, 0.05f)
            };
            Color enemyDust = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.05f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.05f),
                _ => new Color(1f, 1f, 0.999f, 0.05f)
            };

            int dustCount = 8;
            for (int index = 0; index < dustCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.9916f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject dust = new($"Mute Dust {index + 1}");
                dust.transform.SetParent(root);
                dust.transform.position = basePosition;
                dust.transform.rotation = Quaternion.identity;
                dust.transform.localScale = Vector3.one;

                Color dustColor = playerBias ? playerDust : enemyDust;
                CreateMuteDustElement(
                    dust.transform,
                    "Mute Dust Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0012f, 0f),
                    Vector3.one * 0.006f,
                    dustColor);

                muteDusts.Add(new MuteDust
                {
                    Transform = dust.transform,
                    BasePosition = basePosition,
                    BaseScale = dust.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateStillAshes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerAsh = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.035f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.035f),
                _ => new Color(1f, 1f, 1f, 0.035f)
            };
            Color enemyAsh = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.035f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.035f),
                _ => new Color(1f, 1f, 0.999f, 0.035f)
            };

            int ashCount = 8;
            for (int index = 0; index < ashCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.9913f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject ash = new($"Still Ash {index + 1}");
                ash.transform.SetParent(root);
                ash.transform.position = basePosition;
                ash.transform.rotation = Quaternion.identity;
                ash.transform.localScale = Vector3.one;

                Color ashColor = playerBias ? playerAsh : enemyAsh;
                CreateStillAshElement(
                    ash.transform,
                    "Still Ash Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.001f, 0f),
                    Vector3.one * 0.005f,
                    ashColor);

                stillAshes.Add(new StillAsh
                {
                    Transform = ash.transform,
                    BasePosition = basePosition,
                    BaseScale = ash.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateColdSpecks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.025f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.025f),
                _ => new Color(1f, 1f, 1f, 0.025f)
            };
            Color enemySpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.025f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.025f),
                _ => new Color(1f, 1f, 0.999f, 0.025f)
            };

            int speckCount = 8;
            for (int index = 0; index < speckCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.9911f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject speck = new($"Cold Speck {index + 1}");
                speck.transform.SetParent(root);
                speck.transform.position = basePosition;
                speck.transform.rotation = Quaternion.identity;
                speck.transform.localScale = Vector3.one;

                Color speckColor = playerBias ? playerSpeck : enemySpeck;
                CreateColdSpeckElement(
                    speck.transform,
                    "Cold Speck Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0008f, 0f),
                    Vector3.one * 0.0042f,
                    speckColor);

                coldSpecks.Add(new ColdSpeck
                {
                    Transform = speck.transform,
                    BasePosition = basePosition,
                    BaseScale = speck.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFrostMotes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerMote = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.018f),
                _ => new Color(1f, 1f, 1f, 0.018f)
            };
            Color enemyMote = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.018f),
                _ => new Color(1f, 1f, 0.999f, 0.018f)
            };

            int moteCount = 8;
            for (int index = 0; index < moteCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99095f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject mote = new($"Frost Mote {index + 1}");
                mote.transform.SetParent(root);
                mote.transform.position = basePosition;
                mote.transform.rotation = Quaternion.identity;
                mote.transform.localScale = Vector3.one;

                Color moteColor = playerBias ? playerMote : enemyMote;
                CreateFrostMoteElement(
                    mote.transform,
                    "Frost Mote Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00065f, 0f),
                    Vector3.one * 0.0034f,
                    moteColor);

                frostMotes.Add(new FrostMote
                {
                    Transform = mote.transform,
                    BasePosition = basePosition,
                    BaseScale = mote.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateRimeSeeds()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSeed = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.013f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.013f),
                _ => new Color(1f, 1f, 1f, 0.013f)
            };
            Color enemySeed = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.013f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.013f),
                _ => new Color(1f, 1f, 0.999f, 0.013f)
            };

            int seedCount = 8;
            for (int index = 0; index < seedCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99082f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject seed = new($"Rime Seed {index + 1}");
                seed.transform.SetParent(root);
                seed.transform.position = basePosition;
                seed.transform.rotation = Quaternion.identity;
                seed.transform.localScale = Vector3.one;

                Color seedColor = playerBias ? playerSeed : enemySeed;
                CreateRimeSeedElement(
                    seed.transform,
                    "Rime Seed Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0005f, 0f),
                    Vector3.one * 0.0028f,
                    seedColor);

                rimeSeeds.Add(new RimeSeed
                {
                    Transform = seed.transform,
                    BasePosition = basePosition,
                    BaseScale = seed.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateIcePins()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerPin = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.009f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.009f),
                _ => new Color(1f, 1f, 1f, 0.009f)
            };
            Color enemyPin = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.009f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.009f),
                _ => new Color(1f, 1f, 0.999f, 0.009f)
            };

            int pinCount = 8;
            for (int index = 0; index < pinCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99072f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject pin = new($"Ice Pin {index + 1}");
                pin.transform.SetParent(root);
                pin.transform.position = basePosition;
                pin.transform.rotation = Quaternion.identity;
                pin.transform.localScale = Vector3.one;

                Color pinColor = playerBias ? playerPin : enemyPin;
                CreateIcePinElement(
                    pin.transform,
                    "Ice Pin Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0004f, 0f),
                    Vector3.one * 0.0022f,
                    pinColor);

                icePins.Add(new IcePin
                {
                    Transform = pin.transform,
                    BasePosition = basePosition,
                    BaseScale = pin.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateChillNails()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerNail = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.006f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.006f),
                _ => new Color(1f, 1f, 1f, 0.006f)
            };
            Color enemyNail = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.006f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.006f),
                _ => new Color(1f, 1f, 0.999f, 0.006f)
            };

            int nailCount = 8;
            for (int index = 0; index < nailCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99064f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject nail = new($"Chill Nail {index + 1}");
                nail.transform.SetParent(root);
                nail.transform.position = basePosition;
                nail.transform.rotation = Quaternion.identity;
                nail.transform.localScale = Vector3.one;

                Color nailColor = playerBias ? playerNail : enemyNail;
                CreateChillNailElement(
                    nail.transform,
                    "Chill Nail Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00032f, 0f),
                    Vector3.one * 0.0018f,
                    nailColor);

                chillNails.Add(new ChillNail
                {
                    Transform = nail.transform,
                    BasePosition = basePosition,
                    BaseScale = nail.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFrostTacks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTack = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.004f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.004f),
                _ => new Color(1f, 1f, 1f, 0.004f)
            };
            Color enemyTack = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.004f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.004f),
                _ => new Color(1f, 1f, 0.999f, 0.004f)
            };

            int tackCount = 8;
            for (int index = 0; index < tackCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99058f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject tack = new($"Frost Tack {index + 1}");
                tack.transform.SetParent(root);
                tack.transform.position = basePosition;
                tack.transform.rotation = Quaternion.identity;
                tack.transform.localScale = Vector3.one;

                Color tackColor = playerBias ? playerTack : enemyTack;
                CreateFrostTackElement(
                    tack.transform,
                    "Frost Tack Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00025f, 0f),
                    Vector3.one * 0.0014f,
                    tackColor);

                frostTacks.Add(new FrostTack
                {
                    Transform = tack.transform,
                    BasePosition = basePosition,
                    BaseScale = tack.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateGlazeDots()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDot = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.003f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.003f),
                _ => new Color(1f, 1f, 1f, 0.003f)
            };
            Color enemyDot = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.003f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.003f),
                _ => new Color(1f, 1f, 0.999f, 0.003f)
            };

            int dotCount = 8;
            for (int index = 0; index < dotCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99053f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject dot = new($"Glaze Dot {index + 1}");
                dot.transform.SetParent(root);
                dot.transform.position = basePosition;
                dot.transform.rotation = Quaternion.identity;
                dot.transform.localScale = Vector3.one;

                Color dotColor = playerBias ? playerDot : enemyDot;
                CreateGlazeDotElement(
                    dot.transform,
                    "Glaze Dot Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0002f, 0f),
                    Vector3.one * 0.0011f,
                    dotColor);

                glazeDots.Add(new GlazeDot
                {
                    Transform = dot.transform,
                    BasePosition = basePosition,
                    BaseScale = dot.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateHoarBeads()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerBead = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0022f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0022f),
                _ => new Color(1f, 1f, 1f, 0.0022f)
            };
            Color enemyBead = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0022f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0022f),
                _ => new Color(1f, 1f, 0.999f, 0.0022f)
            };

            int beadCount = 8;
            for (int index = 0; index < beadCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99049f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject bead = new($"Hoar Bead {index + 1}");
                bead.transform.SetParent(root);
                bead.transform.position = basePosition;
                bead.transform.rotation = Quaternion.identity;
                bead.transform.localScale = Vector3.one;

                Color beadColor = playerBias ? playerBead : enemyBead;
                CreateHoarBeadElement(
                    bead.transform,
                    "Hoar Bead Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00016f, 0f),
                    Vector3.one * 0.0009f,
                    beadColor);

                hoarBeads.Add(new HoarBead
                {
                    Transform = bead.transform,
                    BasePosition = basePosition,
                    BaseScale = bead.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreatePaleDews()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDew = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0016f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0016f),
                _ => new Color(1f, 1f, 1f, 0.0016f)
            };
            Color enemyDew = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0016f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0016f),
                _ => new Color(1f, 1f, 0.999f, 0.0016f)
            };

            int dewCount = 8;
            for (int index = 0; index < dewCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99046f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject dew = new($"Pale Dew {index + 1}");
                dew.transform.SetParent(root);
                dew.transform.position = basePosition;
                dew.transform.rotation = Quaternion.identity;
                dew.transform.localScale = Vector3.one;

                Color dewColor = playerBias ? playerDew : enemyDew;
                CreatePaleDewElement(
                    dew.transform,
                    "Pale Dew Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00013f, 0f),
                    Vector3.one * 0.00072f,
                    dewColor);

                paleDews.Add(new PaleDew
                {
                    Transform = dew.transform,
                    BasePosition = basePosition,
                    BaseScale = dew.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFaintPearls()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerPearl = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0011f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0011f),
                _ => new Color(1f, 1f, 1f, 0.0011f)
            };
            Color enemyPearl = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0011f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0011f),
                _ => new Color(1f, 1f, 0.999f, 0.0011f)
            };

            int pearlCount = 8;
            for (int index = 0; index < pearlCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99043f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject pearl = new($"Faint Pearl {index + 1}");
                pearl.transform.SetParent(root);
                pearl.transform.position = basePosition;
                pearl.transform.rotation = Quaternion.identity;
                pearl.transform.localScale = Vector3.one;

                Color pearlColor = playerBias ? playerPearl : enemyPearl;
                CreateFaintPearlElement(
                    pearl.transform,
                    "Faint Pearl Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0001f, 0f),
                    Vector3.one * 0.00055f,
                    pearlColor);

                faintPearls.Add(new FaintPearl
                {
                    Transform = pearl.transform,
                    BasePosition = basePosition,
                    BaseScale = pearl.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateWanDroplets()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDroplet = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0008f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0008f),
                _ => new Color(1f, 1f, 1f, 0.0008f)
            };
            Color enemyDroplet = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0008f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0008f),
                _ => new Color(1f, 1f, 0.999f, 0.0008f)
            };

            int dropletCount = 8;
            for (int index = 0; index < dropletCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99041f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject droplet = new($"Wan Droplet {index + 1}");
                droplet.transform.SetParent(root);
                droplet.transform.position = basePosition;
                droplet.transform.rotation = Quaternion.identity;
                droplet.transform.localScale = Vector3.one;

                Color dropletColor = playerBias ? playerDroplet : enemyDroplet;
                CreateWanDropletElement(
                    droplet.transform,
                    "Wan Droplet Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00008f, 0f),
                    Vector3.one * 0.00045f,
                    dropletColor);

                wanDroplets.Add(new WanDroplet
                {
                    Transform = droplet.transform,
                    BasePosition = basePosition,
                    BaseScale = droplet.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateHushMoistures()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerMoisture = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00055f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00055f),
                _ => new Color(1f, 1f, 1f, 0.00055f)
            };
            Color enemyMoisture = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00055f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00055f),
                _ => new Color(1f, 1f, 0.999f, 0.00055f)
            };

            int moistureCount = 8;
            for (int index = 0; index < moistureCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99039f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject moisture = new($"Hush Moisture {index + 1}");
                moisture.transform.SetParent(root);
                moisture.transform.position = basePosition;
                moisture.transform.rotation = Quaternion.identity;
                moisture.transform.localScale = Vector3.one;

                Color moistureColor = playerBias ? playerMoisture : enemyMoisture;
                CreateHushMoistureElement(
                    moisture.transform,
                    "Hush Moisture Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00006f, 0f),
                    Vector3.one * 0.00034f,
                    moistureColor);

                hushMoistures.Add(new HushMoisture
                {
                    Transform = moisture.transform,
                    BasePosition = basePosition,
                    BaseScale = moisture.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateDimCondensates()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerCondensate = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00038f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00038f),
                _ => new Color(1f, 1f, 1f, 0.00038f)
            };
            Color enemyCondensate = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00038f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00038f),
                _ => new Color(1f, 1f, 0.999f, 0.00038f)
            };

            int condensateCount = 8;
            for (int index = 0; index < condensateCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99037f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject condensate = new($"Dim Condensate {index + 1}");
                condensate.transform.SetParent(root);
                condensate.transform.position = basePosition;
                condensate.transform.rotation = Quaternion.identity;
                condensate.transform.localScale = Vector3.one;

                Color condensateColor = playerBias ? playerCondensate : enemyCondensate;
                CreateDimCondensateElement(
                    condensate.transform,
                    "Dim Condensate Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00005f, 0f),
                    Vector3.one * 0.00026f,
                    condensateColor);

                dimCondensates.Add(new DimCondensate
                {
                    Transform = condensate.transform,
                    BasePosition = basePosition,
                    BaseScale = condensate.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateStillFilms()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerFilm = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00026f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00026f),
                _ => new Color(1f, 1f, 1f, 0.00026f)
            };
            Color enemyFilm = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00026f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00026f),
                _ => new Color(1f, 1f, 0.999f, 0.00026f)
            };

            int filmCount = 8;
            for (int index = 0; index < filmCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99036f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject film = new($"Still Film {index + 1}");
                film.transform.SetParent(root);
                film.transform.position = basePosition;
                film.transform.rotation = Quaternion.identity;
                film.transform.localScale = Vector3.one;

                Color filmColor = playerBias ? playerFilm : enemyFilm;
                CreateStillFilmElement(
                    film.transform,
                    "Still Film Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00004f, 0f),
                    Vector3.one * 0.0002f,
                    filmColor);

                stillFilms.Add(new StillFilm
                {
                    Transform = film.transform,
                    BasePosition = basePosition,
                    BaseScale = film.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateQuietSheens()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSheen = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00018f),
                _ => new Color(1f, 1f, 1f, 0.00018f)
            };
            Color enemySheen = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00018f),
                _ => new Color(1f, 1f, 0.999f, 0.00018f)
            };

            int sheenCount = 8;
            for (int index = 0; index < sheenCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99035f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject sheen = new($"Quiet Sheen {index + 1}");
                sheen.transform.SetParent(root);
                sheen.transform.position = basePosition;
                sheen.transform.rotation = Quaternion.identity;
                sheen.transform.localScale = Vector3.one;

                Color sheenColor = playerBias ? playerSheen : enemySheen;
                CreateQuietSheenElement(
                    sheen.transform,
                    "Quiet Sheen Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00003f, 0f),
                    Vector3.one * 0.00016f,
                    sheenColor);

                quietSheens.Add(new QuietSheen
                {
                    Transform = sheen.transform,
                    BasePosition = basePosition,
                    BaseScale = sheen.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateLastLustres()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerLustre = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00012f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00012f),
                _ => new Color(1f, 1f, 1f, 0.00012f)
            };
            Color enemyLustre = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00012f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00012f),
                _ => new Color(1f, 1f, 0.999f, 0.00012f)
            };

            int lustreCount = 8;
            for (int index = 0; index < lustreCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99034f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject lustre = new($"Last Lustre {index + 1}");
                lustre.transform.SetParent(root);
                lustre.transform.position = basePosition;
                lustre.transform.rotation = Quaternion.identity;
                lustre.transform.localScale = Vector3.one;

                Color lustreColor = playerBias ? playerLustre : enemyLustre;
                CreateLastLustreElement(
                    lustre.transform,
                    "Last Lustre Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000025f, 0f),
                    Vector3.one * 0.00012f,
                    lustreColor);

                lastLustres.Add(new LastLustre
                {
                    Transform = lustre.transform,
                    BasePosition = basePosition,
                    BaseScale = lustre.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateThinGlints()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerGlint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00008f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00008f),
                _ => new Color(1f, 1f, 1f, 0.00008f)
            };
            Color enemyGlint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00008f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00008f),
                _ => new Color(1f, 1f, 0.999f, 0.00008f)
            };

            int glintCount = 8;
            for (int index = 0; index < glintCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99033f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject glint = new($"Thin Glint {index + 1}");
                glint.transform.SetParent(root);
                glint.transform.position = basePosition;
                glint.transform.rotation = Quaternion.identity;
                glint.transform.localScale = Vector3.one;

                Color glintColor = playerBias ? playerGlint : enemyGlint;
                CreateThinGlintElement(
                    glint.transform,
                    "Thin Glint Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00002f, 0f),
                    Vector3.one * 0.00009f,
                    glintColor);

                thinGlints.Add(new ThinGlint
                {
                    Transform = glint.transform,
                    BasePosition = basePosition,
                    BaseScale = glint.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFadingGleams()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerGleam = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00005f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00005f),
                _ => new Color(1f, 1f, 1f, 0.00005f)
            };
            Color enemyGleam = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00005f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00005f),
                _ => new Color(1f, 1f, 0.999f, 0.00005f)
            };

            int gleamCount = 8;
            for (int index = 0; index < gleamCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990325f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject gleam = new($"Fading Gleam {index + 1}");
                gleam.transform.SetParent(root);
                gleam.transform.position = basePosition;
                gleam.transform.rotation = Quaternion.identity;
                gleam.transform.localScale = Vector3.one;

                Color gleamColor = playerBias ? playerGleam : enemyGleam;
                CreateFadingGleamElement(
                    gleam.transform,
                    "Fading Gleam Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000016f, 0f),
                    Vector3.one * 0.00007f,
                    gleamColor);

                fadingGleams.Add(new FadingGleam
                {
                    Transform = gleam.transform,
                    BasePosition = basePosition,
                    BaseScale = gleam.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSoftTraces()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00003f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00003f),
                _ => new Color(1f, 1f, 1f, 0.00003f)
            };
            Color enemyTrace = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00003f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00003f),
                _ => new Color(1f, 1f, 0.999f, 0.00003f)
            };

            int traceCount = 8;
            for (int index = 0; index < traceCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990322f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject trace = new($"Soft Trace {index + 1}");
                trace.transform.SetParent(root);
                trace.transform.position = basePosition;
                trace.transform.rotation = Quaternion.identity;
                trace.transform.localScale = Vector3.one;

                Color traceColor = playerBias ? playerTrace : enemyTrace;
                CreateSoftTraceElement(
                    trace.transform,
                    "Soft Trace Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000013f, 0f),
                    Vector3.one * 0.000055f,
                    traceColor);

                softTraces.Add(new SoftTrace
                {
                    Transform = trace.transform,
                    BasePosition = basePosition,
                    BaseScale = trace.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFaintVeils()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerVeil = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00002f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00002f),
                _ => new Color(1f, 1f, 1f, 0.00002f)
            };
            Color enemyVeil = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00002f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00002f),
                _ => new Color(1f, 1f, 0.999f, 0.00002f)
            };

            int veilCount = 8;
            for (int index = 0; index < veilCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990319f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject veil = new($"Faint Veil {index + 1}");
                veil.transform.SetParent(root);
                veil.transform.position = basePosition;
                veil.transform.rotation = Quaternion.identity;
                veil.transform.localScale = Vector3.one;

                Color veilColor = playerBias ? playerVeil : enemyVeil;
                CreateFaintVeilElement(
                    veil.transform,
                    "Faint Veil Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000011f, 0f),
                    Vector3.one * 0.000042f,
                    veilColor);

                faintVeils.Add(new FaintVeil
                {
                    Transform = veil.transform,
                    BasePosition = basePosition,
                    BaseScale = veil.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateGhostSheens()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSheen = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000012f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000012f),
                _ => new Color(1f, 1f, 1f, 0.000012f)
            };
            Color enemySheen = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000012f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000012f),
                _ => new Color(1f, 1f, 0.999f, 0.000012f)
            };

            int sheenCount = 8;
            for (int index = 0; index < sheenCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990317f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject sheen = new($"Ghost Sheen {index + 1}");
                sheen.transform.SetParent(root);
                sheen.transform.position = basePosition;
                sheen.transform.rotation = Quaternion.identity;
                sheen.transform.localScale = Vector3.one;

                Color sheenColor = playerBias ? playerSheen : enemySheen;
                CreateGhostSheenElement(
                    sheen.transform,
                    "Ghost Sheen Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000009f, 0f),
                    Vector3.one * 0.000032f,
                    sheenColor);

                ghostSheens.Add(new GhostSheen
                {
                    Transform = sheen.transform,
                    BasePosition = basePosition,
                    BaseScale = sheen.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFinalTints()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000007f),
                _ => new Color(1f, 1f, 1f, 0.000007f)
            };
            Color enemyTint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000007f),
                _ => new Color(1f, 1f, 0.999f, 0.000007f)
            };

            int tintCount = 8;
            for (int index = 0; index < tintCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990316f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject tint = new($"Final Tint {index + 1}");
                tint.transform.SetParent(root);
                tint.transform.position = basePosition;
                tint.transform.rotation = Quaternion.identity;
                tint.transform.localScale = Vector3.one;

                Color tintColor = playerBias ? playerTint : enemyTint;
                CreateFinalTintElement(
                    tint.transform,
                    "Final Tint Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000007f, 0f),
                    Vector3.one * 0.000024f,
                    tintColor);

                finalTints.Add(new FinalTint
                {
                    Transform = tint.transform,
                    BasePosition = basePosition,
                    BaseScale = tint.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateMuteHues()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerHue = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000004f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000004f),
                _ => new Color(1f, 1f, 1f, 0.000004f)
            };
            Color enemyHue = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000004f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000004f),
                _ => new Color(1f, 1f, 0.999f, 0.000004f)
            };

            int hueCount = 8;
            for (int index = 0; index < hueCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990315f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject hue = new($"Mute Hue {index + 1}");
                hue.transform.SetParent(root);
                hue.transform.position = basePosition;
                hue.transform.rotation = Quaternion.identity;
                hue.transform.localScale = Vector3.one;

                Color hueColor = playerBias ? playerHue : enemyHue;
                CreateMuteHueElement(
                    hue.transform,
                    "Mute Hue Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000006f, 0f),
                    Vector3.one * 0.000018f,
                    hueColor);

                muteHues.Add(new MuteHue
                {
                    Transform = hue.transform,
                    BasePosition = basePosition,
                    BaseScale = hue.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateHushedTints()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerTint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0000025f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000025f),
                _ => new Color(1f, 1f, 1f, 0.0000025f)
            };
            Color enemyTint = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0000025f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000025f),
                _ => new Color(1f, 1f, 0.999f, 0.0000025f)
            };

            int tintCount = 8;
            for (int index = 0; index < tintCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990314f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject tint = new($"Hushed Tint {index + 1}");
                tint.transform.SetParent(root);
                tint.transform.position = basePosition;
                tint.transform.rotation = Quaternion.identity;
                tint.transform.localScale = Vector3.one;

                Color tintColor = playerBias ? playerTint : enemyTint;
                CreateHushedTintElement(
                    tint.transform,
                    "Hushed Tint Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000005f, 0f),
                    Vector3.one * 0.000014f,
                    tintColor);

                hushedTints.Add(new HushedTint
                {
                    Transform = tint.transform,
                    BasePosition = basePosition,
                    BaseScale = tint.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFadedCasts()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerCast = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0000015f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000015f),
                _ => new Color(1f, 1f, 1f, 0.0000015f)
            };
            Color enemyCast = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0000015f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000015f),
                _ => new Color(1f, 1f, 0.999f, 0.0000015f)
            };

            int castCount = 8;
            for (int index = 0; index < castCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990313f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject cast = new($"Faded Cast {index + 1}");
                cast.transform.SetParent(root);
                cast.transform.position = basePosition;
                cast.transform.rotation = Quaternion.identity;
                cast.transform.localScale = Vector3.one;

                Color castColor = playerBias ? playerCast : enemyCast;
                CreateFadedCastElement(
                    cast.transform,
                    "Faded Cast Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000004f, 0f),
                    Vector3.one * 0.000011f,
                    castColor);

                fadedCasts.Add(new FadedCast
                {
                    Transform = cast.transform,
                    BasePosition = basePosition,
                    BaseScale = cast.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSpentShades()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerShade = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000001f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000001f),
                _ => new Color(1f, 1f, 1f, 0.000001f)
            };
            Color enemyShade = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000001f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000001f),
                _ => new Color(1f, 1f, 0.999f, 0.000001f)
            };

            int shadeCount = 8;
            for (int index = 0; index < shadeCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990312f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject shade = new($"Spent Shade {index + 1}");
                shade.transform.SetParent(root);
                shade.transform.position = basePosition;
                shade.transform.rotation = Quaternion.identity;
                shade.transform.localScale = Vector3.one;

                Color shadeColor = playerBias ? playerShade : enemyShade;
                CreateSpentShadeElement(
                    shade.transform,
                    "Spent Shade Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000003f, 0f),
                    Vector3.one * 0.000009f,
                    shadeColor);

                spentShades.Add(new SpentShade
                {
                    Transform = shade.transform,
                    BasePosition = basePosition,
                    BaseScale = shade.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateDryStains()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerStain = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.0000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000007f),
                _ => new Color(1f, 1f, 1f, 0.0000007f)
            };
            Color enemyStain = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.0000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.0000007f),
                _ => new Color(1f, 1f, 0.999f, 0.0000007f)
            };

            int stainCount = 8;
            for (int index = 0; index < stainCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990311f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject stain = new($"Dry Stain {index + 1}");
                stain.transform.SetParent(root);
                stain.transform.position = basePosition;
                stain.transform.rotation = Quaternion.identity;
                stain.transform.localScale = Vector3.one;

                Color stainColor = playerBias ? playerStain : enemyStain;
                CreateDryStainElement(
                    stain.transform,
                    "Dry Stain Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000002f, 0f),
                    Vector3.one * 0.000007f,
                    stainColor);

                dryStains.Add(new DryStain
                {
                    Transform = stain.transform,
                    BasePosition = basePosition,
                    BaseScale = stain.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateWornMarks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerMark = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00000045f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000045f),
                _ => new Color(1f, 1f, 1f, 0.00000045f)
            };
            Color enemyMark = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00000045f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000045f),
                _ => new Color(1f, 1f, 0.999f, 0.00000045f)
            };

            int markCount = 8;
            for (int index = 0; index < markCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.99031f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject mark = new($"Worn Mark {index + 1}");
                mark.transform.SetParent(root);
                mark.transform.position = basePosition;
                mark.transform.rotation = Quaternion.identity;
                mark.transform.localScale = Vector3.one;

                Color markColor = playerBias ? playerMark : enemyMark;
                CreateWornMarkElement(
                    mark.transform,
                    "Worn Mark Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0000015f, 0f),
                    Vector3.one * 0.000005f,
                    markColor);

                wornMarks.Add(new WornMark
                {
                    Transform = mark.transform,
                    BasePosition = basePosition,
                    BaseScale = mark.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateFaintScuffs()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerScuff = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00000028f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000028f),
                _ => new Color(1f, 1f, 1f, 0.00000028f)
            };
            Color enemyScuff = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00000028f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000028f),
                _ => new Color(1f, 1f, 0.999f, 0.00000028f)
            };

            int scuffCount = 8;
            for (int index = 0; index < scuffCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990309f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject scuff = new($"Faint Scuff {index + 1}");
                scuff.transform.SetParent(root);
                scuff.transform.position = basePosition;
                scuff.transform.rotation = Quaternion.identity;
                scuff.transform.localScale = Vector3.one;

                Color scuffColor = playerBias ? playerScuff : enemyScuff;
                CreateFaintScuffElement(
                    scuff.transform,
                    "Faint Scuff Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.000001f, 0f),
                    Vector3.one * 0.0000038f,
                    scuffColor);

                faintScuffs.Add(new FaintScuff
                {
                    Transform = scuff.transform,
                    BasePosition = basePosition,
                    BaseScale = scuff.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateTraceNicks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerNick = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00000018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000018f),
                _ => new Color(1f, 1f, 1f, 0.00000018f)
            };
            Color enemyNick = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00000018f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000018f),
                _ => new Color(1f, 1f, 0.999f, 0.00000018f)
            };

            int nickCount = 8;
            for (int index = 0; index < nickCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990308f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject nick = new($"Trace Nick {index + 1}");
                nick.transform.SetParent(root);
                nick.transform.position = basePosition;
                nick.transform.rotation = Quaternion.identity;
                nick.transform.localScale = Vector3.one;

                Color nickColor = playerBias ? playerNick : enemyNick;
                CreateTraceNickElement(
                    nick.transform,
                    "Trace Nick Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0000007f, 0f),
                    Vector3.one * 0.0000026f,
                    nickColor);

                traceNicks.Add(new TraceNick
                {
                    Transform = nick.transform,
                    BasePosition = basePosition,
                    BaseScale = nick.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreatePinPricks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerPrick = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00000011f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000011f),
                _ => new Color(1f, 1f, 1f, 0.00000011f)
            };
            Color enemyPrick = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00000011f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000011f),
                _ => new Color(1f, 1f, 0.999f, 0.00000011f)
            };

            int prickCount = 8;
            for (int index = 0; index < prickCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990307f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject prick = new($"Pin Prick {index + 1}");
                prick.transform.SetParent(root);
                prick.transform.position = basePosition;
                prick.transform.rotation = Quaternion.identity;
                prick.transform.localScale = Vector3.one;

                Color prickColor = playerBias ? playerPrick : enemyPrick;
                CreatePinPrickElement(
                    prick.transform,
                    "Pin Prick Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0000005f, 0f),
                    Vector3.one * 0.0000018f,
                    prickColor);

                pinPricks.Add(new PinPrick
                {
                    Transform = prick.transform,
                    BasePosition = basePosition,
                    BaseScale = prick.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateNeedleDots()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerDot = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.00000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000007f),
                _ => new Color(1f, 1f, 1f, 0.00000007f)
            };
            Color enemyDot = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.00000007f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.00000007f),
                _ => new Color(1f, 1f, 0.999f, 0.00000007f)
            };

            int dotCount = 8;
            for (int index = 0; index < dotCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990306f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject dot = new($"Needle Dot {index + 1}");
                dot.transform.SetParent(root);
                dot.transform.position = basePosition;
                dot.transform.rotation = Quaternion.identity;
                dot.transform.localScale = Vector3.one;

                Color dotColor = playerBias ? playerDot : enemyDot;
                CreateNeedleDotElement(
                    dot.transform,
                    "Needle Dot Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.0000003f, 0f),
                    Vector3.one * 0.0000012f,
                    dotColor);

                needleDots.Add(new NeedleDot
                {
                    Transform = dot.transform,
                    BasePosition = basePosition,
                    BaseScale = dot.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateDustSpecks()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerSpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000000045f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000045f),
                _ => new Color(1f, 1f, 1f, 0.000000045f)
            };
            Color enemySpeck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000000045f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000045f),
                _ => new Color(1f, 1f, 0.999f, 0.000000045f)
            };

            int speckCount = 8;
            for (int index = 0; index < speckCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990305f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject speck = new($"Dust Speck {index + 1}");
                speck.transform.SetParent(root);
                speck.transform.position = basePosition;
                speck.transform.rotation = Quaternion.identity;
                speck.transform.localScale = Vector3.one;

                Color speckColor = playerBias ? playerSpeck : enemySpeck;
                CreateDustSpeckElement(
                    speck.transform,
                    "Dust Speck Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00000018f, 0f),
                    Vector3.one * 0.00000085f,
                    speckColor);

                dustSpecks.Add(new DustSpeck
                {
                    Transform = speck.transform,
                    BasePosition = basePosition,
                    BaseScale = speck.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateAshMotes()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerMote = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000000028f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000028f),
                _ => new Color(1f, 1f, 1f, 0.000000028f)
            };
            Color enemyMote = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000000028f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000028f),
                _ => new Color(1f, 1f, 0.999f, 0.000000028f)
            };

            int moteCount = 8;
            for (int index = 0; index < moteCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990304f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject mote = new($"Ash Mote {index + 1}");
                mote.transform.SetParent(root);
                mote.transform.position = basePosition;
                mote.transform.rotation = Quaternion.identity;
                mote.transform.localScale = Vector3.one;

                Color moteColor = playerBias ? playerMote : enemyMote;
                CreateAshMoteElement(
                    mote.transform,
                    "Ash Mote Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00000011f, 0f),
                    Vector3.one * 0.00000062f,
                    moteColor);

                ashMotes.Add(new AshMote
                {
                    Transform = mote.transform,
                    BasePosition = basePosition,
                    BaseScale = mote.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void CreateSootFleckLayer()
        {
            if (mapProfile == null || root == null)
            {
                return;
            }

            Color playerFleck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 1f, 0.000000017f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000017f),
                _ => new Color(1f, 1f, 1f, 0.000000017f)
            };
            Color enemyFleck = theme switch
            {
                BattlefieldTheme.CrimsonBasin => new Color(1f, 1f, 0.999f, 0.000000017f),
                BattlefieldTheme.PaleSaltFlats => new Color(1f, 1f, 1f, 0.000000017f),
                _ => new Color(1f, 1f, 0.999f, 0.000000017f)
            };

            int fleckCount = 8;
            for (int index = 0; index < fleckCount; index++)
            {
                bool playerBias = index % 2 == 0;
                float laneT = 0.24f + index * 0.065f;
                Vector3 basePosition = new Vector3(
                    playerBias ? -62f : 62f,
                    7.990303f,
                    Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, laneT) + (playerBias ? 16f : -16f));

                GameObject fleck = new($"Soot Fleck {index + 1}");
                fleck.transform.SetParent(root);
                fleck.transform.position = basePosition;
                fleck.transform.rotation = Quaternion.identity;
                fleck.transform.localScale = Vector3.one;

                Color fleckColor = playerBias ? playerFleck : enemyFleck;
                CreateSootFleckElement(
                    fleck.transform,
                    "Soot Fleck Core",
                    PrimitiveType.Sphere,
                    new Vector3(0f, 0.00000007f, 0f),
                    Vector3.one * 0.00000045f,
                    fleckColor);

                sootFlecks.Add(new SootFleck
                {
                    Transform = fleck.transform,
                    BasePosition = basePosition,
                    BaseScale = fleck.transform.localScale,
                    Phase = Random.value * Mathf.PI * 2f
                });
            }
        }

        private void AnimateDriftLayers()
        {
            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < driftLayers.Count; index++)
            {
                DriftLayer layer = driftLayers[index];
                if (layer.Transform == null)
                {
                    continue;
                }

                layer.Transform.position += layer.Velocity * Time.deltaTime;
                Vector3 position = layer.Transform.position;

                if (position.x > mapProfile.MaxX + 260f)
                {
                    position.x = mapProfile.MinX - 260f;
                }

                if (position.z > mapProfile.MaxZ + 180f)
                {
                    position.z = mapProfile.MinZ - 180f;
                }

                float bob = Mathf.Sin(Time.time * 0.34f + layer.Phase) * 1.8f;
                position.y += bob * Time.deltaTime;
                layer.Transform.position = position;
                layer.Transform.rotation *= Quaternion.Euler(0f, 2.4f * Time.deltaTime, Mathf.Sin(Time.time * 0.4f + layer.Phase) * 0.02f);
                layer.Transform.localScale = layer.BaseScale * (0.94f + Mathf.PingPong(Time.time * 0.08f + layer.Phase, 0.12f));
                driftLayers[index] = layer;
            }
        }

        private void AnimateShrineMotes()
        {
            for (int index = 0; index < shrineMotes.Count; index++)
            {
                Transform mote = shrineMotes[index];
                if (mote == null)
                {
                    continue;
                }

                Vector3 center = shrineMoteCenters[index];
                float phase = shrineMotePhases[index];
                float orbit = Time.time * (0.22f + index * 0.01f) + phase;
                float radius = 22f + Mathf.Sin(Time.time * 0.3f + phase) * 8f;
                float height = 18f + Mathf.Sin(Time.time * 0.7f + phase * 0.8f) * 10f;
                mote.position = center + new Vector3(Mathf.Cos(orbit) * radius, height, Mathf.Sin(orbit) * radius);
                mote.localScale = Vector3.one * (0.5f + Mathf.PingPong(Time.time * 0.5f + phase, 0.7f));
            }
        }

        private void AnimatePatrolFormations()
        {
            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < patrolFormations.Count; index++)
            {
                PatrolFormation formation = patrolFormations[index];
                if (formation.Transform == null)
                {
                    continue;
                }

                formation.Transform.position += formation.Velocity * Time.deltaTime;
                Vector3 position = formation.Transform.position;
                bool outOfBounds = formation.StartsFromWest
                    ? position.x > mapProfile.MaxX + 420f || position.z > mapProfile.MaxZ + 220f
                    : position.x < mapProfile.MinX - 420f || position.z < mapProfile.MinZ - 220f;

                if (outOfBounds)
                {
                    position = new Vector3(
                        formation.StartsFromWest ? mapProfile.MinX - 420f : mapProfile.MaxX + 420f,
                        formation.BaseHeight,
                        Mathf.Lerp(mapProfile.MinZ, mapProfile.MaxZ, formation.LaneT));
                }

                position.y = formation.BaseHeight + Mathf.Sin(Time.time * 0.42f + formation.Phase) * 3.2f;
                formation.Transform.position = position;
                formation.Transform.rotation = Quaternion.LookRotation(formation.Velocity.normalized, Vector3.up)
                    * Quaternion.Euler(
                        Mathf.Sin(Time.time * 0.8f + formation.Phase) * 4f,
                        Mathf.Cos(Time.time * 0.34f + formation.Phase) * 6f,
                        Mathf.Sin(Time.time * 1.2f + formation.Phase) * 9f);
                formation.Transform.localScale = formation.BaseScale * (0.94f + Mathf.PingPong(Time.time * 0.22f + formation.Phase, 0.08f));
                patrolFormations[index] = formation;
            }

            for (int index = 0; index < patrolGlowRenderers.Count; index++)
            {
                Renderer rendererComponent = patrolGlowRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulse = 0.88f + Mathf.PingPong(Time.time * 1.1f + index * 0.37f, 0.18f);
                rendererComponent.material.color = patrolGlowBaseColors[index] * pulse;
            }
        }

        private void AnimateOrbitalLances()
        {
            for (int index = 0; index < orbitalLances.Count; index++)
            {
                OrbitalLance lance = orbitalLances[index];
                if (lance.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.12f + lance.Phase, 1f);
                float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.14f, cycle));
                float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.44f, cycle));
                float intensity = cycle < 0.44f ? Mathf.Clamp01(rise * fade) : 0f;
                float flicker = 0.86f + Mathf.PingPong(Time.time * 8f + index * 0.41f, 0.14f);

                lance.Transform.position = lance.BasePosition + new Vector3(
                    0f,
                    intensity * 6f,
                    Mathf.Sin(Time.time * 0.18f + lance.Phase) * 4f);
                lance.Transform.localScale = new Vector3(
                    lance.BaseScale.x * (0.92f + intensity * 0.18f),
                    lance.BaseScale.y * Mathf.Lerp(0.4f, 1.18f, intensity),
                    lance.BaseScale.z * (0.92f + intensity * 0.18f));
                orbitalLances[index] = lance;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= orbitalLanceRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = orbitalLanceRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float elementScale = rendererOffset == 0 ? 1f : rendererOffset == 1 ? 1.16f : 0.92f;
                    rendererComponent.material.color = orbitalLanceBaseColors[rendererIndex] * Mathf.Max(0.08f, intensity * flicker * elementScale);
                }
            }
        }

        private void AnimateFlakBursts()
        {
            for (int index = 0; index < flakBursts.Count; index++)
            {
                FlakBurst burst = flakBursts[index];
                if (burst.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.28f + burst.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.18f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.18f);
                }
                else if (cycle < 0.42f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.18f) / 0.24f);
                }

                burst.Transform.position = burst.BasePosition + new Vector3(
                    Mathf.Sin(Time.time * 0.36f + burst.Phase) * 6f,
                    intensity * 2.8f,
                    Mathf.Cos(Time.time * 0.32f + burst.Phase) * 4f);
                burst.Transform.localScale = burst.BaseScale * Mathf.Lerp(0.35f, 1.5f, intensity);
                flakBursts[index] = burst;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= flakBurstRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = flakBurstRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.88f + Mathf.PingPong(Time.time * 7.2f + index * 0.43f + rendererOffset, 0.12f);
                    float visibility = rendererOffset == 0
                        ? Mathf.Max(0.06f, intensity * pulse)
                        : Mathf.Max(0.04f, intensity * 0.82f * pulse);
                    rendererComponent.material.color = flakBurstBaseColors[rendererIndex] * visibility;
                }
            }
        }

        private void AnimateFallingWreckage()
        {
            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < fallingWreckages.Count; index++)
            {
                FallingWreckage wreckage = fallingWreckages[index];
                if (wreckage.Transform == null)
                {
                    continue;
                }

                wreckage.Transform.position += wreckage.Velocity * Time.deltaTime;
                Vector3 position = wreckage.Transform.position;
                if (position.y < 18f || position.z < mapProfile.MinZ - 220f || position.x > mapProfile.MaxX + 220f)
                {
                    position = new Vector3(
                        mapProfile.MinX - 120f + (index % 4) * ((mapProfile.MaxX - mapProfile.MinX) * 0.22f),
                        wreckage.ResetHeight,
                        mapProfile.MaxZ + 140f + (index / 4f) * 60f);
                    wreckage.Transform.rotation = Quaternion.Euler(Random.Range(-34f, 34f), Random.Range(0f, 360f), Random.Range(-34f, 34f));
                }

                wreckage.Transform.position = position;
                wreckage.Transform.rotation *= Quaternion.Euler(
                    (28f + index * 3f) * Time.deltaTime,
                    (36f + index * 4f) * Time.deltaTime,
                    (44f + index * 5f) * Time.deltaTime);
                wreckage.Transform.localScale = wreckage.BaseScale * (0.92f + Mathf.PingPong(Time.time * 0.34f + wreckage.Phase, 0.1f));
                fallingWreckages[index] = wreckage;
            }

            for (int index = 0; index < wreckageGlowRenderers.Count; index++)
            {
                Renderer rendererComponent = wreckageGlowRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulse = 0.82f + Mathf.PingPong(Time.time * 2.8f + index * 0.39f, 0.22f);
                rendererComponent.material.color = wreckageGlowBaseColors[index] * pulse;
            }
        }

        private void AnimateShieldImpacts()
        {
            for (int index = 0; index < shieldImpacts.Count; index++)
            {
                ShieldImpact impact = shieldImpacts[index];
                if (impact.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.18f + impact.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.16f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.16f);
                }
                else if (cycle < 0.42f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.16f) / 0.26f);
                }

                impact.Transform.position = impact.BasePosition + new Vector3(
                    0f,
                    intensity * 1.6f,
                    Mathf.Sin(Time.time * 0.24f + impact.Phase) * 3.2f);
                impact.Transform.localScale = impact.BaseScale * Mathf.Lerp(0.48f, 1.36f, intensity);
                shieldImpacts[index] = impact;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= shieldImpactRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = shieldImpactRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 5.8f + index * 0.41f + rendererOffset, 0.1f);
                    float visibility = rendererOffset == 0
                        ? Mathf.Max(0.05f, intensity * pulse)
                        : rendererOffset == 1
                            ? Mathf.Max(0.04f, intensity * 1.08f * pulse)
                            : Mathf.Max(0.03f, intensity * 0.82f * pulse);
                    rendererComponent.material.color = shieldImpactBaseColors[rendererIndex] * visibility;
                }
            }
        }

        private void AnimateSiegeVolleys()
        {
            for (int index = 0; index < siegeVolleys.Count; index++)
            {
                SiegeVolley volley = siegeVolleys[index];
                if (volley.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.11f + volley.Phase, 1f);
                float flightT = cycle < 0.62f ? cycle / 0.62f : 1f;
                float visibility = cycle < 0.62f ? 1f : Mathf.Clamp01(1f - (cycle - 0.62f) / 0.18f);
                Vector3 position = Vector3.Lerp(volley.StartPosition, volley.EndPosition, flightT);
                float arcHeight = 4f * volley.ApexHeight * flightT * (1f - flightT);
                position.y += arcHeight;
                volley.Transform.position = position;

                Vector3 direction = (volley.EndPosition - volley.StartPosition).normalized;
                Vector3 tangent = new Vector3(
                    volley.EndPosition.x - volley.StartPosition.x,
                    4f * volley.ApexHeight * (1f - 2f * flightT),
                    volley.EndPosition.z - volley.StartPosition.z).normalized;
                if (tangent.sqrMagnitude < 0.001f)
                {
                    tangent = direction;
                }

                volley.Transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
                volley.Transform.localScale = volley.BaseScale * Mathf.Lerp(0.34f, 1.18f, visibility);
                siegeVolleys[index] = volley;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= siegeVolleyRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = siegeVolleyRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.2f + index * 0.27f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.72f : 1f;
                    rendererComponent.material.color = siegeVolleyBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateLaunchStreaks()
        {
            for (int index = 0; index < launchStreaks.Count; index++)
            {
                LaunchStreak streak = launchStreaks[index];
                if (streak.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.16f + streak.Phase, 1f);
                float flightT = cycle < 0.34f ? cycle / 0.34f : 1f;
                float visibility = cycle < 0.34f ? 1f : Mathf.Clamp01(1f - (cycle - 0.34f) / 0.18f);
                Vector3 position = Vector3.Lerp(streak.StartPosition, streak.EndPosition, flightT);
                streak.Transform.position = position;

                Vector3 direction = (streak.EndPosition - streak.StartPosition).normalized;
                streak.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                streak.Transform.localScale = streak.BaseScale * Mathf.Lerp(0.28f, 1.14f, visibility);
                launchStreaks[index] = streak;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= launchStreakRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = launchStreakRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.92f + Mathf.PingPong(Time.time * 6f + index * 0.31f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.72f : 1f;
                    rendererComponent.material.color = launchStreakBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateBatteryFlashes()
        {
            for (int index = 0; index < batteryFlashes.Count; index++)
            {
                BatteryFlash flash = batteryFlashes[index];
                if (flash.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.2f + flash.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.1f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.1f);
                }
                else if (cycle < 0.26f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.1f) / 0.16f);
                }

                flash.Transform.position = flash.BasePosition + new Vector3(
                    0f,
                    intensity * 1.2f,
                    Mathf.Sin(Time.time * 0.22f + flash.Phase) * 2.4f);
                flash.Transform.localScale = flash.BaseScale * Mathf.Lerp(0.42f, 1.28f, intensity);
                batteryFlashes[index] = flash;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= batteryFlashRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = batteryFlashRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 6.4f + index * 0.29f + rendererOffset, 0.12f);
                    float alphaScale = rendererOffset == 0 ? 0.72f : 1f;
                    rendererComponent.material.color = batteryFlashBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateBarrageImpacts()
        {
            for (int index = 0; index < barrageImpacts.Count; index++)
            {
                BarrageImpact impact = barrageImpacts[index];
                if (impact.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + impact.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.14f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.14f);
                }
                else if (cycle < 0.4f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.14f) / 0.26f);
                }

                impact.Transform.position = impact.BasePosition + new Vector3(
                    0f,
                    intensity * 1.8f,
                    Mathf.Sin(Time.time * 0.18f + impact.Phase) * 2.8f);
                impact.Transform.localScale = impact.BaseScale * Mathf.Lerp(0.36f, 1.44f, intensity);
                barrageImpacts[index] = impact;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= barrageImpactRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = barrageImpactRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 5.2f + index * 0.33f + rendererOffset, 0.12f);
                    float alphaScale = rendererOffset == 0 ? 0.68f : rendererOffset == 1 ? 1f : 0.54f;
                    rendererComponent.material.color = barrageImpactBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateTargetDesignators()
        {
            for (int index = 0; index < targetDesignators.Count; index++)
            {
                TargetDesignator designator = targetDesignators[index];
                if (designator.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.13f + designator.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.18f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.18f);
                }
                else if (cycle < 0.38f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.18f) / 0.2f);
                }

                designator.Transform.position = designator.BasePosition + new Vector3(
                    0f,
                    intensity * 1.4f,
                    Mathf.Sin(Time.time * 0.2f + designator.Phase) * 2.2f);
                designator.Transform.localScale = designator.BaseScale * Mathf.Lerp(0.4f, 1.22f, intensity);
                targetDesignators[index] = designator;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= targetDesignatorRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = targetDesignatorRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 5.6f + index * 0.28f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.54f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = targetDesignatorBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateCounterScans()
        {
            for (int index = 0; index < counterScans.Count; index++)
            {
                CounterScan scan = counterScans[index];
                if (scan.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.12f + scan.Phase, 1f);
                float intensity = cycle < 0.6f ? 1f : Mathf.Clamp01(1f - (cycle - 0.6f) / 0.18f);
                float sweepYaw = Mathf.Lerp(-44f, 44f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(cycle / 0.6f)));

                scan.Transform.position = scan.BasePosition + new Vector3(
                    0f,
                    Mathf.Sin(Time.time * 0.16f + scan.Phase) * 0.8f,
                    0f);
                scan.Transform.rotation = scan.BaseRotation * Quaternion.Euler(0f, sweepYaw, 0f);
                scan.Transform.localScale = scan.BaseScale * Mathf.Lerp(0.56f, 1.08f, intensity);
                counterScans[index] = scan;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= counterScanRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = counterScanRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.88f + Mathf.PingPong(Time.time * 3.8f + index * 0.24f + rendererOffset, 0.12f);
                    float alphaScale = rendererOffset == 0 ? 0.52f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = counterScanBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateRelayPulses()
        {
            for (int index = 0; index < relayPulses.Count; index++)
            {
                RelayPulse relay = relayPulses[index];
                if (relay.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.16f + relay.Phase, 1f);
                float intensity = cycle < 0.72f ? 1f : Mathf.Clamp01(1f - (cycle - 0.72f) / 0.18f);
                relay.Transform.position = relay.BasePosition + new Vector3(
                    0f,
                    Mathf.Sin(Time.time * 0.18f + relay.Phase) * 0.6f,
                    0f);
                relay.Transform.localScale = relay.BaseScale * Mathf.Lerp(0.72f, 1.08f, intensity);

                if (relay.PulseNode != null)
                {
                    float pulseTravel = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(cycle / 0.72f));
                    relay.PulseNode.localPosition = relay.PulseBaseLocalPosition + new Vector3(0f, pulseTravel * 8.6f, 0f);
                    relay.PulseNode.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.24f, intensity);
                }

                relayPulses[index] = relay;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= relayPulseRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = relayPulseRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.88f + Mathf.PingPong(Time.time * 4.6f + index * 0.23f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.44f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = relayPulseBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateResponseArcs()
        {
            for (int index = 0; index < responseArcs.Count; index++)
            {
                ResponseArc arc = responseArcs[index];
                if (arc.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + arc.Phase, 1f);
                float travelT = cycle < 0.52f ? cycle / 0.52f : 1f;
                float visibility = cycle < 0.52f ? 1f : Mathf.Clamp01(1f - (cycle - 0.52f) / 0.2f);

                Vector3 position = Vector3.Lerp(arc.StartPosition, arc.EndPosition, travelT);
                float arcHeight = 4f * arc.ApexHeight * travelT * (1f - travelT);
                position.y += arcHeight;
                arc.Transform.position = position;

                Vector3 tangent = new Vector3(
                    arc.EndPosition.x - arc.StartPosition.x,
                    4f * arc.ApexHeight * (1f - 2f * travelT),
                    arc.EndPosition.z - arc.StartPosition.z).normalized;
                if (tangent.sqrMagnitude < 0.001f)
                {
                    tangent = (arc.EndPosition - arc.StartPosition).normalized;
                }

                arc.Transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
                arc.Transform.localScale = arc.BaseScale * Mathf.Lerp(0.34f, 1.12f, visibility);
                responseArcs[index] = arc;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= responseArcRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = responseArcRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.8f + index * 0.22f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.68f : 1f;
                    rendererComponent.material.color = responseArcBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateCommandEchos()
        {
            for (int index = 0; index < commandEchos.Count; index++)
            {
                CommandEcho echo = commandEchos[index];
                if (echo.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.18f + echo.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.16f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.16f);
                }
                else if (cycle < 0.42f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.16f) / 0.26f);
                }

                echo.Transform.position = echo.BasePosition + new Vector3(
                    0f,
                    intensity * 1.5f,
                    Mathf.Sin(Time.time * 0.18f + echo.Phase) * 1.8f);
                echo.Transform.localScale = echo.BaseScale * Mathf.Lerp(0.48f, 1.26f, intensity);
                commandEchos[index] = echo;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= commandEchoRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = commandEchoRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 5f + index * 0.21f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.54f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = commandEchoBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateOrderRipples()
        {
            for (int index = 0; index < orderRipples.Count; index++)
            {
                OrderRipple ripple = orderRipples[index];
                if (ripple.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.16f + ripple.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.18f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.18f);
                }
                else if (cycle < 0.54f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.18f) / 0.36f);
                }

                ripple.Transform.position = ripple.BasePosition + new Vector3(
                    0f,
                    intensity * 0.8f,
                    Mathf.Sin(Time.time * 0.16f + ripple.Phase) * 1.4f);
                ripple.Transform.localScale = new Vector3(
                    ripple.BaseScale.x * Mathf.Lerp(0.5f, 1.52f, intensity),
                    ripple.BaseScale.y,
                    ripple.BaseScale.z * Mathf.Lerp(0.5f, 1.52f, intensity));
                orderRipples[index] = ripple;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= orderRippleRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = orderRippleRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.4f + index * 0.19f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.62f : 1f;
                    rendererComponent.material.color = orderRippleBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateAdvanceChevrons()
        {
            for (int index = 0; index < advanceChevrons.Count; index++)
            {
                AdvanceChevron chevron = advanceChevrons[index];
                if (chevron.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.18f + chevron.Phase, 1f);
                float travelT = cycle < 0.64f ? cycle / 0.64f : 1f;
                float visibility = cycle < 0.64f ? 1f : Mathf.Clamp01(1f - (cycle - 0.64f) / 0.18f);

                Vector3 position = Vector3.Lerp(chevron.StartPosition, chevron.EndPosition, travelT);
                chevron.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.2f + chevron.Phase) * 0.16f, 0f);

                Vector3 direction = (chevron.EndPosition - chevron.StartPosition).normalized;
                chevron.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                chevron.Transform.localScale = chevron.BaseScale * Mathf.Lerp(0.42f, 1.12f, visibility);
                advanceChevrons[index] = chevron;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= advanceChevronRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = advanceChevronRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.2f + index * 0.18f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.68f : 1f;
                    rendererComponent.material.color = advanceChevronBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateFrontlineAcknowledges()
        {
            for (int index = 0; index < frontlineAcknowledges.Count; index++)
            {
                FrontlineAcknowledge acknowledge = frontlineAcknowledges[index];
                if (acknowledge.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.2f + acknowledge.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.14f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.14f);
                }
                else if (cycle < 0.34f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.14f) / 0.2f);
                }

                acknowledge.Transform.position = acknowledge.BasePosition + new Vector3(
                    0f,
                    intensity * 0.9f,
                    Mathf.Sin(Time.time * 0.15f + acknowledge.Phase) * 0.8f);
                acknowledge.Transform.localScale = acknowledge.BaseScale * Mathf.Lerp(0.56f, 1.2f, intensity);
                frontlineAcknowledges[index] = acknowledge;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= frontlineAcknowledgeRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = frontlineAcknowledgeRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 5.4f + index * 0.17f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.58f : 1f;
                    rendererComponent.material.color = frontlineAcknowledgeBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateRallyStreams()
        {
            for (int index = 0; index < rallyStreams.Count; index++)
            {
                RallyStream stream = rallyStreams[index];
                if (stream.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.19f + stream.Phase, 1f);
                float travelT = cycle < 0.68f ? cycle / 0.68f : 1f;
                float visibility = cycle < 0.68f ? 1f : Mathf.Clamp01(1f - (cycle - 0.68f) / 0.16f);

                Vector3 position = Vector3.Lerp(stream.StartPosition, stream.EndPosition, travelT);
                stream.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + stream.Phase) * 0.12f, 0f);

                Vector3 direction = (stream.EndPosition - stream.StartPosition).normalized;
                stream.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                stream.Transform.localScale = stream.BaseScale * Mathf.Lerp(0.42f, 1.04f, visibility);
                rallyStreams[index] = stream;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= rallyStreamRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = rallyStreamRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4f + index * 0.16f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.64f : 1f;
                    rendererComponent.material.color = rallyStreamBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateBattlelineHandoffs()
        {
            for (int index = 0; index < battlelineHandoffs.Count; index++)
            {
                BattlelineHandoff handoff = battlelineHandoffs[index];
                if (handoff.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.17f + handoff.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.16f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.16f);
                }
                else if (cycle < 0.42f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.16f) / 0.26f);
                }

                handoff.Transform.position = handoff.BasePosition + new Vector3(
                    0f,
                    intensity * 0.8f,
                    Mathf.Sin(Time.time * 0.14f + handoff.Phase) * 1.1f);
                handoff.Transform.localScale = new Vector3(
                    handoff.BaseScale.x * Mathf.Lerp(0.48f, 1.34f, intensity),
                    handoff.BaseScale.y,
                    handoff.BaseScale.z * Mathf.Lerp(0.48f, 1.34f, intensity));
                battlelineHandoffs[index] = handoff;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= battlelineHandoffRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = battlelineHandoffRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.6f + index * 0.15f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.6f : rendererOffset == 1 ? 1f : 0.7f;
                    rendererComponent.material.color = battlelineHandoffBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateClashPulses()
        {
            for (int index = 0; index < clashPulses.Count; index++)
            {
                ClashPulse pulse = clashPulses[index];
                if (pulse.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.19f + pulse.Phase, 1f);
                float intensity = 0f;
                if (cycle < 0.18f)
                {
                    intensity = Mathf.SmoothStep(0f, 1f, cycle / 0.18f);
                }
                else if (cycle < 0.48f)
                {
                    intensity = 1f - Mathf.SmoothStep(0f, 1f, (cycle - 0.18f) / 0.3f);
                }

                pulse.Transform.position = pulse.BasePosition + new Vector3(
                    0f,
                    intensity * 0.7f,
                    Mathf.Sin(Time.time * 0.12f + pulse.Phase) * 0.8f);
                pulse.Transform.localScale = new Vector3(
                    pulse.BaseScale.x * Mathf.Lerp(0.5f, 1.42f, intensity),
                    pulse.BaseScale.y,
                    pulse.BaseScale.z * Mathf.Lerp(0.5f, 1.42f, intensity));
                clashPulses[index] = pulse;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= clashPulseRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = clashPulseRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 5.2f + index * 0.14f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.62f : rendererOffset == 1 ? 1f : 0.78f;
                    rendererComponent.material.color = clashPulseBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateShockfrontTraces()
        {
            for (int index = 0; index < shockfrontTraces.Count; index++)
            {
                ShockfrontTrace trace = shockfrontTraces[index];
                if (trace.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.17f + trace.Phase, 1f);
                float travelT = cycle < 0.56f ? cycle / 0.56f : 1f;
                float visibility = cycle < 0.56f ? 1f : Mathf.Clamp01(1f - (cycle - 0.56f) / 0.18f);

                Vector3 position = Vector3.Lerp(trace.StartPosition, trace.EndPosition, travelT);
                trace.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.12f + trace.Phase) * 0.08f, 0f);

                Vector3 direction = (trace.EndPosition - trace.StartPosition).normalized;
                trace.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                trace.Transform.localScale = trace.BaseScale * Mathf.Lerp(0.42f, 1.08f, visibility);
                shockfrontTraces[index] = trace;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= shockfrontTraceRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = shockfrontTraceRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.4f + index * 0.13f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.64f : 1f;
                    rendererComponent.material.color = shockfrontTraceBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimatePressureFlares()
        {
            for (int index = 0; index < pressureFlares.Count; index++)
            {
                PressureFlare flare = pressureFlares[index];
                if (flare.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + flare.Phase, 1f);
                float intensity = cycle < 0.48f ? Mathf.Sin(cycle / 0.48f * Mathf.PI) : 0f;
                float drift = Mathf.Sin(Time.time * 0.18f + flare.Phase) * 0.12f;

                flare.Transform.position = flare.BasePosition + new Vector3(0f, drift, 0f);
                flare.Transform.localScale = new Vector3(
                    flare.BaseScale.x * Mathf.Lerp(0.58f, 1.18f, intensity),
                    flare.BaseScale.y * Mathf.Lerp(0.8f, 1.06f, intensity),
                    flare.BaseScale.z * Mathf.Lerp(0.58f, 1.18f, intensity));
                pressureFlares[index] = flare;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= pressureFlareRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = pressureFlareRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.88f + Mathf.PingPong(Time.time * 5f + index * 0.11f + rendererOffset, 0.12f);
                    float alphaScale = rendererOffset == 0 ? 0.56f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = pressureFlareBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateBraceSweeps()
        {
            for (int index = 0; index < braceSweeps.Count; index++)
            {
                BraceSweep sweep = braceSweeps[index];
                if (sweep.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + sweep.Phase, 1f);
                float travelT = cycle < 0.54f ? cycle / 0.54f : 1f;
                float visibility = cycle < 0.54f ? 1f : Mathf.Clamp01(1f - (cycle - 0.54f) / 0.2f);

                Vector3 position = Vector3.Lerp(sweep.StartPosition, sweep.EndPosition, travelT);
                sweep.Transform.position = position + new Vector3(Mathf.Sin(Time.time * 0.16f + sweep.Phase) * 0.08f, 0f, 0f);

                Vector3 direction = (sweep.EndPosition - sweep.StartPosition).normalized;
                sweep.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                sweep.Transform.localScale = sweep.BaseScale * Mathf.Lerp(0.4f, 1.02f, visibility);
                braceSweeps[index] = sweep;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= braceSweepRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = braceSweepRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.8f + index * 0.1f + rendererOffset, 0.09f);
                    float alphaScale = rendererOffset == 0 ? 0.62f : 1f;
                    rendererComponent.material.color = braceSweepBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateHoldBeacons()
        {
            for (int index = 0; index < holdBeacons.Count; index++)
            {
                HoldBeacon beacon = holdBeacons[index];
                if (beacon.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.13f + beacon.Phase, 1f);
                float intensity = cycle < 0.46f ? Mathf.Sin(cycle / 0.46f * Mathf.PI) : 0f;
                beacon.Transform.position = beacon.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.2f + beacon.Phase) * 0.1f, 0f);
                beacon.Transform.localScale = new Vector3(
                    beacon.BaseScale.x * Mathf.Lerp(0.62f, 1.14f, intensity),
                    beacon.BaseScale.y * Mathf.Lerp(0.82f, 1.04f, intensity),
                    beacon.BaseScale.z * Mathf.Lerp(0.62f, 1.14f, intensity));
                holdBeacons[index] = beacon;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= holdBeaconRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = holdBeaconRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.6f + index * 0.1f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.56f : rendererOffset == 1 ? 1f : 0.74f;
                    rendererComponent.material.color = holdBeaconBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateBulwarkLinks()
        {
            for (int index = 0; index < bulwarkLinks.Count; index++)
            {
                BulwarkLink link = bulwarkLinks[index];
                if (link.Transform == null)
                {
                    continue;
                }

                float intensity = 0.62f + Mathf.PingPong(Time.time * 0.22f + link.Phase, 0.38f);
                link.Transform.position = link.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + link.Phase) * 0.05f, 0f);
                link.Transform.localScale = new Vector3(
                    link.BaseScale.x * Mathf.Lerp(0.94f, 1.04f, intensity),
                    link.BaseScale.y * Mathf.Lerp(0.82f, 1.12f, intensity),
                    link.BaseScale.z * Mathf.Lerp(0.94f, 1.08f, intensity));
                bulwarkLinks[index] = link;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= bulwarkLinkRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = bulwarkLinkRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.2f + index * 0.09f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.52f : rendererOffset == 1 ? 1f : 0.78f;
                    rendererComponent.material.color = bulwarkLinkBaseColors[rendererIndex] * Mathf.Max(0.04f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateReserveRelays()
        {
            for (int index = 0; index < reserveRelays.Count; index++)
            {
                ReserveRelay relay = reserveRelays[index];
                if (relay.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.145f + relay.Phase, 1f);
                float travelT = cycle < 0.58f ? cycle / 0.58f : 1f;
                float visibility = cycle < 0.58f ? 1f : Mathf.Clamp01(1f - (cycle - 0.58f) / 0.2f);

                Vector3 position = Vector3.Lerp(relay.StartPosition, relay.EndPosition, travelT);
                relay.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.17f + relay.Phase) * 0.06f, 0f);

                Vector3 direction = (relay.EndPosition - relay.StartPosition).normalized;
                relay.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                relay.Transform.localScale = relay.BaseScale * Mathf.Lerp(0.42f, 1.02f, visibility);
                reserveRelays[index] = relay;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= reserveRelayRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = reserveRelayRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.5f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.6f : 1f;
                    rendererComponent.material.color = reserveRelayBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateReserveAnchors()
        {
            for (int index = 0; index < reserveAnchors.Count; index++)
            {
                ReserveAnchor anchor = reserveAnchors[index];
                if (anchor.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.135f + anchor.Phase, 1f);
                float intensity = cycle < 0.44f ? Mathf.Sin(cycle / 0.44f * Mathf.PI) : 0f;

                anchor.Transform.position = anchor.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.21f + anchor.Phase) * 0.08f, 0f);
                anchor.Transform.localScale = new Vector3(
                    anchor.BaseScale.x * Mathf.Lerp(0.62f, 1.12f, intensity),
                    anchor.BaseScale.y * Mathf.Lerp(0.84f, 1.04f, intensity),
                    anchor.BaseScale.z * Mathf.Lerp(0.62f, 1.12f, intensity));
                reserveAnchors[index] = anchor;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= reserveAnchorRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = reserveAnchorRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.7f + index * 0.1f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.56f : rendererOffset == 1 ? 1f : 0.74f;
                    rendererComponent.material.color = reserveAnchorBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateReserveSurges()
        {
            for (int index = 0; index < reserveSurges.Count; index++)
            {
                ReserveSurge surge = reserveSurges[index];
                if (surge.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + surge.Phase, 1f);
                float travelT = cycle < 0.6f ? cycle / 0.6f : 1f;
                float visibility = cycle < 0.6f ? 1f : Mathf.Clamp01(1f - (cycle - 0.6f) / 0.18f);

                Vector3 position = Vector3.Lerp(surge.StartPosition, surge.EndPosition, travelT);
                surge.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + surge.Phase) * 0.06f, 0f);

                Vector3 direction = (surge.EndPosition - surge.StartPosition).normalized;
                surge.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                surge.Transform.localScale = surge.BaseScale * Mathf.Lerp(0.42f, 1.04f, visibility);
                reserveSurges[index] = surge;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= reserveSurgeRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = reserveSurgeRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.6f + index * 0.11f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.6f : 1f;
                    rendererComponent.material.color = reserveSurgeBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateCommitBeacons()
        {
            for (int index = 0; index < commitBeacons.Count; index++)
            {
                CommitBeacon beacon = commitBeacons[index];
                if (beacon.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + beacon.Phase, 1f);
                float intensity = cycle < 0.42f ? Mathf.Sin(cycle / 0.42f * Mathf.PI) : 0f;

                beacon.Transform.position = beacon.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.22f + beacon.Phase) * 0.08f, 0f);
                beacon.Transform.localScale = new Vector3(
                    beacon.BaseScale.x * Mathf.Lerp(0.64f, 1.1f, intensity),
                    beacon.BaseScale.y * Mathf.Lerp(0.86f, 1.04f, intensity),
                    beacon.BaseScale.z * Mathf.Lerp(0.64f, 1.1f, intensity));
                commitBeacons[index] = beacon;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= commitBeaconRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = commitBeaconRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.8f + index * 0.1f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.56f : rendererOffset == 1 ? 1f : 0.74f;
                    rendererComponent.material.color = commitBeaconBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateForwardSpills()
        {
            for (int index = 0; index < forwardSpills.Count; index++)
            {
                ForwardSpill spill = forwardSpills[index];
                if (spill.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.155f + spill.Phase, 1f);
                float travelT = cycle < 0.58f ? cycle / 0.58f : 1f;
                float visibility = cycle < 0.58f ? 1f : Mathf.Clamp01(1f - (cycle - 0.58f) / 0.18f);

                Vector3 position = Vector3.Lerp(spill.StartPosition, spill.EndPosition, travelT);
                spill.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.19f + spill.Phase) * 0.05f, 0f);

                Vector3 direction = (spill.EndPosition - spill.StartPosition).normalized;
                spill.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                spill.Transform.localScale = spill.BaseScale * Mathf.Lerp(0.42f, 1.02f, visibility);
                forwardSpills[index] = spill;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= forwardSpillRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = forwardSpillRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.7f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.6f : 1f;
                    rendererComponent.material.color = forwardSpillBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateEdgeClashes()
        {
            for (int index = 0; index < edgeClashes.Count; index++)
            {
                EdgeClash clash = edgeClashes[index];
                if (clash.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.145f + clash.Phase, 1f);
                float intensity = cycle < 0.4f ? Mathf.Sin(cycle / 0.4f * Mathf.PI) : 0f;

                clash.Transform.position = clash.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.22f + clash.Phase) * 0.05f, 0f);
                clash.Transform.localScale = new Vector3(
                    clash.BaseScale.x * Mathf.Lerp(0.64f, 1.08f, intensity),
                    clash.BaseScale.y * Mathf.Lerp(0.88f, 1.04f, intensity),
                    clash.BaseScale.z * Mathf.Lerp(0.64f, 1.08f, intensity));
                edgeClashes[index] = clash;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= edgeClashRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = edgeClashRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.9f + index * 0.1f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.54f : rendererOffset == 1 ? 1f : 0.78f;
                    rendererComponent.material.color = edgeClashBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateReboundTraces()
        {
            for (int index = 0; index < reboundTraces.Count; index++)
            {
                ReboundTrace trace = reboundTraces[index];
                if (trace.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + trace.Phase, 1f);
                float travelT = cycle < 0.54f ? cycle / 0.54f : 1f;
                float visibility = cycle < 0.54f ? 1f : Mathf.Clamp01(1f - (cycle - 0.54f) / 0.2f);

                Vector3 position = Vector3.Lerp(trace.StartPosition, trace.EndPosition, travelT);
                trace.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.16f + trace.Phase) * 0.04f, 0f);

                Vector3 direction = (trace.EndPosition - trace.StartPosition).normalized;
                trace.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                trace.Transform.localScale = trace.BaseScale * Mathf.Lerp(0.42f, 0.96f, visibility);
                reboundTraces[index] = trace;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= reboundTraceRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = reboundTraceRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.4f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.58f : 1f;
                    rendererComponent.material.color = reboundTraceBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateFallbackBeacons()
        {
            for (int index = 0; index < fallbackBeacons.Count; index++)
            {
                FallbackBeacon beacon = fallbackBeacons[index];
                if (beacon.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + beacon.Phase, 1f);
                float intensity = cycle < 0.42f ? Mathf.Sin(cycle / 0.42f * Mathf.PI) : 0f;

                beacon.Transform.position = beacon.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.2f + beacon.Phase) * 0.05f, 0f);
                beacon.Transform.localScale = new Vector3(
                    beacon.BaseScale.x * Mathf.Lerp(0.66f, 1.08f, intensity),
                    beacon.BaseScale.y * Mathf.Lerp(0.88f, 1.04f, intensity),
                    beacon.BaseScale.z * Mathf.Lerp(0.66f, 1.08f, intensity));
                fallbackBeacons[index] = beacon;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= fallbackBeaconRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = fallbackBeaconRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.6f + index * 0.1f + rendererOffset, 0.1f);
                    float alphaScale = rendererOffset == 0 ? 0.54f : rendererOffset == 1 ? 1f : 0.74f;
                    rendererComponent.material.color = fallbackBeaconBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateFallbackSweeps()
        {
            for (int index = 0; index < fallbackSweeps.Count; index++)
            {
                FallbackSweep sweep = fallbackSweeps[index];
                if (sweep.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.145f + sweep.Phase, 1f);
                float travelT = cycle < 0.56f ? cycle / 0.56f : 1f;
                float visibility = cycle < 0.56f ? 1f : Mathf.Clamp01(1f - (cycle - 0.56f) / 0.2f);

                Vector3 position = Vector3.Lerp(sweep.StartPosition, sweep.EndPosition, travelT);
                sweep.Transform.position = position + new Vector3(Mathf.Sin(Time.time * 0.17f + sweep.Phase) * 0.04f, 0f, 0f);

                Vector3 direction = (sweep.EndPosition - sweep.StartPosition).normalized;
                sweep.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                sweep.Transform.localScale = sweep.BaseScale * Mathf.Lerp(0.42f, 0.96f, visibility);
                fallbackSweeps[index] = sweep;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= fallbackSweepRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = fallbackSweepRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.5f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.58f : 1f;
                    rendererComponent.material.color = fallbackSweepBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulse * alphaScale);
                }
            }
        }

        private void AnimateRecoveryLattices()
        {
            for (int index = 0; index < recoveryLattices.Count; index++)
            {
                RecoveryLattice lattice = recoveryLattices[index];
                if (lattice.Transform == null)
                {
                    continue;
                }

                float intensity = 0.64f + Mathf.PingPong(Time.time * 0.24f + lattice.Phase, 0.3f);
                lattice.Transform.position = lattice.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + lattice.Phase) * 0.03f, 0f);
                lattice.Transform.localScale = new Vector3(
                    lattice.BaseScale.x * Mathf.Lerp(0.96f, 1.04f, intensity),
                    lattice.BaseScale.y * Mathf.Lerp(0.84f, 1.1f, intensity),
                    lattice.BaseScale.z * Mathf.Lerp(0.96f, 1.03f, intensity));
                recoveryLattices[index] = lattice;

                int rendererStart = index * 4;
                for (int rendererOffset = 0; rendererOffset < 4; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= recoveryLatticeRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = recoveryLatticeRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulse = 0.9f + Mathf.PingPong(Time.time * 4.3f + index * 0.09f + rendererOffset, 0.09f);
                    float alphaScale = rendererOffset == 0 ? 0.5f : rendererOffset == 1 ? 1f : 0.74f;
                    rendererComponent.material.color = recoveryLatticeBaseColors[rendererIndex] * Mathf.Max(0.03f, intensity * pulse * alphaScale);
                }
            }
        }

        private void AnimateStabilityPulses()
        {
            for (int index = 0; index < stabilityPulses.Count; index++)
            {
                StabilityPulse pulse = stabilityPulses[index];
                if (pulse.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + pulse.Phase, 1f);
                float travelT = cycle < 0.58f ? cycle / 0.58f : 1f;
                float visibility = cycle < 0.58f ? 1f : Mathf.Clamp01(1f - (cycle - 0.58f) / 0.18f);

                Vector3 position = Vector3.Lerp(pulse.StartPosition, pulse.EndPosition, travelT);
                pulse.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + pulse.Phase) * 0.03f, 0f);

                Vector3 direction = (pulse.EndPosition - pulse.StartPosition).normalized;
                pulse.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                pulse.Transform.localScale = pulse.BaseScale * Mathf.Lerp(0.48f, 0.98f, visibility);
                stabilityPulses[index] = pulse;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= stabilityPulseRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = stabilityPulseRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 4.4f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.58f : 1f;
                    rendererComponent.material.color = stabilityPulseBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateSentinelEchos()
        {
            for (int index = 0; index < sentinelEchos.Count; index++)
            {
                SentinelEcho echo = sentinelEchos[index];
                if (echo.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + echo.Phase, 1f);
                float intensity = cycle < 0.4f ? Mathf.Sin(cycle / 0.4f * Mathf.PI) : 0f;

                echo.Transform.position = echo.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.2f + echo.Phase) * 0.03f, 0f);
                echo.Transform.localScale = new Vector3(
                    echo.BaseScale.x * Mathf.Lerp(0.7f, 1.08f, intensity),
                    echo.BaseScale.y * Mathf.Lerp(0.9f, 1.04f, intensity),
                    echo.BaseScale.z * Mathf.Lerp(0.7f, 1.08f, intensity));
                sentinelEchos[index] = echo;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= sentinelEchoRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = sentinelEchoRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 4.2f + index * 0.1f + rendererOffset, 0.09f);
                    float alphaScale = rendererOffset == 0 ? 0.54f : rendererOffset == 1 ? 1f : 0.72f;
                    rendererComponent.material.color = sentinelEchoBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateSentinelReturns()
        {
            for (int index = 0; index < sentinelReturns.Count; index++)
            {
                SentinelReturn signalReturn = sentinelReturns[index];
                if (signalReturn.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.15f + signalReturn.Phase, 1f);
                float travelT = cycle < 0.56f ? cycle / 0.56f : 1f;
                float visibility = cycle < 0.56f ? 1f : Mathf.Clamp01(1f - (cycle - 0.56f) / 0.18f);

                Vector3 position = Vector3.Lerp(signalReturn.StartPosition, signalReturn.EndPosition, travelT);
                signalReturn.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.18f + signalReturn.Phase) * 0.03f, 0f);

                Vector3 direction = (signalReturn.EndPosition - signalReturn.StartPosition).normalized;
                signalReturn.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                signalReturn.Transform.localScale = signalReturn.BaseScale * Mathf.Lerp(0.5f, 0.94f, visibility);
                sentinelReturns[index] = signalReturn;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= sentinelReturnRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = sentinelReturnRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 4.3f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.58f : 1f;
                    rendererComponent.material.color = sentinelReturnBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateCircuitSeals()
        {
            for (int index = 0; index < circuitSeals.Count; index++)
            {
                CircuitSeal seal = circuitSeals[index];
                if (seal.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.14f + seal.Phase, 1f);
                float intensity = cycle < 0.38f ? Mathf.Sin(cycle / 0.38f * Mathf.PI) : 0f;

                seal.Transform.position = seal.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.19f + seal.Phase) * 0.025f, 0f);
                seal.Transform.localScale = new Vector3(
                    seal.BaseScale.x * Mathf.Lerp(0.74f, 1.08f, intensity),
                    seal.BaseScale.y * Mathf.Lerp(0.92f, 1.04f, intensity),
                    seal.BaseScale.z * Mathf.Lerp(0.74f, 1.08f, intensity));
                circuitSeals[index] = seal;

                int rendererStart = index * 3;
                for (int rendererOffset = 0; rendererOffset < 3; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= circuitSealRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = circuitSealRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 4.1f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.52f : rendererOffset == 1 ? 1f : 0.7f;
                    rendererComponent.material.color = circuitSealBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateSealRipples()
        {
            for (int index = 0; index < sealRipples.Count; index++)
            {
                SealRipple ripple = sealRipples[index];
                if (ripple.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.135f + ripple.Phase, 1f);
                float intensity = cycle < 0.42f ? Mathf.Sin(cycle / 0.42f * Mathf.PI) : 0f;

                ripple.Transform.position = ripple.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.17f + ripple.Phase) * 0.02f, 0f);
                ripple.Transform.localScale = new Vector3(
                    ripple.BaseScale.x * Mathf.Lerp(0.7f, 1.8f, intensity),
                    ripple.BaseScale.y,
                    ripple.BaseScale.z * Mathf.Lerp(0.7f, 1.8f, intensity));
                sealRipples[index] = ripple;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= sealRippleRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = sealRippleRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.9f + Mathf.PingPong(Time.time * 4f + index * 0.1f + rendererOffset, 0.08f);
                    float alphaScale = rendererOffset == 0 ? 0.5f : 1f;
                    rendererComponent.material.color = sealRippleBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateSealAfterglows()
        {
            for (int index = 0; index < sealAfterglows.Count; index++)
            {
                SealAfterglow glow = sealAfterglows[index];
                if (glow.Transform == null)
                {
                    continue;
                }

                float intensity = 0.56f + Mathf.PingPong(Time.time * 0.18f + glow.Phase, 0.18f);
                glow.Transform.position = glow.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.14f + glow.Phase) * 0.01f, 0f);
                glow.Transform.localScale = new Vector3(
                    glow.BaseScale.x * Mathf.Lerp(0.92f, 1.12f, intensity),
                    glow.BaseScale.y,
                    glow.BaseScale.z * Mathf.Lerp(0.92f, 1.12f, intensity));
                sealAfterglows[index] = glow;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= sealAfterglowRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = sealAfterglowRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.92f + Mathf.PingPong(Time.time * 3.8f + index * 0.08f + rendererOffset, 0.06f);
                    float alphaScale = rendererOffset == 0 ? 0.46f : 1f;
                    rendererComponent.material.color = sealAfterglowBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateAfterglowDrifts()
        {
            for (int index = 0; index < afterglowDrifts.Count; index++)
            {
                AfterglowDrift drift = afterglowDrifts[index];
                if (drift.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.12f + drift.Phase, 1f);
                float travelT = cycle < 0.6f ? cycle / 0.6f : 1f;
                float visibility = cycle < 0.6f ? 1f : Mathf.Clamp01(1f - (cycle - 0.6f) / 0.2f);

                Vector3 position = Vector3.Lerp(drift.StartPosition, drift.EndPosition, travelT);
                drift.Transform.position = position + new Vector3(0f, Mathf.Sin(Time.time * 0.14f + drift.Phase) * 0.01f, 0f);
                drift.Transform.rotation = Quaternion.LookRotation((drift.EndPosition - drift.StartPosition).normalized, Vector3.up);
                drift.Transform.localScale = drift.BaseScale * Mathf.Lerp(0.5f, 0.88f, visibility);
                afterglowDrifts[index] = drift;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= afterglowDriftRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = afterglowDriftRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.94f + Mathf.PingPong(Time.time * 3.6f + index * 0.08f + rendererOffset, 0.04f);
                    float alphaScale = rendererOffset == 0 ? 0.5f : 1f;
                    rendererComponent.material.color = afterglowDriftBaseColors[rendererIndex] * Mathf.Max(0.02f, visibility * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateDormantVeils()
        {
            for (int index = 0; index < dormantVeils.Count; index++)
            {
                DormantVeil veil = dormantVeils[index];
                if (veil.Transform == null)
                {
                    continue;
                }

                float intensity = 0.52f + Mathf.PingPong(Time.time * 0.12f + veil.Phase, 0.12f);
                veil.Transform.position = veil.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.11f + veil.Phase) * 0.006f, 0f);
                veil.Transform.localScale = new Vector3(
                    veil.BaseScale.x * Mathf.Lerp(0.96f, 1.06f, intensity),
                    veil.BaseScale.y,
                    veil.BaseScale.z * Mathf.Lerp(0.96f, 1.06f, intensity));
                dormantVeils[index] = veil;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= dormantVeilRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = dormantVeilRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.94f + Mathf.PingPong(Time.time * 3.2f + index * 0.08f + rendererOffset, 0.04f);
                    float alphaScale = rendererOffset == 0 ? 0.42f : 1f;
                    rendererComponent.material.color = dormantVeilBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateQuietResidues()
        {
            for (int index = 0; index < quietResidues.Count; index++)
            {
                QuietResidue residue = quietResidues[index];
                if (residue.Transform == null)
                {
                    continue;
                }

                float intensity = 0.5f + Mathf.PingPong(Time.time * 0.09f + residue.Phase, 0.08f);
                residue.Transform.position = residue.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.09f + residue.Phase) * 0.003f, 0f);
                residue.Transform.localScale = new Vector3(
                    residue.BaseScale.x * Mathf.Lerp(0.98f, 1.04f, intensity),
                    residue.BaseScale.y,
                    residue.BaseScale.z * Mathf.Lerp(0.98f, 1.04f, intensity));
                quietResidues[index] = residue;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= quietResidueRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = quietResidueRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.96f + Mathf.PingPong(Time.time * 2.8f + index * 0.06f + rendererOffset, 0.03f);
                    float alphaScale = rendererOffset == 0 ? 0.38f : 1f;
                    rendererComponent.material.color = quietResidueBaseColors[rendererIndex] * Mathf.Max(0.02f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateResidualBlinks()
        {
            for (int index = 0; index < residualBlinks.Count; index++)
            {
                ResidualBlink blink = residualBlinks[index];
                if (blink.Transform == null)
                {
                    continue;
                }

                float cycle = Mathf.Repeat(Time.time * 0.1f + blink.Phase, 1f);
                float intensity = cycle < 0.22f ? Mathf.Sin(cycle / 0.22f * Mathf.PI) : 0f;

                blink.Transform.position = blink.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.08f + blink.Phase) * 0.002f, 0f);
                blink.Transform.localScale = blink.BaseScale * Mathf.Lerp(0.7f, 1.06f, intensity);
                residualBlinks[index] = blink;

                int rendererIndex = index;
                if (rendererIndex >= residualBlinkRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = residualBlinkRenderers[rendererIndex];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.96f + Mathf.PingPong(Time.time * 2.4f + index * 0.05f, 0.02f);
                rendererComponent.material.color = residualBlinkBaseColors[rendererIndex] * Mathf.Max(0.015f, intensity * pulseValue);
            }
        }

        private void AnimateLastEmbers()
        {
            for (int index = 0; index < lastEmbers.Count; index++)
            {
                LastEmber ember = lastEmbers[index];
                if (ember.Transform == null)
                {
                    continue;
                }

                float intensity = 0.42f + Mathf.PingPong(Time.time * 0.07f + ember.Phase, 0.06f);
                ember.Transform.position = ember.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.06f + ember.Phase) * 0.0015f, 0f);
                ember.Transform.localScale = ember.BaseScale * Mathf.Lerp(0.94f, 1.04f, intensity);
                lastEmbers[index] = ember;

                int rendererIndex = index;
                if (rendererIndex >= lastEmberRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = lastEmberRenderers[rendererIndex];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.97f + Mathf.PingPong(Time.time * 2.2f + index * 0.04f, 0.015f);
                rendererComponent.material.color = lastEmberBaseColors[rendererIndex] * Mathf.Max(0.012f, intensity * pulseValue);
            }
        }

        private void AnimateFinalHushes()
        {
            for (int index = 0; index < finalHushes.Count; index++)
            {
                FinalHush hush = finalHushes[index];
                if (hush.Transform == null)
                {
                    continue;
                }

                float intensity = 0.38f + Mathf.PingPong(Time.time * 0.05f + hush.Phase, 0.04f);
                hush.Transform.position = hush.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.05f + hush.Phase) * 0.001f, 0f);
                hush.Transform.localScale = hush.BaseScale * Mathf.Lerp(0.985f, 1.02f, intensity);
                finalHushes[index] = hush;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= finalHushRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = finalHushRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.98f + Mathf.PingPong(Time.time * 1.8f + index * 0.04f + rendererOffset, 0.01f);
                    float alphaScale = rendererOffset == 0 ? 0.34f : 1f;
                    rendererComponent.material.color = finalHushBaseColors[rendererIndex] * Mathf.Max(0.01f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateStillTraces()
        {
            for (int index = 0; index < stillTraces.Count; index++)
            {
                StillTrace trace = stillTraces[index];
                if (trace.Transform == null)
                {
                    continue;
                }

                float intensity = 0.34f + Mathf.PingPong(Time.time * 0.04f + trace.Phase, 0.025f);
                trace.Transform.position = trace.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.04f + trace.Phase) * 0.0008f, 0f);
                trace.Transform.localScale = trace.BaseScale * Mathf.Lerp(0.992f, 1.012f, intensity);
                stillTraces[index] = trace;

                int rendererStart = index * 2;
                for (int rendererOffset = 0; rendererOffset < 2; rendererOffset++)
                {
                    int rendererIndex = rendererStart + rendererOffset;
                    if (rendererIndex >= stillTraceRenderers.Count)
                    {
                        break;
                    }

                    Renderer rendererComponent = stillTraceRenderers[rendererIndex];
                    if (rendererComponent == null)
                    {
                        continue;
                    }

                    float pulseValue = 0.985f + Mathf.PingPong(Time.time * 1.4f + index * 0.03f + rendererOffset, 0.008f);
                    float alphaScale = rendererOffset == 0 ? 0.28f : 1f;
                    rendererComponent.material.color = stillTraceBaseColors[rendererIndex] * Mathf.Max(0.008f, intensity * pulseValue * alphaScale);
                }
            }
        }

        private void AnimateSettledSpecks()
        {
            for (int index = 0; index < settledSpecks.Count; index++)
            {
                SettledSpeck speck = settledSpecks[index];
                if (speck.Transform == null)
                {
                    continue;
                }

                float intensity = 0.3f + Mathf.PingPong(Time.time * 0.03f + speck.Phase, 0.018f);
                speck.Transform.position = speck.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.03f + speck.Phase) * 0.0004f, 0f);
                speck.Transform.localScale = speck.BaseScale * Mathf.Lerp(0.996f, 1.008f, intensity);
                settledSpecks[index] = speck;

                if (index >= settledSpeckRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = settledSpeckRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.988f + Mathf.PingPong(Time.time * 1.2f + index * 0.03f, 0.006f);
                rendererComponent.material.color = settledSpeckBaseColors[index] * Mathf.Max(0.006f, intensity * pulseValue);
            }
        }

        private void AnimateSilentGrains()
        {
            for (int index = 0; index < silentGrains.Count; index++)
            {
                SilentGrain grain = silentGrains[index];
                if (grain.Transform == null)
                {
                    continue;
                }

                float intensity = 0.27f + Mathf.PingPong(Time.time * 0.025f + grain.Phase, 0.012f);
                grain.Transform.position = grain.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.025f + grain.Phase) * 0.00025f, 0f);
                grain.Transform.localScale = grain.BaseScale * Mathf.Lerp(0.997f, 1.006f, intensity);
                silentGrains[index] = grain;

                if (index >= silentGrainRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = silentGrainRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99f + Mathf.PingPong(Time.time * 1f + index * 0.025f, 0.004f);
                rendererComponent.material.color = silentGrainBaseColors[index] * Mathf.Max(0.004f, intensity * pulseValue);
            }
        }

        private void AnimateMuteDusts()
        {
            for (int index = 0; index < muteDusts.Count; index++)
            {
                MuteDust dust = muteDusts[index];
                if (dust.Transform == null)
                {
                    continue;
                }

                float intensity = 0.24f + Mathf.PingPong(Time.time * 0.02f + dust.Phase, 0.01f);
                dust.Transform.position = dust.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.02f + dust.Phase) * 0.00018f, 0f);
                dust.Transform.localScale = dust.BaseScale * Mathf.Lerp(0.998f, 1.004f, intensity);
                muteDusts[index] = dust;

                if (index >= muteDustRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = muteDustRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.992f + Mathf.PingPong(Time.time * 0.8f + index * 0.02f, 0.003f);
                rendererComponent.material.color = muteDustBaseColors[index] * Mathf.Max(0.003f, intensity * pulseValue);
            }
        }

        private void AnimateStillAshes()
        {
            for (int index = 0; index < stillAshes.Count; index++)
            {
                StillAsh ash = stillAshes[index];
                if (ash.Transform == null)
                {
                    continue;
                }

                float intensity = 0.22f + Mathf.PingPong(Time.time * 0.016f + ash.Phase, 0.008f);
                ash.Transform.position = ash.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.016f + ash.Phase) * 0.00012f, 0f);
                ash.Transform.localScale = ash.BaseScale * Mathf.Lerp(0.9985f, 1.003f, intensity);
                stillAshes[index] = ash;

                if (index >= stillAshRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = stillAshRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.993f + Mathf.PingPong(Time.time * 0.7f + index * 0.018f, 0.0025f);
                rendererComponent.material.color = stillAshBaseColors[index] * Mathf.Max(0.0025f, intensity * pulseValue);
            }
        }

        private void AnimateColdSpecks()
        {
            for (int index = 0; index < coldSpecks.Count; index++)
            {
                ColdSpeck speck = coldSpecks[index];
                if (speck.Transform == null)
                {
                    continue;
                }

                float intensity = 0.2f + Mathf.PingPong(Time.time * 0.013f + speck.Phase, 0.006f);
                speck.Transform.position = speck.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.013f + speck.Phase) * 0.00008f, 0f);
                speck.Transform.localScale = speck.BaseScale * Mathf.Lerp(0.999f, 1.0022f, intensity);
                coldSpecks[index] = speck;

                if (index >= coldSpeckRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = coldSpeckRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.994f + Mathf.PingPong(Time.time * 0.58f + index * 0.016f, 0.002f);
                rendererComponent.material.color = coldSpeckBaseColors[index] * Mathf.Max(0.002f, intensity * pulseValue);
            }
        }

        private void AnimateFrostMotes()
        {
            for (int index = 0; index < frostMotes.Count; index++)
            {
                FrostMote mote = frostMotes[index];
                if (mote.Transform == null)
                {
                    continue;
                }

                float intensity = 0.18f + Mathf.PingPong(Time.time * 0.011f + mote.Phase, 0.005f);
                mote.Transform.position = mote.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.011f + mote.Phase) * 0.00005f, 0f);
                mote.Transform.localScale = mote.BaseScale * Mathf.Lerp(0.9992f, 1.0018f, intensity);
                frostMotes[index] = mote;

                if (index >= frostMoteRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = frostMoteRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.995f + Mathf.PingPong(Time.time * 0.46f + index * 0.014f, 0.0016f);
                rendererComponent.material.color = frostMoteBaseColors[index] * Mathf.Max(0.0016f, intensity * pulseValue);
            }
        }

        private void AnimateRimeSeeds()
        {
            for (int index = 0; index < rimeSeeds.Count; index++)
            {
                RimeSeed seed = rimeSeeds[index];
                if (seed.Transform == null)
                {
                    continue;
                }

                float intensity = 0.165f + Mathf.PingPong(Time.time * 0.009f + seed.Phase, 0.0038f);
                seed.Transform.position = seed.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.009f + seed.Phase) * 0.000035f, 0f);
                seed.Transform.localScale = seed.BaseScale * Mathf.Lerp(0.99935f, 1.00135f, intensity);
                rimeSeeds[index] = seed;

                if (index >= rimeSeedRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = rimeSeedRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.996f + Mathf.PingPong(Time.time * 0.38f + index * 0.012f, 0.0012f);
                rendererComponent.material.color = rimeSeedBaseColors[index] * Mathf.Max(0.0012f, intensity * pulseValue);
            }
        }

        private void AnimateIcePins()
        {
            for (int index = 0; index < icePins.Count; index++)
            {
                IcePin pin = icePins[index];
                if (pin.Transform == null)
                {
                    continue;
                }

                float intensity = 0.15f + Mathf.PingPong(Time.time * 0.007f + pin.Phase, 0.003f);
                pin.Transform.position = pin.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.007f + pin.Phase) * 0.000025f, 0f);
                pin.Transform.localScale = pin.BaseScale * Mathf.Lerp(0.9995f, 1.0011f, intensity);
                icePins[index] = pin;

                if (index >= icePinRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = icePinRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9965f + Mathf.PingPong(Time.time * 0.31f + index * 0.01f, 0.0009f);
                rendererComponent.material.color = icePinBaseColors[index] * Mathf.Max(0.0009f, intensity * pulseValue);
            }
        }

        private void AnimateChillNails()
        {
            for (int index = 0; index < chillNails.Count; index++)
            {
                ChillNail nail = chillNails[index];
                if (nail.Transform == null)
                {
                    continue;
                }

                float intensity = 0.138f + Mathf.PingPong(Time.time * 0.006f + nail.Phase, 0.0022f);
                nail.Transform.position = nail.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.006f + nail.Phase) * 0.000018f, 0f);
                nail.Transform.localScale = nail.BaseScale * Mathf.Lerp(0.99962f, 1.00085f, intensity);
                chillNails[index] = nail;

                if (index >= chillNailRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = chillNailRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.997f + Mathf.PingPong(Time.time * 0.25f + index * 0.009f, 0.0007f);
                rendererComponent.material.color = chillNailBaseColors[index] * Mathf.Max(0.0007f, intensity * pulseValue);
            }
        }

        private void AnimateFrostTacks()
        {
            for (int index = 0; index < frostTacks.Count; index++)
            {
                FrostTack tack = frostTacks[index];
                if (tack.Transform == null)
                {
                    continue;
                }

                float intensity = 0.13f + Mathf.PingPong(Time.time * 0.005f + tack.Phase, 0.0017f);
                tack.Transform.position = tack.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.005f + tack.Phase) * 0.000014f, 0f);
                tack.Transform.localScale = tack.BaseScale * Mathf.Lerp(0.99972f, 1.00065f, intensity);
                frostTacks[index] = tack;

                if (index >= frostTackRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = frostTackRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9973f + Mathf.PingPong(Time.time * 0.21f + index * 0.008f, 0.0005f);
                rendererComponent.material.color = frostTackBaseColors[index] * Mathf.Max(0.0005f, intensity * pulseValue);
            }
        }

        private void AnimateGlazeDots()
        {
            for (int index = 0; index < glazeDots.Count; index++)
            {
                GlazeDot dot = glazeDots[index];
                if (dot.Transform == null)
                {
                    continue;
                }

                float intensity = 0.124f + Mathf.PingPong(Time.time * 0.004f + dot.Phase, 0.0014f);
                dot.Transform.position = dot.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.004f + dot.Phase) * 0.000011f, 0f);
                dot.Transform.localScale = dot.BaseScale * Mathf.Lerp(0.99978f, 1.00052f, intensity);
                glazeDots[index] = dot;

                if (index >= glazeDotRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = glazeDotRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9975f + Mathf.PingPong(Time.time * 0.18f + index * 0.007f, 0.00035f);
                rendererComponent.material.color = glazeDotBaseColors[index] * Mathf.Max(0.00035f, intensity * pulseValue);
            }
        }

        private void AnimateHoarBeads()
        {
            for (int index = 0; index < hoarBeads.Count; index++)
            {
                HoarBead bead = hoarBeads[index];
                if (bead.Transform == null)
                {
                    continue;
                }

                float intensity = 0.119f + Mathf.PingPong(Time.time * 0.0034f + bead.Phase, 0.0011f);
                bead.Transform.position = bead.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0034f + bead.Phase) * 0.000009f, 0f);
                bead.Transform.localScale = bead.BaseScale * Mathf.Lerp(0.99982f, 1.00043f, intensity);
                hoarBeads[index] = bead;

                if (index >= hoarBeadRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = hoarBeadRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9977f + Mathf.PingPong(Time.time * 0.15f + index * 0.006f, 0.00027f);
                rendererComponent.material.color = hoarBeadBaseColors[index] * Mathf.Max(0.00027f, intensity * pulseValue);
            }
        }

        private void AnimatePaleDews()
        {
            for (int index = 0; index < paleDews.Count; index++)
            {
                PaleDew dew = paleDews[index];
                if (dew.Transform == null)
                {
                    continue;
                }

                float intensity = 0.114f + Mathf.PingPong(Time.time * 0.0029f + dew.Phase, 0.0009f);
                dew.Transform.position = dew.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0029f + dew.Phase) * 0.000007f, 0f);
                dew.Transform.localScale = dew.BaseScale * Mathf.Lerp(0.99986f, 1.00036f, intensity);
                paleDews[index] = dew;

                if (index >= paleDewRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = paleDewRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9979f + Mathf.PingPong(Time.time * 0.13f + index * 0.005f, 0.00022f);
                rendererComponent.material.color = paleDewBaseColors[index] * Mathf.Max(0.00022f, intensity * pulseValue);
            }
        }

        private void AnimateFaintPearls()
        {
            for (int index = 0; index < faintPearls.Count; index++)
            {
                FaintPearl pearl = faintPearls[index];
                if (pearl.Transform == null)
                {
                    continue;
                }

                float intensity = 0.109f + Mathf.PingPong(Time.time * 0.0024f + pearl.Phase, 0.00075f);
                pearl.Transform.position = pearl.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0024f + pearl.Phase) * 0.0000055f, 0f);
                pearl.Transform.localScale = pearl.BaseScale * Mathf.Lerp(0.9999f, 1.0003f, intensity);
                faintPearls[index] = pearl;

                if (index >= faintPearlRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = faintPearlRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9981f + Mathf.PingPong(Time.time * 0.11f + index * 0.004f, 0.00017f);
                rendererComponent.material.color = faintPearlBaseColors[index] * Mathf.Max(0.00017f, intensity * pulseValue);
            }
        }

        private void AnimateWanDroplets()
        {
            for (int index = 0; index < wanDroplets.Count; index++)
            {
                WanDroplet droplet = wanDroplets[index];
                if (droplet.Transform == null)
                {
                    continue;
                }

                float intensity = 0.105f + Mathf.PingPong(Time.time * 0.002f + droplet.Phase, 0.0006f);
                droplet.Transform.position = droplet.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.002f + droplet.Phase) * 0.0000045f, 0f);
                droplet.Transform.localScale = droplet.BaseScale * Mathf.Lerp(0.99993f, 1.00024f, intensity);
                wanDroplets[index] = droplet;

                if (index >= wanDropletRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = wanDropletRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9982f + Mathf.PingPong(Time.time * 0.095f + index * 0.004f, 0.00013f);
                rendererComponent.material.color = wanDropletBaseColors[index] * Mathf.Max(0.00013f, intensity * pulseValue);
            }
        }

        private void AnimateHushMoistures()
        {
            for (int index = 0; index < hushMoistures.Count; index++)
            {
                HushMoisture moisture = hushMoistures[index];
                if (moisture.Transform == null)
                {
                    continue;
                }

                float intensity = 0.102f + Mathf.PingPong(Time.time * 0.0017f + moisture.Phase, 0.00045f);
                moisture.Transform.position = moisture.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0017f + moisture.Phase) * 0.0000035f, 0f);
                moisture.Transform.localScale = moisture.BaseScale * Mathf.Lerp(0.99995f, 1.00018f, intensity);
                hushMoistures[index] = moisture;

                if (index >= hushMoistureRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = hushMoistureRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9983f + Mathf.PingPong(Time.time * 0.082f + index * 0.003f, 0.0001f);
                rendererComponent.material.color = hushMoistureBaseColors[index] * Mathf.Max(0.0001f, intensity * pulseValue);
            }
        }

        private void AnimateDimCondensates()
        {
            for (int index = 0; index < dimCondensates.Count; index++)
            {
                DimCondensate condensate = dimCondensates[index];
                if (condensate.Transform == null)
                {
                    continue;
                }

                float intensity = 0.1f + Mathf.PingPong(Time.time * 0.0014f + condensate.Phase, 0.00034f);
                condensate.Transform.position = condensate.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0014f + condensate.Phase) * 0.0000028f, 0f);
                condensate.Transform.localScale = condensate.BaseScale * Mathf.Lerp(0.99996f, 1.00014f, intensity);
                dimCondensates[index] = condensate;

                if (index >= dimCondensateRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = dimCondensateRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9984f + Mathf.PingPong(Time.time * 0.071f + index * 0.003f, 0.00008f);
                rendererComponent.material.color = dimCondensateBaseColors[index] * Mathf.Max(0.00008f, intensity * pulseValue);
            }
        }

        private void AnimateStillFilms()
        {
            for (int index = 0; index < stillFilms.Count; index++)
            {
                StillFilm film = stillFilms[index];
                if (film.Transform == null)
                {
                    continue;
                }

                float intensity = 0.098f + Mathf.PingPong(Time.time * 0.0012f + film.Phase, 0.00026f);
                film.Transform.position = film.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0012f + film.Phase) * 0.0000022f, 0f);
                film.Transform.localScale = film.BaseScale * Mathf.Lerp(0.99997f, 1.0001f, intensity);
                stillFilms[index] = film;

                if (index >= stillFilmRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = stillFilmRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99845f + Mathf.PingPong(Time.time * 0.062f + index * 0.0026f, 0.00006f);
                rendererComponent.material.color = stillFilmBaseColors[index] * Mathf.Max(0.00006f, intensity * pulseValue);
            }
        }

        private void AnimateQuietSheens()
        {
            for (int index = 0; index < quietSheens.Count; index++)
            {
                QuietSheen sheen = quietSheens[index];
                if (sheen.Transform == null)
                {
                    continue;
                }

                float intensity = 0.096f + Mathf.PingPong(Time.time * 0.001f + sheen.Phase, 0.0002f);
                sheen.Transform.position = sheen.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.001f + sheen.Phase) * 0.0000018f, 0f);
                sheen.Transform.localScale = sheen.BaseScale * Mathf.Lerp(0.99998f, 1.00008f, intensity);
                quietSheens[index] = sheen;

                if (index >= quietSheenRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = quietSheenRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9985f + Mathf.PingPong(Time.time * 0.055f + index * 0.0024f, 0.000045f);
                rendererComponent.material.color = quietSheenBaseColors[index] * Mathf.Max(0.000045f, intensity * pulseValue);
            }
        }

        private void AnimateLastLustres()
        {
            for (int index = 0; index < lastLustres.Count; index++)
            {
                LastLustre lustre = lastLustres[index];
                if (lustre.Transform == null)
                {
                    continue;
                }

                float intensity = 0.094f + Mathf.PingPong(Time.time * 0.00085f + lustre.Phase, 0.00016f);
                lustre.Transform.position = lustre.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00085f + lustre.Phase) * 0.0000014f, 0f);
                lustre.Transform.localScale = lustre.BaseScale * Mathf.Lerp(0.999985f, 1.00006f, intensity);
                lastLustres[index] = lustre;

                if (index >= lastLustreRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = lastLustreRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99855f + Mathf.PingPong(Time.time * 0.048f + index * 0.0021f, 0.000032f);
                rendererComponent.material.color = lastLustreBaseColors[index] * Mathf.Max(0.000032f, intensity * pulseValue);
            }
        }

        private void AnimateThinGlints()
        {
            for (int index = 0; index < thinGlints.Count; index++)
            {
                ThinGlint glint = thinGlints[index];
                if (glint.Transform == null)
                {
                    continue;
                }

                float intensity = 0.092f + Mathf.PingPong(Time.time * 0.00072f + glint.Phase, 0.00012f);
                glint.Transform.position = glint.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00072f + glint.Phase) * 0.0000011f, 0f);
                glint.Transform.localScale = glint.BaseScale * Mathf.Lerp(0.99999f, 1.00004f, intensity);
                thinGlints[index] = glint;

                if (index >= thinGlintRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = thinGlintRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9986f + Mathf.PingPong(Time.time * 0.041f + index * 0.0019f, 0.000024f);
                rendererComponent.material.color = thinGlintBaseColors[index] * Mathf.Max(0.000024f, intensity * pulseValue);
            }
        }

        private void AnimateFadingGleams()
        {
            for (int index = 0; index < fadingGleams.Count; index++)
            {
                FadingGleam gleam = fadingGleams[index];
                if (gleam.Transform == null)
                {
                    continue;
                }

                float intensity = 0.09f + Mathf.PingPong(Time.time * 0.00058f + gleam.Phase, 0.00009f);
                gleam.Transform.position = gleam.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00058f + gleam.Phase) * 0.0000009f, 0f);
                gleam.Transform.localScale = gleam.BaseScale * Mathf.Lerp(0.999992f, 1.00003f, intensity);
                fadingGleams[index] = gleam;

                if (index >= fadingGleamRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = fadingGleamRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99865f + Mathf.PingPong(Time.time * 0.036f + index * 0.0017f, 0.000018f);
                rendererComponent.material.color = fadingGleamBaseColors[index] * Mathf.Max(0.000018f, intensity * pulseValue);
            }
        }

        private void AnimateSoftTraces()
        {
            for (int index = 0; index < softTraces.Count; index++)
            {
                SoftTrace trace = softTraces[index];
                if (trace.Transform == null)
                {
                    continue;
                }

                float intensity = 0.088f + Mathf.PingPong(Time.time * 0.00048f + trace.Phase, 0.00007f);
                trace.Transform.position = trace.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00048f + trace.Phase) * 0.0000007f, 0f);
                trace.Transform.localScale = trace.BaseScale * Mathf.Lerp(0.999994f, 1.000022f, intensity);
                softTraces[index] = trace;

                if (index >= softTraceRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = softTraceRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9987f + Mathf.PingPong(Time.time * 0.031f + index * 0.0015f, 0.000014f);
                rendererComponent.material.color = softTraceBaseColors[index] * Mathf.Max(0.000014f, intensity * pulseValue);
            }
        }

        private void AnimateFaintVeils()
        {
            for (int index = 0; index < faintVeils.Count; index++)
            {
                FaintVeil veil = faintVeils[index];
                if (veil.Transform == null)
                {
                    continue;
                }

                float intensity = 0.086f + Mathf.PingPong(Time.time * 0.00041f + veil.Phase, 0.000055f);
                veil.Transform.position = veil.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00041f + veil.Phase) * 0.00000055f, 0f);
                veil.Transform.localScale = veil.BaseScale * Mathf.Lerp(0.999995f, 1.000018f, intensity);
                faintVeils[index] = veil;

                if (index >= faintVeilRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = faintVeilRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99874f + Mathf.PingPong(Time.time * 0.027f + index * 0.0013f, 0.000011f);
                rendererComponent.material.color = faintVeilBaseColors[index] * Mathf.Max(0.000011f, intensity * pulseValue);
            }
        }

        private void AnimateGhostSheens()
        {
            for (int index = 0; index < ghostSheens.Count; index++)
            {
                GhostSheen sheen = ghostSheens[index];
                if (sheen.Transform == null)
                {
                    continue;
                }

                float intensity = 0.084f + Mathf.PingPong(Time.time * 0.00034f + sheen.Phase, 0.000043f);
                sheen.Transform.position = sheen.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00034f + sheen.Phase) * 0.00000042f, 0f);
                sheen.Transform.localScale = sheen.BaseScale * Mathf.Lerp(0.999996f, 1.000014f, intensity);
                ghostSheens[index] = sheen;

                if (index >= ghostSheenRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = ghostSheenRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99877f + Mathf.PingPong(Time.time * 0.023f + index * 0.0011f, 0.000009f);
                rendererComponent.material.color = ghostSheenBaseColors[index] * Mathf.Max(0.000009f, intensity * pulseValue);
            }
        }

        private void AnimateFinalTints()
        {
            for (int index = 0; index < finalTints.Count; index++)
            {
                FinalTint tint = finalTints[index];
                if (tint.Transform == null)
                {
                    continue;
                }

                float intensity = 0.083f + Mathf.PingPong(Time.time * 0.00029f + tint.Phase, 0.000034f);
                tint.Transform.position = tint.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00029f + tint.Phase) * 0.00000033f, 0f);
                tint.Transform.localScale = tint.BaseScale * Mathf.Lerp(0.999997f, 1.000011f, intensity);
                finalTints[index] = tint;

                if (index >= finalTintRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = finalTintRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99879f + Mathf.PingPong(Time.time * 0.02f + index * 0.001f, 0.000007f);
                rendererComponent.material.color = finalTintBaseColors[index] * Mathf.Max(0.000007f, intensity * pulseValue);
            }
        }

        private void AnimateMuteHues()
        {
            for (int index = 0; index < muteHues.Count; index++)
            {
                MuteHue hue = muteHues[index];
                if (hue.Transform == null)
                {
                    continue;
                }

                float intensity = 0.082f + Mathf.PingPong(Time.time * 0.00024f + hue.Phase, 0.000027f);
                hue.Transform.position = hue.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00024f + hue.Phase) * 0.00000026f, 0f);
                hue.Transform.localScale = hue.BaseScale * Mathf.Lerp(0.9999975f, 1.000009f, intensity);
                muteHues[index] = hue;

                if (index >= muteHueRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = muteHueRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99881f + Mathf.PingPong(Time.time * 0.017f + index * 0.0009f, 0.000005f);
                rendererComponent.material.color = muteHueBaseColors[index] * Mathf.Max(0.000005f, intensity * pulseValue);
            }
        }

        private void AnimateHushedTints()
        {
            for (int index = 0; index < hushedTints.Count; index++)
            {
                HushedTint tint = hushedTints[index];
                if (tint.Transform == null)
                {
                    continue;
                }

                float intensity = 0.081f + Mathf.PingPong(Time.time * 0.0002f + tint.Phase, 0.000021f);
                tint.Transform.position = tint.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0002f + tint.Phase) * 0.00000021f, 0f);
                tint.Transform.localScale = tint.BaseScale * Mathf.Lerp(0.999998f, 1.000007f, intensity);
                hushedTints[index] = tint;

                if (index >= hushedTintRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = hushedTintRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99882f + Mathf.PingPong(Time.time * 0.015f + index * 0.0008f, 0.000004f);
                rendererComponent.material.color = hushedTintBaseColors[index] * Mathf.Max(0.000004f, intensity * pulseValue);
            }
        }

        private void AnimateFadedCasts()
        {
            for (int index = 0; index < fadedCasts.Count; index++)
            {
                FadedCast cast = fadedCasts[index];
                if (cast.Transform == null)
                {
                    continue;
                }

                float intensity = 0.08f + Mathf.PingPong(Time.time * 0.00017f + cast.Phase, 0.000017f);
                cast.Transform.position = cast.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00017f + cast.Phase) * 0.00000017f, 0f);
                cast.Transform.localScale = cast.BaseScale * Mathf.Lerp(0.9999985f, 1.000006f, intensity);
                fadedCasts[index] = cast;

                if (index >= fadedCastRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = fadedCastRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99883f + Mathf.PingPong(Time.time * 0.013f + index * 0.0007f, 0.000003f);
                rendererComponent.material.color = fadedCastBaseColors[index] * Mathf.Max(0.000003f, intensity * pulseValue);
            }
        }

        private void AnimateSpentShades()
        {
            for (int index = 0; index < spentShades.Count; index++)
            {
                SpentShade shade = spentShades[index];
                if (shade.Transform == null)
                {
                    continue;
                }

                float intensity = 0.079f + Mathf.PingPong(Time.time * 0.00014f + shade.Phase, 0.000014f);
                shade.Transform.position = shade.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00014f + shade.Phase) * 0.00000014f, 0f);
                shade.Transform.localScale = shade.BaseScale * Mathf.Lerp(0.999999f, 1.000005f, intensity);
                spentShades[index] = shade;

                if (index >= spentShadeRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = spentShadeRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99884f + Mathf.PingPong(Time.time * 0.011f + index * 0.0006f, 0.000002f);
                rendererComponent.material.color = spentShadeBaseColors[index] * Mathf.Max(0.000002f, intensity * pulseValue);
            }
        }

        private void AnimateDryStains()
        {
            for (int index = 0; index < dryStains.Count; index++)
            {
                DryStain stain = dryStains[index];
                if (stain.Transform == null)
                {
                    continue;
                }

                float intensity = 0.078f + Mathf.PingPong(Time.time * 0.00011f + stain.Phase, 0.000011f);
                stain.Transform.position = stain.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.00011f + stain.Phase) * 0.00000011f, 0f);
                stain.Transform.localScale = stain.BaseScale * Mathf.Lerp(0.9999992f, 1.000004f, intensity);
                dryStains[index] = stain;

                if (index >= dryStainRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = dryStainRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99885f + Mathf.PingPong(Time.time * 0.009f + index * 0.0005f, 0.0000015f);
                rendererComponent.material.color = dryStainBaseColors[index] * Mathf.Max(0.0000015f, intensity * pulseValue);
            }
        }

        private void AnimateWornMarks()
        {
            for (int index = 0; index < wornMarks.Count; index++)
            {
                WornMark mark = wornMarks[index];
                if (mark.Transform == null)
                {
                    continue;
                }

                float intensity = 0.077f + Mathf.PingPong(Time.time * 0.000085f + mark.Phase, 0.000008f);
                mark.Transform.position = mark.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000085f + mark.Phase) * 0.000000085f, 0f);
                mark.Transform.localScale = mark.BaseScale * Mathf.Lerp(0.9999994f, 1.000003f, intensity);
                wornMarks[index] = mark;

                if (index >= wornMarkRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = wornMarkRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99886f + Mathf.PingPong(Time.time * 0.007f + index * 0.0004f, 0.000001f);
                rendererComponent.material.color = wornMarkBaseColors[index] * Mathf.Max(0.000001f, intensity * pulseValue);
            }
        }

        private void AnimateFaintScuffs()
        {
            for (int index = 0; index < faintScuffs.Count; index++)
            {
                FaintScuff scuff = faintScuffs[index];
                if (scuff.Transform == null)
                {
                    continue;
                }

                float intensity = 0.076f + Mathf.PingPong(Time.time * 0.000063f + scuff.Phase, 0.000006f);
                scuff.Transform.position = scuff.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000063f + scuff.Phase) * 0.000000063f, 0f);
                scuff.Transform.localScale = scuff.BaseScale * Mathf.Lerp(0.9999996f, 1.0000022f, intensity);
                faintScuffs[index] = scuff;

                if (index >= faintScuffRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = faintScuffRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99887f + Mathf.PingPong(Time.time * 0.005f + index * 0.0003f, 0.0000007f);
                rendererComponent.material.color = faintScuffBaseColors[index] * Mathf.Max(0.0000007f, intensity * pulseValue);
            }
        }

        private void AnimateTraceNicks()
        {
            for (int index = 0; index < traceNicks.Count; index++)
            {
                TraceNick nick = traceNicks[index];
                if (nick.Transform == null)
                {
                    continue;
                }

                float intensity = 0.075f + Mathf.PingPong(Time.time * 0.000047f + nick.Phase, 0.000004f);
                nick.Transform.position = nick.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000047f + nick.Phase) * 0.000000047f, 0f);
                nick.Transform.localScale = nick.BaseScale * Mathf.Lerp(0.99999975f, 1.0000016f, intensity);
                traceNicks[index] = nick;

                if (index >= traceNickRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = traceNickRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99888f + Mathf.PingPong(Time.time * 0.0038f + index * 0.0002f, 0.0000004f);
                rendererComponent.material.color = traceNickBaseColors[index] * Mathf.Max(0.0000004f, intensity * pulseValue);
            }
        }

        private void AnimatePinPricks()
        {
            for (int index = 0; index < pinPricks.Count; index++)
            {
                PinPrick prick = pinPricks[index];
                if (prick.Transform == null)
                {
                    continue;
                }

                float intensity = 0.074f + Mathf.PingPong(Time.time * 0.000034f + prick.Phase, 0.000003f);
                prick.Transform.position = prick.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000034f + prick.Phase) * 0.000000034f, 0f);
                prick.Transform.localScale = prick.BaseScale * Mathf.Lerp(0.99999985f, 1.0000012f, intensity);
                pinPricks[index] = prick;

                if (index >= pinPrickRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = pinPrickRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99889f + Mathf.PingPong(Time.time * 0.0028f + index * 0.00015f, 0.00000025f);
                rendererComponent.material.color = pinPrickBaseColors[index] * Mathf.Max(0.00000025f, intensity * pulseValue);
            }
        }

        private void AnimateNeedleDots()
        {
            for (int index = 0; index < needleDots.Count; index++)
            {
                NeedleDot dot = needleDots[index];
                if (dot.Transform == null)
                {
                    continue;
                }

                float intensity = 0.073f + Mathf.PingPong(Time.time * 0.000024f + dot.Phase, 0.000002f);
                dot.Transform.position = dot.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000024f + dot.Phase) * 0.000000024f, 0f);
                dot.Transform.localScale = dot.BaseScale * Mathf.Lerp(0.99999992f, 1.0000008f, intensity);
                needleDots[index] = dot;

                if (index >= needleDotRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = needleDotRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.9989f + Mathf.PingPong(Time.time * 0.0019f + index * 0.0001f, 0.00000012f);
                rendererComponent.material.color = needleDotBaseColors[index] * Mathf.Max(0.00000012f, intensity * pulseValue);
            }
        }

        private void AnimateDustSpecks()
        {
            for (int index = 0; index < dustSpecks.Count; index++)
            {
                DustSpeck speck = dustSpecks[index];
                if (speck.Transform == null)
                {
                    continue;
                }

                float intensity = 0.072f + Mathf.PingPong(Time.time * 0.000017f + speck.Phase, 0.0000014f);
                speck.Transform.position = speck.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000017f + speck.Phase) * 0.000000017f, 0f);
                speck.Transform.localScale = speck.BaseScale * Mathf.Lerp(0.99999996f, 1.00000045f, intensity);
                dustSpecks[index] = speck;

                if (index >= dustSpeckRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = dustSpeckRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99891f + Mathf.PingPong(Time.time * 0.0013f + index * 0.00008f, 0.00000007f);
                rendererComponent.material.color = dustSpeckBaseColors[index] * Mathf.Max(0.00000007f, intensity * pulseValue);
            }
        }

        private void AnimateAshMotes()
        {
            for (int index = 0; index < ashMotes.Count; index++)
            {
                AshMote mote = ashMotes[index];
                if (mote.Transform == null)
                {
                    continue;
                }

                float intensity = 0.071f + Mathf.PingPong(Time.time * 0.000012f + mote.Phase, 0.000001f);
                mote.Transform.position = mote.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.000012f + mote.Phase) * 0.000000012f, 0f);
                mote.Transform.localScale = mote.BaseScale * Mathf.Lerp(0.99999998f, 1.00000028f, intensity);
                ashMotes[index] = mote;

                if (index >= ashMoteRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = ashMoteRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99892f + Mathf.PingPong(Time.time * 0.0009f + index * 0.00006f, 0.00000004f);
                rendererComponent.material.color = ashMoteBaseColors[index] * Mathf.Max(0.00000004f, intensity * pulseValue);
            }
        }

        private void AnimateSootFleckLayer()
        {
            for (int index = 0; index < sootFlecks.Count; index++)
            {
                SootFleck fleck = sootFlecks[index];
                if (fleck.Transform == null)
                {
                    continue;
                }

                float intensity = 0.0705f + Mathf.PingPong(Time.time * 0.0000085f + fleck.Phase, 0.0000007f);
                fleck.Transform.position = fleck.BasePosition + new Vector3(0f, Mathf.Sin(Time.time * 0.0000085f + fleck.Phase) * 0.000000008f, 0f);
                fleck.Transform.localScale = fleck.BaseScale * Mathf.Lerp(0.99999999f, 1.00000016f, intensity);
                sootFlecks[index] = fleck;

                if (index >= sootFleckRenderers.Count)
                {
                    continue;
                }

                Renderer rendererComponent = sootFleckRenderers[index];
                if (rendererComponent == null)
                {
                    continue;
                }

                float pulseValue = 0.99893f + Mathf.PingPong(Time.time * 0.00065f + index * 0.00004f, 0.000000025f);
                rendererComponent.material.color = sootFleckBaseColors[index] * Mathf.Max(0.000000025f, intensity * pulseValue);
            }
        }

        private void AnimateHorizonSilhouettes()
        {
            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < horizonSilhouettes.Count; index++)
            {
                HorizonSilhouette silhouette = horizonSilhouettes[index];
                if (silhouette.Transform == null)
                {
                    continue;
                }

                silhouette.Transform.position += silhouette.Velocity * Time.deltaTime;
                Vector3 position = silhouette.Transform.position;
                if (position.x > mapProfile.MaxX + 1200f)
                {
                    position.x = mapProfile.MinX - 1200f;
                }

                float bob = Mathf.Sin(Time.time * 0.12f + silhouette.Phase) * 0.9f;
                position.y += bob * Time.deltaTime;
                silhouette.Transform.position = position;
                silhouette.Transform.localScale = silhouette.BaseScale * (0.98f + Mathf.PingPong(Time.time * 0.04f + silhouette.Phase, 0.04f));
                horizonSilhouettes[index] = silhouette;
            }
        }

        private void AnimateSkyTrails()
        {
            if (mapProfile == null)
            {
                return;
            }

            for (int index = 0; index < skyTrails.Count; index++)
            {
                SkyTrail trail = skyTrails[index];
                if (trail.Transform == null)
                {
                    continue;
                }

                trail.Transform.position += trail.Velocity * Time.deltaTime;
                Vector3 position = trail.Transform.position;
                if (position.x > mapProfile.MaxX + 700f || position.z < mapProfile.MinZ - 700f)
                {
                    position.x = mapProfile.MinX - 700f;
                    position.z = mapProfile.MaxZ + 700f;
                    position.y = 76f + index * 4.2f + Mathf.Sin(Time.time * 0.2f + trail.Phase) * 3f;
                    trail.Transform.position = position;
                }

                float shimmer = 0.86f + Mathf.PingPong(Time.time * 0.8f + trail.Phase, 0.18f);
                trail.Transform.localScale = new Vector3(trail.BaseScale.x * shimmer, trail.BaseScale.y, trail.BaseScale.z * (0.94f + shimmer * 0.1f));

                if (index < skyTrailRenderers.Count && skyTrailRenderers[index] != null)
                {
                    skyTrailRenderers[index].material.color = skyTrailBaseColors[index] * shimmer;
                }

                if (index < skyTrailTipRenderers.Count && skyTrailTipRenderers[index] != null)
                {
                    skyTrailTipRenderers[index].material.color = skyTrailTipBaseColors[index] * (0.92f + shimmer * 0.16f);
                }

                trail.Transform.rotation = Quaternion.LookRotation(trail.Velocity.normalized, Vector3.up);
                skyTrails[index] = trail;
            }
        }

        private static void CreateSilhouetteSpine(Transform parent, Color color, float offset)
        {
            GameObject spine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spine.name = "Silhouette Spine";
            spine.transform.SetParent(parent);
            spine.transform.localPosition = new Vector3(0f, 0.48f, offset);
            spine.transform.localScale = new Vector3(0.84f, 0.12f, 0.18f);
            spine.transform.localRotation = Quaternion.identity;
            Collider spineCollider = spine.GetComponent<Collider>();
            if (spineCollider != null)
            {
                spineCollider.enabled = false;
            }

            Renderer spineRenderer = spine.GetComponent<Renderer>();
            if (spineRenderer != null)
            {
                spineRenderer.material.color = color;
            }
        }

        private static void CreateSilhouetteTower(Transform parent, Color color, Vector3 localPosition, Vector3 localScale)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "Silhouette Tower";
            tower.transform.SetParent(parent);
            tower.transform.localPosition = localPosition;
            tower.transform.localScale = localScale;
            tower.transform.localRotation = Quaternion.identity;
            Collider towerCollider = tower.GetComponent<Collider>();
            if (towerCollider != null)
            {
                towerCollider.enabled = false;
            }

            Renderer towerRenderer = tower.GetComponent<Renderer>();
            if (towerRenderer != null)
            {
                towerRenderer.material.color = color;
            }
        }

        private void CreatePatrolCraft(Transform parent, Color hullColor, Color glowColor, float localX, float localY, float localZ, float scaleMultiplier)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Patrol Craft Body";
            body.transform.SetParent(parent);
            body.transform.localPosition = new Vector3(localX, localY, localZ);
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(5.2f, 0.9f, 11.4f) * scaleMultiplier;
            DisableCollider(body);
            SetRendererColor(body, hullColor);

            GameObject wingLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingLeft.name = "Patrol Craft Wing Left";
            wingLeft.transform.SetParent(body.transform);
            wingLeft.transform.localPosition = new Vector3(-3.8f, -0.1f, -1.2f);
            wingLeft.transform.localRotation = Quaternion.Euler(0f, -12f, 8f);
            wingLeft.transform.localScale = new Vector3(0.34f, 0.12f, 0.72f);
            DisableCollider(wingLeft);
            SetRendererColor(wingLeft, hullColor);

            GameObject wingRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingRight.name = "Patrol Craft Wing Right";
            wingRight.transform.SetParent(body.transform);
            wingRight.transform.localPosition = new Vector3(3.8f, -0.1f, -1.2f);
            wingRight.transform.localRotation = Quaternion.Euler(0f, 12f, -8f);
            wingRight.transform.localScale = new Vector3(0.34f, 0.12f, 0.72f);
            DisableCollider(wingRight);
            SetRendererColor(wingRight, hullColor);

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "Patrol Craft Glow";
            glow.transform.SetParent(body.transform);
            glow.transform.localPosition = new Vector3(0f, 0.12f, 5.2f);
            glow.transform.localRotation = Quaternion.identity;
            glow.transform.localScale = Vector3.one * 1.9f;
            DisableCollider(glow);
            Renderer glowRenderer = glow.GetComponent<Renderer>();
            if (glowRenderer != null)
            {
                glowRenderer.material.color = glowColor;
                patrolGlowRenderers.Add(glowRenderer);
                patrolGlowBaseColors.Add(glowColor);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(body, BattlefieldFogRequirement.Explored);
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(wingLeft, BattlefieldFogRequirement.Explored);
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(wingRight, BattlefieldFogRequirement.Explored);
            PrototypeTerrainPrimitiveFactory.EnsureFogObject(glow, BattlefieldFogRequirement.Explored);
        }

        private static void DisableCollider(GameObject target)
        {
            Collider colliderComponent = target.GetComponent<Collider>();
            if (colliderComponent != null)
            {
                colliderComponent.enabled = false;
            }
        }

        private static void SetRendererColor(GameObject target, Color color)
        {
            Renderer rendererComponent = target.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
            }
        }

        private void CreateOrbitalLanceElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                orbitalLanceRenderers.Add(rendererComponent);
                orbitalLanceBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFlakBurstElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                flakBurstRenderers.Add(rendererComponent);
                flakBurstBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateWreckageElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                if (name.Contains("Trail") || name.Contains("Ember"))
                {
                    wreckageGlowRenderers.Add(rendererComponent);
                    wreckageGlowBaseColors.Add(color);
                }
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateShieldImpactElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                shieldImpactRenderers.Add(rendererComponent);
                shieldImpactBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSiegeVolleyElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                siegeVolleyRenderers.Add(rendererComponent);
                siegeVolleyBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateLaunchStreakElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                launchStreakRenderers.Add(rendererComponent);
                launchStreakBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateBatteryFlashElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                batteryFlashRenderers.Add(rendererComponent);
                batteryFlashBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateBarrageImpactElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                barrageImpactRenderers.Add(rendererComponent);
                barrageImpactBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateTargetDesignatorElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                targetDesignatorRenderers.Add(rendererComponent);
                targetDesignatorBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateCounterScanElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                counterScanRenderers.Add(rendererComponent);
                counterScanBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private Transform CreateRelayPulseElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                relayPulseRenderers.Add(rendererComponent);
                relayPulseBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
            return primitive.transform;
        }

        private void CreateResponseArcElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                responseArcRenderers.Add(rendererComponent);
                responseArcBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateCommandEchoElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                commandEchoRenderers.Add(rendererComponent);
                commandEchoBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateOrderRippleElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                orderRippleRenderers.Add(rendererComponent);
                orderRippleBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateAdvanceChevronElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                advanceChevronRenderers.Add(rendererComponent);
                advanceChevronBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFrontlineAcknowledgeElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                frontlineAcknowledgeRenderers.Add(rendererComponent);
                frontlineAcknowledgeBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateRallyStreamElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                rallyStreamRenderers.Add(rendererComponent);
                rallyStreamBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateBattlelineHandoffElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                battlelineHandoffRenderers.Add(rendererComponent);
                battlelineHandoffBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateClashPulseElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                clashPulseRenderers.Add(rendererComponent);
                clashPulseBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateShockfrontTraceElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                shockfrontTraceRenderers.Add(rendererComponent);
                shockfrontTraceBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreatePressureFlareElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                pressureFlareRenderers.Add(rendererComponent);
                pressureFlareBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateBraceSweepElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                braceSweepRenderers.Add(rendererComponent);
                braceSweepBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateHoldBeaconElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                holdBeaconRenderers.Add(rendererComponent);
                holdBeaconBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateBulwarkLinkElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                bulwarkLinkRenderers.Add(rendererComponent);
                bulwarkLinkBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateReserveRelayElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                reserveRelayRenderers.Add(rendererComponent);
                reserveRelayBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateReserveAnchorElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                reserveAnchorRenderers.Add(rendererComponent);
                reserveAnchorBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateReserveSurgeElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                reserveSurgeRenderers.Add(rendererComponent);
                reserveSurgeBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateCommitBeaconElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                commitBeaconRenderers.Add(rendererComponent);
                commitBeaconBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateForwardSpillElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                forwardSpillRenderers.Add(rendererComponent);
                forwardSpillBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateEdgeClashElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                edgeClashRenderers.Add(rendererComponent);
                edgeClashBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateReboundTraceElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                reboundTraceRenderers.Add(rendererComponent);
                reboundTraceBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFallbackBeaconElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                fallbackBeaconRenderers.Add(rendererComponent);
                fallbackBeaconBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFallbackSweepElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                fallbackSweepRenderers.Add(rendererComponent);
                fallbackSweepBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateRecoveryLatticeElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                recoveryLatticeRenderers.Add(rendererComponent);
                recoveryLatticeBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateStabilityPulseElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                stabilityPulseRenderers.Add(rendererComponent);
                stabilityPulseBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSentinelEchoElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                sentinelEchoRenderers.Add(rendererComponent);
                sentinelEchoBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSentinelReturnElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                sentinelReturnRenderers.Add(rendererComponent);
                sentinelReturnBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateCircuitSealElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                circuitSealRenderers.Add(rendererComponent);
                circuitSealBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSealRippleElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                sealRippleRenderers.Add(rendererComponent);
                sealRippleBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSealAfterglowElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                sealAfterglowRenderers.Add(rendererComponent);
                sealAfterglowBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateAfterglowDriftElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                afterglowDriftRenderers.Add(rendererComponent);
                afterglowDriftBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateDormantVeilElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                dormantVeilRenderers.Add(rendererComponent);
                dormantVeilBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateQuietResidueElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                quietResidueRenderers.Add(rendererComponent);
                quietResidueBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateResidualBlinkElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                residualBlinkRenderers.Add(rendererComponent);
                residualBlinkBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateLastEmberElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                lastEmberRenderers.Add(rendererComponent);
                lastEmberBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFinalHushElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                finalHushRenderers.Add(rendererComponent);
                finalHushBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateStillTraceElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                stillTraceRenderers.Add(rendererComponent);
                stillTraceBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSettledSpeckElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                settledSpeckRenderers.Add(rendererComponent);
                settledSpeckBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSilentGrainElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                silentGrainRenderers.Add(rendererComponent);
                silentGrainBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateMuteDustElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                muteDustRenderers.Add(rendererComponent);
                muteDustBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateStillAshElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                stillAshRenderers.Add(rendererComponent);
                stillAshBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateColdSpeckElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                coldSpeckRenderers.Add(rendererComponent);
                coldSpeckBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFrostMoteElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                frostMoteRenderers.Add(rendererComponent);
                frostMoteBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateRimeSeedElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                rimeSeedRenderers.Add(rendererComponent);
                rimeSeedBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateIcePinElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                icePinRenderers.Add(rendererComponent);
                icePinBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateChillNailElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                chillNailRenderers.Add(rendererComponent);
                chillNailBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFrostTackElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                frostTackRenderers.Add(rendererComponent);
                frostTackBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateGlazeDotElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                glazeDotRenderers.Add(rendererComponent);
                glazeDotBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateHoarBeadElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                hoarBeadRenderers.Add(rendererComponent);
                hoarBeadBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreatePaleDewElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                paleDewRenderers.Add(rendererComponent);
                paleDewBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFaintPearlElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                faintPearlRenderers.Add(rendererComponent);
                faintPearlBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateWanDropletElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                wanDropletRenderers.Add(rendererComponent);
                wanDropletBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateHushMoistureElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                hushMoistureRenderers.Add(rendererComponent);
                hushMoistureBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateDimCondensateElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                dimCondensateRenderers.Add(rendererComponent);
                dimCondensateBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateStillFilmElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                stillFilmRenderers.Add(rendererComponent);
                stillFilmBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateQuietSheenElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                quietSheenRenderers.Add(rendererComponent);
                quietSheenBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateLastLustreElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                lastLustreRenderers.Add(rendererComponent);
                lastLustreBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateThinGlintElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                thinGlintRenderers.Add(rendererComponent);
                thinGlintBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFadingGleamElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                fadingGleamRenderers.Add(rendererComponent);
                fadingGleamBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSoftTraceElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                softTraceRenderers.Add(rendererComponent);
                softTraceBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFaintVeilElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                faintVeilRenderers.Add(rendererComponent);
                faintVeilBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateGhostSheenElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                ghostSheenRenderers.Add(rendererComponent);
                ghostSheenBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFinalTintElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                finalTintRenderers.Add(rendererComponent);
                finalTintBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateMuteHueElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                muteHueRenderers.Add(rendererComponent);
                muteHueBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateHushedTintElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                hushedTintRenderers.Add(rendererComponent);
                hushedTintBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFadedCastElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                fadedCastRenderers.Add(rendererComponent);
                fadedCastBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSpentShadeElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                spentShadeRenderers.Add(rendererComponent);
                spentShadeBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateDryStainElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                dryStainRenderers.Add(rendererComponent);
                dryStainBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateWornMarkElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                wornMarkRenderers.Add(rendererComponent);
                wornMarkBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateFaintScuffElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                faintScuffRenderers.Add(rendererComponent);
                faintScuffBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateTraceNickElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                traceNickRenderers.Add(rendererComponent);
                traceNickBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreatePinPrickElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                pinPrickRenderers.Add(rendererComponent);
                pinPrickBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateNeedleDotElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                needleDotRenderers.Add(rendererComponent);
                needleDotBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateDustSpeckElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                dustSpeckRenderers.Add(rendererComponent);
                dustSpeckBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateAshMoteElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                ashMoteRenderers.Add(rendererComponent);
                ashMoteBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }

        private void CreateSootFleckElement(Transform parent, string name, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            DisableCollider(primitive);

            Renderer rendererComponent = primitive.GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                rendererComponent.material.color = color;
                sootFleckRenderers.Add(rendererComponent);
                sootFleckBaseColors.Add(color);
            }

            PrototypeTerrainPrimitiveFactory.EnsureFogObject(primitive, BattlefieldFogRequirement.Explored);
        }
    }
}

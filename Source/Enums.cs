using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Assembly_CSharp.TasInfo.mm.Source {
    public enum SplitName {

        ManualSplit,

        AbyssShriek,
        
        CrystalHeart,
        
        CycloneSlash,
        
        DashSlash,
        
        DescendingDark,
        
        DesolateDive,
        
        DreamNail,
        
        DreamNail2,
        
        DreamGate,
        
        GreatSlash,
        
        HowlingWraiths,
        
        IsmasTear,
        
        MantisClaw,
        
        MonarchWings,
        
        MothwingCloak,
        
        ShadeCloak,
        
        ShadeSoul,
        
        VengefulSpirit,

        
        CityKey,
        
        HasDelicateFlower,
        
        ElegantKey,
        
        ElegantKeyShoptimised,
        
        GodTuner,
        
        HuntersMark,
        
        KingsBrand,
        
        LoveKey,
        
        LumaflyLantern,
        
        PaleLurkerKey,
        
        PaleOre,
        
        SalubrasBlessing,
        
        SimpleKey,
        
        SlyKey,
        
        TramPass,

        
        MaskFragment1,
        
        MaskFragment2,
        
        MaskFragment3,
        
        Mask1,
        
        MaskFragment5,
        
        MaskFragment6,
        
        MaskFragment7,
        
        Mask2,
        
        MaskFragment9,
        
        MaskFragment10,
        
        MaskFragment11,
        
        Mask3,
        
        MaskFragment13,
        
        MaskFragment14,
        
        MaskFragment15,
        
        Mask4,
        
        NailUpgrade1,
        
        NailUpgrade2,
        
        NailUpgrade3,
        
        NailUpgrade4,
        
        VesselFragment1,
        
        VesselFragment2,
        
        Vessel1,
        
        VesselFragment4,
        
        VesselFragment5,
        
        Vessel2,
        
        VesselFragment7,
        
        VesselFragment8,
        
        Vessel3,

        
        MaskShardMawlek,
        
        MaskShardGrubfather,
        
        MaskShardGoam,
        
        MaskShardQueensStation,
        
        MaskShardBretta,
        
        MaskShardStoneSanctuary,
        
        MaskShardWaterways,
        
        MaskShardFungalCore,
        
        MaskShardEnragedGuardian,
        
        MaskShardHive,
        
        MaskShardSeer,
        
        MaskShardFlower,

        
        VesselFragGreenpath,
        
        VesselFragCrossroadsLift,
        
        VesselFragKingsStation,
        
        VesselFragGarpedes,
        
        VesselFragStagNest,
        
        VesselFragSeer,
        
        VesselFragFountain,

        
        BrokenVessel,
        
        BroodingMawlek,
        
        EnterBroodingMawlek,
        
        Collector,
        
        TransCollector,
        
        CrystalGuardian1,
        
        CrystalGuardian2,
        
        DungDefender,
        
        DungDefenderIdol,
        
        GladeIdol,
        
        ElderHu,
        
        ElderHuEssence,
        
        ElderHuTrans,
        
        FalseKnight,
        
        FailedKnight,
        
        FailedChampionEssence,
        
        Flukemarm,
        
        Galien,
        
        GalienEssence,
        
        GodTamer,
        
        Gorb,
        
        GorbEssence,
        
        GreyPrince,
        
        GreyPrinceEssence,
        
        OnDefeatGPZ,
        
        GruzMother,
        
        HiveKnight,
        
        EnterHiveKnight,
        
        Hornet1,
        
        EnterHornet1,
        
        Hornet2,
        
        EnterHornet2,
        
        LostKin,
        
        LostKinEssence,
        
        MantisLords,
        
        Markoth,
        
        MarkothEssence,
        
        Marmu,
        
        MarmuEssence,
        
        MegaMossCharger,
        
        MegaMossChargerTrans,
        
        NightmareKingGrimm,
        
        NoEyes,
        
        NoEyesEssence,
        
        Nosk,
        
        KilledOblobbles,
        
        MatoOroNailBros,
        
        PureVessel,
        
        RadianceBoss,
        
        HollowKnightBoss,
        
        SheoPaintmaster,
        
        SlyNailsage,
        
        SoulMaster,
        
        EnterSoulMaster,
        
        SoulMasterEncountered,
        
        SoulTyrant,
        
        SoulTyrantEssence,
        
        SoulTyrantEssenceWithSanctumGrub,
        
        TraitorLord,
        
        TroupeMasterGrimm,
        
        EnterTMG,
        
        Uumuu,
        
        UumuuEncountered,
        
        BlackKnight,
        
        BlackKnightTrans,
        
        WhiteDefender,
        
        WhiteDefenderEssence,
        
        OnDefeatWhiteDefender,
        
        Xero,
        
        XeroEssence,

        
        VengeflyKingP,
        
        GruzMotherP,
        
        FalseKnightP,
        
        MassiveMossChargerP,
        
        Hornet1P,
        
        GorbP,
        
        DungDefenderP,
        
        SoulWarriorP,
        
        BroodingMawlekP,
        
        OroMatoNailBrosP,

        
        XeroP,
        
        CrystalGuardianP,
        
        SoulMasterP,
        
        OblobblesP,
        
        MantisLordsP,
        
        MarmuP,
        
        NoskP,
        
        FlukemarmP,
        
        BrokenVesselP,
        
        SheoPaintmasterP,

        
        HiveKnightP,
        
        ElderHuP,
        
        CollectorP,
        
        GodTamerP,
        
        TroupeMasterGrimmP,
        
        GalienP,
        
        GreyPrinceZoteP,
        
        UumuuP,
        
        Hornet2P,
        
        SlyP,

        
        EnragedGuardianP,
        
        LostKinP,
        
        NoEyesP,
        
        TraitorLordP,
        
        WhiteDefenderP,
        
        FailedChampionP,
        
        MarkothP,
        
        WatcherKnightsP,
        
        SoulTyrantP,
        
        PureVesselP,

        
        NoskHornetP,
        
        NightmareKingGrimmP,

        
        Hegemol,
        
        Lurien,
        
        Monomon,

        
        HegemolDreamer,
        
        LurienDreamer,
        
        MonomonDreamer,

        
        Dreamer1,
        
        Dreamer2,
        
        Dreamer3,

        
        PreGrimmShop,
        
        PreGrimmShopTrans,
        
        SlyShopFinished,
        
        AbyssDoor,
        
        AbyssLighthouse,
        
        CanOvercharm,
        
        UnchainedHollowKnight,
        
        WatcherChandelier,
        
        CityGateOpen,
        
        CityGateAndMantisLords,
        
        EndingSplit,
        
        PlayerDeath,
        
        ShadeKilled,
        
        LumaflyLanternTransition,
        
        FlowerQuest,
        
        FlowerRewardGiven,
        
        HappyCouplePlayerDataEvent,
        
        NailsmithKilled,
        
        NailsmithChoice,
        
        NightmareLantern,
        
        NightmareLanternDestroyed,
        
        HollowKnightDreamnail,
        
        SeerDeparts,
        
        SpiritGladeOpen,
        
        BeastsDenTrapBench,
        
        EternalOrdealUnlocked,
        
        EternalOrdealAchieved,
        
        RidingStag,
        
        SavedCloth,
        
        MineLiftOpened,
        
        PureSnail,
        
        ColosseumBronzeUnlocked,
        
        ColosseumSilverUnlocked,
        
        ColosseumGoldUnlocked,
        
        ColosseumBronze,
        
        ColosseumSilver,
        
        ColosseumGold,
        
        ColosseumBronzeEntry,
        
        ColosseumSilverEntry,
        
        ColosseumGoldEntry,
        
        ColosseumBronzeExit,
        
        ColosseumSilverExit,
        
        ColosseumGoldExit,
        
        Pantheon1,
        
        Pantheon2,
        
        Pantheon3,
        
        Pantheon4,
        
        Pantheon5,
        
        PathOfPain,

        
        AspidHunter,
        
        Aluba,
        //
        //Al2ba,
        
        HuskMiner,
        
        GreatHopper,
        
        GorgeousHusk,
        
        MenderBug,
        
        killedSanctumWarrior,
        
        killedSoulTwister,
        //
        //Revek,
        
        MossKnight,
        
        MushroomBrawler,
        
        Zote1,
        
        VengeflyKingTrans,
        
        Zote2,
        
        ZoteKilled,

        
        CrossroadsStation,
        
        GreenpathStation,
        
        QueensStationStation,
        
        QueensGardensStation,
        
        StoreroomsStation,
        
        KingsStationStation,
        
        RestingGroundsStation,
        
        DeepnestStation,
        
        HiddenStationStation,
        
        StagnestStation,

        
        MrMushroom1,
        
        MrMushroom2,
        
        MrMushroom3,
        
        MrMushroom4,
        
        MrMushroom5,
        
        MrMushroom6,
        
        MrMushroom7,

        
        Abyss,
        
        CityOfTears,
        
        Colosseum,
        
        CrystalPeak,
        
        Deepnest,
        
        DeepnestSpa,
        
        Dirtmouth,
        
        FogCanyon,
        
        ForgottenCrossroads,
        
        FungalWastes,
        
        Godhome,
        
        Greenpath,
        
        Hive,
        
        InfectedCrossroads,
        
        KingdomsEdge,
        
        QueensGardens,
        
        RestingGrounds,
        
        RoyalWaterways,
        
        TeachersArchive,
        
        WhitePalace,
        
        WhitePalaceSecretRoom,

        
        AncestralMound,
        
        BasinEntry,
        
        BlueLake,
        
        CatacombsEntry,
        
        CrystalPeakEntry,
        
        CrystalMoundExit,
        
        EnterDeepnest,
        
        EnterDirtmouth,
        
        EnterAnyDream,
        
        FogCanyonEntry,
        
        FungalWastesEntry,
        
        TransGorgeousHusk,
        
        EnterGodhome,
        
        EnterGreenpath,
        
        EnterGreenpathWithOvercharm,
        
        EnterCrown,
        
        EnterRafters,

        
        TransClaw,
        
        TransVS,
        
        TransDescendingDark,
        
        TransTear,
        
        TransTearWithGrub,
        
        EnterJunkPit,
        
        HiveEntry,
        
        KingsPass,
        
        KingsPassEnterFromTown,
        
        KingdomsEdgeEntry,
        
        KingdomsEdgeOvercharmedEntry,
        
        EnterNKG,
        
        QueensGardensEntry,
        
        QueensGardensFrogsTrans,
        
        QueensGardensPostArenaTransition,
        
        EnterSanctum,
        
        EnterSanctumWithShadeSoul,
        
        EnterLoveTower,
        
        WaterwaysEntry,
        
        WhitePalaceEntry,

        
        BaldurShell,
        
        Dashmaster,
        
        DeepFocus,
        
        DefendersCrest,
        
        Dreamshield,
        
        DreamWielder,
        
        Flukenest,
        
        FragileGreed,
        
        FragileHeart,
        
        FragileStrength,
        
        FuryOfTheFallen,
        
        GatheringSwarm,
        
        GlowingWomb,
        
        Grimmchild,
        
        Grimmchild2,
        
        Grimmchild3,
        
        Grimmchild4,
        
        GrubberflysElegy,
        
        Grubsong,
        
        HeavyBlow,
        
        Hiveblood,
        
        JonisBlessing,
        
        WhiteFragmentLeft,
        
        WhiteFragmentRight,
        
        Kingsoul,
        
        LifebloodCore,
        
        LifebloodHeart,
        
        Longnail,
        
        MarkOfPride,
        
        NailmastersGlory,
        
        QuickFocus,
        
        QuickSlash,
        
        ShamanStone,
        
        ShapeOfUnn,
        
        SharpShadow,
        
        SoulCatcher,
        
        SoulEater,
        
        SpellTwister,
        
        SporeShroom,
        
        Sprintmaster,
        
        StalwartShell,
        
        SteadyBody,
        
        ThornsOfAgony,
        
        UnbreakableGreed,
        
        UnbreakableHeart,
        
        UnbreakableStrength,
        
        VoidHeart,
        
        WaywardCompass,
        
        Weaversong,
        
        NotchShrumalOgres,
        
        NotchFogCanyon,
        
        NotchGrimm,
        
        NotchSalubra1,
        
        NotchSalubra2,
        
        NotchSalubra3,
        
        NotchSalubra4,

        
        Lemm2,
        
        AllCharmNotchesLemm2CP,
        
        MetGreyMourner,
        
        GreyMournerSeerAscended,
        
        ElderbugFlower,
        
        givenGodseekerFlower,
        
        givenOroFlower,
        
        givenWhiteLadyFlower,
        
        givenEmilitiaFlower,
        
        BrettaRescued,
        
        BrummFlame,
        
        LittleFool,
        
        SlyRescued,

        
        Flame1,
        
        Flame2,
        
        Flame3,

        
        Ore1,
        
        Ore2,
        
        Ore3,
        
        Ore4,
        
        Ore5,
        
        Ore6,

        
        Grub1,
        
        Grub2,
        
        Grub3,
        
        Grub4,
        
        Grub5,
        
        Grub6,
        
        Grub7,
        
        Grub8,
        
        Grub9,
        
        Grub10,
        
        Grub11,
        
        Grub12,
        
        Grub13,
        
        Grub14,
        
        Grub15,
        
        Grub16,
        
        Grub17,
        
        Grub18,
        
        Grub19,
        
        Grub20,
        
        Grub21,
        
        Grub22,
        
        Grub23,
        
        Grub24,
        
        Grub25,
        
        Grub26,
        
        Grub27,
        
        Grub28,
        
        Grub29,
        
        Grub30,
        
        Grub31,
        
        Grub32,
        
        Grub33,
        
        Grub34,
        
        Grub35,
        
        Grub36,
        
        Grub37,
        
        Grub38,
        
        Grub39,
        
        Grub40,
        
        Grub41,
        
        Grub42,
        
        Grub43,
        
        Grub44,
        
        Grub45,
        
        Grub46,

        
        GrubBasinDive,
        
        GrubBasinWings,
        
        GrubCityBelowLoveTower,
        
        GrubCityBelowSanctum,
        
        GrubCityCollectorAll,
        
        GrubCityCollector,
        
        GrubCityGuardHouse,
        
        GrubCitySanctum,
        
        GrubCitySpire,
        
        GrubCliffsBaldurShell,
        
        GrubCrossroadsAcid,
        
        GrubCrossroadsGuarded,
        
        GrubCrossroadsSpikes,
        
        GrubCrossroadsVengefly,
        
        GrubCrossroadsWall,
        
        GrubCrystalPeaksBottomLever,
        
        GrubCrystalPeaksCrown,
        
        GrubCrystalPeaksCrushers,
        
        GrubCrystalPeaksCrystalHeart,
        
        GrubCrystalPeaksMimics,
        
        GrubCrystalPeaksMound,
        
        GrubCrystalPeaksSpikes,
        
        GrubDeepnestBeastsDen,
        
        GrubDeepnestDark,
        
        GrubDeepnestMimics,
        
        GrubDeepnestNosk,
        
        GrubDeepnestSpikes,
        
        GrubFogCanyonArchives,
        
        GrubFungalBouncy,
        
        GrubFungalSporeShroom,
        
        GrubGreenpathCornifer,
        
        GrubGreenpathHunter,
        
        GrubGreenpathMossKnight,
        
        GrubGreenpathVesselFragment,
        
        GrubHiveExternal,
        
        GrubHiveInternal,
        
        GrubKingdomsEdgeCenter,
        
        GrubKingdomsEdgeOro,
        
        GrubQueensGardensBelowStag,
        
        GrubQueensGardensUpper,
        
        GrubQueensGardensWhiteLady,
        
        GrubRestingGroundsCrypts,
        
        GrubWaterwaysCenter,
        
        GrubWaterwaysHwurmps,
        
        GrubWaterwaysIsma,

        
        Mimic1,
        
        Mimic2,
        
        Mimic3,
        
        Mimic4,
        
        Mimic5,

        
        TreeMound,
        
        TreeCity,
        
        TreePeak,
        
        TreeDeepnest,
        
        TreeCrossroads,
        
        TreeLegEater,
        
        TreeMantisVillage,
        
        TreeGreenpath,
        
        TreeHive,
        
        TreeCliffs,
        
        TreeKingdomsEdge,
        
        TreeQueensGardens,
        
        TreeRestingGrounds,
        
        TreeWaterways,
        
        TreeGlade,

        
        Essence100,
        
        Essence200,
        
        Essence300,
        
        Essence400,
        
        Essence500,
        
        Essence600,
        
        Essence700,
        
        Essence800,
        
        Essence900,
        
        Essence1000,
        
        Essence1100,
        
        Essence1200,
        
        Essence1300,
        
        Essence1400,
        
        Essence1500,
        
        Essence1600,
        
        Essence1700,
        
        Essence1800,
        
        Essence1900,
        
        Essence2000,
        
        Essence2100,
        
        Essence2200,
        
        Essence2300,
        
        Essence2400,

        
        BenchAny,
        /*
        
        BenchDirtmouth,
        
        BenchMato,
        
        BenchCrossroadsHotsprings,
        */
        
        BenchCrossroadsStag,
        /*
        
        BenchSalubra,
        
        BenchAncestralMound,
        
        BenchBlackEgg,
        
        BenchWaterfall,
        
        BenchStoneSanctuary,
        
        BenchGreenpathToll,
        */
        
        BenchGreenpathStag,
        /*
        
        BenchLakeOfUnn,
        
        BenchSheo,
        
        BenchArchives,
        */
        
        BenchQueensStation,
        /*
        
        BenchLegEater,
        
        BenchBretta,
        
        BenchMantisVillage,
        
        BenchQuirrel,
        
        BenchCityToll,
        */
        
        BenchStorerooms,
        
        BenchSpire,
        
        BenchSpireGHS,
        
        BenchKingsStation,
        /*
        
        BenchPleasureHouse,
        
        BenchWaterways,
        
        BenchDeepnestHotsprings,
        
        BenchFailedTramway,
        
        BenchDeepnestSpiderTown,
        
        BenchBasinToll,
        */
        
        BenchHiddenStation,
        /*
        
        BenchOro,
        
        BenchCamp,
        
        BenchColosseum,
        
        BenchHive,
        */
        
        BenchRGStag,
        /*
        
        BenchDarkRoom,
        
        BenchCG1,
        
        BenchFlowerQuest,
        
        BenchQGCornifer,
        
        BenchQGToll,
        */
        
        BenchQGStag,
        /*
        
        BenchTram,
        
        BenchWhitePalaceEntrance,
        
        BenchWhitePalaceAtrium,
        
        BenchWhitePalaceBalcony,
        
        BenchGodhomeAtrium,
        
        BenchHallOfGods,
        */

        
        TollBenchQG,
        
        TollBenchCity,
        
        TollBenchBasin,
        
        WaterwaysManhole,
        
        TramDeepnest,

        
        WhitePalaceOrb1,
        
        WhitePalaceOrb3,
        
        WhitePalaceOrb2,

        
        WhitePalaceLowerEntry,
        
        WhitePalaceLowerOrb,
        
        WhitePalaceLeftEntry,
        
        WhitePalaceLeftWingMid,
        
        WhitePalaceRightEntry,
        
        WhitePalaceRightClimb,
        
        WhitePalaceRightSqueeze,
        
        WhitePalaceRightDone,
        
        WhitePalaceTopEntry,
        
        WhitePalaceTopClimb,
        
        WhitePalaceTopLeverRoom,
        
        WhitePalaceTopLastPlats,
        
        WhitePalaceThroneRoom,
        
        WhitePalaceAtrium,

        
        PathOfPainEntry,
        
        PathOfPainTransition1,
        
        PathOfPainTransition2,
        
        PathOfPainTransition3,

        
        DgateKingdomsEdgeAcid,

        
        GodhomeBench,
        
        GodhomeLoreRoom,
        
        Pantheon1to4Entry,
        
        Pantheon5Entry,

        
        Menu,
        
        MenuClaw,
        
        MenuGorgeousHusk,

        
        CorniferAtHome,
        
        AllSeals,
        
        AllEggs,
        
        SlySimpleKey,
        
        AllBreakables,
        
        AllUnbreakables,
        
        MetEmilitia,

        
        mapDirtmouth,
        
        mapCrossroads,
        
        mapGreenpath,
        
        mapFogCanyon,
        
        mapRoyalGardens,
        
        mapFungalWastes,
        
        mapCity,
        
        mapWaterways,
        
        mapMines,
        
        mapDeepnest,
        
        mapCliffs,
        
        mapOutskirts,
        
        mapRestingGrounds,
        
        mapAbyss,

        
        OnObtainGhostMarissa,
        
        OnObtainGhostCaelifFera,
        
        OnObtainGhostPoggy,
        
        OnObtainGhostGravedigger,
        
        OnObtainGhostJoni,
        
        OnObtainGhostCloth,
        
        OnObtainGhostVespa,
        
        OnObtainGhostRevek,

        
        OnObtainWanderersJournal,
        
        OnObtainHallownestSeal,
        
        OnObtainKingsIdol,
        
        ArcaneEgg8,
        
        OnObtainArcaneEgg,
        
        OnObtainRancidEgg,
        
        OnObtainMaskShard,
        
        OnObtainVesselFragment,
        
        OnObtainSimpleKey,
        
        OnUseSimpleKey,
        
        OnObtainGrub,
        
        OnObtainPaleOre,
        
        OnObtainWhiteFragment,

        
        Bronze1a,
        
        Bronze1b,
        
        Bronze1c,
        
        Bronze2,
        
        Bronze3a,
        
        Bronze3b,
        
        Bronze4,
        
        Bronze5,
        
        Bronze6,
        
        Bronze7,
        
        Bronze8a,
        
        Bronze8b,
        
        Bronze9,
        
        Bronze10,
        
        Bronze11a,
        
        Bronze11b,
        
        BronzeEnd,

        
        Silver1,
        
        Silver2,
        
        Silver3,
        
        Silver4,
        
        Silver5,
        
        Silver6,
        
        Silver7,
        
        Silver8,
        
        Silver9,
        
        Silver10,
        
        Silver11,
        
        Silver12,
        
        Silver13,
        
        Silver14,
        
        Silver15,
        
        Silver16,
        
        SilverEnd,

        
        Gold1,
        
        Gold3,
        
        Gold4,
        
        Gold5,
        
        Gold6,
        
        Gold7,
        
        Gold8a,
        
        Gold8,
        
        Gold9a,
        
        Gold9b,
        
        Gold10,
        
        Gold11,
        
        Gold12a,
        
        Gold12b,
        
        Gold14a,
        
        Gold14b,
        
        Gold15,
        
        Gold16,
        
        Gold17a,
        
        Gold17b,
        
        Gold17c,
        
        GoldEnd,

        
        AnyTransition,
        
        TransitionAfterSaveState,
        
        RandoWake,

        /*
        
        MageDoor,
        
        MageWindow,
        
        MageLordEncountered,
        
        MageDoor2,
        
        MageWindowGlass,
        
        MageLordEncountered2,
        */



        /*
        
        EquippedFragileHealth,
        */

    }
    public enum Offset : int {
        health,
        maxHealthBase,
        MPCharge,
        MPReserveMax,
        mapZone,
        nailDamage,
        fireballLevel,
        quakeLevel,
        screamLevel,
        hasCyclone,
        hasDashSlash,
        hasUpwardSlash,
        hasDreamNail,
        dreamNailUpgraded,
        hasDash,
        hasWallJump,
        hasSuperDash,
        hasShadowDash,
        hasAcidArmour,
        hasDoubleJump,
        hasLantern,
        hasTramPass,
        hasLoveKey,
        hasKingsBrand,
        ore,
        simpleKeys,
        notchShroomOgres,
        notchFogCanyon,
        lurienDefeated,
        hegemolDefeated,
        monomonDefeated,
        visitedDeepnestSpa,
        zoteRescuedBuzzer,
        zoteRescuedDeepnest,
        killedZote,
        mothDeparted,
        salubraNotch1,
        salubraNotch2,
        salubraNotch3,
        salubraNotch4,
        nailSmithUpgrades,
        colosseumBronzeOpened,
        colosseumSilverOpened,
        colosseumGoldOpened,
        colosseumBronzeCompleted,
        colosseumSilverCompleted,
        colosseumGoldCompleted,
        openedCrossroads,
        openedGreenpath,
        openedRuins1,
        openedRuins2,
        openedFungalWastes,
        openedRoyalGardens,
        openedRestingGrounds,
        openedDeepnest,
        openedStagNest,
        openedHiddenStation,
        gotCharm_1,
        gotCharm_2,
        gotCharm_3,
        gotCharm_4,
        gotCharm_5,
        gotCharm_6,
        gotCharm_7,
        gotCharm_8,
        gotCharm_9,
        gotCharm_10,
        gotCharm_11,
        gotCharm_12,
        gotCharm_13,
        gotCharm_14,
        gotCharm_15,
        gotCharm_16,
        gotCharm_17,
        gotCharm_18,
        gotCharm_19,
        gotCharm_20,
        gotCharm_21,
        gotCharm_22,
        gotCharm_23,
        gotCharm_24,
        gotCharm_25,
        gotCharm_26,
        gotCharm_27,
        gotCharm_28,
        gotCharm_29,
        gotCharm_30,
        gotCharm_31,
        gotCharm_32,
        gotCharm_33,
        gotCharm_34,
        gotCharm_35,
        gotCharm_36,
        gotCharm_37,
        gotCharm_38,
        gotCharm_39,
        gotCharm_40,
        charmCost_36,
        fragileHealth_unbreakable,
        fragileGreed_unbreakable,
        fragileStrength_unbreakable,
        killsSpitter,
        killedBigFly,
        killedMawlek,
        killedMenderBug,
        killedMossKnight,
        killedInfectedKnight,
        killedMegaJellyfish,
        killsMushroomBrawler,
        killedBlackKnight,
        killedMageLord,
        killedFlukeMother,
        killedDungDefender,
        killsMegaBeamMiner,
        killedMimicSpider,
        killedHornet,
        killedTraitorLord,
        killedGhostAladar,
        killedGhostXero,
        killedGhostHu,
        killedGhostMarmu,
        killedGhostNoEyes,
        killedGhostMarkoth,
        killedGhostGalien,
        killedHollowKnight,
        killedFinalBoss,
        killedFalseKnight,
        falseKnightDreamDefeated,
        killedGrimm,
        killedNightmareGrimm,
        killedBindingSeal,
        grimmChildLevel,
        nightmareLanternLit,
        destroyedNightmareLantern,
        flamesCollected,
        heartPieces,
        vesselFragments,
        mawlekDefeated,
        collectorDefeated,
        hornetOutskirtsDefeated,
        mageLordDreamDefeated,
        infectedKnightDreamDefeated,
        isInvincible,
        visitedDirtmouth,
        visitedCrossroads,
        visitedGreenpath,
        visitedFungus,
        visitedHive,
        visitedRuins,
        visitedMines,
        visitedRoyalGardens,
        visitedFogCanyon,
        visitedDeepnest,
        visitedRestingGrounds,
        visitedWaterways,
        visitedWhitePalace,
        crossroadsInfected,
        megaMossChargerDefeated,
        defeatedMantisLords,
        defeatedMegaBeamMiner,
        defeatedMegaBeamMiner2,
        gotShadeCharm,
        disablePause,
        mrMushroomState,
        killedWhiteDefender,
        killedGreyPrince,
        hasDreamGate,
        metRelicDealer,
        metRelicDealerShop,
        elderbugGaveFlower,
        brettaRescued,
        killedLobsterLancer,
        gotBrummsFlame,
        killedNailsage,
        killedPaintmaster,
        killedNailBros,
        killedHollowKnightPrime,
        visitedGodhome,
        visitedAbyss,
        gotLurkerKey,
        hasGodfinder,
        hasSlykey,
        bossDoorStateTier1,
        bossDoorStateTier2,
        bossDoorStateTier3,
        bossDoorStateTier4,
        bossDoorStateTier5,
        newDataBindingSeal,
        killedZombieMiner,
        killsZombieMiner,
        royalCharmState,
        visitedOutskirts,
        seenColosseumTitle,
        littleFoolMet,
        killedHunterMark,
        salubraBlessing,
        watcherChandelier,
        spiderCapture,
        unchainedHollowKnight,
        killedGiantHopper,
        killedGorgeousHusk,
        gladeDoorOpened,
        killedLazyFlyer,
        killsLazyFlyer,
        hasWhiteKey,
        killedHiveKnight,
        grubsCollected,
        guardiansDefeated,
        scenesEncounteredDreamPlantC,

        openedCityGate,

        abyssGateOpened,
        abyssLighthouse,
        blueVineDoor,

        whitePalaceOrb_1,
        whitePalaceOrb_2,
        whitePalaceOrb_3,
        whitePalaceSecretRoomVisited,

        dreamOrbs,

        slyRescued,
        hornetGreenpath,
        nailsmithKilled,
        nailsmithSpared,

        metQueen,
        gotQueenFragment,
        gotKingFragment,

        travelling,
        stagPosition,

        openedTown,
        openedTownBuilding, // Town stag station lever?

        openedMageDoor,
        openedMageDoor_v2,
        brokenMageWindow,
        brokenMageWindowGlass,
        mageLordEncountered,
        mageLordEncountered_2,

        mineLiftOpened,
        gotGrimmNotch,

        xunFlowerGiven,

        kingsStationNonDisplay,

        openedTramLower,
        openedTramRestingGrounds,
        tramLowerPosition,
        tramRestingGroundsPosition,

        waterwaysGate,
        openedWaterwaysManhole,
        waterwaysAcidDrained,
        dungDefenderWallBroken,
        openedLoveDoor,

        hasCityKey,

        tollBenchQueensGardens,
        tollBenchCity,
        tollBenchAbyss,
        maskBrokenLurien,
        maskBrokenHegemol,
        maskBrokenMonomon,

        equippedCharm_5,
        equippedCharm_7,
        equippedCharm_17,
        equippedCharm_28,

        equippedCharm_23,
        brokenCharm_23,
        brokenCharm_24,
        brokenCharm_25,
        canOvercharm,
        overcharmed,
        slyShellFrag4,
        slyVesselFrag4,
        metXun,
        hasXunFlower,
        xunRewardGiven,

        currentArea,
        killedMageKnight,
        killedMage,

        dreamGateScene,
        dreamGateX,
        dreamGateY,

        falseKnightOrbsCollected,
        mageLordOrbsCollected,
        infectedKnightOrbsCollected,
        whiteDefenderOrbsCollected,
        greyPrinceOrbsCollected,

        aladarSlugDefeated,
        xeroDefeated,
        elderHuDefeated,
        mumCaterpillarDefeated,
        noEyesDefeated,
        markothDefeated,
        galienDefeated,

        trinket1,   // Journal : int
        foundTrinket1,
        trinket2,   // Seal : int
        foundTrinket2,
        trinket3,   // Idol : int
        foundTrinket3,
        trinket4,   // Arcane Egg : int
        foundTrinket4,
        noTrinket1,
        noTrinket2,
        noTrinket3,
        noTrinket4,
        soldTrinket1,
        soldTrinket2,
        soldTrinket3,
        soldTrinket4,
        geo,

        nailsmithConvoArt,
        slySimpleKey,
        pooedFragileHeart,
        pooedFragileGreed,
        pooedFragileStrength,
        rancidEggs,
        jinnEggsSold,
        mapDirtmouth,
        mapCrossroads,
        mapGreenpath,
        mapFogCanyon,
        mapRoyalGardens,
        mapFungalWastes,
        mapCity,
        mapWaterways,
        mapMines,
        mapDeepnest,
        mapCliffs,
        mapOutskirts,
        mapRestingGrounds,
        mapAbyss,
        givenGodseekerFlower,
        givenOroFlower,
        givenWhiteLadyFlower,
        givenEmilitiaFlower,
        zoteDead,
        corniferAtHome,
        metEmilitia,
        scenesGrubRescued,
        zoteStatueWallBroken,
        ordealAchieved,
        whiteDefenderDefeats,
        greyPrinceDefeats,
        savedCloth,
        atBench,
        soulLimited,
        encounteredMegaJelly,

        killsAngryBuzzer,
        killsBigBuzzer,
        killsBigFly,
        killsBlobble,
        killsBurstingBouncer,
        killsBuzzer,
        killsCeilingDropper,
        killsColFlyingSentry,
        killsColHopper,
        killsColMiner,
        killsColMosquito,
        killsColRoller,
        killsColShield,
        killsColWorm,
        killsElectricMage,
        killsGiantHopper,
        killsGrubMimic,
        killsHeavyMantis,
        killsHopper,
        killsOblobble,
        killsLesserMawlek,
        killsLobsterLancer,
        killsMage,
        killsMageBlob,
        killsMageKnight,
        killsMantisHeavyFlyer,
        killsMawlek,
        killsSuperSpitter,
        killsGreatShieldZombie
    }
}

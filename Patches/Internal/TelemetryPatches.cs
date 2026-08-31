using Backtrace.Unity.Model;
using GorillaNetworking;
using HarmonyLib;
using Liv.Lck.Telemetry;

namespace StupidTemplate.Patches.Internal;

public static class TelemetryPatches
{
    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueTelemetryEvent))]
    private static class GorillaTelemetryEnqueuePatch
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.EnqueueZoneEvent))]
    private static class GorillaTelemetryZonePatch
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaTelemetry), nameof(GorillaTelemetry.FlushMothershipTelemetry))]
    private static class GorillaTelemetryFlushPatch
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(Gorillanalytics), nameof(Gorillanalytics.UploadGorillanalytics))]
    private static class GorillanalyticsUploadPatch
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaServer), nameof(GorillaServer.UploadGorillanalytics))]
    private static class GorillaServerGorillanalyticsPatch
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(CustomMapTelemetry), nameof(CustomMapTelemetry.StartMapTracking))]
    private static class CustomMapTelemetryStartPatch
    {
        private static bool Prefix() =>
                false;
    }

    //This disables unity's crash analytics telemetry
    [HarmonyPatch(typeof(BacktraceManager), "<Awake>b__1_0")]
    private static class BacktraceBeforeSendPatch
    {
        private static bool Prefix(ref BacktraceData __result)
        {
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(LckTelemetryClient), nameof(LckTelemetryClient.SendTelemetry))]
    private static class LckTelemetryPatch
    {
        private static bool Prefix() =>
                false;
    }
}
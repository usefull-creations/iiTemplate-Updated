using StupidTemplate.Classes;
using UnityEngine;

namespace StupidTemplate.Mods.Settings;

public class Movement
{
    private static readonly string[] flySpeedNames =
    [
            "Very Slow",
            "Slow",
            "Normal",
            "Fast",
            "Very Fast",
            "Extreme",
    ];

    private static readonly float[] flySpeedValues =
    [
            5f,
            10f,
            15f,
            20f,
            30f,
            50f,
    ];

    [SavedSetting(OnLoaded = nameof(ApplySavedFlySpeed))]
    public static int flySpeedIndex = 2;

    public static float flySpeed = 15f;

    public static string FlySpeedName =>
            flySpeedNames[flySpeedIndex];

    public static void ChangeFlySpeed(
            bool increment)
    {
        flySpeedIndex +=
                increment
                        ? 1
                        : -1;

        if (flySpeedIndex >= flySpeedNames.Length)
            flySpeedIndex = 0;
        else if (flySpeedIndex < 0)
            flySpeedIndex = flySpeedNames.Length - 1;

        ApplySavedFlySpeed();
    }

    public static void SetFlySpeedIndex(
            int index)
    {
        flySpeedIndex =
                Mathf.Clamp(
                        index,
                        0,
                        flySpeedValues.Length - 1);

        ApplySavedFlySpeed();
    }

    private static void ApplySavedFlySpeed()
    {
        flySpeedIndex =
                Mathf.Clamp(
                        flySpeedIndex,
                        0,
                        flySpeedValues.Length - 1);

        flySpeed =
                flySpeedValues[
                        flySpeedIndex];
    }
}
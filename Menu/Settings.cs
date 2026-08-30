using System;
using StupidTemplate.Classes;
using UnityEngine;

namespace StupidTemplate.Menu;

public enum GradientAnimationMode
{
    Scroll,
    Pulse,
}

public class Settings
{
    /*
     * These are the settings for the menu.
     *
     * To change the colors, you need to modify the ExtGradient variables.
     * Here are some examples on how to use ExtGradient:
     *
     * Solid Color:
     *  new ExtGradient { colors = ExtGradient.GetSolidGradient(Color.black) }
     *
     * Simple Gradient:
     *  new ExtGradient { colors = ExtGradient.GetSimpleGradient(Color.black, Color.white) }
     *
     * Rainbow Color:
     *   new ExtGradient { rainbow = true }
     *
     * Epileptic Color (random color every frame):
     *   new ExtGradient { epileptic = true }
     *
     * Self Color:
     *   new ExtGradient { copyRigColor = true }
     *
     * To change the font, you may use the following code:
     *   Font.CreateDynamicFontFromOSFont("Comic Sans MS", 24)
     */

    [SavedSetting] public static bool dropMenu    = true;
    [SavedSetting] public static bool animateMenu = true;

    [SavedSetting] public static float menuAnimationSpeed = 14f;

    [SavedSetting] public static GradientAnimationMode gradientAnimationMode =
            GradientAnimationMode.Scroll;

    public static ExtGradient backgroundColor;

    public static readonly ExtGradient[] buttonColors =
    {
            new(),
            new(),
    };

    public static ExtGradient outlineColor;

    public static readonly Color[] textColors =
    {
            Color.white,
            Color.white,
    };

    public static readonly MenuTheme[] themes =
    {
            new()
            {
                    name = "Purple",

                    backgroundPrimary   = new Color32(23, 16, 46, 255),
                    backgroundSecondary = new Color32(38, 25, 73, 255),

                    disabledButtonPrimary   = new Color32(17, 13, 32, 255),
                    disabledButtonSecondary = new Color32(31, 23, 54, 255),

                    enabledButtonPrimary   = new Color32(95,  75,  216, 255),
                    enabledButtonSecondary = new Color32(139, 109, 255, 255),

                    outline = new Color32(9, 7, 18, 255),
            },

            new()
            {
                    name = "Blue",

                    backgroundPrimary   = new Color32(10, 24, 44, 255),
                    backgroundSecondary = new Color32(17, 47, 78, 255),

                    disabledButtonPrimary   = new Color32(10, 19, 31, 255),
                    disabledButtonSecondary = new Color32(16, 34, 55, 255),

                    enabledButtonPrimary   = new Color32(32, 105, 196, 255),
                    enabledButtonSecondary = new Color32(74, 158, 255, 255),

                    outline = new Color32(5, 12, 22, 255),
            },

            new()
            {
                    name = "Red",

                    backgroundPrimary   = new Color32(42, 12, 17, 255),
                    backgroundSecondary = new Color32(73, 19, 28, 255),

                    disabledButtonPrimary   = new Color32(28, 12, 15, 255),
                    disabledButtonSecondary = new Color32(49, 19, 24, 255),

                    enabledButtonPrimary   = new Color32(184, 46, 65,  255),
                    enabledButtonSecondary = new Color32(245, 85, 105, 255),

                    outline = new Color32(20, 5, 8, 255),
            },

            new()
            {
                    name = "Mint",

                    backgroundPrimary   = new Color32(9,  36, 33, 255),
                    backgroundSecondary = new Color32(14, 61, 54, 255),

                    disabledButtonPrimary   = new Color32(8,  27, 25, 255),
                    disabledButtonSecondary = new Color32(13, 45, 41, 255),

                    enabledButtonPrimary   = new Color32(31, 171, 140, 255),
                    enabledButtonSecondary = new Color32(83, 226, 192, 255),

                    outline = new Color32(4, 18, 16, 255),
            },

            new()
            {
                    name = "Monochrome",

                    backgroundPrimary   = new Color32(18, 18, 18, 255),
                    backgroundSecondary = new Color32(35, 35, 35, 255),

                    disabledButtonPrimary   = new Color32(24, 24, 24, 255),
                    disabledButtonSecondary = new Color32(45, 45, 45, 255),

                    enabledButtonPrimary   = new Color32(100, 100, 100, 255),
                    enabledButtonSecondary = new Color32(160, 160, 160, 255),

                    outline = new Color32(5, 5, 5, 255),
            },
    };

    [SavedSetting(OnLoaded = nameof(ApplyTheme))]
    public static int themeIndex;

    [SavedSetting(OnLoaded = nameof(ApplyTheme))]
    public static bool rainbowColors = true;
    [SavedSetting] public static bool outlines;

    [SavedSetting] public static float menuCornerRadius = 0.06f;
    [SavedSetting] public static float outlineThickness = 0.018f;

    public static Font currentFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

    [SavedSetting] public static bool fpsCounter       = true;
    [SavedSetting] public static bool disconnectButton = true;
    [SavedSetting] public static bool rightHanded;
    [SavedSetting] public static bool disableNotifications;

    [SavedSetting] public static KeyCode keyboardButton = KeyCode.Q;

    [SavedSetting] public static Vector3 menuSize       = new(0.1f, 1f, 1f); // Depth, width, height
    [SavedSetting] public static int     buttonsPerPage = 8;

    [SavedSetting] public static bool incrementalButtons      = true;
    [SavedSetting] public static bool buttonGradients         = true;
    [SavedSetting] public static bool roundedButtons          = true;
    [SavedSetting] public static bool verticalButtonGradients = false;

    [SavedSetting] public static bool pageButtonsAtTop;

    [SavedSetting] public static float searchKeyboardFollowSpeed = 5f;

    [SavedSetting] public static Vector3 buttonSize = new(0.09f, 0.9f, 0.08f);

    [SavedSetting] public static float buttonSpacing      = 0.1f;
    [SavedSetting] public static float buttonCornerRadius = 0.018f;

    [SavedSetting] public static int buttonCornerSegments = 5;

    [SavedSetting] public static float gradientSpeed = 0.5f; // Speed of colors

    [SavedSetting] public static GunSettings gunSettings = new();

    static Settings() =>
            ApplyTheme();

    public static string GradientAnimationName =>
            gradientAnimationMode.ToString();

    public static string ThemeName => themes[themeIndex].name;

    public static void ChangeGradientAnimation(bool increment)
    {
        int count = Enum.GetValues(typeof(GradientAnimationMode)).Length;

        int index =
                (int)gradientAnimationMode +
                (increment ? 1 : -1);

        if (index >= count)
            index = 0;
        else if (index < 0)
            index = count - 1;

        gradientAnimationMode =
                (GradientAnimationMode)index;

        ColorChanger.ClearGradientCache();
    }

    public static void ChangeTheme(bool increment)
    {
        themeIndex += increment ? 1 : -1;

        if (themeIndex >= themes.Length)
            themeIndex = 0;
        else if (themeIndex < 0)
            themeIndex = themes.Length - 1;

        ApplyTheme();
    }

    public static void SetRainbowColors(bool enabled)
    {
        rainbowColors = enabled;

        ApplyTheme();
    }

    public static void ApplyTheme()
    {
        if (themes.Length == 0)
            return;

        themeIndex = Mathf.Clamp(
                themeIndex,
                0,
                themes.Length - 1);

        MenuTheme theme = themes[themeIndex];

        backgroundColor = rainbowColors
                                  ? new ExtGradient
                                  {
                                          rainbow = true,
                                  }
                                  : CreateGradient(
                                          theme.backgroundPrimary,
                                          theme.backgroundSecondary);

        buttonColors[0] = CreateGradient(
                theme.disabledButtonPrimary,
                theme.disabledButtonSecondary);

        buttonColors[1] = rainbowColors
                                  ? new ExtGradient
                                  {
                                          rainbow = true,
                                  }
                                  : CreateGradient(
                                          theme.enabledButtonPrimary,
                                          theme.enabledButtonSecondary);

        outlineColor = new ExtGradient
        {
                colors = ExtGradient.GetSolidGradient(theme.outline),
        };

        textColors[0] = theme.disabledText;
        textColors[1] = theme.enabledText;

        ColorChanger.ClearGradientCache();
    }

    private static ExtGradient CreateGradient(Color first, Color second) =>
            new()
            {
                    colors = ExtGradient.GetSimpleGradient(first, second),
            };
}
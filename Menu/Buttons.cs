using System;
using System.Collections.Generic;
using System.Linq;
using StupidTemplate.Classes;
using StupidTemplate.Mods;
using static StupidTemplate.Menu.Main;
using static StupidTemplate.Menu.Settings;
using Movement = StupidTemplate.Mods.Settings.Movement;

namespace StupidTemplate.Menu;

public abstract class Buttons
{
    /*
     * Here is where all of your buttons are located.
     *
     * Move to Category:
     *   new ButtonInfo
     *   {
     *       buttonText = "Settings",
     *       method     = () => SetCategory("Settings"),
     *       mode       = ButtonMode.Action,
     *       toolTip    = "Opens the main settings page for the menu.",
     *   },
     *
     * Togglable Mod:
     *   new ButtonInfo
     *   {
     *       buttonText = "Platforms",
     *       method     = () => Mods.Movement.Platforms(),
     *       toolTip    = "Spawns platforms on your hands when pressing grip.",
     *   },
     *
     * Action:
     *   new ButtonInfo
     *   {
     *       buttonText = "Disconnect",
     *       method     = () => NetworkSystem.Instance.ReturnToSinglePlayer(),
     *       mode       = ButtonMode.Action,
     *       toolTip    = "Disconnects you from the room.",
     *   },
     *
     * Incremental:
     *   new ButtonInfo
     *   {
     *       buttonText      = "Change Fly Speed",
     *       mode            = ButtonMode.Incremental,
     *       incrementMethod = Movement.ChangeFlySpeed,
     *       displayText     = () => $"Change Fly Speed [{Movement.FlySpeedName}]",
     *       toolTip         = "Changes the speed of the fly mod.",
     *   },
     */

    private static readonly ButtonInfo[] MainButtons =
    {
            new()
            {
                    buttonText = "Settings",
                    method     = () => SetCategory("Settings"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the main settings page for the menu.",
            },

            new()
            {
                    buttonText = "Room Mods",
                    method     = () => SetCategory("Room Mods"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the room mods tab.",
            },

            new()
            {
                    buttonText = "Movement Mods",
                    method     = () => SetCategory("Movement Mods"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the movement mods tab.",
            },

            new()
            {
                    buttonText = "Safety Mods",
                    method     = () => SetCategory("Safety Mods"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the safety mods tab.",
            },
    };

    private static readonly ButtonInfo[] SettingsButtons =
    {
            new()
            {
                    buttonText = "Return to Main",
                    method     = () => SetCategory("Main"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main page of the menu.",
            },

            new()
            {
                    buttonText = "Menu",
                    method     = () => SetCategory("Menu Settings"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the settings for the menu.",
            },

            new()
            {
                    buttonText = "Movement",
                    method     = () => SetCategory("Movement Settings"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Opens the movement settings for the menu.",
            },
    };

    private static readonly ButtonInfo[] MenuSettingsButtons =
    {
            new()
            {
                    buttonText = "Return to Settings",
                    method     = () => SetCategory("Settings"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main settings page for the menu.",
            },

            new()
            {
                    buttonText    = "Right Hand",
                    enableMethod  = () => rightHanded = true,
                    disableMethod = () => rightHanded = false,
                    enabled       = rightHanded,
                    toolTip       = "Puts the menu on your right hand.",
            },

            new()
            {
                    buttonText    = "Notifications",
                    enableMethod  = () => disableNotifications = false,
                    disableMethod = () => disableNotifications = true,
                    enabled       = !disableNotifications,
                    toolTip       = "Toggles the notifications.",
            },

            new()
            {
                    buttonText    = "FPS Counter",
                    enableMethod  = () => fpsCounter = true,
                    disableMethod = () => fpsCounter = false,
                    enabled       = fpsCounter,
                    toolTip       = "Toggles the FPS counter.",
            },

            new()
            {
                    buttonText    = "Disconnect Button",
                    enableMethod  = () => disconnectButton = true,
                    disableMethod = () => disconnectButton = false,
                    enabled       = disconnectButton,
                    toolTip       = "Toggles the disconnect button.",
            },

            new()
            {
                    buttonText    = "Incremental Buttons",
                    enableMethod  = () => incrementalButtons = true,
                    disableMethod = () => incrementalButtons = false,
                    enabled       = incrementalButtons,
                    toolTip       = "Shows separate minus and plus controls for incremental settings.",
            },

            new()
            {
                    buttonText    = "Rounded Menu",
                    enableMethod  = () => roundedButtons = true,
                    disableMethod = () => roundedButtons = false,
                    enabled       = roundedButtons,
                    toolTip       = "Rounds the menu, buttons, search keyboard, and other menu objects.",
            },

            new()
            {
                    buttonText    = "Drop Menu",
                    enableMethod  = () => dropMenu = true,
                    disableMethod = () => dropMenu = false,
                    enabled       = dropMenu,
                    toolTip       = "Makes the menu open while held and drop when released.",
            },

            new()
            {
                    buttonText    = "Animated Menu",
                    enableMethod  = () => animateMenu = true,
                    disableMethod = () => animateMenu = false,
                    enabled       = animateMenu,
                    toolTip       = "Adds a simple grow and shrink animation to the menu.",
            },

            new()
            {
                    buttonText      = "Gradient Animation",
                    mode            = ButtonMode.Incremental,
                    incrementMethod = ChangeGradientAnimation,
                    displayText     = () => $"Gradient Animation [{GradientAnimationName}]",
                    toolTip         = "Changes how animated gradients move.",
            },

            new()
            {
                    buttonText    = "Rainbow Colours",
                    enableMethod  = () => SetRainbowColors(true),
                    disableMethod = () => SetRainbowColors(false),
                    enabled       = rainbowColors,
                    toolTip       = "Uses animated rainbow colours instead of the selected colour scheme.",
            },

            new()
            {
                    buttonText      = "Colour Scheme",
                    mode            = ButtonMode.Incremental,
                    incrementMethod = ChangeTheme,
                    displayText     = () => $"Colour Scheme [{ThemeName}]",
                    toolTip         = "Changes the colour scheme used by the menu.",
            },

            new()
            {
                    buttonText    = "Outlines",
                    enableMethod  = () => outlines = true,
                    disableMethod = () => outlines = false,
                    enabled       = outlines,
                    toolTip       = "Adds outlines around the menu and its buttons.",
            },

            new()
            {
                    buttonText    = "Button Gradients",
                    enableMethod  = () => buttonGradients = true,
                    disableMethod = () => buttonGradients = false,
                    enabled       = buttonGradients,
                    toolTip       = "Toggles spatial gradients on buttons.",
            },

            new()
            {
                    buttonText    = "Vertical Gradients",
                    enableMethod  = () => verticalButtonGradients = true,
                    disableMethod = () => verticalButtonGradients = false,
                    enabled       = verticalButtonGradients,
                    toolTip       = "Changes button gradients between horizontal and vertical.",
            },

            new()
            {
                    buttonText    = "Top Page Buttons",
                    enableMethod  = () => pageButtonsAtTop = true,
                    disableMethod = () => pageButtonsAtTop = false,
                    enabled       = pageButtonsAtTop,
                    toolTip       = "Moves the page buttons into the first two normal button slots.",
            },
    };

    private static readonly ButtonInfo[] MovementSettingsButtons =
    {
            new()
            {
                    buttonText = "Return to Settings",
                    method     = () => SetCategory("Settings"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main settings page for the menu.",
            },

            new()
            {
                    buttonText      = "Change Fly Speed",
                    mode            = ButtonMode.Incremental,
                    incrementMethod = Movement.ChangeFlySpeed,
                    displayText     = () => $"Change Fly Speed [{Movement.FlySpeedName}]",
                    toolTip         = "Changes the speed of the fly mod.",
            },
    };

    private static readonly ButtonInfo[] RoomModsButtons =
    {
            new()
            {
                    buttonText = "Return to Main",
                    method     = () => SetCategory("Main"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main page of the menu.",
            },

            new()
            {
                    buttonText = "Disconnect",
                    method     = () => NetworkSystem.Instance.ReturnToSinglePlayer(),
                    mode       = ButtonMode.Action,
                    toolTip    = "Disconnects you from the room.",
            },
    };

    private static readonly ButtonInfo[] MovementModsButtons =
    {
            new()
            {
                    buttonText = "Return to Main",
                    method     = () => SetCategory("Main"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main page of the menu.",
            },

            new()
            {
                    buttonText = "Platforms",
                    method     = Mods.Movement.Platforms,
                    toolTip    = "Spawns platforms on your hands when pressing grip.",
            },

            new()
            {
                    buttonText = "Fly",
                    method     = Mods.Movement.Fly,
                    toolTip    = "Sends you forward when holding A.",
            },

            new()
            {
                    buttonText = "Teleport Gun",
                    method     = Mods.Movement.TeleportGun,
                    toolTip    = "Teleports you to wherever your pointer is when pressing trigger.",
            },
    };

    private static readonly ButtonInfo[] SafetyModsButtons =
    {
            new()
            {
                    buttonText = "Return to Main",
                    method     = () => SetCategory("Main"),
                    mode       = ButtonMode.Action,
                    toolTip    = "Returns to the main page of the menu.",
            },

            new()
            {
                    buttonText = "Anti Report",
                    method     = Safety.AntiReportDisconnect,
                    toolTip    = "Disconnects you when someone tries to report you.",
            },
    };

    // The order here does not matter, setting categories is done by name rather than position in the array
    private static readonly ButtonCategory[] Categories =
    {
            new()
            {
                    name    = "Main",
                    buttons = MainButtons,
            },

            new()
            {
                    name    = "Settings",
                    buttons = SettingsButtons,
            },

            new()
            {
                    name    = "Menu Settings",
                    buttons = MenuSettingsButtons,
            },

            new()
            {
                    name    = "Movement Settings",
                    buttons = MovementSettingsButtons,
            },

            new()
            {
                    name    = "Room Mods",
                    buttons = RoomModsButtons,
            },

            new()
            {
                    name    = "Movement Mods",
                    buttons = MovementModsButtons,
            },

            new()
            {
                    name    = "Safety Mods",
                    buttons = SafetyModsButtons,
            },
    };

    private static readonly Dictionary<string, ButtonCategory> CategoryLookup = BuildCategoryLookup();
    private static readonly Dictionary<string, ButtonInfo>     ButtonLookup   = BuildButtonLookup();
    
    private static readonly Dictionary<string, ButtonInfo> SavedToggleButtonLookup =
                    BuildSavedToggleButtonLookup();

    public static IReadOnlyDictionary<string, ButtonInfo> SavedToggleButtons =>
                    SavedToggleButtonLookup;

    public static ButtonInfo[] AllButtons { get; } = BuildButtonArray();

    public static ButtonCategory GetCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        CategoryLookup.TryGetValue(categoryName, out ButtonCategory category);

        return category;
    }

    public static ButtonInfo GetIndex(string buttonText)
    {
        if (string.IsNullOrWhiteSpace(buttonText))
            return null;

        ButtonLookup.TryGetValue(buttonText, out ButtonInfo button);

        return button;
    }

    private static Dictionary<string, ButtonCategory> BuildCategoryLookup()
    {
        Dictionary<string, ButtonCategory> lookup = new(StringComparer.OrdinalIgnoreCase);

        foreach (ButtonCategory category in Categories)
        {
            if (string.IsNullOrWhiteSpace(category.name))
                throw new InvalidOperationException("A button category does not have a name.");

            if (!lookup.TryAdd(category.name, category))
                throw new InvalidOperationException($"Duplicate button category named {category.name}.");

        }

        return lookup;
    }

    private static Dictionary<string, ButtonInfo> BuildButtonLookup()
    {
        Dictionary<string, ButtonInfo> lookup = new(StringComparer.OrdinalIgnoreCase);

        foreach (ButtonCategory category in Categories)
        {
            foreach (ButtonInfo button in category.buttons)
            {
                if (string.IsNullOrWhiteSpace(button.buttonText))
                    continue;

                // Duplicate names such as "Return to Main" are allowed.
                // Actual menu presses reference their ButtonInfo directly,
                // so this lookup is only used when something explicitly requests a button by name.
                lookup.TryAdd(button.buttonText, button);
            }
        }

        return lookup;
    }

    private static ButtonInfo[] BuildButtonArray()
    {
        int buttonCount = Categories.Sum(category => category.buttons.Length);

        ButtonInfo[] result = new ButtonInfo[buttonCount];

        int index = 0;

        foreach (ButtonCategory category in Categories)
        {
            foreach (ButtonInfo button in category.buttons)
            {
                result[index] = button;
                index++;
            }
        }

        return result;
    }
    
    private static Dictionary<string, ButtonInfo> BuildSavedToggleButtonLookup()
    {
            Dictionary<string, ButtonInfo> result =
                            new(
                                            StringComparer.Ordinal);

            foreach (ButtonCategory category in Categories)
            {
                    foreach (ButtonInfo button in category.buttons)
                    {
                            if (button.mode != ButtonMode.Toggle)
                                    continue;

                            string key =
                                            category.name +
                                            "/"           +
                                            button.buttonText;

                            if (!result.TryAdd(key, button))
                            {
                                    throw new InvalidOperationException(
                                                    $"Duplicate saved toggle button key {key}.");
                            }

                    }
            }

            return result;
    }
}
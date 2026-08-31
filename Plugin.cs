using System.ComponentModel;
using BepInEx;
using StupidTemplate.Classes;
using StupidTemplate.Classes.Console;
using StupidTemplate.Patches;

namespace StupidTemplate;

[Description(Constants.Description)]
[BepInPlugin(
        Constants.Guid,
        Constants.Name,
        Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Preferences.Load();
        
        gameObject.AddComponent<CoroutineManager>();
        gameObject.AddComponent<HamburburData>();

        GorillaTagger.OnPlayerSpawned(
                OnPlayerSpawned);
    }

    private void OnApplicationQuit() => Preferences.Save();

    private void OnPlayerSpawned()
    {
        PatchHandler.PatchAll();

        Preferences.ApplyButtonStates();
    }
}
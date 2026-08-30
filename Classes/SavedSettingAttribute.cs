using System;

namespace StupidTemplate.Classes;

 // Add [SavedSetting] above any static field you want saved between game restarts.
 // If the setting needs extra code to apply after loading, use:
 // [SavedSetting(OnLoaded = nameof(NameOfApplyMethod))]

    [AttributeUsage(
            AttributeTargets.Field,
            AllowMultiple = false,
            Inherited = false)]
    public sealed class SavedSettingAttribute : Attribute
    {
        public string Key { get; set; }

        public string OnLoaded { get; set; }
    }
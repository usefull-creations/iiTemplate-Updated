using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Serialization;
using Valve.Newtonsoft.Json.Linq;
using static StupidTemplate.Menu.Buttons;

namespace StupidTemplate.Classes;

public static class Preferences
{
    private static readonly JsonSerializerSettings JsonSettings =
            new()
            {
                    ContractResolver =
                            new UnitySavedSettingContractResolver(),

                    ReferenceLoopHandling =
                            ReferenceLoopHandling.Ignore,
            };
    
    private static readonly string SavePath =
            Path.Combine(
                    Paths.ConfigPath,
                    Constants.Guid + ".json");

    private static readonly List<SavedField> SavedFields =
            FindSavedFields();

    private static SaveData loadedData;
    private static JObject  legacyData;

    private static bool loaded;
    private static bool buttonStatesApplied;

    public static void Load()
    {
        if (loaded)
            return;

        loaded = true;

        loadedData = LoadFile();

        LoadSavedSettings();
    }

    public static void ApplyButtonStates()
    {
        if (!loaded)
            Load();

        if (buttonStatesApplied)
            return;

        buttonStatesApplied = true;

        IReadOnlyDictionary<string, ButtonInfo> buttons =
                SavedToggleButtons;

        foreach (KeyValuePair<string, ButtonInfo> pair in buttons)
        {
            if (!loadedData.buttonStates.TryGetValue(
                        pair.Key,
                        out bool enabled))
            {
                continue;
            }

            ButtonInfo button = pair.Value;

            button.enabled = enabled;

            try
            {
                if (enabled)
                    button.enableMethod?.Invoke();
                else
                    button.disableMethod?.Invoke();
            }
            catch (Exception exc)
            {
                Debug.LogError(
                        $"{Constants.Name} // Failed to apply saved state for {button.buttonText}: {exc}");
            }
        }

        Save();
    }

    public static void Save()
    {
        if (!loaded)
            return;

        try
        {
            SaveData data = new();

            SaveSettings(data);
            SaveButtons(data);

            string json =
                    JsonConvert.SerializeObject(
                            data,
                            Formatting.Indented);

            string directory =
                    Path.GetDirectoryName(
                            SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(
                    SavePath,
                    json);

            loadedData = data;
            legacyData = null;
        }
        catch (Exception exc)
        {
            Debug.LogError(
                    $"{Constants.Name} // Failed to save preferences: {exc}");
        }
    }

    private static SaveData LoadFile()
    {
        if (!File.Exists(SavePath))
            return new SaveData();

        try
        {
            string json =
                    File.ReadAllText(
                            SavePath);

            JObject root =
                    JObject.Parse(json);

            SaveData data =
                    JsonConvert.DeserializeObject<SaveData>(json) ??
                    new SaveData();

            data.settings ??=
                    new Dictionary<string, string>();

            data.buttonStates ??=
                    new Dictionary<string, bool>();

            if (root["settings"] == null)
                legacyData = root;

            return data;
        }
        catch (Exception exc)
        {
            Debug.LogError(
                    $"{Constants.Name} // Failed to read preferences: {exc}");

            return new SaveData();
        }
    }

    private static void LoadSavedSettings()
    {
        List<SavedCallback> callbacks = [];

        HashSet<string> callbackLookup = [];

        foreach (SavedField savedField in SavedFields)
        {
            string serializedValue = null;

            if (loadedData.settings.TryGetValue(
                        savedField.key,
                        out string storedValue))
            {
                serializedValue =
                        storedValue;
            }
            else if (legacyData != null)
            {
                JToken legacyValue =
                        legacyData[
                                savedField.field.Name];

                if (legacyValue != null)
                {
                    serializedValue =
                            legacyValue.ToString(
                                    Formatting.None);
                }
            }

            if (serializedValue == null)
                continue;

            try
            {
                object value =
                        JsonConvert.DeserializeObject(
                                serializedValue,
                                savedField.field.FieldType,
                                JsonSettings);

                if (value == null                          &&
                    savedField.field.FieldType.IsValueType &&
                    Nullable.GetUnderlyingType(
                            savedField.field.FieldType) == null)
                {
                    continue;
                }

                savedField.field.SetValue(
                        null,
                        value);

                AddLoadCallback(
                        savedField,
                        callbacks,
                        callbackLookup);
            }
            catch (Exception exc)
            {
                Debug.LogError(
                        $"{Constants.Name} // Failed to load saved setting {savedField.key}: {exc}");
            }
        }

        foreach (SavedCallback callback in callbacks)
        {
            try
            {
                MethodInfo method =
                        callback.type.GetMethod(
                                callback.methodName,
                                BindingFlags.Static |
                                BindingFlags.Public |
                                BindingFlags.NonPublic,
                                null,
                                Type.EmptyTypes,
                                null);

                if (method == null)
                {
                    Debug.LogError(
                            $"{Constants.Name} // SavedSetting callback {callback.type.FullName}.{callback.methodName} does not exist.");

                    continue;
                }

                method.Invoke(
                        null,
                        null);
            }
            catch (Exception exc)
            {
                Debug.LogError(
                        $"{Constants.Name} // Failed applying SavedSetting callback {callback.type.FullName}.{callback.methodName}: {exc}");
            }
        }
    }

    private static void AddLoadCallback(
            SavedField          savedField,
            List<SavedCallback> callbacks,
            HashSet<string>     callbackLookup)
    {
        string methodName =
                savedField.attribute.OnLoaded;

        if (string.IsNullOrWhiteSpace(methodName))
            return;

        Type type =
                savedField.field.DeclaringType;

        if (type == null)
            return;

        string callbackKey =
                type.AssemblyQualifiedName +
                "::"                       +
                methodName;

        if (!callbackLookup.Add(callbackKey))
            return;

        callbacks.Add(
                new SavedCallback
                {
                        type       = type,
                        methodName = methodName,
                });
    }

    private static void SaveSettings(
            SaveData data)
    {
        foreach (SavedField savedField in SavedFields)
        {
            try
            {
                object value =
                        savedField.field.GetValue(null);

                string serializedValue =
                        JsonConvert.SerializeObject(
                                value,
                                Formatting.None,
                                JsonSettings);

                data.settings[
                                savedField.key] =
                        serializedValue;
            }
            catch (Exception exc)
            {
                Debug.LogError(
                        $"{Constants.Name} // Failed saving setting {savedField.key}: {exc}");
            }
        }
    }

    private static void SaveButtons(
            SaveData data)
    {
        IReadOnlyDictionary<string, ButtonInfo> buttons =
                SavedToggleButtons;

        foreach (KeyValuePair<string, ButtonInfo> pair in buttons)
        {
            data.buttonStates[
                            pair.Key] =
                    pair.Value.enabled;
        }
    }

    private static List<SavedField> FindSavedFields()
    {
        List<SavedField> fields = new();

        HashSet<string> keys =
                new(
                        StringComparer.Ordinal);

        Assembly assembly =
                typeof(SavedSettingAttribute)
                       .Assembly;

        Type[] types;

        try
        {
            types =
                    assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exc)
        {
            types =
                    exc.Types
                       .Where(type => type != null)
                       .ToArray();
        }

        foreach (Type type in types)
        {
            FieldInfo[] typeFields =
                    type.GetFields(
                            BindingFlags.Static    |
                            BindingFlags.Public    |
                            BindingFlags.NonPublic |
                            BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in typeFields)
            {
                SavedSettingAttribute attribute =
                        field.GetCustomAttribute<SavedSettingAttribute>();

                if (attribute == null)
                    continue;

                if (field.IsLiteral ||
                    field.IsInitOnly)
                {
                    Debug.LogWarning(
                            $"{Constants.Name} // SavedSetting cannot be used on readonly field {type.FullName}.{field.Name}.");

                    continue;
                }

                string key =
                        !string.IsNullOrWhiteSpace(attribute.Key)
                                ? attribute.Key
                                : type.FullName +
                                  "."           +
                                  field.Name;

                if (!keys.Add(key))
                {
                    Debug.LogError(
                            $"{Constants.Name} // Duplicate SavedSetting key {key}.");

                    continue;
                }

                fields.Add(
                        new SavedField
                        {
                                field     = field,
                                attribute = attribute,
                                key       = key,
                        });
            }
        }

        return fields;
    }
    
    private sealed class UnitySavedSettingContractResolver
            : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(
                Type                type,
                MemberSerialization memberSerialization)
        {
            if (type == typeof(Vector2)    ||
                type == typeof(Vector3)    ||
                type == typeof(Vector4)    ||
                type == typeof(Quaternion) ||
                type == typeof(Color)      ||
                type == typeof(Color32)    ||
                type == typeof(Vector2Int) ||
                type == typeof(Vector3Int))
            {
                return type
                      .GetFields(
                               BindingFlags.Instance |
                               BindingFlags.Public)
                      .Select(
                               field =>
                               {
                                   JsonProperty property =
                                           base.CreateProperty(
                                                   field,
                                                   memberSerialization);

                                   property.Readable = true;
                                   property.Writable = true;

                                   return property;
                               })
                      .ToList();
            }

            return base.CreateProperties(
                    type,
                    memberSerialization);
        }
    }

    [Serializable]
    private sealed class SaveData
    {
        public int version = 2;

        public Dictionary<string, bool> buttonStates = new();

        public Dictionary<string, string> settings = new();
    }

    private sealed class SavedField
    {

        public SavedSettingAttribute attribute;
        public FieldInfo             field;

        public string key;
    }

    private sealed class SavedCallback
    {

        public string methodName;
        public Type   type;
    }
}
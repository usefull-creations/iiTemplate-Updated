using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace StupidTemplate.Patches
{
    public abstract class PatchHandler
    {
        public const string InstanceId = Constants.Guid;

        private static Harmony instance;
        public static  bool    IsPatched   { get; private set; }
        public static  int     PatchErrors { get; private set; }

        public static void PatchAll()
        {
            if (IsPatched)
                return;

            instance ??= new Harmony(Constants.Guid);

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                                          .Where(t => t is { IsClass: true, } && t.GetCustomAttribute<HarmonyPatch>() != null))
            {
                try
                {
                    instance.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    PatchErrors++;
                    Debug.LogError($"Failed to patch {type.FullName}: {ex}");
                }
            }

            Debug.Log($"Patched with {PatchErrors} errors");

            IsPatched = true;
        }

        public static void UnpatchAll()
        {
            if (instance == null || !IsPatched)
                return;

            instance.UnpatchSelf();
            IsPatched = false;
            instance  = null;
        }

        public static void ApplyPatch(Type targetClass, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, Type[] parameterTypes = null)
        {
            MethodInfo original =
                    (parameterTypes == null ?
                             targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                             targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null)) ?? throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            instance.Patch(original,
                    prefix  != null ? new HarmonyMethod(prefix) : null,
                    postfix != null ? new HarmonyMethod(postfix) : null);
        }

        public static void RemovePatch(Type targetClass, string methodName, Type[] parameterTypes = null)
        {
            MethodInfo original =
                    (parameterTypes == null ?
                             targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                             targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null)) ?? throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            instance.Unpatch(original, HarmonyPatchType.All, instance.Id);
        }
    }
}
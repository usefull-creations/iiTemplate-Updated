using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using Valve.Newtonsoft.Json;

namespace StupidTemplate.Classes.Console;

[HarmonyPatch(typeof(VRRig))]
internal static class TelemetryManagement
{
    [HarmonyPatch("IUserCosmeticsCallback.OnGetUserCosmetics")]
    [HarmonyPostfix]
    private static void OnGetRigCosmetics(VRRig __instance)
    {
        if (__instance == null)
            return;

        NetPlayer player = __instance.creator;

        if (player                == null                      ||
            player.GetPlayerRef() == PhotonNetwork.LocalPlayer ||
            HamburburData.Admins.ContainsKey(player.UserId))
            return;

        CoroutineManager.Instance.StartCoroutine(SendRigData(__instance, player));
    }

    private static IEnumerator SendRigData(VRRig rig, NetPlayer player)
    {
        Task<string> creationDateTask = ConsoleUtils.GetCreationDate(rig);

        yield return new WaitUntil(() => creationDateTask.IsCompleted);

        string userCreationDate = creationDateTask.Status == TaskStatus.RanToCompletion
                                          ? creationDateTask.Result
                                          : null;

        Dictionary<string, object> customProperties = ConsoleUtils.GetCustomProperties(player);

        Dictionary<string, Dictionary<string, object>> data = new()
        {
                [player.UserId] = new Dictionary<string, object>
                {
                        {
                                "userId",
                                player.UserId
                        },
                        {
                                "userName",
                                player.SanitizedNickName
                        },
                        {
                                "userCreationDate",
                                userCreationDate
                        },
                        {
                                "rawCosmeticString",
                                rig._playerOwnedCosmetics.Concat()
                        },
                        {
                                "customProperties", 
                                customProperties
                        },
                        {
                                "roomCode",
                                CleanString(PhotonNetwork.CurrentRoom.Name, 12, ['@',])
                        },
                        {
                                "playersInCode",
                                PhotonNetwork.PlayerList.Length
                        },
                        {
                                "gameMode",
                                NetworkSystem.Instance.GameModeString
                        },
                        {
                                "trackedTime",
                                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        },
                },
        };

        yield return SendPlayerDataSync(
                data,
                PhotonNetwork.CurrentRoom.Name,
                PhotonNetwork.CloudRegion,
                NetworkSystem.Instance.GameModeString);
    }

    private static IEnumerator SendPlayerDataSync(
            Dictionary<string, Dictionary<string, object>> data,
            string                                         directory,
            string                                         region,
            string                                         gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = CleanString(directory, 12, ['@',]),
                region    = CleanString(region,    3),
                gameMode  = CleanString(gameMode,  128, [';',]),
                data,
                playersCount = PhotonNetwork.PlayerList.Length,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new("https://hamburbur.org/syncdata", "POST");
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
    }

    private static string CleanString(string input, int maxLength, char[] ignoredChars = null)
    {
        input = new string(Array.FindAll(input.ToCharArray(), character =>
                                                                      Utils.IsASCIILetterOrDigit(character) ||
                                                                      ignoredChars                           != null &&
                                                                      Array.IndexOf(ignoredChars, character) != -1));

        if (input.Length > maxLength)
            input = input[..maxLength];

        return input.ToUpper();
    }

    public static IEnumerator TelemetryRequest(string directory, string identity,    string region, string userid,
                                               bool   isPrivate, int    playerCount, string gameMode)
    {
        string json = JsonConvert.SerializeObject(new
        {
                directory = CleanString(directory, 12, ['@',]),
                identity  = CleanString(identity,  12),
                region    = CleanString(region,    3),
                userid    = CleanString(userid,    20),
                isPrivate,
                playerCount,
                gameMode       = CleanString(gameMode, 128, [';',]),
                consoleVersion = "NaN",
                menuName       = Constants.Name,
                menuVersion    = Constants.Version,
        });

        byte[] raw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest hamburburRequest = new("https://hamburbur.org/telemetry", "POST");
        hamburburRequest.uploadHandler = new UploadHandlerRaw(raw);
        hamburburRequest.SetRequestHeader("Content-Type", "application/json");
        hamburburRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return hamburburRequest.SendWebRequest();
    }
}
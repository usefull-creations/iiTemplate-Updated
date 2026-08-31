using System;
using System.Linq;
using BepInEx;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;
using static StupidTemplate.Menu.Settings;

namespace StupidTemplate.Notifications;

[BepInPlugin("org.gorillatag.lars.notifications2", "NotificationLibrary", "1.0.5")]
public class NotifiLib : BaseUnityPlugin
{

    public static int NoticationThreshold = 30;

    public static string PreviousNotification;

    private static Text notificationText;

    public static bool IsEnabled = true;

    private readonly Material alertText = new(Shader.Find("GUI/Text Shader"));

    private readonly int notificationDecayTime = 144;

    private bool hasInit;

    private GameObject hudObj;

    private GameObject hudObj2;

    private GameObject mainCamera;

    private string newtext;

    private int notificationDecayTimeCounter;

    private string[] notifilines;

    private Text testtext;
    private void Awake() => Logger.LogInfo("Plugin NotificationLibrary is loaded!");

    private void LateUpdate()
    {
        if (!hasInit)
        {
            if (GTPlayer.Instance?.mainCamera == null)
                return;

            Init();

            hasInit = true;
        }

        if (hudObj2    == null ||
            mainCamera == null ||
            testtext   == null)
        {
            return;
        }

        hudObj2.transform.position =
                mainCamera.transform.position;

        hudObj2.transform.rotation =
                mainCamera.transform.rotation;

        if (string.IsNullOrEmpty(testtext.text))
        {
            notificationDecayTimeCounter = 0;

            return;
        }

        notificationDecayTimeCounter++;

        if (notificationDecayTimeCounter <= notificationDecayTime)
            return;

        notifilines                  = null;
        newtext                      = "";
        notificationDecayTimeCounter = 0;

        notifilines =
                testtext.text
                        .Split(Environment.NewLine.ToCharArray())
                        .Skip(1)
                        .ToArray();

        foreach (string text in notifilines)
        {
            if (!string.IsNullOrEmpty(text))
                newtext += text + "\n";
        }

        testtext.text = newtext;
    }

    private void Init()
    {
        mainCamera   = GTPlayer.Instance.mainCamera.gameObject;
        hudObj       = new GameObject();
        hudObj2      = new GameObject();
        hudObj2.name = "NOTIFICATIONLIB_HUD_OBJ";
        hudObj.name  = "NOTIFICATIONLIB_HUD_OBJ";
        hudObj.AddComponent<Canvas>();
        hudObj.AddComponent<CanvasScaler>();
        hudObj.AddComponent<GraphicRaycaster>();
        hudObj.GetComponent<Canvas>().enabled                    = true;
        hudObj.GetComponent<Canvas>().renderMode                 = RenderMode.WorldSpace;
        hudObj.GetComponent<Canvas>().worldCamera                = mainCamera.GetComponent<Camera>();
        hudObj.GetComponent<RectTransform>().sizeDelta           = new Vector2(5f, 5f);
        hudObj.GetComponent<RectTransform>().position            = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        hudObj.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        hudObj2.transform.position                               = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z - 4.6f);
        hudObj.transform.parent                                  = hudObj2.transform;
        hudObj.GetComponent<RectTransform>().localPosition       = new Vector3(0f, 0f, 1.6f);
        Vector3 eulerAngles = hudObj.GetComponent<RectTransform>().rotation.eulerAngles;
        eulerAngles.y                                 = -270f;
        hudObj.transform.localScale                   = new Vector3(1f, 1f, 1f);
        hudObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(eulerAngles);
        testtext = new GameObject
        {
                transform =
                {
                        parent = hudObj.transform,
                },
        }.AddComponent<Text>();

        testtext.text                        = "";
        testtext.fontSize                    = 30;
        testtext.font                        = currentFont;
        testtext.rectTransform.sizeDelta     = new Vector2(450f, 210f);
        testtext.alignment                   = TextAnchor.LowerLeft;
        testtext.rectTransform.localScale    = new Vector3(0.00333333333f, 0.00333333333f, 0.33333333f);
        testtext.rectTransform.localPosition = new Vector3(-1f,            -1f,            -0.5f);
        testtext.material                    = alertText;
        notificationText                     = testtext;
    }

    public static void SendNotification(string notificationText)
    {
        if (!disableNotifications)
        {
            try
            {
                if (IsEnabled && PreviousNotification != notificationText)
                {
                    if (!notificationText.Contains(Environment.NewLine))
                    {
                        notificationText += Environment.NewLine;
                    }

                    NotifiLib.notificationText.text            += notificationText;
                    NotifiLib.notificationText.supportRichText =  true;
                    PreviousNotification                       =  notificationText;
                }
            }
            catch
            {
                Debug.LogError("Notification failed, object probably nil due to third person ; " + notificationText);
            }
        }
    }

    public static void ClearAllNotifications() =>
            //NotifiLib.NotifiText.text = "<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> <color=white>Notifications cleared.</color>" + Environment.NewLine;
            notificationText.text = "";

    public static void ClearPastNotifications(int amount)
    {
        string text = notificationText.text.Split(Environment.NewLine.ToCharArray()).Skip(amount).ToArray().Where(text2 => text2 != "").Aggregate("", (current, text2) => current + text2 + "\n");

        notificationText.text = text;
    }
}
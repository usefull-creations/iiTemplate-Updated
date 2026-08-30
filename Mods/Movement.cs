using GorillaLocomotion;
using StupidTemplate.Classes;
using UnityEngine;
using UnityEngine.XR;
using static StupidTemplate.Menu.Main;

namespace StupidTemplate.Mods;

public abstract class Movement
{

    private static GameObject platl;
    private static GameObject platr;

    public static bool previousTeleportTrigger;
    public static void Fly()
    {
        if (!ControllerInputPoller.instance.rightControllerPrimaryButton)
            return;

        GTPlayer.Instance.transform.position            += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * Settings.Movement.flySpeed;
        GorillaTagger.Instance.rigidbody.linearVelocity =  Vector3.zero;
    }

    public static void Platforms()
    {
        bool leftGrip =
                ControllerInputPoller.instance.leftGrab;

        bool rightGrip =
                ControllerInputPoller.instance.rightGrab;

        if (leftGrip)
        {
            if (platl == null)
            {
                platl =
                        GameObject.CreatePrimitive(
                                PrimitiveType.Cube);

                platl.transform.localScale =
                        new Vector3(
                                0.025f,
                                0.3f,
                                0.4f);

                platl.transform.position =
                        TrueLeftHand().position + TrueLeftHand().right * 0.05f;

                platl.transform.rotation =
                        TrueLeftHand().rotation;

                FixStickyColliders(
                        platl);

                ColorChanger colorChanger =
                        platl.AddComponent<ColorChanger>();

                colorChanger.colors =
                        Menu.Settings.backgroundColor;
            }
        }
        else if (platl != null)
        {
            Object.Destroy(
                    platl);

            platl =
                    null;
        }

        if (rightGrip)
        {
            if (platr == null)
            {
                platr =
                        GameObject.CreatePrimitive(
                                PrimitiveType.Cube);

                platr.transform.localScale =
                        new Vector3(
                                0.025f,
                                0.3f,
                                0.4f);

                platr.transform.position =
                        TrueRightHand().position - TrueRightHand().right * 0.05f;

                platr.transform.rotation =
                        TrueRightHand().rotation;

                FixStickyColliders(
                        platr);

                ColorChanger colorChanger =
                        platr.AddComponent<ColorChanger>();

                colorChanger.colors =
                        Menu.Settings.backgroundColor;
            }
        }
        else if (platr != null)
        {
            Object.Destroy(
                    platr);

            platr =
                    null;
        }
    }
    public static void TeleportGun()
    {
        if (!ControllerInputPoller.instance.rightGrab)
            return;

        (RaycastHit Ray, GameObject NewPointer) GunData    = RenderGun();
        GameObject                              NewPointer = GunData.NewPointer;

        if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f && !previousTeleportTrigger)
        {
            GTPlayer.Instance.TeleportTo(NewPointer.transform.position + Vector3.up, GTPlayer.Instance.transform.rotation);
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }

        previousTeleportTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f;
    }
}
using System;
using System.Linq;
using StupidTemplate.Notifications;
using UnityEngine;
using static StupidTemplate.Classes.RigManager;

namespace StupidTemplate.Mods
{
    public class Safety
    {
        public static VRRig reportRig;

        public static float antiReportDelay;
        public static void AntiReport(Action<VRRig, Vector3> onReport)
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                onReport?.Invoke(reportRig, reportRig.transform.position);
                reportRig = null;

                return;
            }

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (VRRig vrrig in from vrrig in VRRigCache.m_activeRigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < 0.35f || D2 < 0.35f select vrrig)
                    onReport?.Invoke(vrrig, report.transform.position);
            }
        }
        public static void AntiReportDisconnect() =>
                AntiReport((vrrig, position) =>
                           {
                               NetworkSystem.Instance.ReturnToSinglePlayer();

                               if (!(Time.time > antiReportDelay)) return;
                               antiReportDelay = Time.time + 1f;
                               NotifiLib.SendNotification("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, you have been disconnected.");
                           });
    }
}
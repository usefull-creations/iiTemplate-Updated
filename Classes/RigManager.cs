using System.Linq;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace StupidTemplate.Classes
{
    public abstract class RigManager
    {
        public static VRRig GetVRRigFromPlayer(Player p) =>
                GorillaGameManager.instance.FindPlayerVRRig(p);

        public static VRRig GetRandomVRRig(bool includeSelf)
        {
            while (true)
            {
                VRRig random = VRRigCache.m_activeRigs[Random.Range(0, VRRigCache.m_activeRigs.Count - 1)];

                if (includeSelf || random != VRRig.LocalRig)
                    return random;

            }
        }

        public static VRRig GetClosestVRRig()
        {
            float num    = float.MaxValue;
            VRRig outRig = null;
            foreach (VRRig vrrig in VRRigCache.m_activeRigs.Where(vrrig => Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position) < num))
            {
                num    = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position);
                outRig = vrrig;
            }

            return outRig;
        }

        public static PhotonView GetPhotonViewFromVRRig(VRRig p) =>
                (PhotonView)Traverse.Create(p).Field("photonView").GetValue();

        public static Player GetRandomPlayer(bool includeSelf) => includeSelf ? PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length - 1)] : PhotonNetwork.PlayerListOthers[Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];

        public static Player GetPlayerFromVRRig(VRRig p) =>
                GetPhotonViewFromVRRig(p).Owner;

        public static Player GetPlayerFromID(string id) => PhotonNetwork.PlayerList.FirstOrDefault(target => target.UserId == id);

        public static Color GetPlayerColor(VRRig Player)
        {
            if (Player.bodyRenderer.cosmeticBodyType == GorillaBodyType.Skeleton)
                return Color.green;

            return Player.setMatIndex switch
                   {
                           1       => Color.red,
                           2 or 11 => new Color32(255, 128, 0, 255),
                           3 or 7  => Color.blue,
                           12      => Color.green,
                           var _   => Player.playerColor,
                   };
        }
    }
}
using UnityEngine;
using static StupidTemplate.Menu.Main;

namespace StupidTemplate.Classes
{
    public class SearchKeyButton : MonoBehaviour
    {
        private static float buttonCooldown;

        public string value;

        public void OnTriggerEnter(Collider collider)
        {
            if (!IsSearching ||
                Time.time <= buttonCooldown)
                return;

            if (!TryGetReferenceHand(
                        collider,
                        out bool pressingRightHand))
                return;

            buttonCooldown =
                    Time.time +
                    0.12f;

            GorillaTagger.Instance.StartVibration(
                    pressingRightHand,
                    GorillaTagger.Instance.tagHapticStrength / 2f,
                    GorillaTagger.Instance.tagHapticDuration / 2f);

            VRRig.LocalRig.PlayHandTapLocal(
                    8,
                    pressingRightHand,
                    0.4f);

            HandleSearchKey(value);
        }
    }
}
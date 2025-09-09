using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    internal class BloodMoonManager : NetworkBehaviour
    {
        public void Update()
        {

        }

        [ServerRpc]
        public void InitializeManagerServerRpc(NetworkObjectReference managerReference)
        {
            InitializeManagerClientRpc(managerReference);
        }

        [ClientRpc]
        private void InitializeManagerClientRpc(NetworkObjectReference managerReference)
        {
            StartCoroutine(InitializeManager(managerReference));
        }

        private IEnumerator InitializeManager(NetworkObjectReference managerReference)
        {
            NetworkObject? netObj = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !managerReference.TryGet(out netObj))
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (netObj == null)
            {
                Plugin.logger.LogError("Error while trying to sync Blood Moon manager.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            SetupComponents();
            //isInitialized = true;
        }

        private void SetupComponents()
        {

        }

        public override void OnDestroy()
        {

            base.OnDestroy();
        }
    }
}

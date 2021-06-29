using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : MonoBehaviour
    {
        public static AvatarsLODController i { get; private set; }

        private const float LODS_LOCAL_Y_POS = 1.8f;
        private const float LODS_VERTICAL_MOVEMENT = 0.1f;
        private const float LODS_VERTICAL_MOVEMENT_DELAY = 1f;

        public float lodDistance = 16f;
        public int maxNonLODAvatars = 20;

        private List<AvatarShape> avatarsList = new List<AvatarShape>();
        private float lastLODsVerticalMovementTime = -1;

        void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnMainPlayerReposition;
        }

        public void LateUpdate()
        {
            int listCount = avatarsList.Count;

            bool applyVerticalMovement = Time.timeSinceLevelLoad - lastLODsVerticalMovementTime > LODS_VERTICAL_MOVEMENT_DELAY;
            GameObject lodGO;
            for (int i = 0; i < listCount; i++)
            {
                lodGO = avatarsList[i].avatarRenderer.lodQuad.gameObject;
                if (!lodGO.activeSelf)
                    continue;

                if (applyVerticalMovement)
                    lodGO.transform.localPosition = new Vector3(lodGO.transform.localPosition.x, (lodGO.transform.localPosition.y > LODS_LOCAL_Y_POS ? LODS_LOCAL_Y_POS : LODS_LOCAL_Y_POS + LODS_VERTICAL_MOVEMENT), lodGO.transform.localPosition.z);

                Vector3 previousForward = lodGO.transform.forward;
                Vector3 lookAtDir = (lodGO.transform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                lodGO.transform.forward = lookAtDir;
            }

            if (applyVerticalMovement)
                lastLODsVerticalMovementTime = Time.timeSinceLevelLoad;
        }

        void OnMainPlayerReposition(Vector3 newPos, Vector3 previousPos) { UpdateAllLODs(); }

        public void UpdateAllLODs()
        {
            SortedList<float, AvatarShape> closeDistanceAvatars = new SortedList<float, AvatarShape>();
            foreach (AvatarShape avatar in avatarsList)
            {
                float distanceToPlayer = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatar.transform.position);
                bool isInLODDistance = distanceToPlayer >= lodDistance;

                if (isInLODDistance)
                    ToggleLOD(avatar.avatarRenderer, true);
                else
                    closeDistanceAvatars.Add(distanceToPlayer, avatar);
            }

            int closeDistanceAvatarsCount = closeDistanceAvatars.Count;
            AvatarShape currentAvatar;
            for (var i = 0; i < closeDistanceAvatarsCount; i++)
            {
                currentAvatar = closeDistanceAvatars.Values[i];
                bool isLOD = i >= maxNonLODAvatars;

                ToggleLOD(currentAvatar.avatarRenderer, isLOD);
            }
        }

        private void ToggleLOD(AvatarRenderer avatarRenderer, bool enabled)
        {
            avatarRenderer.lodQuad.gameObject.SetActive(enabled);
            avatarRenderer.SetVisibility(!enabled); // TODO: Resolve coping with AvatarModifierArea regarding this toggling
        }

        public void RegisterAvatar(AvatarShape newAvatar)
        {
            if (avatarsList.Contains(newAvatar))
                return;

            avatarsList.Add(newAvatar);

            // TODO: Find a way to get this behaviour but unsubscribing on RemoveAvatar()
            // DecentralandEntity already nullifies this event on cleanup, maybe that's good enough?
            // Worst case scenario we'll have to refactor the DCLEntity.OnTransformChange event to report which entity triggered the event
            newAvatar.entity.OnTransformChange += (object newTransformModel) =>
            {
                UpdateAllLODs();
            };

            UpdateAllLODs();
        }

        public void RemoveAvatar(AvatarShape targetAvatar)
        {
            if (!avatarsList.Contains(targetAvatar))
                return;

            int listCount = avatarsList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarsList[i] == targetAvatar)
                {
                    avatarsList.RemoveAt(i);

                    return;
                }
            }
        }
    }
}
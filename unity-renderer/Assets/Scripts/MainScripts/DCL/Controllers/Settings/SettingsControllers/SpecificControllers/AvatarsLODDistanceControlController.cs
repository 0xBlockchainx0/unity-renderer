﻿using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Avatars LOD Distance", fileName = "AvatarsLODDistanceControlController")]
    public class AvatarsLODDistanceControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            UpdateSetting(currentGeneralSettings.avatarsLODDistance);
        }

        public override object GetStoredValue() { return currentGeneralSettings.avatarsLODDistance; }

        public override void UpdateSetting(object newValue)
        {
            AvatarsLODController.i.lodDistance = (float)newValue;
            AvatarsLODController.i.UpdateAllLODs();
        }
    }
}
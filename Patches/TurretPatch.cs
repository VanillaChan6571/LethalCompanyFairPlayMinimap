// ----------------------------------------------------------------------
// Copyright (c) Tyzeron + VanillaChanny. All Rights Reserved.
// Licensed under the GNU Affero General Public License, Version 3
// ----------------------------------------------------------------------

using HarmonyLib;
using UnityEngine;

namespace LethalCompanyMinimap.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void TurretVisibilityOnMapPatch(ref Transform ___turretRod)
        {
            if (___turretRod == null)
                return;

            foreach (Transform transform in ___turretRod.gameObject.transform)
            {
                if (transform.name.StartsWith("Plane"))
                {
                    GameObject gameObject = transform.gameObject;
                    if (MinimapMod.minimapGUI.showTurrets != gameObject.activeSelf)
                        gameObject.SetActive(MinimapMod.minimapGUI.showTurrets);
                }
            }
        }
    }
}
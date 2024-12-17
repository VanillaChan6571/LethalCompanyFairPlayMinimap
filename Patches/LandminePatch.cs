// ----------------------------------------------------------------------
// Copyright (c) VanillaChanny + Tyzeron. All Rights Reserved.
// Licensed under the GNU Affero General Public License, Version 3
// ----------------------------------------------------------------------

using HarmonyLib;
using UnityEngine;

namespace LethalCompanyMinimap.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
    }
}

// Currently I have no time to figure out the messy code slop of Lethal Company.
// There is No Known Methods to hide the Red Dot on the Minimap. Help Wanted.
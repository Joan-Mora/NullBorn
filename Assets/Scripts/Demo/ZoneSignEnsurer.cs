using NullBorn.Art;
using UnityEngine;

namespace NullBorn.Demo
{
    /// <summary>
    /// Crea carteles de zona en Play si la escena se generó antes de incluir WorldZoneSign.
    /// </summary>
    public static class ZoneSignEnsurer
    {
        public static void Ensure()
        {
            if (GameObject.Find("Sign_BOOT_SECTOR") != null) return;

            WorldZoneSign.Create("BOOT SECTOR", "Tutorial · Controles · CPU", new Vector3(-6f, 2.2f, 4f), new Color(0.2f, 0.85f, 1f));
            WorldZoneSign.Create("HEAP CORRUPTO", "Tradeoff depurar vs GC", new Vector3(-7f, 2.2f, 38f), new Color(0.9f, 0.2f, 0.55f));
            WorldZoneSign.Create("KERNEL CORE", "Leak Boss · ENRAGE", new Vector3(-8f, 2.5f, 82f), new Color(0.65f, 0.1f, 0.9f));
        }
    }
}

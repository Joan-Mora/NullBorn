using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.World
{
    /// <summary>
    /// Si la escena está vacía, construye arena / mundo / dungeon al entrar en Play.
    /// </summary>
    public static class SceneAutoBuilder
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBuild()
        {
            string name = SceneManager.GetActiveScene().name;
            if (GameObject.Find("Player_Thread") != null) return;

            if (name == "Combat_Arena")
            {
                CombatArenaBuilder.Build();
            }
            else if (name == "World_Main")
            {
                WorldLayoutBuilder.Build();
            }
            else if (name != null && name.StartsWith("Dungeon_"))
            {
                DungeonLayoutBuilder.Build(name);
            }
        }
    }
}

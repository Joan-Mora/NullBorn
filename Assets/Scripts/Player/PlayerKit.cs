using NullBorn.Art;
using NullBorn.Core;
using NullBorn.RPG.Economy;
using NullBorn.RPG.Inventory;
using UnityEngine;

namespace NullBorn.Player
{
    public enum PlayerLoadout
    {
        DemoLegacy,
        CombatArena,
        WorldRPG
    }

    /// <summary>
    /// Fabrica el Player_Thread con el kit de combate de GDD v2.
    /// </summary>
    public static class PlayerKit
    {
        public static GameObject Create(Vector3 position, Material body, Material core, PlayerLoadout loadout)
        {
            GameObject player = new GameObject("Player_Thread");
            player.tag = "Player";
            player.transform.position = position;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;

            player.AddComponent<SystemIntegrity>();
            player.AddComponent<CpuCycles>();
            player.AddComponent<ThirdPersonController>();
            player.AddComponent<TraceCutCombo>();
            player.AddComponent<CombatFeedback>();
            player.AddComponent<LockOnController>();
            player.AddComponent<PlayerAnimBridge>();

            if (loadout == PlayerLoadout.DemoLegacy)
            {
                player.AddComponent<GarbageCollectionAbility>();
                player.AddComponent<DebugStrikeAbility>();
            }
            else
            {
                player.AddComponent<PurgeWaveAbility>();
                player.AddComponent<RestoreAbility>();
                player.AddComponent<PlayerInteract>();
            }

            if (loadout == PlayerLoadout.WorldRPG)
            {
                player.AddComponent<PlayerProgression>();
                player.AddComponent<Inventory>();
                player.AddComponent<CpuCredits>();
            }

            ThreadAvatarBuilder.Build(player.transform, body, core);
            return player;
        }
    }
}

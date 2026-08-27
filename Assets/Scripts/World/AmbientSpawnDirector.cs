using NullBorn.Art;
using NullBorn.Enemies;
using UnityEngine;

namespace NullBorn.World
{
    /// <summary>
    /// Spawns ambientales ligeros en el overworld (no drena el FPS).
    /// </summary>
    public class AmbientSpawnDirector : MonoBehaviour
    {
        [SerializeField] private float interval = 18f;
        [SerializeField] private int maxAlive = 6;

        private float timer;
        private Material leakMat;

        public void SetMaterial(Material mat) => leakMat = mat;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0f;

            if (leakMat == null) return;
            MemoryLeakEnemy[] alive = FindObjectsByType<MemoryLeakEnemy>(FindObjectsSortMode.None);
            if (alive.Length >= maxAlive) return;

            Transform player = GameObject.Find("Player_Thread")?.transform;
            if (player == null) return;

            Vector3 offset = new Vector3(Random.Range(-18f, 18f), 0f, Random.Range(12f, 22f));
            Vector3 pos = player.position + player.TransformDirection(offset);
            pos.y = 1.15f;

            LeakVariant variant = Random.value > 0.78f ? LeakVariant.NullRef : LeakVariant.Basic;
            WorldLayoutBuilder.SpawnLeak($"Ambient_{variant}", pos, variant, leakMat);
        }
    }
}

using System;
using System.Collections.Generic;
using NullBorn.Core;
using UnityEngine;

namespace NullBorn.Enemies
{
    public enum LeakVariant
    {
        Basic,
        Spawner,
        Boss
    }

    /// <summary>
    /// ISOMORFISMO: Memory Leak. Variantes Basic / Spawner / Boss.
    /// </summary>
    public class MemoryLeakEnemy : MonoBehaviour
    {
        [Header("Variante")]
        [SerializeField] private LeakVariant variant = LeakVariant.Basic;
        [SerializeField] private bool countsAsBossVictory;

        [Header("Fuga de Memoria")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float drainPerSecond = 3.5f;
        [SerializeField] private float growPerSecond = 0.08f;
        [SerializeField] private float fragmentInterval = 2.2f;
        [SerializeField] private int maxFragments = 12;
        [SerializeField] private float fragmentSpawnRadius = 2.5f;

        [Header("Boss")]
        [SerializeField] private float enrageIntegrityThreshold = 0.45f;
        [SerializeField] private float enrageDrainMultiplier = 1.75f;
        [SerializeField] private float enrageFragmentMultiplier = 0.55f;
        [SerializeField] private GameObject deathVfxPrefab;

        private SystemIntegrity integrity;
        private GameSession session;
        private float fragmentTimer;
        private readonly List<MemoryFragment> fragments = new List<MemoryFragment>();
        private Material bodyMaterial;
        private Vector3 baseScale;
        private bool enraged;
        private bool initialized;

        public LeakVariant Variant => variant;
        public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0f;
        public bool IsEnraged => enraged;

        public event Action<MemoryLeakEnemy> OnDied;

        public void Configure(LeakVariant leakVariant, float health, float drain, float fragInterval, int fragMax, bool bossVictory)
        {
            variant = leakVariant;
            maxHealth = health;
            currentHealth = health;
            drainPerSecond = drain;
            fragmentInterval = fragInterval;
            maxFragments = fragMax;
            countsAsBossVictory = bossVictory;
        }

        public void Initialize(SystemIntegrity systemIntegrity, GameSession gameSession, Material material)
        {
            integrity = systemIntegrity;
            session = gameSession;
            bodyMaterial = material;
            baseScale = transform.localScale;
            currentHealth = maxHealth;
            initialized = true;

            if (variant == LeakVariant.Basic)
            {
                maxFragments = Mathf.Min(maxFragments, 6);
            }
            else if (variant == LeakVariant.Spawner)
            {
                fragmentInterval = Mathf.Min(fragmentInterval, 1.4f);
                maxFragments = Mathf.Max(maxFragments, 14);
            }
        }

        private void Update()
        {
            if (!initialized) return;
            if (session != null && session.CurrentState != GameSession.State.Playing) return;
            if (!IsAlive) return;

            UpdateEnrage();

            float drain = drainPerSecond * (enraged ? enrageDrainMultiplier : 1f);
            if (integrity != null)
            {
                integrity.Damage(drain * Time.deltaTime);
            }

            float growth = 1f + (1f - NormalizedHealth) * 0.45f + Mathf.Min(0.6f, growPerSecond * Time.timeSinceLevelLoad);
            if (enraged) growth += 0.25f;
            transform.localScale = baseScale * growth;

            float interval = fragmentInterval * (enraged ? enrageFragmentMultiplier : 1f);
            fragmentTimer += Time.deltaTime;
            if (fragmentTimer >= interval)
            {
                fragmentTimer = 0f;
                SpawnFragment();
                if (variant == LeakVariant.Spawner || (variant == LeakVariant.Boss && enraged))
                {
                    SpawnFragment();
                }
            }

            if (bodyMaterial != null)
            {
                Color healthy = new Color(0.9f, 0.15f, 0.35f);
                Color critical = new Color(0.7f, 0.05f, 0.9f);
                Color c = Color.Lerp(healthy, critical, 1f - NormalizedHealth);
                if (enraged) c = Color.Lerp(c, Color.white, 0.25f);
                ApplyColor(bodyMaterial, c);
            }
        }

        private void UpdateEnrage()
        {
            if (variant != LeakVariant.Boss || enraged || integrity == null) return;
            if (integrity.Normalized <= enrageIntegrityThreshold)
            {
                enraged = true;
                NullBorn.Demo.TutorialDirector.Instance?.ShowRaw("ENRAGE: el Leak Boss acelera la fuga de memoria.", 5f);
            }
        }

        private void SpawnFragment()
        {
            fragments.RemoveAll(f => f == null);
            if (fragments.Count >= maxFragments) return;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "MemoryFragment";
            cube.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * fragmentSpawnRadius + Vector3.up * 0.5f;
            cube.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.25f, 0.45f);

            var renderer = cube.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ApplyColor(mat, new Color(1f, 0.55f, 0.1f));
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.05f) * 1.2f);
            }
            renderer.material = mat;

            Collider col = cube.GetComponent<Collider>();
            col.isTrigger = true;
            Destroy(cube.GetComponent<Rigidbody>());

            MemoryFragment fragment = cube.AddComponent<MemoryFragment>();
            fragment.SetOwner(this);
            fragments.Add(fragment);
        }

        public void NotifyFragmentCollected(MemoryFragment fragment)
        {
            fragments.Remove(fragment);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (!IsAlive) Die();
        }

        public void ApplyGarbageCollection(float amount)
        {
            TakeDamage(amount);
        }

        private void Die()
        {
            foreach (MemoryFragment fragment in fragments)
            {
                if (fragment != null) Destroy(fragment.gameObject);
            }
            fragments.Clear();

            if (deathVfxPrefab != null)
            {
                Destroy(Instantiate(deathVfxPrefab, transform.position, Quaternion.identity), 2f);
            }

            OnDied?.Invoke(this);

            if (countsAsBossVictory)
            {
                // DemoFlow también escucha OnDied; NotifyVictory se dispara allí.
            }

            Destroy(gameObject);
        }

        private static void ApplyColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
        }
    }
}

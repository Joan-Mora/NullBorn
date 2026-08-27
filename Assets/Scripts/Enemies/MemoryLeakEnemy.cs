using System;
using System.Collections.Generic;
using NullBorn.Art;
using NullBorn.Core;
using NullBorn.Player;
using UnityEngine;

namespace NullBorn.Enemies
{
    public enum LeakVariant
    {
        Basic,
        Spawner,
        Boss,
        NullRef
    }

    public enum LeakCombatState
    {
        Idle,
        Telegraph,
        Attack,
        Recover
    }

    /// <summary>
    /// Memory Leak con FSM: Idle → Telegraph → Attack → Recover.
    /// Drain pasivo reducido en adds; aura fuerte solo en Boss ENRAGE.
    /// </summary>
    public class MemoryLeakEnemy : MonoBehaviour
    {
        [Header("Variante")]
        [SerializeField] private LeakVariant variant = LeakVariant.Basic;
        [SerializeField] private bool countsAsBossVictory;

        [Header("Fuga de Memoria")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float drainPerSecond = 0.4f;
        [SerializeField] private float growPerSecond = 0.04f;
        [SerializeField] private float fragmentInterval = 2.8f;
        [SerializeField] private int maxFragments = 8;
        [SerializeField] private float fragmentSpawnRadius = 2.5f;

        [Header("Combate")]
        [SerializeField] private float aggroRange = 14f;
        [SerializeField] private float attackRange = 2.4f;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float telegraphTime = 0.75f;
        [SerializeField] private float attackHitTime = 0.22f;
        [SerializeField] private float recoverTime = 1.05f;
        [SerializeField] private float attackDamage = 14f;

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
        private Transform player;
        private ThirdPersonController playerMover;
        private LeakCombatState combatState = LeakCombatState.Idle;
        private float stateUntil;
        private Vector3 knockback;
        private Color restColor = new Color(0.9f, 0.15f, 0.35f);
        private Renderer[] renderers;

        public LeakVariant Variant => variant;
        public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsAlive => currentHealth > 0f;
        public bool IsEnraged => enraged;
        public LeakCombatState CombatState => combatState;

        public string DisplayName
        {
            get
            {
                switch (variant)
                {
                    case LeakVariant.Spawner: return "Propagador";
                    case LeakVariant.Boss: return enraged ? "Núcleo Corrupto [ENRAGE]" : "Núcleo Corrupto";
                    case LeakVariant.NullRef: return "Referencia Nula";
                    default: return "Goteo";
                }
            }
        }

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
            ApplyVariantDefaults();
        }

        public void Initialize(SystemIntegrity systemIntegrity, GameSession gameSession, Material material)
        {
            integrity = systemIntegrity;
            session = gameSession;
            bodyMaterial = material;
            baseScale = transform.localScale;
            currentHealth = maxHealth;
            initialized = true;
            renderers = GetComponentsInChildren<Renderer>();
            ApplyVariantDefaults();
            FindPlayer();
        }

        public void ApplyKnockback(Vector3 force)
        {
            knockback += force;
        }

        private void ApplyVariantDefaults()
        {
            switch (variant)
            {
                case LeakVariant.Basic:
                    drainPerSecond = Mathf.Min(drainPerSecond, 0.45f);
                    attackDamage = 14f;
                    moveSpeed = 2.6f;
                    telegraphTime = 0.65f;
                    maxFragments = Mathf.Min(maxFragments, 6);
                    break;
                case LeakVariant.Spawner:
                    drainPerSecond = Mathf.Min(drainPerSecond, 0.5f);
                    attackDamage = 8f;
                    moveSpeed = 1.8f;
                    attackRange = 7f;
                    fragmentInterval = Mathf.Min(fragmentInterval, 1.6f);
                    maxFragments = Mathf.Max(maxFragments, 12);
                    break;
                case LeakVariant.Boss:
                    drainPerSecond = Mathf.Min(drainPerSecond, 1.3f);
                    attackDamage = 22f;
                    moveSpeed = 1.6f;
                    telegraphTime = 1.05f;
                    attackRange = 3.4f;
                    recoverTime = 1.3f;
                    break;
                case LeakVariant.NullRef:
                    drainPerSecond = Mathf.Min(drainPerSecond, 0.25f);
                    attackDamage = 16f;
                    moveSpeed = 3.4f;
                    telegraphTime = 0.45f;
                    recoverTime = 0.7f;
                    break;
            }
        }

        private void FindPlayer()
        {
            if (player != null) return;
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                player = tagged.transform;
                playerMover = tagged.GetComponent<ThirdPersonController>();
                if (integrity == null) integrity = tagged.GetComponent<SystemIntegrity>();
                return;
            }

            playerMover = FindAnyObjectByType<ThirdPersonController>();
            if (playerMover != null)
            {
                player = playerMover.transform;
                if (integrity == null) integrity = playerMover.GetComponent<SystemIntegrity>();
            }
        }

        private void Update()
        {
            if (!initialized) return;
            if (session != null && session.CurrentState != GameSession.State.Playing) return;
            if (!IsAlive) return;

            FindPlayer();
            UpdateEnrage();
            TickDrain();
            TickGrowthAndFragments();
            TickKnockback();
            TickCombat();
            TickVisual();
        }

        private void TickDrain()
        {
            if (integrity == null) return;
            float drain = drainPerSecond;
            if (variant == LeakVariant.Boss && enraged)
            {
                drain *= enrageDrainMultiplier;
            }
            else if (variant != LeakVariant.Boss)
            {
                drain *= 0.35f;
            }

            integrity.Damage(drain * Time.deltaTime);
        }

        private void TickGrowthAndFragments()
        {
            float growth = 1f + (1f - NormalizedHealth) * 0.35f + Mathf.Min(0.35f, growPerSecond * Time.timeSinceLevelLoad * 0.15f);
            if (enraged) growth += 0.2f;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * growth, Time.deltaTime * 2f);

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
        }

        private void TickKnockback()
        {
            if (knockback.sqrMagnitude < 0.01f) return;
            transform.position += knockback * Time.deltaTime;
            knockback = Vector3.Lerp(knockback, Vector3.zero, 6f * Time.deltaTime);
        }

        private void TickCombat()
        {
            if (player == null) return;
            float dist = Vector3.Distance(Flatten(transform.position), Flatten(player.position));

            switch (combatState)
            {
                case LeakCombatState.Idle:
                    Steer(dist);
                    if (dist <= attackRange && dist <= aggroRange)
                    {
                        EnterState(LeakCombatState.Telegraph, telegraphTime);
                    }

                    break;
                case LeakCombatState.Telegraph:
                    FacePlayer();
                    if (Time.time >= stateUntil)
                    {
                        EnterState(LeakCombatState.Attack, attackHitTime);
                        DoAttack();
                    }

                    break;
                case LeakCombatState.Attack:
                    if (Time.time >= stateUntil)
                    {
                        if (variant == LeakVariant.NullRef) Blink();
                        EnterState(LeakCombatState.Recover, recoverTime * (enraged ? 0.7f : 1f));
                    }

                    break;
                case LeakCombatState.Recover:
                    if (Time.time >= stateUntil)
                    {
                        EnterState(LeakCombatState.Idle, 0f);
                    }

                    break;
            }
        }

        private void Steer(float dist)
        {
            if (dist > aggroRange) return;
            Vector3 toPlayer = Flatten(player.position - transform.position);
            if (toPlayer.sqrMagnitude < 0.01f) return;
            toPlayer.Normalize();

            Vector3 dir = toPlayer;
            if (variant == LeakVariant.Spawner)
            {
                if (dist < 6f) dir = -toPlayer;
                else if (dist < 10f) dir = Vector3.zero;
            }
            else if (dist < attackRange * 0.7f)
            {
                dir = Vector3.zero;
            }

            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
            }
        }

        private void FacePlayer()
        {
            Vector3 to = Flatten(player.position - transform.position);
            if (to.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to.normalized), 10f * Time.deltaTime);
        }

        private void DoAttack()
        {
            if (variant == LeakVariant.Spawner)
            {
                SpawnFragment();
                SpawnFragment();
            }

            float radius = variant == LeakVariant.Boss ? 3.2f : 1.6f;
            Vector3 origin = transform.position + transform.forward * 1.1f;
            Collider[] hits = Physics.OverlapSphere(origin, radius);
            bool struck = false;
            for (int i = 0; i < hits.Length; i++)
            {
                ThirdPersonController mover = hits[i].GetComponentInParent<ThirdPersonController>();
                if (mover == null) continue;
                if (mover.IsInvulnerable) continue;
                SystemIntegrity hp = mover.GetComponent<SystemIntegrity>();
                hp?.Damage(attackDamage);
                mover.AddImpulse((mover.transform.position - transform.position).normalized * 4f);
                mover.GetComponent<PlayerAnimBridge>()?.PlayHit();
                struck = true;
            }

            if (struck)
            {
                CombatFeedback.Instance?.Shake(variant == LeakVariant.Boss ? 0.22f : 0.1f);
                CombatFeedback.PlayTone(transform.position, 90f, 0.12f);
            }
        }

        private void Blink()
        {
            Vector3 away = Flatten(transform.position - player.position);
            if (away.sqrMagnitude < 0.01f) away = UnityEngine.Random.insideUnitSphere;
            away.y = 0f;
            transform.position += away.normalized * 6f + Vector3.up * 0.05f;
        }

        private void EnterState(LeakCombatState next, float duration)
        {
            combatState = next;
            stateUntil = Time.time + duration;
        }

        private void TickVisual()
        {
            if (bodyMaterial == null && renderers != null && renderers.Length > 0)
            {
                bodyMaterial = renderers[0].material;
            }

            if (bodyMaterial == null) return;

            Color healthy = restColor;
            Color critical = new Color(0.7f, 0.05f, 0.9f);
            Color c = Color.Lerp(healthy, critical, 1f - NormalizedHealth);
            if (enraged) c = Color.Lerp(c, Color.white, 0.25f);
            if (combatState == LeakCombatState.Telegraph)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 14f);
                c = Color.Lerp(c, Color.white, pulse);
            }

            RuntimeMaterials.ApplyColor(bodyMaterial, c);
            RuntimeMaterials.ApplyEmission(bodyMaterial, c * (combatState == LeakCombatState.Telegraph ? 2.2f : 1.1f));
        }

        private void UpdateEnrage()
        {
            if (variant != LeakVariant.Boss || enraged || integrity == null) return;
            if (integrity.Normalized <= enrageIntegrityThreshold)
            {
                enraged = true;
                NullBorn.Demo.TutorialDirector.Instance?.ShowRaw("ENRAGE: el Núcleo Corrupto acelera la fuga de memoria.", 5f);
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
            Material mat = RuntimeMaterials.Lit(new Color(1f, 0.55f, 0.1f), new Color(1f, 0.35f, 0.05f) * 1.2f);
            renderer.material = mat;

            Collider col = cube.GetComponent<Collider>();
            col.isTrigger = true;
            Rigidbody rb = cube.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

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
            CombatFeedback.Instance?.Flash(renderers ?? GetComponentsInChildren<Renderer>(), Color.white);
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
            else
            {
                CombatVfx.PlayPulse(transform.position, 2.2f, new Color(1f, 0.3f, 0.7f), new Color(1f, 0.2f, 0.8f) * 2f);
            }

            int xp = variant == LeakVariant.Boss ? 80 : variant == LeakVariant.Spawner ? 35 : variant == LeakVariant.NullRef ? 30 : 18;
            PlayerProgression progression = FindAnyObjectByType<PlayerProgression>();
            progression?.AddXp(xp, "leak");

            NullBorn.RPG.Quest.QuestManager quests = NullBorn.RPG.Quest.QuestManager.Instance;
            quests?.NotifyKill(variant);

            OnDied?.Invoke(this);
            Destroy(gameObject);
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v;
        }
    }
}

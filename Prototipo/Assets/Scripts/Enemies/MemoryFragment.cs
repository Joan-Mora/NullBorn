using UnityEngine;

namespace NullBorn.Enemies
{
    /// <summary>
    /// Fragmento de RAM. Puede explotar si no se purga; Onda de Purga los encadena.
    /// </summary>
    public class MemoryFragment : MonoBehaviour
    {
        [SerializeField] private float lifetime = 14f;
        [SerializeField] private float explodeDamage = 6f;
        [SerializeField] private float explodeRadius = 2.2f;

        private MemoryLeakEnemy owner;
        private float born;

        public void SetOwner(MemoryLeakEnemy leakOwner)
        {
            owner = leakOwner;
            born = Time.time;
        }

        private void Update()
        {
            transform.Rotate(20f * Time.deltaTime, 40f * Time.deltaTime, 0f);
            if (lifetime > 0f && Time.time - born >= lifetime)
            {
                Detonate();
            }
        }

        public void Collect()
        {
            if (owner != null) owner.NotifyFragmentCollected(this);
            NullBorn.RPG.Quest.QuestManager.Instance?.NotifyCollect("fragment");
            Destroy(gameObject);
        }

        private void Detonate()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explodeRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                NullBorn.Player.ThirdPersonController mover = hits[i].GetComponentInParent<NullBorn.Player.ThirdPersonController>();
                if (mover == null || mover.IsInvulnerable) continue;
                NullBorn.Core.SystemIntegrity integrity = mover.GetComponent<NullBorn.Core.SystemIntegrity>();
                integrity?.Damage(explodeDamage);
            }

            Collect();
        }
    }
}

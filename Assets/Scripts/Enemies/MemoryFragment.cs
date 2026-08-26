using UnityEngine;

namespace NullBorn.Enemies
{
    /// <summary>
    /// ISOMORFISMO: Bloque de memoria no liberada (basura).
    /// El GC los destruye para liberar RAM visual del entorno.
    /// </summary>
    public class MemoryFragment : MonoBehaviour
    {
        private MemoryLeakEnemy owner;

        public void SetOwner(MemoryLeakEnemy leakOwner)
        {
            owner = leakOwner;
        }

        public void Collect()
        {
            if (owner != null)
            {
                owner.NotifyFragmentCollected(this);
            }

            Destroy(gameObject);
        }
    }
}

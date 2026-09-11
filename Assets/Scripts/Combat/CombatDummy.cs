using System.Collections;
using UnityEngine;

namespace NullBorn.Game.Combat
{
    /// <summary>
    /// ArtTest dummy — tag Dummy / layer Enemy. Simple HP + URP color flash on hit.
    /// </summary>
    public class CombatDummy : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly Color FlashColor = new Color(1.4f, 0.35f, 0.2f, 1f);

        [SerializeField] float maxHp = 3f;
        [SerializeField] float flashSeconds = 0.12f;

        Renderer[] renderers;
        MaterialPropertyBlock block;
        float hp;
        bool dead;
        Coroutine flashRoutine;

        public float Hp => hp;
        public bool IsDead => dead;

        void Awake()
        {
            hp = maxHp;
            renderers = GetComponentsInChildren<Renderer>(true);
            block = new MaterialPropertyBlock();
        }

        /// <returns>true if this hit applied damage (used for hitstop).</returns>
        public bool ReceiveHit(float damage)
        {
            if (dead) return false;

            hp -= Mathf.Max(0.01f, damage);
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash());

            if (hp <= 0f)
            {
                dead = true;
                hp = 0f;
                Debug.Log("[NullBorn][Combat] Dummy down (HP 0).");
            }
            else
            {
                Debug.Log("[NullBorn][Combat] Dummy hit — HP " + hp + "/" + maxHp);
            }

            return true;
        }

        IEnumerator Flash()
        {
            ApplyFlash(true);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.04f, flashSeconds));
            ApplyFlash(false);
            flashRoutine = null;
        }

        void ApplyFlash(bool on)
        {
            if (renderers == null) return;
            for (var i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                if (on)
                {
                    r.GetPropertyBlock(block);
                    block.SetColor(BaseColorId, FlashColor);
                    block.SetColor(ColorId, FlashColor);
                    r.SetPropertyBlock(block);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
        }
    }
}

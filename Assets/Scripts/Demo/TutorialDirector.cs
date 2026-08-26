using System.Collections.Generic;
using UnityEngine;

namespace NullBorn.Demo
{
    /// <summary>
    /// Mensajes pedagógicos contextuales (no quizzes).
    /// </summary>
    public class TutorialDirector : MonoBehaviour
    {
        public static TutorialDirector Instance { get; private set; }

        [System.Serializable]
        public class Tip
        {
            public string id;
            [TextArea] public string message;
            public float duration = 5f;
        }

        [SerializeField] private List<Tip> tips = new List<Tip>();

        private string currentMessage = string.Empty;
        private float hideAt;
        private readonly HashSet<string> shown = new HashSet<string>();

        public string CurrentMessage => Time.unscaledTime < hideAt ? currentMessage : string.Empty;

        private void Awake()
        {
            Instance = this;
            if (tips.Count == 0)
            {
                tips.AddRange(DefaultTips());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(string id, bool once = true)
        {
            if (once && shown.Contains(id)) return;

            Tip tip = tips.Find(t => t.id == id);
            if (tip == null)
            {
                currentMessage = id;
                hideAt = Time.unscaledTime + 4f;
                shown.Add(id);
                return;
            }

            shown.Add(id);
            currentMessage = tip.message;
            hideAt = Time.unscaledTime + tip.duration;
        }

        public void ShowRaw(string message, float duration = 4f)
        {
            currentMessage = message;
            hideAt = Time.unscaledTime + duration;
        }

        private static IEnumerable<Tip> DefaultTips()
        {
            yield return new Tip
            {
                id = "boot_welcome",
                duration = 6f,
                message = "Eres un THREAD. Mantén la INTEGRIDAD DEL SISTEMA. WASD mover · Mouse mirar."
            };
            yield return new Tip
            {
                id = "boot_cpu",
                duration = 6f,
                message = "Las acciones gastan CICLOS DE CPU. Si te quedas sin CPU, espera a que se regeneren."
            };
            yield return new Tip
            {
                id = "boot_gc",
                duration = 7f,
                message = "E / G = GARBAGE COLLECTION: libera fragmentos de memoria y restaura integridad."
            };
            yield return new Tip
            {
                id = "boot_debug",
                duration = 6f,
                message = "Click / F = DEPURAR: daña el Memory Leak (cuesta CPU)."
            };
            yield return new Tip
            {
                id = "heap_tradeoff",
                duration = 7f,
                message = "HEAP CORRUPTO: elige entre depurar leaks o limpiar basura. Malgastar CPU te deja vulnerable."
            };
            yield return new Tip
            {
                id = "kernel_boss",
                duration = 7f,
                message = "KERNEL CORE: elimina el LEAK BOSS. Si la integridad baja, entrará en ENRAGE."
            };
        }
    }
}

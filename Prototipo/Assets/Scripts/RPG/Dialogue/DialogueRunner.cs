using System;
using NullBorn.RPG.Quest;
using NullBorn.UI;
using UnityEngine;

namespace NullBorn.RPG.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string label;
        public string nextId;
        public string questNotifyId;
        public int creditDelta;
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        [TextArea] public string text;
        public DialogueChoice[] choices;
        public string nextId;
        public string questTalkId;
    }

    /// <summary>
    /// Diálogo lineal con 2–3 opciones. Reutiliza el panel de tutorial del HUD.
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        public static DialogueRunner Instance { get; private set; }

        public bool IsOpen { get; private set; }

        private DialogueNode[] nodes;
        private DialogueNode current;
        private Action onClosed;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Play(DialogueNode[] tree, Action closed = null)
        {
            if (tree == null || tree.Length == 0) return;
            nodes = tree;
            onClosed = closed;
            IsOpen = true;
            Time.timeScale = 0.0001f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Show(tree[0]);
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Pick(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Pick(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) Pick(2);
            else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (current == null || current.choices == null || current.choices.Length == 0)
                {
                    Advance(current != null ? current.nextId : null);
                }
            }
        }

        private void Show(DialogueNode node)
        {
            current = node;
            if (current == null)
            {
                Close();
                return;
            }

            if (!string.IsNullOrEmpty(current.questTalkId))
            {
                QuestManager.Instance?.NotifyTalk(current.questTalkId);
            }

            string body = current.text;
            if (current.choices != null && current.choices.Length > 0)
            {
                body += "\n";
                for (int i = 0; i < current.choices.Length; i++)
                {
                    body += $"\n[{i + 1}] {current.choices[i].label}";
                }
            }
            else
            {
                body += "\n\n[E] continuar";
            }

            CanvasHUD hud = FindAnyObjectByType<CanvasHUD>();
            hud?.SetDialogue(body);
        }

        private void Pick(int index)
        {
            if (current?.choices == null || index < 0 || index >= current.choices.Length) return;
            DialogueChoice c = current.choices[index];
            if (c.creditDelta != 0)
            {
                FindAnyObjectByType<NullBorn.RPG.Economy.CpuCredits>()?.Add(c.creditDelta);
            }

            if (!string.IsNullOrEmpty(c.questNotifyId))
            {
                QuestManager.Instance?.NotifyTalk(c.questNotifyId);
            }

            Advance(c.nextId);
        }

        private void Advance(string nextId)
        {
            if (string.IsNullOrEmpty(nextId))
            {
                Close();
                return;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].id == nextId)
                {
                    Show(nodes[i]);
                    return;
                }
            }

            Close();
        }

        private void Close()
        {
            IsOpen = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            FindAnyObjectByType<CanvasHUD>()?.SetDialogue(string.Empty);
            onClosed?.Invoke();
        }
    }
}

using NullBorn.RPG.Quest;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.World
{
    /// <summary>
    /// Carga dungeons aditivos. Si la escena no está en Build Settings, usa greybox in-world.
    /// </summary>
    public class AdditiveDungeonLoader : MonoBehaviour
    {
        public static AdditiveDungeonLoader Instance { get; private set; }

        private GameObject worldRoot;
        private Vector3 returnPos;
        private string loadedScene;
        private bool inDungeon;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Enter(string sceneName, Vector3 returnPosition)
        {
            AdditiveDungeonLoader loader = Instance ?? Create();
            loader.EnterInternal(sceneName, returnPosition);
        }

        public static void EnterInWorldBoss(Vector3 bossArena)
        {
            GameObject player = GameObject.Find("Player_Thread");
            if (player == null) return;
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = bossArena;
            if (cc != null) cc.enabled = true;
        }

        public static void Exit()
        {
            Instance?.ExitInternal();
        }

        private static AdditiveDungeonLoader Create()
        {
            GameObject go = new GameObject("DungeonLoader");
            return go.AddComponent<AdditiveDungeonLoader>();
        }

        private void EnterInternal(string sceneName, Vector3 returnPosition)
        {
            returnPos = returnPosition;
            worldRoot = GameObject.Find("WorldRoot");
            loadedScene = sceneName;

            if (SceneInBuild(sceneName))
            {
                if (worldRoot != null) worldRoot.SetActive(false);
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += _ =>
                {
                    inDungeon = true;
                    MovePlayer(new Vector3(0f, 1.2f, 0f));
                };
            }
            else
            {
                inDungeon = true;
                Vector3 fallback = sceneName.Contains("Stack") ? new Vector3(-90f, 1.2f, 40f) : new Vector3(-8f, 1.2f, 92f);
                DungeonLayoutBuilder.BuildInline(sceneName, fallback);
                MovePlayer(fallback);
                Debug.Log($"Dungeon {sceneName} no está en Build Settings — greybox in-world.");
            }
        }

        private void ExitInternal()
        {
            if (!string.IsNullOrEmpty(loadedScene) && SceneInBuild(loadedScene))
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }

            if (worldRoot != null) worldRoot.SetActive(true);
            MovePlayer(returnPos + Vector3.back * 3f);
            QuestManager.Instance?.NotifyDungeonCleared(loadedScene);
            inDungeon = false;
            loadedScene = null;
        }

        private static void MovePlayer(Vector3 pos)
        {
            GameObject player = GameObject.Find("Player_Thread");
            if (player == null) return;
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = pos;
            if (cc != null) cc.enabled = true;
        }

        private static bool SceneInBuild(string name)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name) return true;
            }

            return false;
        }

        public bool IsInDungeon => inDungeon;
    }
}

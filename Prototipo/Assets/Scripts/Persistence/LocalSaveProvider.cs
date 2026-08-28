using System.IO;
using UnityEngine;

namespace NullBorn.Persistence
{
    public class LocalSaveProvider
    {
        public const string FileName = "nullborn_save.json";

        public string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public bool Exists => File.Exists(Path);

        public void Write(SaveData data)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        }

        public SaveData Read()
        {
            if (!Exists) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
        }

        public void Delete()
        {
            if (Exists) File.Delete(Path);
        }
    }
}

using UnityEngine;

namespace NullBorn.World
{
    [CreateAssetMenu(fileName = "Region", menuName = "NullBorn/Region")]
    public class RegionDefinition : ScriptableObject
    {
        public RegionId id;
        public string displayName;
        public Color ambientColor = new Color(0.32f, 0.36f, 0.44f);
        public Color fogColor = new Color(0.05f, 0.07f, 0.1f);
        public float fogDensity = 0.008f;
        public float ramPressureBias;
        public string musicId;

        public static RegionDefinition CreateRuntime(RegionId id, string name, Color ambient, Color fog, float density, float ramBias)
        {
            RegionDefinition def = CreateInstance<RegionDefinition>();
            def.id = id;
            def.displayName = name;
            def.ambientColor = ambient;
            def.fogColor = fog;
            def.fogDensity = density;
            def.ramPressureBias = ramBias;
            return def;
        }
    }
}

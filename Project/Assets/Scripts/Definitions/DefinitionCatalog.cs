using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "DefinitionCatalog", menuName = "Project06/Definitions/Catalog")]
    public class DefinitionCatalog : ScriptableObject
    {
        // 플레이어 Definition
        public PlayerDefinition player;
    }
}

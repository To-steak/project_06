using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "DefinitionCatalog", menuName = "Project06/Definitions/Catalog")]
    public class DefinitionCatalog : ScriptableObject
    {
        // 플레이어 Definition
        public PlayerDefinition player;
        // 쉘터 Definition
        public ShelterDefinition shelter;
        // 모든 무기 Definition. 엔티티는 무기를 이 배열의 인덱스로 가리킨다.
        public WeaponDefinition[] weapons;
        // 모든 구경 Definition. 엔티티는 구경을 이 배열의 인덱스로 가리킨다.
        public CaliberDefinition[] calibers;
    }
}

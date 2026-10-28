using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "PlayerDefinition", menuName = "Project06/Definitions/Player")]
    public class PlayerDefinition : ScriptableObject
    {
        // 플레이어 생성 시 엔티티에 복사할 플레이어 수치
        public PlayerComponent player;
        // 플레이어 생성 시 엔티티에 복사할 이동 수치
        public PlayerMovementComponent movement;
        // 플레이어 생성 시 장착할 무기. DefinitionCatalog.weapons에 들어 있어야 한다.
        public WeaponDefinition startWeapon;
    }
}

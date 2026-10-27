using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "PlayerDefinition", menuName = "Project06/Definitions/Player")]
    public class PlayerDefinition : ScriptableObject
    {
        // 플레이어 생성 시 엔티티에 복사할 이동 수치
        public PlayerMovementComponent movement;
    }
}

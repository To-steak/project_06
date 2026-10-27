using Project06.Components;
using UnityEngine;

namespace Project06.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        // 게임 시작 시 플레이어 엔티티에 복사할 이동 수치
        public PlayerMovementComponent movement = new PlayerMovementComponent { moveSpeed = 5f };
    }
}

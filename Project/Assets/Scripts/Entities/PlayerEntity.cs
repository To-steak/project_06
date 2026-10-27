using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct PlayerEntity
    {
        // 살아 있는지 여부
        public bool isAlive;
        // 직전 틱의 위치. View가 보간할 때 쓴다.
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // XZ 평면의 이동 방향. 길이는 0~1이다.
        public Vector3 moveDirection;
        // 이동 수치
        public PlayerMovementComponent movement;
    }
}

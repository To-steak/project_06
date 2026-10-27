using UnityEngine;

namespace Project06.Commands
{
    public struct MoveCommand
    {
        // XZ 평면의 입력 방향. 길이는 0~1이다.
        public Vector3 direction;
        // 달리기 입력을 누르고 있는지 여부
        public bool run;
    }
}

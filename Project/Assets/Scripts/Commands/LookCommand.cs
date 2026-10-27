using UnityEngine;

namespace Project06.Commands
{
    public struct LookCommand
    {
        // 플레이어에서 마우스가 가리키는 지면 지점으로 향하는 XZ 평면의 방향. 정규화하지 않은 값이며, 길이가 0이면 방향을 바꾸지 않는다.
        public Vector3 direction;
    }
}

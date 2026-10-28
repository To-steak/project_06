using UnityEngine;

namespace Project06.Commands
{
    public struct FireCommand
    {
        // 발사 입력을 누르고 있는지 여부
        public bool fire;
        // 투사체가 출발할 총구의 월드 위치
        public Vector3 origin;
        // 총구에서 마우스가 가리키는 총구 높이 지점으로 향하는 XZ 평면의 방향. 정규화하지 않은 값이며, 길이가 0이면 바라보는 방향으로 쏜다.
        public Vector3 direction;
    }
}

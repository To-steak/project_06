using UnityEngine;

namespace Project06.Commands
{
    public struct FireCommand
    {
        // 발사 입력을 누르고 있는지 여부
        public bool fire;
        // 투사체가 출발할 총구의 월드 위치
        public Vector3 origin;
    }
}

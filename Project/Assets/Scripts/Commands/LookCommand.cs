using UnityEngine;

namespace Project06.Commands
{
    public struct LookCommand
    {
        // 조준점을 구했는지 여부. false면 바라보는 방향을 바꾸지 않는다.
        public bool hasTarget;
        // 마우스가 가리키는 지점을 총구 높이의 수평면에 투영한 월드 위치. 플레이어는 총구에서 뻗은 앞쪽 직선이 이 점을 지나도록 돈다.
        public Vector3 targetPosition;
        // 플레이어 기준 총구의 로컬 위치. X는 오른쪽, Z는 앞쪽이 양수다.
        public Vector3 muzzleOffset;
    }
}

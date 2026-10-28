using UnityEngine;

namespace Project06.Entities
{
    public struct ProjectileEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 발사한 무기의 DefinitionCatalog.weapons 인덱스. View가 프리팹을 고를 때 쓴다.
        public int weaponIndex;
        // 직전 틱의 위치. View가 보간할 때 쓴다.
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // 속도(m/s)
        public Vector3 velocity;
        // 사라질 때까지 남은 비행 거리(m)
        public float remainingDistance;
        // 한 발의 데미지
        public float damage;
    }
}

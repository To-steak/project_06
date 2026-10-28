using UnityEngine;

namespace Project06.Requests
{
    public struct ProjectileSpawnRequest
    {
        // 발사한 무기의 DefinitionCatalog.weapons 인덱스
        public int weaponIndex;
        // 투사체가 생성될 월드 위치
        public Vector3 position;
        // 투사체 속도(m/s)
        public Vector3 velocity;
        // 투사체 사거리(m)
        public float range;
        // 한 발의 데미지
        public float damage;
    }
}

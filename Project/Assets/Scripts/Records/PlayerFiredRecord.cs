using UnityEngine;

namespace Project06.Records
{
    public struct PlayerFiredRecord
    {
        // 발사한 무기의 DefinitionCatalog.weapons 인덱스
        public int weaponIndex;
        // 투사체가 출발한 월드 위치
        public Vector3 position;
        // 발사 방향. 길이는 1이다.
        public Vector3 direction;
    }
}

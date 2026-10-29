using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct ShelterEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 쉘터의 월드 위치
        public Vector3 position;
        // 쉘터 수치
        public ShelterComponent shelter;
        // 남은 체력
        public float health;
        // 파괴되었는지 여부. 체력이 0 이하가 되면 true가 된다. 슬롯은 살아 있지만 적의 목표가 아니며 맞지 않는다.
        public bool isDestroyed;
    }
}

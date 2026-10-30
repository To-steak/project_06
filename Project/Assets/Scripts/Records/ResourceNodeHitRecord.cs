using Project06.Entities;

namespace Project06.Records
{
    public struct ResourceNodeHitRecord
    {
        // 플레이어의 근접 공격에 맞은 자원 노드
        public EntityIdentifier node;
        // 이 타격으로 체력이 0이 되어 다 캤는지 여부
        public bool isDepleted;
    }
}

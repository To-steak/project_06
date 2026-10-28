using Project06.Entities;

namespace Project06.Records
{
    public struct EnemyAttackStartedRecord
    {
        // 공격을 시작한 적
        public EntityIdentifier enemy;
        // 시작한 공격의 enemy.attacks 인덱스
        public int attackIndex;
    }
}

using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Project06/Definitions/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        // 적 생성 시 엔티티에 복사할 적 수치
        public EnemyComponent enemy;
        // 적을 표시할 프리팹. 피벗이 발밑이고 앞쪽(Z+)이 바라보는 방향을 향한다.
        public GameObject enemyPrefab;
        // 공격별로 시작할 때 켤 Animator Trigger 이름. enemy.attacks와 같은 순서이며, 비어 있는 항목은 애니메이션을 재생하지 않는다.
        public string[] attackTriggerNames;
        // 원거리 공격 투사체를 표시할 프리팹. 이 종류의 모든 원거리 공격이 함께 쓴다. 앞쪽(Z+)이 날아가는 방향을 향한다. 원거리 공격이 없으면 비워 둔다.
        public GameObject projectilePrefab;
    }
}

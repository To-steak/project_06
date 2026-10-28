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
    }
}

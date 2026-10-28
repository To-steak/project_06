using Project06.Definitions;
using UnityEngine;

namespace Project06.Authoring
{
    // 적의 시작 위치와 바라보는 방향을 씬에 표시한다. 수치는 definition이 가리키는 EnemyDefinition이 가진다.
    public class EnemyAuthoring : MonoBehaviour
    {
        // 이 위치에 생성할 적의 종류
        public EnemyDefinition definition;
    }
}

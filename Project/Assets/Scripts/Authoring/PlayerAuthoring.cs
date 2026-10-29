using Project06.Definitions;
using UnityEngine;

namespace Project06.Authoring
{
    // 플레이어 시작 위치와 바라보는 방향을 씬에 표시한다. 수치와 프리팹은 definition이 가리키는 PlayerDefinition이 가진다.
    public class PlayerAuthoring : MonoBehaviour
    {
        // 이 위치에 생성할 플레이어의 수치
        public PlayerDefinition definition;
    }
}

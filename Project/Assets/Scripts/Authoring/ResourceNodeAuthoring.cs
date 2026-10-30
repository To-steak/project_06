using Project06.Definitions;
using UnityEngine;

namespace Project06.Authoring
{
    // 나무나 바위 같은 자원 노드의 위치를 씬에 표시한다. 수치는 definition이 가리키는 ResourceNodeDefinition이 가진다.
    // 모습은 이 오브젝트에 붙인 모델을 그대로 쓰며, 같은 오브젝트의 ResourceNodeView가 연출한다.
    public class ResourceNodeAuthoring : MonoBehaviour
    {
        // 이 위치에 생성할 자원 노드의 수치
        public ResourceNodeDefinition definition;
    }
}

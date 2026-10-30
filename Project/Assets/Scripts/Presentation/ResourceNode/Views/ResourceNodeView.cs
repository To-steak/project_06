using UnityEngine;

namespace Project06.Presentation.ResourceNode.Views
{
    // 씬에 놓인 자원 노드 오브젝트에 ResourceNodeAuthoring과 함께 붙인다. 다 캐면 이 오브젝트를 끄고, 재생하면 다시 켠다.
    public class ResourceNodeView : MonoBehaviour
    {
        // 근접 공격에 맞았을 때 하나를 무작위로 골라 이 자원 노드 위치에서 재생할 타격음. 어떤 무기로 맞아도 같다. 다 캔 타격에서는 재생하지 않는다. 비어 있으면 재생하지 않는다.
        public AudioClip[] hitClips;
        // 마지막 타격으로 다 캤을 때 타격음 대신 하나를 무작위로 골라 이 자원 노드 위치에서 재생할 파괴 소리. 비어 있으면 재생하지 않는다.
        public AudioClip[] depletedClips;
    }
}

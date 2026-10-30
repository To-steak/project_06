using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "ResourceNodeDefinition", menuName = "Project06/Definitions/Resource Node")]
    public class ResourceNodeDefinition : ScriptableObject
    {
        // 자원 노드 생성 시 엔티티에 복사할 수치
        public ResourceNodeComponent node;
    }
}

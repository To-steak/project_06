using Project06.Components;

namespace Project06.Records
{
    public struct ResourceNodeHitRecord
    {
        // 자원 노드를 맞힌 근접 무기의 DefinitionCatalog.weapons 인덱스
        public int weaponIndex;
        // 맞은 자원 노드의 종류
        public ResourceNodeKind kind;
    }
}

namespace Project06.Records
{
    public struct PlayerMeleeSwungRecord
    {
        // 휘두른 근접 무기의 DefinitionCatalog.weapons 인덱스. 자원 노드를 맞힌 공격은 이 기록 대신 ResourceNodeHitRecord를 남긴다.
        public int weaponIndex;
    }
}

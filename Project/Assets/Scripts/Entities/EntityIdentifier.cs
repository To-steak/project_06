namespace Project06.Entities
{
    public struct EntityIdentifier
    {
        // 엔티티 저장소 배열의 슬롯 인덱스
        public int index;
        // 슬롯이 재사용된 횟수. 저장소의 엔티티와 다르면 이미 소멸한 엔티티를 가리킨다.
        public int generation;
    }
}

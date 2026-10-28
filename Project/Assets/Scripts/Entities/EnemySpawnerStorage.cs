using System.Collections.Generic;

namespace Project06.Entities
{
    public class EnemySpawnerStorage
    {
        // 적 스포너 슬롯 배열. 길이가 동시에 존재할 수 있는 적 스포너의 최대 수다.
        public EnemySpawnerEntity[] entities;
        // 비어 있는 슬롯 인덱스 목록
        public List<int> freeIndices;

        /// <summary>
        /// 모든 슬롯이 빈 저장소를 만든다.
        /// </summary>
        /// <param name="capacity">동시에 존재할 수 있는 적 스포너의 최대 수.</param>
        public EnemySpawnerStorage(int capacity)
        {
            entities = new EnemySpawnerEntity[capacity];
            freeIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                freeIndices.Add(index);
            }
        }
    }
}

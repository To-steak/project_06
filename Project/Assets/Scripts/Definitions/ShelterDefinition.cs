using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "ShelterDefinition", menuName = "Project06/Definitions/Shelter")]
    public class ShelterDefinition : ScriptableObject
    {
        // 쉘터 생성 시 엔티티에 복사할 쉘터 수치
        public ShelterComponent shelter;
    }
}

using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "CaliberDefinition", menuName = "Project06/Definitions/Caliber")]
    public class CaliberDefinition : ScriptableObject
    {
        // 판 시작 시 플레이어가 가진 이 구경의 예비 탄약 수
        public int startReserveAmmo;
    }
}

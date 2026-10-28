using UnityEngine;

namespace Project06.Presentation.Player.Views
{
    public class PlayerView : MonoBehaviour
    {
        // 플레이어 모델의 Animator
        public Animator animator;
        // 장착한 총을 붙일 손의 Transform
        public Transform weaponSocket;
        // 총을 쥐는 오른손 손가락 본. 순서는 Index1~3, Middle1~3, Ring1~3, Pinky1~3, Thumb1~3이며 WeaponView.rightHandFingerTargets와 같아야 한다.
        public Transform[] rightHandFingers;
    }
}

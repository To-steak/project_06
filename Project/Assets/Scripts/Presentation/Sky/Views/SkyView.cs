using UnityEngine;

namespace Project06.Presentation.Sky.Views
{
    public class SkyView : MonoBehaviour
    {
        // 해 Directional Light의 Transform. Super Simple Skybox의 Sun 컴포넌트가 붙어 있으며, 이 방향으로 하늘의 해 위치와 빛 세기가 정해진다.
        public Transform sun;
        // 달 Directional Light의 Transform. Super Simple Skybox의 Moon 컴포넌트가 붙어 있으며, 이 방향으로 하늘의 달 위치와 빛 세기가 정해진다.
        public Transform moon;
    }
}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Project06.Definitions;
using Project06.Entities;
using UnityEngine;

namespace Project06.Core
{
    // 화면 디버그 표시. 에디터와 개발 빌드에서만 컴파일된다.
    public partial class Runner
    {
        // 디버그 표시 영역의 화면 가장자리 여백과 크기(픽셀)
        private const float DEBUG_GUI_MARGIN = 10f;
        private const float DEBUG_GUI_WIDTH = 320f;
        private const float DEBUG_GUI_HEIGHT = 140f;

        // 디버그용으로 플레이어의 무기·탄약·재장전·행동 상태를 화면 왼쪽 위에 표시한다.
        private void OnGUI()
        {
            int playerIndex = _world != null ? FindAlivePlayerIndex() : -1;
            if (playerIndex < 0)
            {
                return;
            }

            ref PlayerEntity player = ref _world.players.entities[playerIndex];
            WeaponDefinition[] weapons = _definitionCatalog != null ? _definitionCatalog.weapons : null;
            CaliberDefinition[] calibers = _definitionCatalog != null ? _definitionCatalog.calibers : null;

            string weaponName = weapons != null && player.weaponIndex >= 0 && player.weaponIndex < weapons.Length && weapons[player.weaponIndex] != null
                ? weapons[player.weaponIndex].name
                : "없음";
            string caliberName = calibers != null && player.caliberIndex >= 0 && player.caliberIndex < calibers.Length && calibers[player.caliberIndex] != null
                ? calibers[player.caliberIndex].name
                : "없음";
            int reserveAmmo = player.reserveAmmo != null && player.caliberIndex >= 0 && player.caliberIndex < player.reserveAmmo.Length
                ? player.reserveAmmo[player.caliberIndex]
                : 0;
            string reloadText = player.isReloading
                ? $"재장전 중 {player.weapon.reloadTicks - player.reloadRemainingTicks} / {player.weapon.reloadTicks} 틱"
                : "재장전 안 함";

            GUILayout.BeginArea(new Rect(DEBUG_GUI_MARGIN, DEBUG_GUI_MARGIN, DEBUG_GUI_WIDTH, DEBUG_GUI_HEIGHT), GUI.skin.box);
            GUILayout.Label($"무기: {weaponName} ({caliberName})");
            GUILayout.Label($"탄창: {player.magazineAmmo} / {player.weapon.magazineCapacity}");
            GUILayout.Label($"예비 탄약: {reserveAmmo}");
            GUILayout.Label(reloadText);
            GUILayout.Label($"조준 {player.isAiming} · 발사 {player.isFiring} · 달리기 {player.isRunning}");
            GUILayout.EndArea();
        }
    }
}
#endif

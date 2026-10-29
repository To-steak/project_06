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
        private const float DEBUG_GUI_HEIGHT = 180f;

        // 디버그용으로 플레이어의 체력·무기·탄약·재장전·행동 상태와 첫 쉘터의 체력을 화면 왼쪽 위에 표시한다.
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
            GUILayout.Label(player.isGettingUp
                ? $"일어나는 중 · 조작까지 {player.getUpRemainingTicks} 틱"
                : player.isDead
                    ? $"사망 · 부활까지 {player.respawnRemainingTicks} 틱"
                    : $"체력: {player.health} / {player.player.maxHealth}");
            GUILayout.Label($"무기: {weaponName} ({caliberName})");
            GUILayout.Label($"탄창: {player.magazineAmmo} / {player.weapon.magazineCapacity}");
            GUILayout.Label($"예비 탄약: {reserveAmmo}");
            GUILayout.Label(reloadText);
            GUILayout.Label($"조준 {player.isAiming} · 발사 {player.isFiring} · 달리기 {player.isRunning}");
            GUILayout.Label(GetShelterText());
            GUILayout.EndArea();
        }

        // 살아 있는 첫 쉘터의 체력이나 파괴 여부를 글로 만든다. 쉘터가 없으면 없다고 쓴다.
        private string GetShelterText()
        {
            ShelterEntity[] shelters = _world.shelters.entities;
            for (int index = 0; index < shelters.Length; index++)
            {
                ref ShelterEntity shelter = ref shelters[index];
                if (shelter.isAlive)
                {
                    return shelter.isDestroyed ? "쉘터: 파괴됨" : $"쉘터 체력: {shelter.health} / {shelter.shelter.maxHealth}";
                }
            }

            return "쉘터: 없음";
        }
    }
}
#endif

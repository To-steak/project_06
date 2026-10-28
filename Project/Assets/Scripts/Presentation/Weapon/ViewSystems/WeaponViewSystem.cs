using System.Collections.Generic;
using Project06.Entities;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Weapon.Views;
using Project06.Records;
using UnityEngine;

namespace Project06.Presentation.Weapon.ViewSystems
{
    public static class WeaponViewSystem
    {
        /// <summary>
        /// 무기별 총 프리팹과 총 View 목록을 준비한다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="weaponPrefabs">무기별 총 프리팹. DefinitionCatalog.weapons와 같은 순서이며 null이 섞여 있어도 된다.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] weaponPrefabs, ViewContext context)
        {
            context.weaponPrefabs = weaponPrefabs;
            context.weaponViews = new WeaponView[weaponPrefabs.Length];
            context.equippedWeaponIndex = -1;
            context.equippedWeaponView = null;
        }

        /// <summary>
        /// 플레이어가 장착한 무기가 바뀌었으면 손의 총을 바꾸고, 이번 프레임에 발사 기록이 있으면 총구 섬광과 발사음을 한 번 재생한다.
        /// 총 View는 무기마다 처음 장착할 때 한 번만 생성하고, 이후에는 켜고 끄기만 한다.
        /// </summary>
        /// <param name="player">총을 든 플레이어.</param>
        /// <param name="playerFiredRecords">이번 프레임에 쌓인 플레이어 발사 기록.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, List<PlayerFiredRecord> playerFiredRecords, ViewContext context)
        {
            if (context.playerView == null || !player.isAlive)
            {
                return;
            }

            if (player.weaponIndex != context.equippedWeaponIndex)
            {
                if (context.equippedWeaponView != null)
                {
                    context.equippedWeaponView.gameObject.SetActive(false);
                }

                context.equippedWeaponIndex = player.weaponIndex;
                context.equippedWeaponView = GetOrCreateWeaponView(player.weaponIndex, context.playerView, context);
                if (context.equippedWeaponView != null)
                {
                    context.equippedWeaponView.gameObject.SetActive(true);
                }
            }

            WeaponView weaponView = context.equippedWeaponView;
            if (weaponView == null || playerFiredRecords.Count == 0)
            {
                return;
            }

            if (weaponView.muzzleFlash != null)
            {
                weaponView.muzzleFlash.Play();
            }

            if (weaponView.fireAudioSource != null)
            {
                weaponView.fireAudioSource.Play();
            }
        }

        // 무기의 총 View를 돌려준다. 아직 없으면 손 소켓 아래에 생성해 보관한다. 만들 수 없으면 null을 돌려준다.
        private static WeaponView GetOrCreateWeaponView(int weaponIndex, PlayerView playerView, ViewContext context)
        {
            if (weaponIndex < 0 || weaponIndex >= context.weaponViews.Length)
            {
                return null;
            }

            WeaponView weaponView = context.weaponViews[weaponIndex];
            if (weaponView != null)
            {
                return weaponView;
            }

            GameObject prefab = context.weaponPrefabs[weaponIndex];
            if (prefab == null || playerView.weaponSocket == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, playerView.weaponSocket, false);
            weaponView = instance.GetComponent<WeaponView>();
            if (weaponView == null)
            {
                Debug.LogError($"{prefab.name} 프리팹의 루트에 WeaponView가 없다.");
                Object.Destroy(instance);
                return null;
            }

            AlignGripToSocket(weaponView);
            context.weaponViews[weaponIndex] = weaponView;
            return weaponView;
        }

        // 소켓 바로 아래에 붙은 총의 로컬 위치·회전을 바꿔, grip이 소켓과 같은 위치·회전에 오게 한다.
        private static void AlignGripToSocket(WeaponView weaponView)
        {
            Transform grip = weaponView.grip;
            if (grip == null)
            {
                return;
            }

            Transform root = weaponView.transform;
            Quaternion gripRotationInRoot = Quaternion.Inverse(root.rotation) * grip.rotation;
            Vector3 gripPositionInRoot = Vector3.Scale(root.localScale, root.InverseTransformPoint(grip.position));

            Quaternion rootLocalRotation = Quaternion.Inverse(gripRotationInRoot);
            root.localRotation = rootLocalRotation;
            root.localPosition = -(rootLocalRotation * gripPositionInRoot);
        }
    }
}

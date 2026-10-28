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
        private static readonly int FIRE_PARAMETER_HASH = Animator.StringToHash(FIRE_PARAMETER);
        private static readonly int IS_SLIDE_LOCKED_PARAMETER_HASH = Animator.StringToHash(IS_SLIDE_LOCKED_PARAMETER);

        // 총 Animator에서 발사한 프레임에 켜는 Trigger. 마지막 탄을 쏴서 슬라이드가 멈출 때는 켜지 않는다.
        private const string FIRE_PARAMETER = "Fire";
        // 총 Animator에서 슬라이드가 뒤로 젖혀진 채 멈춰 있는지 여부(Bool). 탄창이 비어 있는 동안 true이며, 재장전이 끝나 탄창이 채워지면 false가 된다.
        private const string IS_SLIDE_LOCKED_PARAMETER = "IsSlideLocked";

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
        /// 플레이어가 장착한 무기가 바뀌었으면 손의 총을 바꾸고, 총 Animator에 슬라이드 멈춤 여부를 넘기며, 장전 중이면 장전 진행 틱에 맞춰 장전음을 재생한다.
        /// 이번 프레임에 발사 기록이 있으면 총 Animator의 발사 애니메이션, 총구 섬광, 발사음을 한 번 재생하고, 쏜 발 수만큼 탄피를 지연 시간 뒤에 내보낸다.
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
                context.pendingShellEjectionTimes.Clear();
                context.equippedWeaponView = GetOrCreateWeaponView(player.weaponIndex, context.playerView, context);
                if (context.equippedWeaponView != null)
                {
                    context.equippedWeaponView.gameObject.SetActive(true);
                }
            }

            WeaponView weaponView = context.equippedWeaponView;
            if (weaponView == null)
            {
                return;
            }

            bool isSlideLocked = player.magazineAmmo <= 0;
            bool hasFired = playerFiredRecords.Count > 0;
            if (weaponView.animator != null)
            {
                weaponView.animator.SetBool(IS_SLIDE_LOCKED_PARAMETER_HASH, isSlideLocked);
                if (hasFired && !isSlideLocked)
                {
                    weaponView.animator.SetTrigger(FIRE_PARAMETER_HASH);
                }
            }

            PlayReloadSounds(in player, weaponView, context);
            ScheduleShellEjections(playerFiredRecords.Count, weaponView, context);
            EjectDueShells(weaponView, context);

            if (!hasFired)
            {
                return;
            }

            if (weaponView.muzzleFlash != null)
            {
                // 이전 화염이 아직 재생 중이면 Play가 무시되므로, 지우고 처음부터 다시 재생한다.
                weaponView.muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                weaponView.muzzleFlash.Play(true);
            }

            PlayFireSound(weaponView);
        }

        // 이번 프레임에 쏜 발 수만큼 탄피 배출 시각을 예약한다. 발사 후 shellEjectionDelayTicks만큼 지난 시각이다.
        private static void ScheduleShellEjections(int firedCount, WeaponView weaponView, ViewContext context)
        {
            if (weaponView.shellEjection == null)
            {
                return;
            }

            float ejectionTime = Time.time + weaponView.shellEjectionDelayTicks * Time.fixedDeltaTime;
            for (int shotIndex = 0; shotIndex < firedCount; shotIndex++)
            {
                context.pendingShellEjectionTimes.Add(ejectionTime);
            }
        }

        // 배출 시각이 지난 탄피를 한 개씩 내보낸다.
        private static void EjectDueShells(WeaponView weaponView, ViewContext context)
        {
            List<float> ejectionTimes = context.pendingShellEjectionTimes;
            for (int ejectionIndex = ejectionTimes.Count - 1; ejectionIndex >= 0; ejectionIndex--)
            {
                if (ejectionTimes[ejectionIndex] > Time.time)
                {
                    continue;
                }

                ejectionTimes.RemoveAt(ejectionIndex);
                if (weaponView.shellEjection != null)
                {
                    weaponView.shellEjection.Emit(1);
                }
            }
        }

        // 지난 프레임 이후 장전 경과 틱이 지나친 장전음을 재생한다. 한 프레임에 여러 틱이 지나도 빠뜨리지 않는다.
        // 장전이 끝나거나 취소되면 기록을 지워, 다음 장전에서 처음부터 다시 재생한다.
        private static void PlayReloadSounds(in PlayerEntity player, WeaponView weaponView, ViewContext context)
        {
            if (!player.isReloading)
            {
                context.presentedReloadElapsedTicks = -1;
                return;
            }

            int elapsedTicks = player.weapon.reloadTicks - player.reloadRemainingTicks;
            int previousElapsedTicks = context.presentedReloadElapsedTicks;
            if (elapsedTicks < previousElapsedTicks)
            {
                // 프레임 사이에 장전이 취소되고 다시 시작되었다.
                previousElapsedTicks = -1;
            }

            context.presentedReloadElapsedTicks = elapsedTicks;

            AudioSource audioSource = weaponView.reloadAudioSource;
            AudioClip[] clips = weaponView.reloadClips;
            int[] clipTicks = weaponView.reloadClipTicks;
            if (audioSource == null || clips == null || clipTicks == null)
            {
                return;
            }

            int count = Mathf.Min(clips.Length, clipTicks.Length);
            for (int clipIndex = 0; clipIndex < count; clipIndex++)
            {
                int clipTick = clipTicks[clipIndex];
                if (clipTick > previousElapsedTicks && clipTick <= elapsedTicks && clips[clipIndex] != null)
                {
                    audioSource.PlayOneShot(clips[clipIndex]);
                }
            }
        }

        // 발사음 중 하나를 무작위로 골라 겹쳐 재생한다. 연사로 이전 소리가 끝나기 전에 쏴도 끊기지 않는다.
        private static void PlayFireSound(WeaponView weaponView)
        {
            AudioSource audioSource = weaponView.fireAudioSource;
            if (audioSource == null)
            {
                return;
            }

            AudioClip[] clips = weaponView.fireClips;
            AudioClip clip = clips != null && clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : audioSource.clip;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
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
            SetShellCollisionPlane(weaponView, playerView);
            context.weaponViews[weaponIndex] = weaponView;
            return weaponView;
        }

        // 탄피가 부딪힐 바닥 평면을 플레이어 루트로 지정한다. 플레이어 루트는 늘 발밑에 있고 수평으로만 돌아서 위쪽이 하늘을 향한다.
        private static void SetShellCollisionPlane(WeaponView weaponView, PlayerView playerView)
        {
            if (weaponView.shellEjection == null)
            {
                return;
            }

            ParticleSystem.CollisionModule collision = weaponView.shellEjection.collision;
            collision.SetPlane(0, playerView.transform);
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

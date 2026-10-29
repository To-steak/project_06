using Project06.Entities;
using Project06.Presentation.Sky.Views;
using UnityEngine;

namespace Project06.Presentation.Sky.ViewSystems
{
    public static class SkyViewSystem
    {
        // 해와 달이 한 시간대 동안 하늘을 가로지르는 각도. 지평선에서 떠서 반대편 지평선으로 진다.
        private const float HALF_TURN_DEGREES = 180f;
        // 어두운 정도의 한낮, 노을, 한밤 값
        private const float DAY_DARKNESS = 0f;
        private const float TWILIGHT_DARKNESS = 0.5f;
        private const float NIGHT_DARKNESS = 1f;

        /// <summary>
        /// 하늘 View와 조명 수치를 받아 두고, 판 시작 시 씬에 놓인 해의 수평 방향을 해와 달이 뜨고 지는 방향으로 기록한다. 판 시작 시 한 번 호출한다.
        /// 조명 수치 중 Gradient나 곡선이 null이면 그 항목은 바꾸지 않는다.
        /// </summary>
        /// <param name="skyView">씬에 배치된 하늘 View. 없으면 하늘을 움직이지 않는다.</param>
        /// <param name="twilightTicks">낮과 밤 사이 노을 구간의 길이(틱).</param>
        /// <param name="sunColor">어두운 정도에 따른 해 빛 색.</param>
        /// <param name="sunIntensity">어두운 정도에 따른 해 빛 세기.</param>
        /// <param name="moonColor">달 빛 색.</param>
        /// <param name="moonIntensity">어두운 정도에 따른 달 빛 세기.</param>
        /// <param name="ambientSkyColor">어두운 정도에 따른 위쪽 환경광 색.</param>
        /// <param name="ambientEquatorColor">어두운 정도에 따른 옆쪽 환경광 색.</param>
        /// <param name="ambientGroundColor">어두운 정도에 따른 아래쪽 환경광 색.</param>
        /// <param name="fogColor">어두운 정도에 따른 안개 색.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(
            SkyView skyView, int twilightTicks,
            Gradient sunColor, AnimationCurve sunIntensity, Color moonColor, AnimationCurve moonIntensity,
            Gradient ambientSkyColor, Gradient ambientEquatorColor, Gradient ambientGroundColor, Gradient fogColor,
            ViewContext context)
        {
            context.skyView = skyView;
            float yaw = skyView != null && skyView.sun != null ? skyView.sun.eulerAngles.y : 0f;
            context.skyYawRotation = Quaternion.Euler(0f, yaw, 0f);
            context.skySunLight = skyView != null && skyView.sun != null ? skyView.sun.GetComponent<Light>() : null;
            context.skyMoonLight = skyView != null && skyView.moon != null ? skyView.moon.GetComponent<Light>() : null;
            context.skyTwilightTicks = twilightTicks;
            context.skySunColor = sunColor;
            context.skySunIntensity = sunIntensity;
            context.skyMoonColor = moonColor;
            context.skyMoonIntensity = moonIntensity;
            context.skyAmbientSkyColor = ambientSkyColor;
            context.skyAmbientEquatorColor = ambientEquatorColor;
            context.skyAmbientGroundColor = ambientGroundColor;
            context.skyFogColor = fogColor;
        }

        /// <summary>
        /// 낮에는 해가, 밤에는 달이 시간대가 시작될 때 지평선에서 떠서 끝날 때 반대편 지평선으로 지도록 돌린다. 해와 달은 항상 반대편에 있다.
        /// 시간대의 처음과 끝 노을 구간에서는 노을 색으로 섞으며, 어두운 정도에 따라 해·달 빛 색과 세기, 환경광, 안개 색을 바꾼다.
        /// 틱 사이는 보간해 매끄럽게 바꾼다. 하늘 셰이더 값은 Super Simple Skybox의 Sun·Moon 컴포넌트가 방향을 읽어 정한다.
        /// </summary>
        /// <param name="dayNight">표시할 낮밤.</param>
        /// <param name="interpolation">직전 틱과 현재 틱 사이의 보간 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in DayNightEntity dayNight, float interpolation, ViewContext context)
        {
            SkyView skyView = context.skyView;
            if (skyView == null || !dayNight.isAlive)
            {
                return;
            }

            int phaseTicks = Mathf.Max(1, dayNight.isNight ? dayNight.dayNight.nightTicks : dayNight.dayNight.dayTicks);
            float elapsedTicks = Mathf.Clamp(phaseTicks - dayNight.remainingTicks + interpolation, 0f, phaseTicks);
            PresentRotation(dayNight.isNight, elapsedTicks / phaseTicks, context);
            PresentLighting(GetDarkness(dayNight.isNight, elapsedTicks, phaseTicks, context.skyTwilightTicks), context);
        }

        // 0°면 지평선에서 뜨고, 90°면 바로 위에서 내리쬐고, 180°면 반대편 지평선으로 진다. 180°를 넘으면 지평선 아래다.
        private static void PresentRotation(bool isNight, float progress, ViewContext context)
        {
            SkyView skyView = context.skyView;
            float sunAngle = (isNight ? HALF_TURN_DEGREES : 0f) + progress * HALF_TURN_DEGREES;
            if (skyView.sun != null)
            {
                skyView.sun.rotation = context.skyYawRotation * Quaternion.Euler(sunAngle, 0f, 0f);
            }

            if (skyView.moon != null)
            {
                skyView.moon.rotation = context.skyYawRotation * Quaternion.Euler(sunAngle + HALF_TURN_DEGREES, 0f, 0f);
            }
        }

        // 시간대의 처음과 끝 노을 구간에서는 노을(0.5)과 한낮(0) 또는 한밤(1) 사이를 선형으로 잇고, 그 밖에서는 한낮이나 한밤 값을 돌려준다.
        private static float GetDarkness(bool isNight, float elapsedTicks, int phaseTicks, int twilightTicks)
        {
            float fullDarkness = isNight ? NIGHT_DARKNESS : DAY_DARKNESS;
            float twilightLength = Mathf.Min(twilightTicks, phaseTicks * 0.5f);
            if (twilightLength <= 0f)
            {
                return fullDarkness;
            }

            float edgeTicks = Mathf.Min(elapsedTicks, phaseTicks - elapsedTicks);
            float twilightWeight = 1f - Mathf.Clamp01(edgeTicks / twilightLength);
            return Mathf.Lerp(fullDarkness, TWILIGHT_DARKNESS, twilightWeight);
        }

        // 해와 달 빛은 곡선이 정한 대로 켜고 끈다. 낮밤이 바뀌는 노을(0.5) 앞뒤로 두 곡선이 겹치면 해와 달 빛이 섞이며 넘어간다.
        private static void PresentLighting(float darkness, ViewContext context)
        {
            Light sunLight = context.skySunLight;
            if (sunLight != null)
            {
                if (context.skySunColor != null)
                {
                    sunLight.color = context.skySunColor.Evaluate(darkness);
                }

                if (context.skySunIntensity != null)
                {
                    sunLight.intensity = Mathf.Max(0f, context.skySunIntensity.Evaluate(darkness));
                }
            }

            Light moonLight = context.skyMoonLight;
            // 달 빛 색은 곡선과 함께 받으므로, 곡선이 없으면 색도 받지 않은 것으로 본다.
            if (moonLight != null && context.skyMoonIntensity != null)
            {
                moonLight.color = context.skyMoonColor;
                moonLight.intensity = Mathf.Max(0f, context.skyMoonIntensity.Evaluate(darkness));
            }

            if (context.skyAmbientSkyColor != null)
            {
                RenderSettings.ambientSkyColor = context.skyAmbientSkyColor.Evaluate(darkness);
            }

            if (context.skyAmbientEquatorColor != null)
            {
                RenderSettings.ambientEquatorColor = context.skyAmbientEquatorColor.Evaluate(darkness);
            }

            if (context.skyAmbientGroundColor != null)
            {
                RenderSettings.ambientGroundColor = context.skyAmbientGroundColor.Evaluate(darkness);
            }

            if (context.skyFogColor != null)
            {
                RenderSettings.fogColor = context.skyFogColor.Evaluate(darkness);
            }
        }
    }
}

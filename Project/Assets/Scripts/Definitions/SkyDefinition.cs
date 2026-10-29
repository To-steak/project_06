using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "SkyDefinition", menuName = "Project06/Definitions/Sky")]
    public class SkyDefinition : ScriptableObject
    {
        // 낮과 밤 사이 노을 구간의 길이(틱). 낮의 처음·끝과 밤의 처음·끝이 각각 이만큼 노을 색으로 섞인다. 시간대 길이의 절반을 넘으면 절반으로 본다.
        public int twilightTicks = 1000;

        // 아래 Gradient와 곡선의 가로축은 어두운 정도다. 0이면 한낮, 0.5면 낮밤이 바뀌는 순간의 노을, 1이면 한밤이다.
        [Header("해")]
        // 어두운 정도에 따른 해 빛 색
        [GradientUsage(false)]
        public Gradient sunColor = CreateGradient(new Color(1f, 0.944f, 0.844f), new Color(1f, 0.55f, 0.3f), new Color(1f, 0.55f, 0.3f));
        // 어두운 정도에 따른 해 빛 세기. 노을(0.5) 앞뒤에서 달 빛 세기 곡선과 겹치게 해야 낮밤이 바뀔 때 빛이 끊기지 않는다. 밤 쪽에서는 해가 지평선 아래에 있으니 금방 0으로 내린다.
        public AnimationCurve sunIntensity = new AnimationCurve(new Keyframe(0f, 1.5f), new Keyframe(0.5f, 0.4f), new Keyframe(0.65f, 0f));

        [Header("달")]
        // 달 빛 색
        public Color moonColor = new Color(0.6f, 0.7f, 1f);
        // 어두운 정도에 따른 달 빛 세기. 노을(0.5) 앞뒤에서 해 빛 세기 곡선과 겹치게 해야 낮밤이 바뀔 때 빛이 끊기지 않는다. 낮 쪽에서는 달이 지평선 아래에 있으니 금방 0으로 내린다.
        public AnimationCurve moonIntensity = new AnimationCurve(new Keyframe(0.35f, 0f), new Keyframe(0.5f, 0.3f), new Keyframe(1f, 0.5f));

        [Header("환경광 (Environment Lighting Source가 Gradient일 때)")]
        // 어두운 정도에 따른 위쪽 환경광 색
        [GradientUsage(false)]
        public Gradient ambientSkyColor = CreateGradient(new Color(0.326f, 0.566f, 0.837f), new Color(0.45f, 0.35f, 0.55f), new Color(0.12f, 0.17f, 0.35f));
        // 어두운 정도에 따른 옆쪽 환경광 색
        [GradientUsage(false)]
        public Gradient ambientEquatorColor = CreateGradient(new Color(0.432f, 0.557f, 0.7f), new Color(0.6f, 0.4f, 0.35f), new Color(0.1f, 0.13f, 0.25f));
        // 어두운 정도에 따른 아래쪽 환경광 색
        [GradientUsage(false)]
        public Gradient ambientGroundColor = CreateGradient(new Color(0.604f, 0.604f, 0.604f), new Color(0.25f, 0.2f, 0.2f), new Color(0.06f, 0.07f, 0.12f));

        [Header("안개")]
        // 어두운 정도에 따른 안개 색
        [GradientUsage(false)]
        public Gradient fogColor = CreateGradient(new Color(0.515f, 0.725f, 0.943f), new Color(0.85f, 0.55f, 0.45f), new Color(0.06f, 0.09f, 0.2f));

        // 한낮(0), 노을(0.5), 한밤(1) 세 색을 잇는 Gradient를 만든다.
        private static Gradient CreateGradient(Color dayColor, Color twilightColor, Color nightColor)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(dayColor, 0f),
                    new GradientColorKey(twilightColor, 0.5f),
                    new GradientColorKey(nightColor, 1f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f),
                });
            return gradient;
        }
    }
}

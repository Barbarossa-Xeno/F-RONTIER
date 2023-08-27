using UnityEngine;

namespace Game.Menu
{
    public class LightManager : MonoBehaviour
    {
        ///<summary>点滅させるライト。</summary>
        private new Light light;
        ///<summary>線形アニメーションの設定。</summary>
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.Linear(0, 2f, 4f, 4f);
        ///<summary>アニメーションの周期。</summary>
        private float cycle;
        ///<summary>内部時間の計測に使う。</summary>
        private float time;
        // Start is called before the first frame update
        void Start()
        {
            light = this.GetComponent<Light>();
            if (animationCurve.length < 1)
            {
                return;
            }
            cycle = animationCurve.keys[animationCurve.length - 1].time;
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;

            if (time > cycle)
            {
                time = Mathf.Repeat(time, cycle);
            }

            float intensity = animationCurve.Evaluate(time);

            light.intensity = intensity;
        }
    }
}
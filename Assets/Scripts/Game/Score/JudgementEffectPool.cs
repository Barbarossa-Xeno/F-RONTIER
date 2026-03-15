using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace FRONTIER.Game.Score
{
    /// <summary>
    /// スコアの判定エフェクトを生成するオブジェクトプール。
    /// </summary>
    [RequireComponent(typeof(ScoreManager))]
    public class JudgementEffectPool : MonoBehaviour
    {
        [Header("Prefabs/JudgementEffect のプレハブを登録する"), SerializeField] public JudgementEffects prefabs;

        /// <summary>
        /// エフェクト生成時の親となるトランスフォーム。
        /// </summary>
        [SerializeField] private Transform effectParent;

        private ObjectPool<GameObject> perfect, great, good, bad, miss;

        public ObjectPool<GameObject> Perfect => perfect;
        public ObjectPool<GameObject> Great => great;
        public ObjectPool<GameObject> Good => good;
        public ObjectPool<GameObject> Bad => bad;
        public ObjectPool<GameObject> Miss => miss;

        [System.Serializable]
        public class JudgementEffects
        {
            public GameObject perfect;
            public GameObject great;
            public GameObject good;
            public GameObject bad;
            public GameObject miss;
        }
        private const int MAX_POOL_SIZE = 10;

        void Awake()
        {
            perfect = new ObjectPool<GameObject>
            (
                () => Create(prefabs.perfect),
                target => target.SetActive(true),
                target => target.SetActive(false),
                target => Destroy(target),
                false,
                MAX_POOL_SIZE,
                MAX_POOL_SIZE
            );

            great = new ObjectPool<GameObject>
            (
                () => Create(prefabs.great),
                target => target.SetActive(true),
                target => target.SetActive(false),
                target => Destroy(target),
                false,
                MAX_POOL_SIZE,
                MAX_POOL_SIZE
            );

            good = new ObjectPool<GameObject>
            (
                () => Create(prefabs.good),
                target => target.SetActive(true),
                target => target.SetActive(false),
                target => Destroy(target),
                false,
                MAX_POOL_SIZE,
                MAX_POOL_SIZE
            );

            bad = new ObjectPool<GameObject>
            (
                () => Create(prefabs.bad),
                target => target.SetActive(true),
                target => target.SetActive(false),
                target => Destroy(target),
                false,
                MAX_POOL_SIZE,
                MAX_POOL_SIZE
            );

            miss = new ObjectPool<GameObject>
            (
                () => Create(prefabs.miss),
                target => target.SetActive(true),
                target => target.SetActive(false),
                target => Destroy(target),
                false,
                MAX_POOL_SIZE,
                MAX_POOL_SIZE
            );
        }

        /// <summary>
        /// オブジェクトプールからオブジェクト（エフェクト）を生成するための関数。
        /// </summary>
        /// <param name="prefab">生成するプレハブ</param>
        /// <returns>生成されたオブジェクト</returns>
        private GameObject Create(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab);

            // プレハブ側のコンポーネントにあるプールの参照に自分を渡す
            obj.GetComponent<JudgementEffect>().Effect = this;

            // 親オブジェクト設定など
            obj.transform.SetParent(effectParent);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);

            return obj;
        }
    }
}

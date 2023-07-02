using UnityEngine;
using UnityEngine.Pool;

public class ScoreObjectPool : MonoBehaviour
{
    [SerializeField] public ScoreObjects scoreObjects;
    public ObjectPool<GameObject> perfect, great, good, bad, miss;
    [System.Serializable]
    public class ScoreObjects
    {
        public GameObject perfect;
        public GameObject great;
        public GameObject good;
        public GameObject bad;
        public GameObject miss;
    }
    private const int maxPoolSize = 10;

    void Awake()
    {
        perfect = new ObjectPool<GameObject>(() => Instantiate(scoreObjects.perfect),
                                             target => target.SetActive(!target.activeSelf ? true : true),
                                             target => target.SetActive(target.activeSelf ? false : false),
                                             target => Destroy(target),
                                             false,
                                             maxPoolSize,
                                             maxPoolSize
                                             );
        great = new ObjectPool<GameObject>(() => Instantiate(scoreObjects.great),
                                           target => target.SetActive(!target.activeSelf ? true : true),
                                           target => target.SetActive(target.activeSelf ? false : false),
                                           target => Destroy(target),
                                           false,
                                           maxPoolSize,
                                           maxPoolSize
                                           );
        good = new ObjectPool<GameObject>(() => Instantiate(scoreObjects.good),
                                          target => target.SetActive(!target.activeSelf ? true : true),
                                          target => target.SetActive(target.activeSelf ? false : false),
                                          target => Destroy(target),
                                          false,
                                          maxPoolSize,
                                          maxPoolSize
                                          );
        bad = new ObjectPool<GameObject>(() => Instantiate(scoreObjects.bad),
                                         target => target.SetActive(!target.activeSelf ? true : true),
                                         target => target.SetActive(target.activeSelf ? false : false),
                                         target => Destroy(target),
                                         false,
                                         maxPoolSize,
                                         maxPoolSize
                                         );
        miss = new ObjectPool<GameObject>(() => Instantiate(scoreObjects.miss),
                                          target => target.SetActive(!target.activeSelf ? true : true),
                                          target => target.SetActive(target.activeSelf ? false : false),
                                          target => Destroy(target),
                                          false,
                                          maxPoolSize,
                                          maxPoolSize
                                          );
    }
}

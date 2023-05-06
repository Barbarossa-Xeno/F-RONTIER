using UnityEngine;

public class Status : MonoBehaviour
{
    [SerializeField] StatusType type;
    private enum StatusType{
        perfect, great, good, bad, miss
    }
    Judge judge;
    ScoreObjectPool scoreObjectPool;
    void Start(){
        judge = GameObject.Find("JudgementManager").GetComponent<Judge>();
        scoreObjectPool = GameObject.Find("JudgementManager").GetComponent<ScoreObjectPool>();
    }
    public void Active(){
        switch((int)type){
            case 0:
            scoreObjectPool.perfect.Release(this.gameObject);
            break;
            case 1:
            scoreObjectPool.great.Release(this.gameObject);
            break;
            case 2:
            scoreObjectPool.good.Release(this.gameObject);
            break;
            case 3:
            scoreObjectPool.bad.Release(this.gameObject);
            break;
            case 4:
            scoreObjectPool.miss.Release(this.gameObject);
            break;
            default: return;
        }
    }
}

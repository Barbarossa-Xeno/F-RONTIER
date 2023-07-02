using UnityEngine;

public class CacheObject : MonoBehaviour
{
   public static CacheObject instance {get; private set;}

    void Awake(){
      if(instance == null){
        instance = this;
        DontDestroyOnLoad(this.gameObject);
      }
      else{
        Destroy(this.gameObject);
      }
   }
}
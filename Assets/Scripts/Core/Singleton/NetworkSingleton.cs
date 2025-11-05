using Unity.Netcode;

namespace Core.Singleton
{
    public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        public static T NetInstance;

        protected virtual void Awake()
        {
            if(NetInstance == null)
            {
                NetInstance = GetComponent<T>();
            }else
            {
                if (NetInstance.gameObject.CompareTag("ScoreManager") || NetworkManager.Singleton == null) Destroy(gameObject);
            }
        }
    }
}
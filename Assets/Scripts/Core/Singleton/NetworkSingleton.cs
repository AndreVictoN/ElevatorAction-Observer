using Unity.Netcode;
using UnityEngine;

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
                if (NetworkManager.Singleton == null) Destroy(gameObject);
            }
        }
    }
}
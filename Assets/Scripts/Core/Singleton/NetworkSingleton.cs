using Unity.Netcode;
using UnityEngine;

namespace Core.Singleton
{
    public class NetworkSingleton<T> : NetworkBehaviour where T : MonoBehaviour
    {
        public static T NetInstance;

        protected virtual void Awake()
        {
            if(NetInstance == null)
            {
                NetInstance = GetComponent<T>();
            }else
            {
                Destroy(gameObject);
            }
        }
    }
}
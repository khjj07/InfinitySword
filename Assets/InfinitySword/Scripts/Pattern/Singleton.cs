using UnityEngine;

namespace Assets.InfinitySword.Scripts.Pattern
{
    public class Singleton<T> : MonoBehaviour where T:MonoBehaviour
    {
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = (T)FindObjectOfType<T>();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.StackTrace);
                        return null;
                    }
                }
                return _instance;
            }
        }
        private static T _instance;
    }
}

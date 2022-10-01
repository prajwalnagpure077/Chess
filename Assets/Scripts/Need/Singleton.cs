using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    internal static T Instance;
    public abstract void Awake();
}

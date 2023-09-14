using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _Instance = null;

    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                // Don't need to exist game object before use
                _Instance = FindObjectOfType<T>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(T).Name;
                    _Instance = go.AddComponent<T>();

                    // Persistent across the scenes
                    DontDestroyOnLoad(go);
                }
            }
            return _Instance;
        }
    }

    public virtual void Initialize()
    {

    }

    public virtual void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

using System;
using UnityEngine;

namespace Project.Core
{
public abstract class LazySingleton<T> : MonoBehaviour where T: MonoBehaviour
{
  public static T Instance
  {
    get
    {
      if (_instance == null)
        _instance = CreateSingleton();

      return _instance;
    }
  }

  private static T _instance;

  private static T CreateSingleton()
  {
    /*if (!ApplicationState.IsPlaying)
      return null;*/
    var ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
    DontDestroyOnLoad(ownerObject);
    return ownerObject.AddComponent<T>();
  }

  private void OnDestroy()
  {
    _instance = null;
  }

  private void OnApplicationQuit()
  {
    _instance = null;
    Destroy(gameObject);
  }
}
}

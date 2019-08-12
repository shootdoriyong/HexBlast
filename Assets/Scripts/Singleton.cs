using UnityEngine;
using System.Collections;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {

	private static T m_Instance = null;
	public static T instance
	{
		get
		{
			if(m_Instance == null)
			{
				m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

				if(m_Instance == null)
				{
					GameObject singleton = new GameObject();
					singleton.name = typeof(T).Name;
					m_Instance = singleton.AddComponent(typeof(T)) as T;
				}
			}
			return m_Instance;
		}
	}
}

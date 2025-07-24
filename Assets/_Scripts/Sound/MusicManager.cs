using System;
using UnityEngine;

namespace _Scripts.Sound
{
    public class MusicManager : MonoBehaviour
    {
        private static MusicManager _instance;
        private void Awake()
        {
            if (_instance is null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

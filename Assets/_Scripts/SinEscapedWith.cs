
using System;
using UnityEngine;

namespace _Scripts
{
    public class SinEscapedWith : MonoBehaviour
    {
        public int sinHeldOnEscape;
        public int sinLeftInLevelOnEscape;
        
        #region Singleton

        public static SinEscapedWith Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(SinEscapedWith)) as SinEscapedWith;

                return _instance;
            }
            set { _instance = value; }
        }

        private static SinEscapedWith _instance;

        #endregion

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
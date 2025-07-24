using UnityEngine;
using TMPro;

namespace _Scripts.Menus
{
    public class EscapeScreenMenu : MonoBehaviour
    {
        public TextMeshProUGUI sinEscapedWithText;
        public TextMeshProUGUI sinLeftText; 
        
        private void Start()
        {
            sinEscapedWithText.text = "You've Escaped With " + SinEscapedWith.Instance.sinHeldOnEscape +
                                      " Sin.";
            sinLeftText.text = "You Have " + SinEscapedWith.Instance.sinLeftInLevelOnEscape +
                               " Left To Collect.";
        }

        public void ReturnToBank()
        {
            LevelLoader.Instance.LoadLevel(LevelLoader.Instance.map);
        }

        public void MainMenu()
        {
            LevelLoader.Instance.LoadLevel(LevelLoader.Instance.menu);
        }
    }
}

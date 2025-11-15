using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _quicktimeEventPanel;
    [SerializeField] private GameObject _quicktimeEventProgressPanel;
    [SerializeField] private GameObject _quicktimeEventProgressMeter;
    [SerializeField] private GameObject _quicktimeEventTimeLeftMeter;
    [SerializeField] private GameObject _pauseMenuDefaultButton;
    [SerializeField] private GameObject _deathScreenDefaultButton;
    [SerializeField] private GameObject _deathPanel;

    public GameObject QuicktimeEventPanel => _quicktimeEventPanel;
    public GameObject QuicktimeEventProgressPanel => _quicktimeEventProgressPanel;
    public GameObject QuicktimeEventProgressMeter => _quicktimeEventProgressMeter;
    public GameObject QuicktimeEventTimeLeftMeter => _quicktimeEventTimeLeftMeter;

    private void Start()
    {
        SetPauseMenuFocus();
    }

    public void ShowDeathPanel()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_deathScreenDefaultButton);
        }

        _deathPanel.SetActive(true);
    }

    public void HideDeathPanel()
    {
        _deathPanel.SetActive(false);
    }

    public void SetPauseMenuFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_pauseMenuDefaultButton);
        }
    }

    public void ShowQuicktimeEvent()
    {
        _quicktimeEventPanel.SetActive(true);
    }

    public void HideQuicktimeEvent()
    {
        _quicktimeEventPanel.SetActive(false);
    }
}

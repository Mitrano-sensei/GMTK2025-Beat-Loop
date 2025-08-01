using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseAction;

    [SerializeField] private GameObject pauseMenu;

    void Start()
    {
        pauseAction.action.performed += _ => pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
}
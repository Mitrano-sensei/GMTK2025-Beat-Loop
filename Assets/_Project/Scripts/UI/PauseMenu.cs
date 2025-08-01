using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseAction;

    [SerializeField] private GameObject pauseMenu;

    void Start()
    {
        pauseAction.action.performed += PauseInput();
    }

    private void OnDestroy()
    {
        pauseAction.action.performed -= PauseInput();
    }

    private Action<InputAction.CallbackContext> PauseInput()
    {
        return _ => pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
}
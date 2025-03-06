using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class inputManager : MonoBehaviour
{
    public static inputManager instance;

    public bool menuOpenCloseInput {get; private set;}

    private PlayerInput _playerInput;

    private InputAction _menuOpenCloseAction;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        
        _playerInput = GetComponent<PlayerInput>();
        _menuOpenCloseAction = _playerInput.actions["menuOpenClose"];
    }

    private void Update()
    {
        menuOpenCloseInput = _menuOpenCloseAction.WasPressedThisFrame();
    }
    
}

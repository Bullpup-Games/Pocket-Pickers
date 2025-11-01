using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameManager _gameManager;

    private void Awake()
    {
        ServiceLocator.Instance.Register(_inputHandler);
        ServiceLocator.Instance.Register(_gameManager);

        // _gameManager.Initialize();
        // _inputHandler.Initialize();
    }
}

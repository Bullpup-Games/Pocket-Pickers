using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Player;
using UnityEngine;

public class TutorialArrowFlip : MonoBehaviour
{
    private bool _facingBank = true;
    private float _towardsBankPos = -27.3f;
    private float _towardsExitPos = -43.86f;


    private void Update()
    {
        if (PlayerVariables.Instance is null) return;

        var sinHeld = PlayerVariables.Instance.sinHeld;

        if (sinHeld == 0 && !_facingBank)
        {
            FaceTowardsBank();
            return;
        }

        if (sinHeld != 0 && _facingBank)
        {
            FaceTowardsExit();
        }
    }

    private void FaceTowardsBank()
    {
        transform.position = new Vector3(_towardsBankPos, -7.9f, 0f);
        transform.rotation = Quaternion.Euler(0, 0, -90);
        _facingBank = true;
    }

    private void FaceTowardsExit()
    {
        transform.position = new Vector3(_towardsExitPos, -7.9f, 0f);
        transform.rotation = Quaternion.Euler(0, 0, 90);
        _facingBank = false;
    }
}

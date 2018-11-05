using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class ArrowPadCharacterController : MonoBehaviour {

    public ArrowPad ArrowPad;

    private ThirdPersonCharacter _character;
    private Vector3 _characterMove = Vector3.zero;
	
    void Start()
    {
		_character = GetComponent<ThirdPersonCharacter>();

        ArrowPad.ButtonHeld += ControlCharacter;
        ArrowPad.ButtonReleased += (btn) =>
        {
			//Debug.LogFormat("Button Released: {0}", btn.ToString());

            switch (btn)
            {
                case ArrowPad.ButtonType.Up:
                case ArrowPad.ButtonType.Down:
                    _characterMove.z = 0f;
                    break;
                case ArrowPad.ButtonType.Left:
                case ArrowPad.ButtonType.Right:
                    _characterMove.x = 0f;
                    break;
            }
        };
    }

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
        //Debug.LogFormat("Character Move: {0}", _characterMove);
        _character.Move(_characterMove, false, false);
	}

    private void ControlCharacter(ArrowPad.ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ArrowPad.ButtonType.Up:
                if (_characterMove.z < 1.0f) _characterMove.z += 1.0f * Time.deltaTime;
                break;
            case ArrowPad.ButtonType.Down:
                if (_characterMove.z > -1.0f) _characterMove.z -= 1.0f * Time.deltaTime;
                break;
            case ArrowPad.ButtonType.Left:
                if (_characterMove.x < 1.0f) _characterMove.x += 1.0f * Time.deltaTime;
                break;
            case ArrowPad.ButtonType.Right:
                if (_characterMove.x > -1.0f) _characterMove.x -= 1.0f * Time.deltaTime;
                break;
        }
    }
}

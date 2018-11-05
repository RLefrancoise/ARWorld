using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ArrowPad : MonoBehaviour {

	[SerializeField]
	private Button upButton;

    [SerializeField]
	private Button downButton;

    [SerializeField]
	private Button leftButton;

    [SerializeField]
	private Button rightButton;

	public enum ButtonType {
		Up,
		Down,
		Left,
		Right
	}

	private Dictionary<ButtonType, bool> PressedButtons = new Dictionary<ButtonType, bool>
	{
		{ButtonType.Up, false},
		{ButtonType.Down, false},
		{ButtonType.Left, false},
		{ButtonType.Right, false}
	};

	public Action<ButtonType> ButtonPressed;
	public Action<ButtonType> ButtonReleased;
	public Action<ButtonType> ButtonHeld;

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void Awake()
	{
		/*PressedButtons = new Dictionary<ButtonType, bool>
		{
			{ButtonType.Up, false},
			{ButtonType.Down, false},
			{ButtonType.Left, false},
			{ButtonType.Right, false}
		};*/
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		if(ButtonHeld == null) return;

		foreach(var btn in PressedButtons.Keys)
		{
			if(PressedButtons[btn]) ButtonHeld.Invoke(btn);
		}
	}

	private void ArrowPressed(ButtonType buttonType, bool pressed)
	{
		PressedButtons[buttonType] = pressed;
		if(pressed && ButtonPressed != null) ButtonPressed.Invoke(buttonType);
		else if(!pressed && ButtonReleased != null) ButtonReleased.Invoke(buttonType);
	}

	public bool IsPressed(ButtonType buttonType) {
		return PressedButtons[buttonType];
	}

	public void UpPressed() {
		ArrowPressed(ButtonType.Up, true);
	}

	public void UpReleased() {
        ArrowPressed(ButtonType.Up, false);
	}

	public void DownPressed() {
        ArrowPressed(ButtonType.Down, true);
	}

	public void DownReleased() {
        ArrowPressed(ButtonType.Down, false);
	}

	public void LeftPressed() {
        ArrowPressed(ButtonType.Left, true);
	}

	public void LeftReleased() {
        ArrowPressed(ButtonType.Left, false);
	}

	public void RightPressed() {
        ArrowPressed(ButtonType.Right, true);
	}

	public void RightReleased() {
        ArrowPressed(ButtonType.Right, false);
	}
}

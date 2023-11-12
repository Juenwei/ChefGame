using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCharacterSelectionUI : MonoBehaviour
{
	[SerializeField] private Button characterSelectionButton;

	private void Awake()
	{
		characterSelectionButton.onClick.AddListener(() =>
		{
			CharacterSelectionReady.Instance.SetPlayerReady();
		});
	}
}

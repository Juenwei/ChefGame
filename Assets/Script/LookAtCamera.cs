using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private enum LookAtOption
	{
		LookAt,
		LookAtInverted,
		CameraForward,
		CameraForwardInverted
	}

	[SerializeField] private LookAtOption lookAtOption;

	private void LateUpdate()
	{
		switch(lookAtOption)
		{
			case LookAtOption.LookAt:
				transform.LookAt(Camera.main.transform);
				break;

			case LookAtOption.LookAtInverted:
				var dirFromCamera = transform.position - Camera.main.transform.position;
				transform.LookAt(transform.position + dirFromCamera);
				break;

			case LookAtOption.CameraForward:
				transform.forward = Camera.main.transform.forward;
				break;

			case LookAtOption.CameraForwardInverted:
				transform.forward = -Camera.main.transform.forward;
				break;
		}

	}
}

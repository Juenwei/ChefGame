using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
	private const string CUT = "Cut";

	[SerializeField] private CuttingCounter cuttingCounter;

	Animator animator;
	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	void Start()
	{
		cuttingCounter.OnCut += CuttingCounter_OnCut;
	}

	private void CuttingCounter_OnCut(object sender, EventArgs e)
	{
		animator.SetTrigger(CUT);
	}
}

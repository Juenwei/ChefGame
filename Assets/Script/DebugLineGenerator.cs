using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DebugLineGenerator : NetworkBehaviour
{
	private LineRenderer m_LineRenderer;
	private Vector3[] m_Positions;
	private Material m_LineMaterial;

	[Tooltip("The default state of the netcode debug render lines")]
	[SerializeField]
	private bool m_EnableDebugLines = true;

	[Tooltip("The length of the line")]
	public float LineLength = 4.0f;

	[Tooltip("The width of the line")]
	public float LineWidth = 0.3f;

	private void Awake()
	{
		// Add a line renderer component
		m_LineRenderer = gameObject.AddComponent<LineRenderer>();

		// Switch to a legacy built-in shader that will work with just colors and/or a color gradient
		m_LineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

		// Assign this new material to the line renderer
		m_LineRenderer.material = m_LineMaterial;

		// Create our start color for the gradient
		var colorKeyStart = new GradientColorKey();
		colorKeyStart.color = Color.red;
		// The "start" should be 0.0f
		colorKeyStart.time = 0.0f;

		// Create our end color for the gradient
		var colorKeyEnd = new GradientColorKey();
		colorKeyEnd.color = Color.blue;
		// The "end" should be 1.0f
		colorKeyEnd.time = 1.0f;

		// Now create and apply the gradient
		m_LineRenderer.colorGradient = new Gradient()
		{
			colorKeys =
			new GradientColorKey[2] { colorKeyStart, colorKeyEnd },
			mode = GradientMode.Blend
		};

		//declare two positions for the start and end of the line
		m_Positions = new Vector3[2];

		//Ensure that the line renderer knows to draw in world space
		m_LineRenderer.useWorldSpace = true;

		//set the beginning and ending width of the line in units
		m_LineRenderer.startWidth = 0.3f;
		m_LineRenderer.endWidth = 0.3f;
	}

	public override void OnDestroy()
	{
		// With NetworkBehaviours, always make sure to invoke the
		// base.OnDestroy method
		base.OnDestroy();

		// We should always destroy any runtime materials we create
		if (m_LineMaterial != null)
		{
			Destroy(m_LineMaterial);
		}
	}

	/// <summary>
	/// Update the position of the line
	/// </summary>
	private void Update()
	{
		if (IsSpawned && m_EnableDebugLines)
		{
			//set the start and end positions of our line
			m_Positions[0] = transform.position;
			m_Positions[1] = transform.position + Vector3.up * LineLength;

			//feed the values to the line renderer
			m_LineRenderer.SetPositions(m_Positions);
		}
	}

	/// <summary>
	/// Example for having netcode debug render lines
	/// </summary>
	[ClientRpc]
	private void ToggleDebugLinesClientRpc(bool drawDebugLines)
	{
		m_EnableDebugLines = drawDebugLines;
	}

	/// <summary>
	/// Server-Side only
	/// Enables/Disables the debug line rendering
	/// </summary>
	public void DebugLinesEnabled(bool enableDebugLines)
	{
		if (IsSpawned && IsServer)
		{
			m_EnableDebugLines = enableDebugLines;
			ToggleDebugLinesClientRpc(m_EnableDebugLines);
		}
	}
}

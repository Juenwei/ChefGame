using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer, bodyMeshRenderer;

	private Material material;
	private void Awake()
	{
		//Retrieve the materail data and copy to the new materal clone so that it won't affect other player's material.
		material = new Material(headMeshRenderer.material);

		//Apply the clone material to the mesh ,so that each player visual have individual material (wont affect each other)
		headMeshRenderer.material = material;
		bodyMeshRenderer.material = material;
	}

	public void SetPlayerColor(Color color)
	{
		material.color = color;
	}

}

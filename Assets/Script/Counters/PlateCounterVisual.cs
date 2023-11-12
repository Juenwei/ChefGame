using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private Transform counterTopTransform;
    [SerializeField] private GameObject plateVisualPrefab;
    [SerializeField] private PlateCounter plateCounter;
	private const float plateYOffset = 0.1f;

	private List<GameObject> plateVisualGameObjects;

	private void Awake()
	{
		plateVisualGameObjects = new List<GameObject>();
	}

	private void Start()
	{
		plateCounter.OnPlateSpawn += PlateCounter_OnPlateSpawn;
		plateCounter.OnPlateRemoved += PlateCounter_OnPlateRemoved;
	}

	private void PlateCounter_OnPlateRemoved(object sender, EventArgs e)
	{
		var plateGameObject = plateVisualGameObjects[plateVisualGameObjects.Count - 1];
		plateVisualGameObjects.Remove(plateGameObject);
		Destroy(plateGameObject);
	}

	private void PlateCounter_OnPlateSpawn(object sender, EventArgs e)
	{
		var plateVisualTransform = Instantiate(plateVisualPrefab, counterTopTransform).transform;
		plateVisualTransform.localPosition = new Vector3(0,plateYOffset * plateVisualGameObjects.Count,0);

		plateVisualGameObjects.Add(plateVisualTransform.gameObject);
	}

	
}

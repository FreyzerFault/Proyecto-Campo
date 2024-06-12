﻿using DavidUtils.Spawning;
using UnityEngine;

namespace TreesGeneration
{
	[RequireComponent(typeof(OliveGroveGenerator))]
	public class OliveSpawner : SpawnerBoxInTerrain
	{
		[SerializeField] private Spawneable[] olivoPrefabs;

		private OliveGroveGenerator _generator;

		protected override void Awake()
		{
			base.Awake();
			_generator = GetComponent<OliveGroveGenerator>();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_generator.OnEndedGeneration += HandleOnEndedGeneration;
			_generator.OnRegionPopulated += HandleOnRegionPopulated;
			_generator.OnClear += Clear;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_generator.OnEndedGeneration -= HandleOnEndedGeneration;
			_generator.OnRegionPopulated -= HandleOnRegionPopulated;
			_generator.OnClear -= Clear;
		}

		private void HandleOnRegionPopulated(RegionData data) => Spawn2D(data.olivosInterior);

		// TODO Ya veremos que hago cuando se completa
		private void HandleOnEndedGeneration(RegionData[] data) => Debug.Log("Ended Generation");

		protected override Spawneable InstantiateItem(Spawneable prefab = null) =>
			base.InstantiateItem(prefab ?? GetRandomModel());

		private Spawneable GetRandomModel() => olivoPrefabs[Random.Range(0, olivoPrefabs.Length)];
	}
}

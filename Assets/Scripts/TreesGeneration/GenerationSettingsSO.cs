using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreesGeneration
{
	[CreateAssetMenu(fileName = "Generation Settings", menuName = "Generation/Generation Settings")]
	public class GenerationSettingsSO : ScriptableObject
	{
		public OliveGlobalParams globalParams = new()
		{
			probTraditionalCrop = .5f,
			probIntensiveCrop = .3f,
			probSuperIntensiveCrop = .2f,
			lindeWidth = 12
		};

		#region CROP TYPE

		public CropTypeParams[] cropTypeParams =
		{
			new()
			{
				type = CropType.Traditional,
				// 10x10 - 12x12
				separationMin = new Vector2(10, 10),
				separationMax = new Vector2(12, 12),
				scale = 2
			},
			new()
			{
				type = CropType.Intesive,
				// 3x6 - 6x6
				separationMin = new Vector2(3, 6),
				separationMax = new Vector2(6, 6),
				scale = 1
			},
			new()
			{
				type = CropType.SuperIntesive,
				// 1x4 - 2x4
				separationMin = new Vector2(1, 4),
				separationMax = new Vector2(2, 4),
				scale = .5f
			}
		};

		private Dictionary<CropType, CropTypeParams> _cropTypeParamsDictionary;

		private Dictionary<CropType, CropTypeParams> InitializeCropTypeParamsDictionary() =>
			_cropTypeParamsDictionary =
				new Dictionary<CropType, CropTypeParams>(
					cropTypeParams.Select(
						p =>
							new KeyValuePair<CropType, CropTypeParams>(p.type, p)
					)
				);

		public CropTypeParams GetCropTypeParams(CropType type) =>
			(_cropTypeParamsDictionary ??= InitializeCropTypeParamsDictionary())[type];

		#endregion

		private void Awake() => InitializeCropTypeParamsDictionary();

		private void OnValidate()
		{
			globalParams.NormalizeProbabilities();
			InitializeCropTypeParamsDictionary();
		}
	}
}
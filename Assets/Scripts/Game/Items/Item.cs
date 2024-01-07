
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FoodForTheGods.Items
{
	[CreateAssetMenu(fileName = "Item", menuName = "Food For The Gods/Items/Item")]
	public class Item : SerializedScriptableObject
	{
		public string itemID;
		public string ItemTypeIdentifier = Guid.NewGuid().ToString();

		[field: SerializeField]
		public string DisplayName { get; private set; } = string.Empty;

		[field: SerializeField]
		public string Description { get; private set; } = string.Empty;

		[field: SerializeField]
		public Sprite? Icon { get; private set; } = null;

		[field: SerializeField]
		public GameObject? GroundPrefab { get; private set; } = null;


		private void OnValidate()
		{
#if UNITY_EDITOR
			itemID = this.name;
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}
}
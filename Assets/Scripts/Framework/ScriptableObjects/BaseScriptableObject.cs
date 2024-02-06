
using FoodForTheGods.Framework.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace FoodForTheGods.Framework.ScriptableObjects
{
	public class BaseScriptableObject : ScriptableObject
	{
		/// <summary>
		/// The unique type id of this <see cref="ScriptableObject"/> that is used to identify it.
		/// Each <see cref="ScriptableObject"/> should have it's own unique id, so that it can be
		/// identified and loaded as needed.
		/// </summary>
		[FormerlySerializedAs("UniqueId")]
		[ScriptableObjectId]
		public string UniqueTypeId;
	}
}
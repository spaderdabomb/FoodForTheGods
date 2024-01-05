using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoodForTheGods
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Food For The Gods/Items/ItemData")]
    public class ItemData : SerializedScriptableObject
    {
        [SerializeField] string itemID;

        private void OnValidate()
        {
#if UNITY_EDITOR
            itemID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

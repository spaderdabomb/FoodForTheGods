
#nullable  enable
using System;
using System.Collections.Generic;
using System.Reflection;
using FishNet.Serializing;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FoodForTheGods.Items
{
	[CreateAssetMenu(fileName = "Item", menuName = "Food For The Gods/Items/Item")]
	public class Item : SerializedScriptableObject
	{
		public string UniqueItemTypeGuid = Guid.NewGuid().ToString();

		public string UniqueId { get; set; } = string.Empty;

		[field: SerializeField]
		public string DisplayName { get; private set; } = string.Empty;

		[field: SerializeField]
		public string Description { get; private set; } = string.Empty;

		[field: SerializeField]
		public Sprite Icon { get; private set; } = null;

		[field: SerializeField, Range(1, 999)]
		public uint MaxStackCount { get; private set; } = 1;

		public uint InventoryX { get; set; } = 0;

		public uint InventoryY { get; set; } = 0;

		public uint StackCount { get; set; } = 1;

		public bool IsStackable => MaxStackCount > 1;

		public bool IsStackFull => StackCount >= MaxStackCount;

		public bool SetStackCount(uint stackCount)
		{
			if (stackCount > MaxStackCount)
			{
				Debug.LogWarning($"Cannot set stack count to {stackCount} because it is greater than the max stack count of {MaxStackCount}.");
				return false;
			}

			StackCount = stackCount;
			return true;
		}

		public bool AddToStack(uint stackCount)
		{
			if (StackCount + stackCount > MaxStackCount)
			{
				Debug.LogWarning($"Cannot add {stackCount} to stack because it would exceed the max stack count of {MaxStackCount}.");
				return false;
			}

			StackCount += stackCount;
			return true;
		}

		public bool RemoveFromStack(uint stackCount)
		{
			StackCount -= stackCount;
			StackCount = Math.Clamp(StackCount, 0, MaxStackCount);
			return true;
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return UniqueId.GetHashCode();
		}

		public override bool Equals(object? obj)
		{
			return obj is Item item && UniqueId == item.UniqueId;
		}

		public static bool operator ==(Item? a, Item? b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
			{
				return false;
			}

			return a.UniqueId == b.UniqueId;
		}

		public static bool operator !=(Item? a, Item? b)
		{
			return !(a == b);
		}
	}

	public static class ItemExtensions
	{
		public static readonly Dictionary<string, Item> s_ItemDatabase = new();

		public static Item? GetItem(string uniqueItemTypeGuid)
		{
			if (s_ItemDatabase.Count == 0)
			{
				PopulateItemCache();
			}

			return s_ItemDatabase.GetValueOrDefault(uniqueItemTypeGuid);
		}

		public static Item? GetItemAsInstantiated(string uniqueItemTypeGuid)
		{
			if (GetItem(uniqueItemTypeGuid) is not { } item)
			{
				return null;
			}

			return Object.Instantiate(item);
		}

		public static Item? CloneItem(this Item item)
		{
			return Object.Instantiate(item);
		}

		private static void PopulateItemCache()
		{
			Item[] items = Resources.LoadAll<Item>("ScriptableObjects/Items");

			foreach (Item item in items)
			{
				s_ItemDatabase.Add(item.UniqueItemTypeGuid, item);
			}
		}

		public static void WriteItem(this Writer writer, Item item)
		{
			writer.WriteString(item.UniqueItemTypeGuid);
			writer.WriteString(item.UniqueId);
			writer.WriteUInt32(item.InventoryX);
			writer.WriteUInt32(item.InventoryY);
			writer.WriteUInt32(item.StackCount);

			foreach (PropertyInfo property in item.GetType().GetProperties())
			{
				if (!property.IsDefined(typeof(ItemData), false))
				{
					continue;
				}

				object value = property.GetValue(item);

				if (property.PropertyType == typeof(string))
				{
					writer.WriteString((string)value);
				}
				else if (property.PropertyType == typeof(int))
				{
					writer.WriteInt32((int)value);
				}
				else if (property.PropertyType == typeof(uint))
				{
					writer.WriteUInt32((uint)value);
				}
				else if (property.PropertyType == typeof(long))
				{
					writer.WriteInt64((long)value);
				}
				else if (property.PropertyType == typeof(ulong))
				{
					writer.WriteUInt64((ulong)value);
				}
				else if (property.PropertyType == typeof(float))
				{
					writer.WriteSingle((float)value);
				}
				else if (property.PropertyType == typeof(double))
				{
					writer.WriteDouble((double)value);
				}
				else
				{
					throw new NotImplementedException($"Property type {property.PropertyType} is not supported.");
				}
			}
		}

		public static Item ReadItem(this Reader reader)
		{
			string uniqueItemTypeGuid = reader.ReadString();

			Item? item =  GetItemAsInstantiated(uniqueItemTypeGuid);

			if (item == null)
			{
				throw new Exception($"Item with unique item type guid {uniqueItemTypeGuid} does not exist.");
			}

			item.UniqueId = reader.ReadString();
			item.InventoryX = reader.ReadUInt32();
			item.InventoryY = reader.ReadUInt32();
			item.StackCount = reader.ReadUInt32();

			foreach (PropertyInfo property in item.GetType().GetProperties())
			{
				if (!property.IsDefined(typeof(ItemData), false))
				{
					continue;
				}

				if (property.PropertyType == typeof(string))
				{
					property.SetValue(item, reader.ReadString());
				}
				else if (property.PropertyType == typeof(int))
				{
					property.SetValue(item, reader.ReadInt32());
				}
				else if (property.PropertyType == typeof(uint))
				{
					property.SetValue(item, reader.ReadUInt32());
				}
				else if (property.PropertyType == typeof(long))
				{
					property.SetValue(item, reader.ReadInt64());
				}
				else if (property.PropertyType == typeof(ulong))
				{
					property.SetValue(item, reader.ReadUInt64());
				}
				else if (property.PropertyType == typeof(float))
				{
					property.SetValue(item, reader.ReadSingle());
				}
				else if (property.PropertyType == typeof(double))
				{
					property.SetValue(item, reader.ReadDouble());
				}
				else
				{
					throw new NotImplementedException($"Property type {property.PropertyType} is not supported.");
				}
			}

			return item;
		}
	}
}
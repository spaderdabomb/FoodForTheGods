
#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FoodForTheGods.Framework.Helpers
{
	public readonly struct ResourcePath
	{
		/// <summary>
		/// A <see cref="Dictionary{TKey,TValue}"/> of all resources that have been loaded and cached.
		/// </summary>
		private static readonly Dictionary<ResourcePath, object> m_LoadedResources = new();

		/// <summary>
		/// The path to the resource to load.
		/// </summary>
		private string m_Path { get; }

		public ResourcePath(string path)
		{
			m_Path = path;
		}

		/// <summary>
		/// Loads a singular resource at the given <see cref="m_Path"/> and caches it to the <see cref="m_LoadedResources"/> dictionary.
		/// Any subsequent calls to this method will return the cached resource.
		/// </summary>
		/// <typeparam name="T">The type of resource to load.</typeparam>
		/// <returns>The loaded resource of type <see cref="T"/>. Or null if the resource failed to load.</returns>
		public T? LoadSingle<T>() where T : Object
		{
			if (m_LoadedResources.TryGetValue(this, out var resource))
			{
				// we have already loaded this resource, return it
				return (T)resource;
			}

			T? loadedResource = Resources.Load<T>(m_Path);

			if (loadedResource == null)
			{
				Debug.LogError($"Failed to load resource at path: {m_Path} for type: {typeof(T)}");
				return null;
			}

			m_LoadedResources.Add(this, loadedResource);

			return loadedResource;
		}

		/// <summary>
		/// Loads all resources at the given <see cref="m_Path"/> and caches them to the <see cref="m_LoadedResources"/> dictionary.
		/// </summary>
		/// <typeparam name="T">The type of resource to load.</typeparam>
		/// <returns>The loaded resources of type <see cref="T"/>. Or an empty list if the resources failed to load.</returns>
		public List<T> LoadAll<T>() where T : Object
		{
			if (m_LoadedResources.TryGetValue(this, out var resource))
			{
				// we have already loaded this resource, return it
				return (List<T>)resource;
			}

			List<T> loadedResources = Resources.LoadAll<T>(m_Path).ToList();

			if (loadedResources.Count == 0)
			{
				Debug.LogError($"Failed to load resource at path: {m_Path} for type: {typeof(T)}");
				return new List<T>();
			}

			m_LoadedResources.Add(this, loadedResources);

			return loadedResources;
		}

		/// <summary>
		/// This will load the given resource <see cref="m_Path"/> without caching it to the <see cref="m_LoadedResources"/> dictionary.
		/// Can be useful for fire and forget resources that are only ever used once.
		/// </summary>
		/// <typeparam name="T">The type of resource to load.</typeparam>
		/// <returns>The loaded resource of type <see cref="T"/>. Or null if the resource failed to load.</returns>
		public T? LoadSingleWithoutCaching<T>() where T : Object
		{
			return Resources.Load<T>(m_Path);
		}

		public List<T> LoadAllWithoutCaching<T>() where T : Object
		{
			return Resources.LoadAll<T>(m_Path).ToList();
		}

		/// <summary>
		/// Unloads the resource at the given <see cref="m_Path"/> from the <see cref="m_LoadedResources"/> dictionary.
		/// </summary>
		public void Unload()
		{
			if (!m_LoadedResources.TryGetValue(this, out var resource))
			{
				return;
			}

			if (resource is Object unityObject)
			{
				Resources.UnloadAsset(unityObject);
			}

			m_LoadedResources.Remove(this);
		}

		/// <summary>
		/// Unloads all resources that have been loaded and cached.
		/// </summary>
		public static void UnloadAll()
		{
			foreach (var (resource, _) in m_LoadedResources)
			{
				resource.Unload();
			}
		}

		public override string ToString()
		{
			return $"ResourcePath: {m_Path}, Cached: {m_LoadedResources.ContainsKey(this)}";
		}
	}
}
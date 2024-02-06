
#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FoodForTheGods.Game.Pooling
{
	public class ObjectPooling : IObjectPoolingBuilder, IDisposable
	{
		/// <summary>
		/// The <see cref="GameObject"/> that will be pooled.
		/// </summary>
		public GameObject Prefab { get; }

		/// <summary>
		/// The maximum number of objects that can be pooled.
		/// </summary>
		public UInt32 PoolSize { get; private set; }

		/// <summary>
		/// Whether or not the pool should automatically expand when there's no more objects available in the pool.
		/// </summary>
		public bool ShouldAutoExpand { get; private set; } = false;

		/// <summary>
		/// The default name of the pooled object when it's created.
		/// </summary>
		public string DefaultName { get; private set; } = "PoolableObject";

		/// <summary>
		/// The <see cref="Transform"/> that will be used as the parent of the pooled objects. (if any)
		/// </summary>
		public Transform? Parent { get; private set; } = null;

		/// <summary>
		/// A list of all <see cref="GameObject"/>s that were created and are in the pool.
		/// This isn't used to indicate whether or not an object is available, use <see cref="m_AvailableObjects"/> for that.
		/// </summary>
		private readonly List<GameObject> m_PooledObjects = new();

		/// <summary>
		/// The <see cref="GameObject"/>s that aren't currently in use.
		/// </summary>
		private readonly Queue<GameObject> m_AvailableObjects = new();

		private ObjectPooling(GameObject prefab)
		{
			Prefab = prefab;
		}

		public static ObjectPooling Create(GameObject prefab)
		{
			return new ObjectPooling(prefab);
		}

		public IObjectPoolingBuilder WithParent(Transform parent)
		{
			Parent = parent;
			return this;
		}

		public IObjectPoolingBuilder WithName(string name)
		{
			DefaultName = name;
			return this;
		}

		public IObjectPoolingBuilder WithPoolSize(uint poolSize)
		{
			PoolSize = poolSize;
			return this;
		}

		public IObjectPoolingBuilder WithAutoExpand()
		{
			ShouldAutoExpand = true;
			return this;
		}

		public ObjectPooling Build()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				CreatePooledObject();
			}

			return this;
		}

		/// <summary>
		/// Creates a new <see cref="GameObject"/> and adds it to the pool.
		/// </summary>
		/// <returns></returns>
		private void CreatePooledObject()
		{
			GameObject pooledObject = Object.Instantiate(Prefab, Parent);
			pooledObject.name = $"{DefaultName} ({m_PooledObjects.Count})";
			pooledObject.SetActive(false);

			m_PooledObjects.Add(pooledObject);
			m_AvailableObjects.Enqueue(pooledObject);
		}

		/// <summary>
		/// Tries to get the next available <see cref="GameObject"/> from the pool. If there's no more available objects
		/// if <see cref="ShouldAutoExpand"/> is <see langword="true"/>, it will expand the pool and return a new object.
		/// Otherwise, it will return <see langword="null"/>.
		/// </summary>
		/// <returns>The next available <see cref="GameObject"/> from the pool if there's any, otherwise <see langword="null"/>.</returns>
		public GameObject? Next()
		{
			if (m_AvailableObjects.Count == 0)
			{
				if (ShouldAutoExpand)
				{
					PoolSize++; // increase the pool size as we're expanding it
					Debug.Log($"{this} is expanding the pool for {DefaultName}, new size: {PoolSize}");
					CreatePooledObject();
				}
				else
				{
					Debug.LogWarning($"{this} has no more objects available in the pool for {DefaultName}.");
					return null;
				}
			}

			GameObject nextObject = m_AvailableObjects.Dequeue();
			nextObject.SetActive(true);

			return nextObject;
		}

		/// <summary>
		/// Releases the <see cref="GameObject"/> back into the pool.
		/// </summary>
		/// <param name="pooledObject">The <see cref="GameObject"/> to release back into the pool.</param>
		public void Release(GameObject pooledObject)
		{
			if (!m_PooledObjects.Contains(pooledObject))
			{
				Debug.LogWarning($"{this} is trying to release an object that isn't in the pool for {DefaultName}.");
				return;
			}

			pooledObject.SetActive(false);
			m_AvailableObjects.Enqueue(pooledObject);
		}

		public List<GameObject> GetPooledObjects()
		{
			return m_PooledObjects;
		}

		/// <summary>
		/// The total number of objects remaining in the pool that are available to be used.
		/// </summary>
		public Int32 GetAvailableObjectsCount()
		{
			return m_AvailableObjects.Count;
		}

		public void Dispose()
		{
			// clear the available object pool as we're destroying the pooled objects
			m_AvailableObjects.Clear();

			foreach (GameObject pooledObject in m_PooledObjects)
			{
				// clear the pooled object pool
				Object.Destroy(pooledObject);
			}

			m_PooledObjects.Clear();
		}
	}
}
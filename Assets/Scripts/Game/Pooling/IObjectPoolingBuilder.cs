
using UnityEngine;

namespace FoodForTheGods.Game.Pooling
{
	public interface IObjectPoolingBuilder
	{
		public IObjectPoolingBuilder WithParent(Transform parent);

		public IObjectPoolingBuilder WithName(string name);

		public IObjectPoolingBuilder WithPoolSize(uint poolSize);

		public IObjectPoolingBuilder WithAutoExpand();

		public ObjectPooling Build();
	}
}
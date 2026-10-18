namespace Crisis.Runtime.FullSource
{
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using System.Collections.Generic;

public sealed class DynamicLimbTexturePool
{
	private static DynamicLimbTexturePool instance = null;

	private static readonly object padlock = new object();

	private LinkedList<DynamicLimbTextureData> pool = new LinkedList<DynamicLimbTextureData>();

	public int ActiveTextures { get; private set; }

	public static DynamicLimbTexturePool Instance
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new DynamicLimbTexturePool();
				}
				return instance;
			}
		}
	}

	public DynamicLimbTextureData Create(LimbRenderer owner, int width, int height)
	{
		DynamicLimbTextureData dynamicLimbTextureData = new DynamicLimbTextureData();
		dynamicLimbTextureData.Initialize(owner, width, height);
		pool.AddFirst(dynamicLimbTextureData);
		ActiveTextures += 3;
		return dynamicLimbTextureData;
	}

	public void Update()
	{
		LinkedListNode<DynamicLimbTextureData> linkedListNode = pool.First;
		while (linkedListNode != null)
		{
			LinkedListNode<DynamicLimbTextureData> next = linkedListNode.Next;
			if (!linkedListNode.Value.renderer.IsDynamic)
			{
				linkedListNode.Value.Release();
				pool.Remove(linkedListNode);
				ActiveTextures -= 3;
			}
			linkedListNode = next;
		}
	}

	public void ReleaseAll()
	{
		LinkedListNode<DynamicLimbTextureData> linkedListNode = pool.First;
		while (linkedListNode != null)
		{
			LinkedListNode<DynamicLimbTextureData> next = linkedListNode.Next;
			linkedListNode.Value.Release();
			pool.Remove(linkedListNode);
			ActiveTextures -= 3;
			linkedListNode = next;
		}
	}
}

}

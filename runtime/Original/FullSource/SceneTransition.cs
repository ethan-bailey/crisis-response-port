namespace Crisis.Runtime.FullSource
{
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
	public enum Scene
	{
		Title = 1,
		Transition = 2,
		Mission = 3,
		Marksman = 4,
		Benchmark = 5,
		Editor = 6,
		Campaign = 7
	}

	public Scene targetScene;

	public int TargetScene
	{
		get
		{
			return SceneIndex(targetScene);
		}
	}

	public Scene CurrentScene
	{
		get
		{
			return SceneFromIndex(SceneManager.GetActiveScene().buildIndex);
		}
	}

	public static int SceneIndex(Scene scene)
	{
		return (int)scene;
	}

	public static Scene SceneFromIndex(int index)
	{
		return (Scene)index;
	}
}

}

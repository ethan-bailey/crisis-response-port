namespace Crisis.Runtime.FullSource
{
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour
{
	public enum Status
	{
		Pending = 0,
		Selected = 1,
		Cancelled = 2
	}

	public Text headerText;

	public Button cancelButton;

	public Button selectButton;

	public Transform container;

	public FileBrowserListItem itemPrefab;

	private UnityEventString onSelected = new UnityEventString();

	private UnityEventString onFinished = new UnityEventString();

	private UnityEventVoid onCancelled = new UnityEventVoid();

	private UnityEventVoid onClosed = new UnityEventVoid();

	private UnityEventString onListItemSelected = new UnityEventString();

	public string CurrentDirectory { get; private set; }

	public string CurrentFile { get; private set; }

	public Status CurrentStatus { get; private set; }

	private void Start()
	{
		cancelButton.onClick.AddListener(OnCancelButtonPressed);
		selectButton.onClick.AddListener(OnSelectedButtonPressed);
		onListItemSelected.AddListener(OnListItemSelected);
	}

	public void Open(string path, string filters, string instruction)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		headerText.text = instruction;
		CurrentDirectory = path;
		base.gameObject.SetActive(true);
		CurrentStatus = Status.Pending;
		ClearList();
		string[] array = filters.Split(';');
		string[] array2 = array;
		foreach (string searchPattern in array2)
		{
			string[] files = Directory.GetFiles(CurrentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
			string[] array3 = files;
			foreach (string text in array3)
			{
				FileInfo fileInfo = new FileInfo(text);
				FileAttributes attributes = File.GetAttributes(text);
				FileBrowserListItem fileBrowserListItem = Object.Instantiate(itemPrefab);
				string text2 = fileInfo.Name;
				if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
				{
					text2 += " (Dir)";
				}
				fileBrowserListItem.Initialize(text2, fileInfo.FullName, container, onListItemSelected);
			}
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
		onClosed.Invoke();
	}

	public void SetCallbacks(UnityAction<string> selected, UnityAction<string> finished, UnityAction cancelled, UnityAction closed)
	{
		SetSelectedCallback(selected);
		SetFinishedCallback(finished);
		SetCancelledCallback(cancelled);
		SetClosedCallback(closed);
	}

	public void SetSelectedCallback(UnityAction<string> callback)
	{
		onSelected.RemoveAllListeners();
		if (callback != null)
		{
			onSelected.AddListener(callback);
		}
	}

	public void SetFinishedCallback(UnityAction<string> callback)
	{
		onFinished.RemoveAllListeners();
		if (callback != null)
		{
			onFinished.AddListener(callback);
		}
	}

	public void SetCancelledCallback(UnityAction callback)
	{
		onCancelled.RemoveAllListeners();
		if (callback != null)
		{
			onCancelled.AddListener(callback);
		}
	}

	public void SetClosedCallback(UnityAction callback)
	{
		onClosed.RemoveAllListeners();
		if (callback != null)
		{
			onClosed.AddListener(callback);
		}
	}

	private void ClearList()
	{
		foreach (Transform item in container)
		{
			Object.Destroy(item.gameObject);
		}
	}

	private void OnListItemSelected(string fullPath)
	{
		CurrentFile = fullPath;
		onSelected.Invoke(CurrentFile);
	}

	private void OnSelectedButtonPressed()
	{
		CurrentStatus = Status.Selected;
		onFinished.Invoke(CurrentFile);
		Close();
	}

	private void OnCancelButtonPressed()
	{
		CurrentStatus = Status.Cancelled;
		onCancelled.Invoke();
		Close();
	}

	public static bool FileExists(string path)
	{
		return File.Exists(path);
	}

	public static string ParentPath(string path, bool useForwardSlashes = false)
	{
		string path2 = path.Replace("/", "\\");
		DirectoryInfo directoryInfo = new DirectoryInfo(path2);
		if (useForwardSlashes)
		{
			return directoryInfo.Parent.FullName.Replace("\\", "/");
		}
		return directoryInfo.Parent.FullName;
	}

	public static bool CreateDirectory(string path)
	{
		DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
		return directoryInfo.Exists;
	}

	public static string EditorPath(string subPath, bool useForwardSlashes = false)
	{
		string dataPath = Application.dataPath;
		dataPath = dataPath + "/Editor/" + subPath;
		if (!useForwardSlashes)
		{
			return dataPath.Replace("/", "\\");
		}
		return dataPath;
	}
}

}

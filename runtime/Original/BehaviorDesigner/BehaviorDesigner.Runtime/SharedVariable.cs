using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using Object = UnityEngine.Object;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using System;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime;

public abstract class SharedVariable
{
	[SerializeField]
	private bool mIsShared;

	[SerializeField]
	private bool mIsGlobal;

	[SerializeField]
	private string mName;

	[SerializeField]
	private string mPropertyMapping;

	[SerializeField]
	private GameObject mPropertyMappingOwner;

	[SerializeField]
	private bool mNetworkSync;

	public bool IsShared
	{
		get
		{
			return mIsShared;
		}
		set
		{
			mIsShared = value;
		}
	}

	public bool IsGlobal
	{
		get
		{
			return mIsGlobal;
		}
		set
		{
			mIsGlobal = value;
		}
	}

	public string Name
	{
		get
		{
			return mName;
		}
		set
		{
			mName = value;
		}
	}

	public string PropertyMapping
	{
		get
		{
			return mPropertyMapping;
		}
		set
		{
			mPropertyMapping = value;
		}
	}

	public GameObject PropertyMappingOwner
	{
		get
		{
			return mPropertyMappingOwner;
		}
		set
		{
			mPropertyMappingOwner = value;
		}
	}

	public bool NetworkSync
	{
		get
		{
			return mNetworkSync;
		}
		set
		{
			mNetworkSync = value;
		}
	}

	public bool IsNone => mIsShared && string.IsNullOrEmpty(mName);

	public void ValueChanged()
	{
	}

	public virtual void InitializePropertyMapping(BehaviorSource behaviorSource)
	{
	}

	public abstract object GetValue();

	public abstract void SetValue(object value);
}
public abstract class SharedVariable<T> : SharedVariable
{
	private Func<T> mGetter;

	private Action<T> mSetter;

	[SerializeField]
	protected T mValue;

	public T Value
	{
		get
		{
			return (mGetter == null) ? mValue : mGetter();
		}
		set
		{
			int num;
			if (base.NetworkSync)
			{
				ref T reference = ref mValue;
				object obj = value;
				num = ((!reference.Equals(obj)) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag = (byte)num != 0;
			if (mSetter != null)
			{
				mSetter(value);
			}
			else
			{
				mValue = value;
			}
			if (flag)
			{
				ValueChanged();
			}
		}
	}

	public override void InitializePropertyMapping(BehaviorSource behaviorSource)
	{
		if (!Application.isPlaying || !(behaviorSource.Owner.GetObject() is Behavior) || string.IsNullOrEmpty(base.PropertyMapping))
		{
			return;
		}
		string[] array = base.PropertyMapping.Split('/');
		GameObject gameObject = (object.Equals(base.PropertyMappingOwner, null) ? (behaviorSource.Owner.GetObject() as Behavior).gameObject : base.PropertyMappingOwner);
		if (gameObject == null)
		{
			Debug.LogError("Error: Unable to find GameObject on " + behaviorSource.behaviorName + " for property mapping with variable " + base.Name);
			return;
		}
		Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly(array[0]));
		if (component == null)
		{
			Debug.LogError("Error: Unable to find component on " + behaviorSource.behaviorName + " for property mapping with variable " + base.Name);
			return;
		}
		Type type = component.GetType();
		PropertyInfo property = type.GetProperty(array[1]);
		if (property != null)
		{
			MethodInfo getMethod = property.GetGetMethod();
			if (getMethod != null)
			{
				mGetter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), component, getMethod);
			}
			getMethod = property.GetSetMethod();
			if (getMethod != null)
			{
				mSetter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), component, getMethod);
			}
		}
	}

	public override object GetValue()
	{
		return Value;
	}

	public override void SetValue(object value)
	{
		if (mSetter != null)
		{
			mSetter((T)value);
		}
		else
		{
			mValue = (T)value;
		}
	}

	public override string ToString()
	{
		return (Value != null) ? Value.ToString() : "(null)";
	}
}

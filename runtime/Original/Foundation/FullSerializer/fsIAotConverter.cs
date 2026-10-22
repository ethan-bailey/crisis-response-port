using System;

namespace FullSerializer
{
	public interface fsIAotConverter
	{
		Type ModelType { get; }

		fsAotVersionInfo VersionInfo { get; }
	}
}

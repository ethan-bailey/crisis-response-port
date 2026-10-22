using FullSerializer.Internal;

namespace FullSerializer
{
	public struct fsAotVersionInfo
	{
		public struct Member
		{
			public string MemberName;

			public string JsonName;

			public string StorageType;

			public string OverrideConverterType;

			public Member(fsMetaProperty property)
			{
				MemberName = property.MemberName;
				JsonName = property.JsonName;
				StorageType = property.StorageType.CSharpName(true);
				OverrideConverterType = null;
				if (property.OverrideConverterType != null)
				{
					OverrideConverterType = property.OverrideConverterType.CSharpName();
				}
			}

			public override bool Equals(object obj)
			{
				if (!(obj is Member))
				{
					return false;
				}
				return this == (Member)obj;
			}

			public override int GetHashCode()
			{
				return MemberName.GetHashCode() + 17 * JsonName.GetHashCode() + 17 * StorageType.GetHashCode() + ((!string.IsNullOrEmpty(OverrideConverterType)) ? (17 * OverrideConverterType.GetHashCode()) : 0);
			}

			public static bool operator ==(Member a, Member b)
			{
				return a.MemberName == b.MemberName && a.JsonName == b.JsonName && a.StorageType == b.StorageType && a.OverrideConverterType == b.OverrideConverterType;
			}

			public static bool operator !=(Member a, Member b)
			{
				return !(a == b);
			}
		}

		public bool IsConstructorPublic;

		public Member[] Members;
	}
}

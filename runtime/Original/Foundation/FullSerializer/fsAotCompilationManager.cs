using System;
using System.Collections.Generic;
using System.Text;
using FullSerializer.Internal;

namespace FullSerializer
{
	public class fsAotCompilationManager
	{
		public static HashSet<Type> AotCandidateTypes = new HashSet<Type>();

		private static bool HasMember(fsAotVersionInfo versionInfo, fsAotVersionInfo.Member member)
		{
			fsAotVersionInfo.Member[] members = versionInfo.Members;
			foreach (fsAotVersionInfo.Member member2 in members)
			{
				if (member2 == member)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsAotModelUpToDate(fsMetaType currentModel, fsIAotConverter aotModel)
		{
			if (currentModel.IsDefaultConstructorPublic != aotModel.VersionInfo.IsConstructorPublic)
			{
				return false;
			}
			if (currentModel.Properties.Length != aotModel.VersionInfo.Members.Length)
			{
				return false;
			}
			fsMetaProperty[] properties = currentModel.Properties;
			foreach (fsMetaProperty property in properties)
			{
				if (!HasMember(aotModel.VersionInfo, new fsAotVersionInfo.Member(property)))
				{
					return false;
				}
			}
			return true;
		}

		public static string RunAotCompilationForType(fsConfig config, Type type)
		{
			fsMetaType fsMetaType2 = fsMetaType.Get(config, type);
			fsMetaType2.EmitAotData(true);
			return GenerateDirectConverterForTypeInCSharp(type, fsMetaType2.Properties, fsMetaType2.IsDefaultConstructorPublic);
		}

		private static string EmitVersionInfo(string prefix, Type type, fsMetaProperty[] members, bool isConstructorPublic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("new fsAotVersionInfo {");
			stringBuilder.AppendLine(prefix + "    IsConstructorPublic = " + ((!isConstructorPublic) ? "false" : "true") + ",");
			stringBuilder.AppendLine(prefix + "    Members = new fsAotVersionInfo.Member[] {");
			foreach (fsMetaProperty fsMetaProperty2 in members)
			{
				stringBuilder.AppendLine(prefix + "        new fsAotVersionInfo.Member {");
				stringBuilder.AppendLine(prefix + "            MemberName = \"" + fsMetaProperty2.MemberName + "\",");
				stringBuilder.AppendLine(prefix + "            JsonName = \"" + fsMetaProperty2.JsonName + "\",");
				stringBuilder.AppendLine(prefix + "            StorageType = \"" + fsMetaProperty2.StorageType.CSharpName(true) + "\",");
				if (fsMetaProperty2.OverrideConverterType != null)
				{
					stringBuilder.AppendLine(prefix + "            OverrideConverterType = \"" + fsMetaProperty2.OverrideConverterType.CSharpName(true) + "\",");
				}
				stringBuilder.AppendLine(prefix + "        },");
			}
			stringBuilder.AppendLine(prefix + "    }");
			stringBuilder.Append(prefix + "}");
			return stringBuilder.ToString();
		}

		private static string GetConverterString(fsMetaProperty member)
		{
			if (member.OverrideConverterType == null)
			{
				return "null";
			}
			return string.Format("typeof({0})", member.OverrideConverterType.CSharpName(true));
		}

		public static string GetQualifiedConverterNameForType(Type type)
		{
			return "FullSerializer.Speedup." + type.CSharpName(true, true) + "_DirectConverter";
		}

		private static string GenerateDirectConverterForTypeInCSharp(Type type, fsMetaProperty[] members, bool isConstructorPublic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = type.CSharpName(true);
			string text2 = type.CSharpName(true, true);
			stringBuilder.AppendLine("using System;");
			stringBuilder.AppendLine("using System.Collections.Generic;");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("namespace FullSerializer {");
			stringBuilder.AppendLine("    partial class fsConverterRegistrar {");
			stringBuilder.AppendLine("        public static Speedup." + text2 + "_DirectConverter Register_" + text2 + ";");
			stringBuilder.AppendLine("    }");
			stringBuilder.AppendLine("}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("namespace FullSerializer.Speedup {");
			stringBuilder.AppendLine("    public class " + text2 + "_DirectConverter : fsDirectConverter<" + text + ">, fsIAotConverter {");
			stringBuilder.AppendLine("        private fsAotVersionInfo _versionInfo = " + EmitVersionInfo("        ", type, members, isConstructorPublic) + ";");
			stringBuilder.AppendLine("        fsAotVersionInfo fsIAotConverter.VersionInfo { get { return _versionInfo; } }");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("        protected override fsResult DoSerialize(" + text + " model, Dictionary<string, fsData> serialized) {");
			stringBuilder.AppendLine("            var result = fsResult.Success;");
			stringBuilder.AppendLine();
			foreach (fsMetaProperty fsMetaProperty2 in members)
			{
				stringBuilder.AppendLine("            result += SerializeMember(serialized, " + GetConverterString(fsMetaProperty2) + ", \"" + fsMetaProperty2.JsonName + "\", model." + fsMetaProperty2.MemberName + ");");
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("            return result;");
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref " + text + " model) {");
			stringBuilder.AppendLine("            var result = fsResult.Success;");
			stringBuilder.AppendLine();
			for (int j = 0; j < members.Length; j++)
			{
				fsMetaProperty fsMetaProperty3 = members[j];
				stringBuilder.AppendLine("            var t" + j + " = model." + fsMetaProperty3.MemberName + ";");
				stringBuilder.AppendLine("            result += DeserializeMember(data, " + GetConverterString(fsMetaProperty3) + ", \"" + fsMetaProperty3.JsonName + "\", out t" + j + ");");
				stringBuilder.AppendLine("            model." + fsMetaProperty3.MemberName + " = t" + j + ";");
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("            return result;");
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("        public override object CreateInstance(fsData data, Type storageType) {");
			if (isConstructorPublic)
			{
				stringBuilder.AppendLine("            return new " + text + "();");
			}
			else
			{
				stringBuilder.AppendLine("            return Activator.CreateInstance(typeof(" + text + "), /*nonPublic:*/true);");
			}
			stringBuilder.AppendLine("        }");
			stringBuilder.AppendLine("    }");
			stringBuilder.AppendLine("}");
			return stringBuilder.ToString();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Saves.Migrations.PrefsSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.ProfileSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.RunHistories;
using MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;
using MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public static class IMigrationSubtypes
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t0 = typeof(PrefsSaveV1ToV2);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t1 = typeof(ProfileSaveV1ToV2);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t2 = typeof(ProgressSaveV20ToV21);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t3 = typeof(ProgressSaveV21ToV22);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t4 = typeof(ProgressSaveV22ToV23);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t5 = typeof(ProgressSaveV23ToV24);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t6 = typeof(RunHistoryV7ToV8);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t7 = typeof(RunHistoryV8ToV9);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t8 = typeof(RunHistoryV9ToV10);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t9 = typeof(SerializableRunV12ToV13);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t10 = typeof(SerializableRunV13ToV14);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t11 = typeof(SerializableRunV14ToV15);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t12 = typeof(SerializableRunV15ToV16);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t13 = typeof(SerializableRunV16ToV17);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t14 = typeof(SerializableRunV17ToV18);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t15 = typeof(SerializableRunV18ToV19);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t16 = typeof(SerializableRunV19ToV20);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t17 = typeof(SettingsSaveV3ToV4);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t18 = typeof(SettingsSaveV4ToV5);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t19 = typeof(SettingsSaveV5ToV6);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t20 = typeof(SettingsSaveV6ToV7);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t21 = typeof(SettingsSaveV7ToV8);

	private static readonly Type[] _subtypes = new Type[22]
	{
		_t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8, _t9,
		_t10, _t11, _t12, _t13, _t14, _t15, _t16, _t17, _t18, _t19,
		_t20, _t21
	};

	public static int Count => 22;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}

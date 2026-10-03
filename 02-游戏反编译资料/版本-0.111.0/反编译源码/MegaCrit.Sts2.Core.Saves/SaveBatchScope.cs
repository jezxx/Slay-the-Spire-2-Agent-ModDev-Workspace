using System;
using System.Runtime.CompilerServices;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// Disposable scope returned by <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.BeginSaveBatch" />.
/// Calls <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.EndSaveBatch" /> on dispose, allowing batch usage with <c>using</c>.
/// </summary>
public readonly struct SaveBatchScope : IDisposable
{
	[CompilerGenerated]
	private readonly SaveManager _003CsaveManager_003EP;

	/// <summary>
	/// Disposable scope returned by <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.BeginSaveBatch" />.
	/// Calls <see cref="M:MegaCrit.Sts2.Core.Saves.SaveManager.EndSaveBatch" /> on dispose, allowing batch usage with <c>using</c>.
	/// </summary>
	public SaveBatchScope(SaveManager saveManager)
	{
		_003CsaveManager_003EP = saveManager;
	}

	public void Dispose()
	{
		_003CsaveManager_003EP.EndSaveBatch();
	}
}

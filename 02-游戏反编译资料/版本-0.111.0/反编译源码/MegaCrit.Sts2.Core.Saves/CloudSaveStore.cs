using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

/// <summary>
/// An implementation of ISaveStore which saves files to both local and cloud storage.
/// This is the main entry point for cloud saves. You should not use any cloud-enabled save stores directly - instead,
/// write and read through this abstraction layer.
/// When files are read, they are always read from local storage. SyncCloudToLocal should be called once at the beginning
/// of the game's lifecycle before any files are read so that you are reading the up-to-date data.
///
/// CLOUD ERROR POLICY: All cloud operations are best-effort. A cloud failure must never prevent local saves from
/// working or the game from starting. Exception handlers catch broadly (Exception, not specific types) because
/// SteamRemoteSaveStore methods are P/Invoke calls into Valve's native steam_api DLL, which can throw SEHException
/// at any time in addition to the managed exceptions (InvalidOperationException, SteamRemoteSaveStoreException)
/// that our wrapper code throws.
/// </summary>
public class CloudSaveStore : ICloudSaveStore, ISaveStore
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass36_0
	{
		public CloudSaveStore _003C_003E4__this;

		public string directoryPath;

		internal int _003COverwriteCloudWithLocalDirectory_003Eb__0(string p1, string p2)
		{
			return _003C_003E4__this.LocalStore.GetLastModifiedTime(directoryPath + "/" + p2).CompareTo(_003C_003E4__this.LocalStore.GetLastModifiedTime(directoryPath + "/" + p1));
		}
	}

	[CompilerGenerated]
	private sealed class _003COverwriteCloudWithLocalDirectory_003Ed__36 : IEnumerable<Task>, IEnumerable, IEnumerator<Task>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Task _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public CloudSaveStore _003C_003E4__this;

		private string directoryPath;

		public string _003C_003E3__directoryPath;

		private _003C_003Ec__DisplayClass36_0 _003C_003E8__1;

		private int? byteLimit;

		public int? _003C_003E3__byteLimit;

		private int? fileLimit;

		public int? _003C_003E3__fileLimit;

		private HashSet<string> _003CfilePathsRead_003E5__2;

		private string[] _003C_003E7__wrap2;

		private int _003C_003E7__wrap3;

		private int _003CtotalFilesWritten_003E5__5;

		private List<string>.Enumerator _003C_003E7__wrap5;

		private int _003CbytesToWrite_003E5__7;

		Task IEnumerator<Task>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003COverwriteCloudWithLocalDirectory_003Ed__36(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E8__1 = null;
			_003CfilePathsRead_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E7__wrap5 = default(List<string>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				CloudSaveStore cloudSaveStore = _003C_003E4__this;
				List<string> list;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass36_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					_003C_003E8__1.directoryPath = directoryPath;
					Log.Debug("Writing all files in directory " + _003C_003E8__1.directoryPath + " to cloud");
					_003CfilePathsRead_003E5__2 = new HashSet<string>();
					string[] array = Array.Empty<string>();
					try
					{
						if (cloudSaveStore.CloudStore.DirectoryExists(_003C_003E8__1.directoryPath))
						{
							array = cloudSaveStore.CloudStore.GetFilesInDirectory(_003C_003E8__1.directoryPath);
						}
					}
					catch (Exception ex)
					{
						Log.Warn("Failed to list cloud files in " + _003C_003E8__1.directoryPath + ", skipping cloud delete sync: " + ex.Message);
						SentryService.CaptureException(ex);
					}
					_003C_003E7__wrap2 = array;
					_003C_003E7__wrap3 = 0;
					goto IL_0184;
				}
				case 1:
					_003C_003E1__state = -1;
					goto IL_0176;
				case 2:
					{
						_003C_003E1__state = -3;
						_003C_003E7__wrap3 += _003CbytesToWrite_003E5__7;
						_003CtotalFilesWritten_003E5__5++;
						goto IL_038b;
					}
					IL_0176:
					_003C_003E7__wrap3++;
					goto IL_0184;
					IL_0184:
					if (_003C_003E7__wrap3 < _003C_003E7__wrap2.Length)
					{
						string text = _003C_003E7__wrap2[_003C_003E7__wrap3];
						if (!ShouldSyncFileToCloud(text))
						{
							cloudSaveStore.DeleteStaleBackupFromCloud(_003C_003E8__1.directoryPath, text);
							goto IL_0176;
						}
						_003CfilePathsRead_003E5__2.Add(text);
						_003C_003E2__current = cloudSaveStore.OverwriteCloudWithLocal(_003C_003E8__1.directoryPath + "/" + text);
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003E7__wrap2 = null;
					if (!cloudSaveStore.LocalStore.DirectoryExists(_003C_003E8__1.directoryPath))
					{
						break;
					}
					list = cloudSaveStore.LocalStore.GetFilesInDirectory(_003C_003E8__1.directoryPath).ToList();
					_003C_003E7__wrap3 = 0;
					_003CtotalFilesWritten_003E5__5 = 0;
					if (byteLimit.HasValue || fileLimit.HasValue)
					{
						list.Sort((string p1, string p2) => _003C_003E8__1._003C_003E4__this.LocalStore.GetLastModifiedTime(_003C_003E8__1.directoryPath + "/" + p2).CompareTo(_003C_003E8__1._003C_003E4__this.LocalStore.GetLastModifiedTime(_003C_003E8__1.directoryPath + "/" + p1)));
					}
					_003C_003E7__wrap5 = list.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_038b;
					IL_038b:
					while (_003C_003E7__wrap5.MoveNext())
					{
						string current = _003C_003E7__wrap5.Current;
						if (!_003CfilePathsRead_003E5__2.Contains(current) && ShouldSyncFileToCloud(current))
						{
							string path = _003C_003E8__1.directoryPath + "/" + current;
							_003CbytesToWrite_003E5__7 = cloudSaveStore.LocalStore.GetFileSize(path);
							bool flag = (byteLimit.HasValue && _003C_003E7__wrap3 + _003CbytesToWrite_003E5__7 > byteLimit.Value) || (fileLimit.HasValue && _003CtotalFilesWritten_003E5__5 + 1 > fileLimit.Value);
							if (flag)
							{
								Log.Info($"File {current} will be immediately forgotten after writing to cloud. Bytes written:{_003C_003E7__wrap3 + _003CbytesToWrite_003E5__7}. Files written: {_003CtotalFilesWritten_003E5__5 + 1}");
							}
							_003C_003E2__current = cloudSaveStore.OverwriteCloudWithLocal(path, flag);
							_003C_003E1__state = 2;
							return true;
						}
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap5 = default(List<string>.Enumerator);
					break;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap5).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Task> IEnumerable<Task>.GetEnumerator()
		{
			_003COverwriteCloudWithLocalDirectory_003Ed__36 _003COverwriteCloudWithLocalDirectory_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003COverwriteCloudWithLocalDirectory_003Ed__ = this;
			}
			else
			{
				_003COverwriteCloudWithLocalDirectory_003Ed__ = new _003COverwriteCloudWithLocalDirectory_003Ed__36(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003COverwriteCloudWithLocalDirectory_003Ed__.directoryPath = _003C_003E3__directoryPath;
			_003COverwriteCloudWithLocalDirectory_003Ed__.byteLimit = _003C_003E3__byteLimit;
			_003COverwriteCloudWithLocalDirectory_003Ed__.fileLimit = _003C_003E3__fileLimit;
			return _003COverwriteCloudWithLocalDirectory_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Task>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSyncCloudToLocalDirectory_003Ed__34 : IEnumerable<Task>, IEnumerable, IEnumerator<Task>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Task _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string directoryPath;

		public string _003C_003E3__directoryPath;

		public CloudSaveStore _003C_003E4__this;

		private HashSet<string> _003CfilePathsRead_003E5__2;

		private string[] _003C_003E7__wrap2;

		private int _003C_003E7__wrap3;

		Task IEnumerator<Task>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSyncCloudToLocalDirectory_003Ed__34(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CfilePathsRead_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			CloudSaveStore cloudSaveStore = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				Log.Debug("Syncing all files in " + directoryPath + " from cloud to local");
				_003CfilePathsRead_003E5__2 = new HashSet<string>();
				string[] array = Array.Empty<string>();
				try
				{
					if (cloudSaveStore.CloudStore.DirectoryExists(directoryPath))
					{
						array = cloudSaveStore.CloudStore.GetFilesInDirectory(directoryPath);
					}
				}
				catch (Exception ex)
				{
					Log.Warn("Failed to list cloud files in " + directoryPath + ", skipping cloud sync: " + ex.Message);
					SentryService.CaptureException(ex);
				}
				_003C_003E7__wrap2 = array;
				_003C_003E7__wrap3 = 0;
				goto IL_0146;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_0138;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0206;
				}
				IL_0206:
				_003C_003E7__wrap3++;
				goto IL_0214;
				IL_0146:
				if (_003C_003E7__wrap3 < _003C_003E7__wrap2.Length)
				{
					string text = _003C_003E7__wrap2[_003C_003E7__wrap3];
					if (!ShouldSyncFileToCloud(text))
					{
						cloudSaveStore.DeleteStaleBackupFromCloud(directoryPath, text);
						goto IL_0138;
					}
					string text2 = directoryPath + "/" + text;
					_003CfilePathsRead_003E5__2.Add(text2);
					Log.Debug("Checking file " + text2 + " in cloud saves");
					_003C_003E2__current = cloudSaveStore.SyncCloudToLocal(text2);
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap2 = null;
				if (!cloudSaveStore.LocalStore.DirectoryExists(directoryPath))
				{
					break;
				}
				_003C_003E7__wrap2 = cloudSaveStore.LocalStore.GetFilesInDirectory(directoryPath);
				_003C_003E7__wrap3 = 0;
				goto IL_0214;
				IL_0138:
				_003C_003E7__wrap3++;
				goto IL_0146;
				IL_0214:
				if (_003C_003E7__wrap3 < _003C_003E7__wrap2.Length)
				{
					string text3 = _003C_003E7__wrap2[_003C_003E7__wrap3];
					if (ShouldSyncFileToCloud(text3))
					{
						string text4 = directoryPath + "/" + text3;
						if (!_003CfilePathsRead_003E5__2.Contains(text4))
						{
							Log.Debug("Checking file " + text4 + " in local saves");
							_003C_003E2__current = cloudSaveStore.SyncCloudToLocal(text4);
							_003C_003E1__state = 2;
							return true;
						}
					}
					goto IL_0206;
				}
				_003C_003E7__wrap2 = null;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Task> IEnumerable<Task>.GetEnumerator()
		{
			_003CSyncCloudToLocalDirectory_003Ed__34 _003CSyncCloudToLocalDirectory_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CSyncCloudToLocalDirectory_003Ed__ = this;
			}
			else
			{
				_003CSyncCloudToLocalDirectory_003Ed__ = new _003CSyncCloudToLocalDirectory_003Ed__34(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CSyncCloudToLocalDirectory_003Ed__.directoryPath = _003C_003E3__directoryPath;
			return _003CSyncCloudToLocalDirectory_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Task>)this).GetEnumerator();
		}
	}

	private readonly CloudSyncFailureReporter _syncFailureReporter = new CloudSyncFailureReporter();

	public ISaveStore LocalStore { get; }

	public ICloudSaveStore CloudStore { get; }

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="localStore">The class which will be used to save to local storage.</param>
	/// <param name="cloudStore">The class which will be used to save to cloud storage.</param>
	public CloudSaveStore(ISaveStore localStore, ICloudSaveStore cloudStore)
	{
		LocalStore = localStore;
		CloudStore = cloudStore;
	}

	/// <summary>
	/// Reads a file from local storage.
	/// </summary>
	public string? ReadFile(string path)
	{
		return LocalStore.ReadFile(path);
	}

	/// <summary>
	/// Reads a file asynchronously from local storage.
	/// </summary>
	public Task<string?> ReadFileAsync(string path)
	{
		return LocalStore.ReadFileAsync(path);
	}

	/// <summary>
	/// Checks if a file exists in local storage.
	/// </summary>
	public bool FileExists(string path)
	{
		return LocalStore.FileExists(path);
	}

	/// <summary>
	/// Checks if a directory exists in local storage.
	/// </summary>
	public bool DirectoryExists(string path)
	{
		return LocalStore.DirectoryExists(path);
	}

	/// <summary>
	/// Writes a file synchronously to both local and remote storage.
	/// </summary>
	public void WriteFile(string path, string content)
	{
		LocalStore.WriteFile(path, content);
		try
		{
			CloudStore.WriteFile(path, content);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file synchronously to both local and remote storage.
	/// </summary>
	public void WriteFile(string path, byte[] bytes)
	{
		LocalStore.WriteFile(path, bytes);
		try
		{
			CloudStore.WriteFile(path, bytes);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file asynchronously to both local and remote storage.
	/// </summary>
	public async Task WriteFileAsync(string path, string content)
	{
		await LocalStore.WriteFileAsync(path, content);
		try
		{
			await CloudStore.WriteFileAsync(path, content);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Writes a file asynchronously to both local and remote storage.
	/// </summary>
	public async Task WriteFileAsync(string path, byte[] bytes)
	{
		await LocalStore.WriteFileAsync(path, bytes);
		try
		{
			await CloudStore.WriteFileAsync(path, bytes);
			SyncLocalTimestamp(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex.Message);
			SyncLocalTimestamp(path);
		}
	}

	/// <summary>
	/// Deletes a file from both local and remote storage.
	/// </summary>
	public void DeleteFile(string path)
	{
		LocalStore.DeleteFile(path);
		try
		{
			CloudStore.DeleteFile(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete failed for " + path + ", local delete preserved: " + ex.Message);
		}
	}

	/// <summary>
	/// Renames a file in both local and remote storage.
	/// Try to avoid using this, Steam's remote storage doesn't allow atomic renaming.
	/// </summary>
	public void RenameFile(string sourcePath, string destinationPath)
	{
		LocalStore.RenameFile(sourcePath, destinationPath);
		try
		{
			CloudStore.RenameFile(sourcePath, destinationPath);
		}
		catch (Exception ex)
		{
			Log.Warn($"Cloud rename failed for {sourcePath} -> {destinationPath}, local rename preserved: {ex.Message}");
		}
	}

	/// <summary>
	/// Returns the files in the directory from local storage.
	/// </summary>
	public string[] GetFilesInDirectory(string directoryPath)
	{
		return LocalStore.GetFilesInDirectory(directoryPath);
	}

	/// <summary>
	/// Returns the directories in the directory read from local storage.
	/// </summary>
	public string[] GetDirectoriesInDirectory(string directoryPath)
	{
		return LocalStore.GetDirectoriesInDirectory(directoryPath);
	}

	/// <summary>
	/// Creates a directory in both local and remote storage.
	/// </summary>
	public void CreateDirectory(string directoryPath)
	{
		LocalStore.CreateDirectory(directoryPath);
		try
		{
			CloudStore.CreateDirectory(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud create directory failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Deletes a directory and any remaining contents in both local and remote storage.
	/// </summary>
	public void DeleteDirectory(string directoryPath)
	{
		LocalStore.DeleteDirectory(directoryPath);
		try
		{
			CloudStore.DeleteDirectory(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete directory failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Deletes temporary files from both local and remote storage.
	/// </summary>
	public void DeleteTemporaryFiles(string directoryPath)
	{
		LocalStore.DeleteTemporaryFiles(directoryPath);
		try
		{
			CloudStore.DeleteTemporaryFiles(directoryPath);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud delete temporary files failed for " + directoryPath + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Gets the last modified time of a file in local storage.
	/// </summary>
	public DateTimeOffset GetLastModifiedTime(string path)
	{
		return LocalStore.GetLastModifiedTime(path);
	}

	/// <summary>
	/// Gets the size of a file without reading the entire file.
	/// </summary>
	public int GetFileSize(string path)
	{
		return LocalStore.GetFileSize(path);
	}

	/// <summary>
	/// Sets the last modified time of a file in local storage.
	/// </summary>
	public void SetLastModifiedTime(string path, DateTimeOffset time)
	{
		LocalStore.SetLastModifiedTime(path, time);
	}

	/// <summary>
	/// Returns the full path of a file in local storage.
	/// </summary>
	public string GetFullPath(string filename)
	{
		return LocalStore.GetFullPath(filename);
	}

	/// <summary>
	/// Checks if the cloud storage has any files stored.
	/// </summary>
	public bool HasCloudFiles()
	{
		return CloudStore.HasCloudFiles();
	}

	/// <summary>
	/// Removes a file from remote storage, but keeps it in the local storage.
	/// </summary>
	public void ForgetFile(string path)
	{
		try
		{
			CloudStore.ForgetFile(path);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud forget failed for " + path + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Returns true if the file at the path is in remote storage.
	/// The file can also be in local storage, but this is disregarded.
	/// </summary>
	public bool IsFilePersisted(string path)
	{
		return CloudStore.IsFilePersisted(path);
	}

	/// <inheritdoc />
	public void BeginSaveBatch()
	{
		CloudStore.BeginSaveBatch();
	}

	/// <inheritdoc />
	public void EndSaveBatch()
	{
		CloudStore.EndSaveBatch();
	}

	/// <summary>
	/// Synchronizes a file from the remote to local storage.
	/// If the file exists on remote storage and the last-modified timestamp differs at all from the local one, then the
	/// local file will be overwritten with the one from remote storage.
	/// If the file doesn't exist on remote storage but does locally, then the local file will be deleted.
	/// </summary>
	public async Task SyncCloudToLocal(string path)
	{
		try
		{
			await SyncCloudToLocalInternal(path);
		}
		catch (Exception ex)
		{
			Log.Warn("SteamRemoteStorage: Failed to sync " + path + " from cloud, skipping: " + ex.Message);
			if (_syncFailureReporter.ShouldReport(ex))
			{
				SentryService.CaptureException(ex);
			}
		}
	}

	private async Task SyncCloudToLocalInternal(string path)
	{
		bool flag = CloudStore.FileExists(path);
		bool flag2 = LocalStore.FileExists(path);
		if (flag)
		{
			DateTimeOffset lastModifiedTime = CloudStore.GetLastModifiedTime(path);
			DateTimeOffset? dateTimeOffset = (flag2 ? new DateTimeOffset?(LocalStore.GetLastModifiedTime(path)) : null);
			bool flag3 = !flag2 || lastModifiedTime != dateTimeOffset;
			if (!flag3)
			{
				string value = LocalStore.ReadFile(path);
				if (string.IsNullOrWhiteSpace(value))
				{
					Log.Warn("Local file " + path + " appears corrupt (empty content) despite matching cloud timestamp, forcing re-download from cloud");
					flag3 = true;
				}
			}
			if (flag3)
			{
				Log.Info($"Copying {path} from cloud to local. Local file exists: {flag2} Cloud save time: {lastModifiedTime} Local save time: {dateTimeOffset}");
				string text = await CloudStore.ReadFileAsync(path);
				if (string.IsNullOrWhiteSpace(text) || text[0] == '\0')
				{
					Log.Warn("Cloud file " + path + " has empty content, skipping download");
					return;
				}
				await LocalStore.WriteFileAsync(path, text);
				SyncLocalTimestamp(path);
			}
			else
			{
				Log.Debug($"Skipping sync for {path}, last modified time matches on local and remote ({lastModifiedTime})");
			}
		}
		else if (flag2)
		{
			Log.Info("Deleting " + path + " because it does not exist on remote");
			LocalStore.DeleteFile(path);
			LocalStore.DeleteFile(path + ".backup");
		}
		else
		{
			Log.Debug("Skipping sync for " + path + ", it doesn't exist on either local or cloud");
		}
	}

	/// <summary>
	/// Synchronizes an entire directory from cloud to local storage.
	/// The rules for <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.SyncCloudToLocal(System.String)" /> are followed for every file found in the given directory, first for
	/// those found on the cloud, then for those found on the local side (if any files existed locally but not in the
	/// cloud).
	/// </summary>
	[IteratorStateMachine(typeof(_003CSyncCloudToLocalDirectory_003Ed__34))]
	public IEnumerable<Task> SyncCloudToLocalDirectory(string directoryPath)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSyncCloudToLocalDirectory_003Ed__34(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__directoryPath = directoryPath
		};
	}

	/// <summary>
	/// Overwrites the state of a file in the cloud with the state of a file on the local side.
	/// This is used when the player launches the game for the first time with cloud sync enabled.
	/// If the file exists locally, then the file on the cloud is replaced unconditionally. If the file doesn't exist
	/// locally but exists on the cloud, the file is deleted from the cloud.
	/// </summary>
	/// <param name="path">The relative path to the file.</param>
	/// <param name="forgetImmediately">If this is true, then after the file is written to the cloud, then it is forgotten
	/// immediately. See comments in <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.OverwriteCloudWithLocalDirectory(System.String,System.Nullable{System.Int32},System.Nullable{System.Int32})" /> for when you might use this.</param>
	public async Task OverwriteCloudWithLocal(string path, bool forgetImmediately = false)
	{
		if (LocalStore.FileExists(path))
		{
			Log.Debug("Writing file " + path + " to cloud");
			string content = await LocalStore.ReadFileAsync(path);
			try
			{
				await CloudStore.WriteFileAsync(path, content);
				if (forgetImmediately)
				{
					try
					{
						Log.Debug("Immediately forgetting " + path);
						CloudStore.ForgetFile(path);
					}
					catch (Exception ex)
					{
						Log.Warn("Cloud forget failed for " + path + ": " + ex.Message);
					}
				}
				SyncLocalTimestamp(path);
				return;
			}
			catch (Exception ex2)
			{
				Log.Warn("Cloud write failed for " + path + ", local file preserved: " + ex2.Message);
				SyncLocalTimestamp(path);
				return;
			}
		}
		try
		{
			if (CloudStore.FileExists(path))
			{
				Log.Debug("Deleting file " + path + " from cloud because it doesn't exist on local");
				CloudStore.DeleteFile(path);
			}
		}
		catch (Exception ex3)
		{
			Log.Warn("Cloud delete failed for " + path + ": " + ex3.Message);
		}
	}

	/// <summary>
	/// Overwrites the state of a directory in the cloud with the state of a directory on the local side.
	/// This is used when the player launches the game for the first time with cloud sync enabled.
	/// The rules for <see cref="M:MegaCrit.Sts2.Core.Saves.CloudSaveStore.SyncCloudToLocal(System.String)" /> are followed for every file found in the given directory, first for
	/// those found on the cloud, then for those found on the local side (if any files existed locally but not in the
	/// cloud).
	///
	/// Files are written in order of last-modified time. Once we exceed either byteLimit or fileLimit, then files are
	/// written to the cloud storage, but they are forgotten from the remote cloud storage (not deleted, just forgotten).
	///
	/// So why write-and-forget? Why not just... not write? Later, when we go to sync cloud files to the local storage,
	/// we need the files to exist in the remote storage. Otherwise, we'll delete the files in the local storage.
	/// </summary>
	/// <param name="directoryPath">The directory to sync.</param>
	/// <param name="byteLimit">The maximum number of bytes that can be written to storage.</param>
	/// <param name="fileLimit">The maximum number of files that can be written to storage.</param>
	[IteratorStateMachine(typeof(_003COverwriteCloudWithLocalDirectory_003Ed__36))]
	public IEnumerable<Task> OverwriteCloudWithLocalDirectory(string directoryPath, int? byteLimit, int? fileLimit)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003COverwriteCloudWithLocalDirectory_003Ed__36(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__directoryPath = directoryPath,
			_003C_003E3__byteLimit = byteLimit,
			_003C_003E3__fileLimit = fileLimit
		};
	}

	/// <summary>
	/// Forgets the oldest files that would cause us to go over the byte/file limit quotas.
	/// This is used when the player is writing a new run history file, which might exceed a limit that we set on the
	/// count/size of run histories.
	/// </summary>
	/// <param name="directoryPath">The directory to sync.</param>
	/// <param name="bytesToBeWritten">The number bytes that will be written to the new run history file.</param>
	/// <param name="byteLimit">The maximum number of bytes that can be written to storage.</param>
	/// <param name="fileLimit">The maximum number of files that can be written to storage.</param>
	public void ForgetFilesInDirectoryBeforeWritingIfNecessary(string directoryPath, int bytesToBeWritten, int byteLimit, int fileLimit)
	{
		try
		{
			ForgetFilesInDirectoryBeforeWritingIfNecessaryInternal(directoryPath, bytesToBeWritten, byteLimit, fileLimit);
		}
		catch (Exception ex)
		{
			Log.Warn("Cloud quota management failed for " + directoryPath + ": " + ex.Message);
			SentryService.CaptureException(ex);
		}
	}

	private void ForgetFilesInDirectoryBeforeWritingIfNecessaryInternal(string directoryPath, int bytesToBeWritten, int byteLimit, int fileLimit)
	{
		int num = bytesToBeWritten;
		int num2 = 1;
		string[] filesInDirectory = CloudStore.GetFilesInDirectory(directoryPath);
		List<string> list = new List<string>();
		string[] array = filesInDirectory;
		foreach (string text in array)
		{
			if (ShouldSyncFileToCloud(text))
			{
				string text2 = directoryPath + "/" + text;
				if (CloudStore.IsFilePersisted(text2))
				{
					list.Add(text2);
					num += CloudStore.GetFileSize(text2);
					num2++;
				}
			}
		}
		if (num > byteLimit || num2 > fileLimit)
		{
			list.Sort((string p1, string p2) => GetLastModifiedTime(p2).CompareTo(GetLastModifiedTime(p1)));
			while (num > byteLimit || num2 > fileLimit)
			{
				string text3 = list[list.Count - 1];
				num -= CloudStore.GetFileSize(text3);
				num2--;
				Log.Info($"Forgetting file {text3} from cloud storage because we're past our quota. Bytes after forgetting: {num}. Files after forgetting: {num2}");
				CloudStore.ForgetFile(text3);
				list.RemoveAt(list.Count - 1);
			}
		}
	}

	/// <summary>
	/// Returns true if the file should be included in cloud sync operations. Filters out .backup files,
	/// which are local crash-recovery artifacts created by CopyBackup. These should never be uploaded to
	/// or synced from cloud storage.
	/// </summary>
	private static bool ShouldSyncFileToCloud(string fileName)
	{
		return !fileName.EndsWith(".backup");
	}

	/// <summary>
	/// Removes a .backup file that should never have been uploaded to cloud storage. These are local-only
	/// artifacts that were uploaded before OverwriteCloudWithLocalDirectory filtered them. Cleaning them
	/// up frees cloud quota.
	/// </summary>
	private void DeleteStaleBackupFromCloud(string directoryPath, string cloudPath)
	{
		string text = directoryPath + "/" + cloudPath;
		Log.Info("Removing stale .backup file from cloud: " + text);
		try
		{
			CloudStore.DeleteFile(text);
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to remove .backup from cloud " + text + ": " + ex.Message);
		}
	}

	/// <summary>
	/// Syncs the local file's last modified time to the cloud file's last modified time.
	/// This is best-effort: on Windows, the file can become temporarily unavailable (antivirus, cloud sync tools, file
	/// system contention) between the write and the timestamp update. A failed sync just means the next cloud sync might
	/// redundantly re-copy the file, which is harmless.
	///
	/// Called in both the success path (to sync timestamps after a cloud write) and the failure path (to prevent
	/// SyncCloudToLocal from overwriting local with stale cloud data on next startup).
	///
	/// Catches Exception broadly (not just IOException) because CloudStore.GetLastModifiedTime is a P/Invoke call
	/// that can throw several exception types (SEHException, InvalidOperationException, etc.). The consequence of
	/// not catching broadly here is a game crash, which is not acceptable for a timestamp sync failure.
	/// </summary>
	private void SyncLocalTimestamp(string path)
	{
		try
		{
			LocalStore.SetLastModifiedTime(path, CloudStore.GetLastModifiedTime(path));
		}
		catch (Exception ex)
		{
			Log.Warn("Failed to sync timestamp for " + path + ", will re-sync on next launch: " + ex.Message);
		}
	}

	public bool HasUserEnabledCloudSync()
	{
		return CloudStore.HasUserEnabledCloudSync();
	}
}

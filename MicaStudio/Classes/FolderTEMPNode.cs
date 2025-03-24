using CommunityToolkit.Mvvm.ComponentModel;
using MicaStudio.Core.Classes.Explorer;
using MicaStudio.Core.Interfaces.Explorer;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MicaStudio.Classes
{
	public partial class FolderTEMPNode : ObservableObject, IExplorerParentNode
	{
		[ObservableProperty]
		private string displayName = "";

		[ObservableProperty]
		private bool isExpanded = false;

		public ObservableCollection<IExplorerNode> Children { get; } = new();

		public string FilePath { get; } = "";

		public FolderTEMPNode(string filePath)
		{
			FilePath = filePath;
			DisplayName = Path.GetFileName(filePath);
		}

		public async void Expanding()
		{
			if (Children.Count > 0) return; //TEMP: do not load if there are items already
			var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
			//await Task.Run(() =>
		//	{
				try
				{
				Stopwatch stopwatch = Stopwatch.StartNew();

				var subFolders = Directory.GetDirectories(FilePath);
				foreach (var subFolder in subFolders)
					Children.Add(new FolderNode(subFolder));

				var files = Directory.GetFiles(FilePath);
				foreach (var file in files)
					Children.Add(new FileNode(file));

					stopwatch.Stop();
					Debug.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms to load {Children.Count}");
				}
				catch (Exception ex)
				{
					// Handle exceptions like unauthorized access, path not found, etc.
					Debug.WriteLine(ex.Message);
				}
		//	});
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WIN32_FIND_DATA
		{
			public uint dwFileAttributes;
			public long ftCreationTime;
			public long ftLastAccessTime;
			public long ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternateFileName;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll")] private static extern bool FindClose(IntPtr hFindFile);
	}
}

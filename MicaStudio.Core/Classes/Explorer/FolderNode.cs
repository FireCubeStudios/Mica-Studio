using CommunityToolkit.Mvvm.ComponentModel;
using MicaStudio.Core.Interfaces.Explorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MicaStudio.Core.Classes.Explorer
{
	public partial class FolderNode : ObservableObject, IExplorerParentNode
	{
		[ObservableProperty]
		private string displayName = "";

		[ObservableProperty]
		private bool isExpanded = false;

		public ObservableCollection<IExplorerNode> Children { get; } = new();

		public string FilePath { get; } = "";

		public FolderNode(string filePath)
		{
			FilePath = filePath;
			DisplayName = Path.GetFileName(filePath);
		}

		public void Expanding()
		{
			if (Children.Count > 0) return; //TEMPORARY do not load if there are items already
			try
			{
				// Get subfolders in this folder
				var subFolders = Directory.GetDirectories(FilePath);
				foreach (var subFolder in subFolders)
					Children.Add(new FolderNode(subFolder));

				// Get files in this folder
				var files = Directory.GetFiles(FilePath);
				foreach (var file in files)
					Children.Add(new FileNode(file));
			}
			catch (Exception ex)
			{
				// Handle exceptions like unauthorized access, path not found, etc.
				Debug.WriteLine(ex.Message);
			}
		}
	}
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MicaStudio.Classes;
using MicaStudio.Core.Classes.Explorer;
using MicaStudio.Core.Interfaces.Explorer;
using MicaStudio.Core.Messages.Commands.Files;
using MicaStudio.Core.Messages.Explorer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using WinUIEditor;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio.Panels
{
	public record TreeNode(string Name, string Path, bool isFolder, ObservableCollection<TreeNode> Children);

	[INotifyPropertyChanged]
	public sealed partial class ExplorerPanel : UserControl
	{
		private ObservableCollection<IExplorerNode> files = new();
		[ObservableProperty]
		private FolderTEMPNode rootFolderNode;
		public ExplorerPanel()
		{
			this.InitializeComponent();

			// Listen for "FolderOpenedMessage" and load the folder in ExplorerPanel expanded by default
			WeakReferenceMessenger.Default.Register<FolderOpenedMessage>(this, (r, m) =>
			{
				files.Clear();
				//var rootFolderNode = new FolderNode(m.Value.Item2);
				RootFolderNode = new FolderTEMPNode(m.Value.Item2);
				files.Add(RootFolderNode);
				RootFolderNode.IsExpanded = true;
				RootFolderNode.Expanding();
			});
		}

		// Notify IExplorerParentNode it has been expanded
		private void ExplorerTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
		{
			if(args.Item is IExplorerParentNode)
				((IExplorerParentNode)args.Item).Expanding();
		}

		private void ExplorerTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
		{
			foreach(var node in args.AddedItems)
				WeakReferenceMessenger.Default.Send(new ExplorerSelectionChangedMessage((IExplorerNode)node));
		}

		/*private async Task LoadFolderContentAsyncWinRT(TreeNode node)
		{
			if (!node.isFolder) return;
			var currentFolder = node.StorageItem as StorageFolder;
			var subFolders = await currentFolder.GetFoldersAsync();
			var files = await currentFolder.GetFilesAsync();
			foreach (var subFolder in subFolders)
			{
				node.Children.Add(new TreeNode(subFolder.Name, subFolder, true, new ObservableCollection<TreeNode>()));
			}

			// Add files one by one
			foreach (var file in files)
			{
				node.Children.Add(new TreeNode(file.Name, file, false, new ObservableCollection<TreeNode>()));
			}
		}*/
	}
}

using CommunityToolkit.Mvvm.Messaging;
using MicaStudio.Core.Interfaces.Explorer;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio.Controls.Explorer
{
	public sealed partial class ExplorerListView : UserControl
	{
		// Dependency Property for Folder
		public static readonly DependencyProperty FolderProperty =
			DependencyProperty.Register(
				"Folder",
				typeof(IExplorerParentNode),
				typeof(FolderControl),
				new PropertyMetadata(null));

		// CLR Property wrapper for Folder Dependency Property
		public IExplorerParentNode Folder
		{
			get { return (IExplorerParentNode)GetValue(FolderProperty); }
			set { SetValue(FolderProperty, value); }
		}
		public ExplorerListView()
		{
			this.InitializeComponent();
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0) return;
			var item = e.AddedItems[0];
			if (item is IExplorerParentNode)
				((IExplorerParentNode)item).Expanding();
			else
				WeakReferenceMessenger.Default.Send(new ExplorerSelectionChangedMessage((IExplorerNode)item));
		}
	}
}

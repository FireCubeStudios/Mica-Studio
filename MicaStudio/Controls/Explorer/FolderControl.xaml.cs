using MicaStudio.Core.Interfaces.Explorer;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio.Controls.Explorer
{
	public sealed partial class FolderControl : UserControl
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

		public FolderControl()
		{
			this.InitializeComponent();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicaStudio.Core.Interfaces.Explorer
{
	/*
	 * Interface for a TreeView item shown in the Explorer panel
	 * The TreeView would bind to a collection representing a Tree of IExplorerNodes
	 * This interface has a string to display on the TreeView
	 */
	public interface IExplorerNode : INotifyPropertyChanged
	{
		public string DisplayName { get; set; }
	}
}

using MicaStudio.Utilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using Windows.Storage;
using WinUIEditor;

namespace MicaStudio.Highlighters
{
	// Highlights Scintilla Editor with random access algorithm
	// Highlights file from top to bottom line by line on file load
	public partial class RandomAccessHighlighter
	{
		private IGrammar grammar;
		private Registry registry;
		private Editor Editor;
		private DispatcherQueue DispatcherQueue;
		public RandomAccessHighlighter(Editor editor, DispatcherQueue dispatcherQueue)
		{
			this.Editor = editor;
			DispatcherQueue = dispatcherQueue;
		}

		public async void FileLoaded(StorageFile file)
		{
			try
			{
				await Task.Run(async () =>
				{
					Stopwatch stopwatch = Stopwatch.StartNew();

					RegistryOptions options = new RegistryOptions(ThemeName.DarkPlus);
					registry = new Registry(options);
					grammar = registry.LoadGrammar(options.GetScopeByExtension(file.FileType)); // parameter is initial scope name

					if (grammar is not null)
					{
						await HighlightRange(Editor.FirstVisibleLine, Editor.LinesOnScreen);
						//Editor.UpdateUI += Editor_UpdateUI;
						//Editor.ZoomChanged += Editor_ZoomChanged;
						Editor.SetILexer(0); // Needed to enable STYLENEEDED notifications
						Editor.Modified += Editor_Modified;

						// WE DISABLED RANDOM HIGHLIGHTING FOR BACTH HIGHLIGHT TESTING
						//Editor.StyleNeeded += Editor_StyleNeeded;
						await HighlightRange(Editor.FirstVisibleLine, Editor.LineCount);
					}

					stopwatch.Stop();
					Debug.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
				});
			}
			catch { }
		}

		private Queue<long> frontier = new Queue<long>();
		private async void Editor_Modified(Editor sender, ModifiedEventArgs args)
		{
			// args.linesAdded = Number of added lines. If negative, the number of deleted lines. Set to 0 if not used or no lines added or deleted.
			// 0x01 == inserted text
			// 0x02 == deleted text

			// If a single line is edited then we add it to the frontier
			// (preferably actually add the line befor eit however our frontier method already takes the "state" of the previous line)
			// If a region of text is replaced then we delete the cached states and add to frontier the previous line
			// e.g if lines 5 - 10 are replaced we delete cache entries for 5 - 10 then we add 4 to frontier
			// TODO: Handle inserting or deleting lines
			if ((args.ModificationType & 0x01) != 0)
			{
				if(args.LinesAdded > 0) // multiple lines edited
				{

				}
				else // single line edit
				{

					// Add the edited line to the frontier
					frontier.Enqueue(Editor.LineFromPosition(args.Position));
					Revalidate();
				}
			}

			if ((args.ModificationType & 0x02) != 0)
			{
				if (args.LinesAdded < 0) // multiple lines deleted
				{

				}
				else // single line edit
				{
					// Add the edited line to the frontier
					frontier.Enqueue(Editor.LineFromPosition(args.Position));
					Revalidate();
				}
			}
		}

		// A function which revalidates by clearing the frontier
		// Loops through 
		public async void Revalidate()
		{
			await Task.Run(() =>
			{
				while (frontier.Count > 0)
				{

					long currentLine = frontier.Dequeue();
					if (currentLine >= Editor.LineCount) return; // end of document reached

					var currentState = cache[currentLine]; // get current state of line

					// calculate new state of line using state of previous line which is guaranteed valid
					ITokenizeLineResult result = grammar.TokenizeLine(Editor.GetLine(currentLine), cache[currentLine - 1], TimeSpan.MaxValue);
					var newState = result.RuleStack;
					cache[currentLine] = newState; // save new state to cache

					// check if the cached state of the next line is different from the new state of this line
					// or if the cache state does not exist then also add to frontier
					if(!cache.ContainsKey(currentLine + 1) || cache[currentLine + 1] != newState)
						frontier.Enqueue(currentLine + 1); // add next line to frontier since state different

					// Syntax highlight current line with new state
					if (result.Tokens.Count() == 0) continue;
					parseTokens(result, Editor.GetLine(currentLine), currentLine);
				}
			});
		}

		private CancellationTokenSource? cancel; // to cancel previous highlightrange
		private int coount = 0;
		private async void Editor_StyleNeeded(Editor sender, StyleNeededEventArgs args)
		{
			// Cancel old styling operation
			cancel?.Cancel();
			cancel = new CancellationTokenSource();

			// Style from the line that was last styled all the way to the current line needing styling
			var start = Editor.LineFromPosition(Editor.EndStyled);
			var end = Editor.LineFromPosition(args.Position);
			await HighlightRange(start, end, cancel.Token);
			//Debug.WriteLine(coount+ " start: " + start + " end: " + end);
			//coount++;
		}

		// cache to store states per line
		private ConcurrentDictionary<long, IStateStack> cache = new ConcurrentDictionary<long, IStateStack>();
		// Highlight a range of lines from a "start" line
		// We get the cache of the start line then continue highlighting from there
		// We also store rulestacks if they are not cached for a line
		public async Task HighlightRange(long start, long length, CancellationToken? token = null)
		{
			try
			{
				await Task.Run(async () =>
				{
					// Get state of line before start line
					IStateStack? ruleStack = await GetStateOfLine(start - 1, token);
					 // loop through lines highlighting
					for (long i = start; i <= start + length; i++)
					{
						if (token?.IsCancellationRequested ?? false)
							return;

						string line = Editor.GetLine(i);
						ITokenizeLineResult result = grammar.TokenizeLine(line, ruleStack, TimeSpan.MaxValue);
						ruleStack = result.RuleStack;
						cache[i] = ruleStack;

						if (result.Tokens.Count() == 0) continue; // continue if no tokens
						parseTokens(result, line, i);
					}
				});
			}
			catch(Exception e) {
				Debug.WriteLine(e.Message);
			}
		}

		/*
		 * Get the state of a line "linePosition"
		 * This is done by using the cache, if the state is in the cache it is returned
		 * Otherwise we get the closest state right before the line and calculate the state from there to the line
		 */	
		public async Task<IStateStack?> GetStateOfLine(long linePosition, CancellationToken? token)
		{
			IStateStack? ruleStack = null;
			await Task.Run(() =>
			{
				if (cache.ContainsKey(linePosition)) // rulestack cached so return it
					ruleStack = cache[linePosition];
				else
				{ // otherwise calculate rule stacks
					// get the nearest cached line
					var cacheIndex = cache.Keys.LastOrDefault(k => k <= 600);
					for (long i = cacheIndex + 1; i <= linePosition; i++)
					{
						if (token?.IsCancellationRequested ?? false)
							return;

						string line = Editor.GetLine(i);
						ITokenizeLineResult result = grammar.TokenizeLine(line, ruleStack, TimeSpan.MaxValue);
						ruleStack = result.RuleStack;
					//	if(i % 50 == 0 || i < 50)
							cache[i] = ruleStack; // cache it only every 10 lines
					}
				}

			});
			return ruleStack;
		}

/*
		// broken methods for highlighting
		private async void Editor_ZoomChanged(Editor sender, ZoomChangedEventArgs args)
		{
			cancel?.Cancel();
			cancel = new CancellationTokenSource();

			await HighlightRange(Editor.FirstVisibleLine, Editor.LinesOnScreen, cancel.Token);
		}

		// Update highlighting on scroll
		private async void Editor_UpdateUI(Editor sender, UpdateUIEventArgs args)
		{
			// SC_UPDATE_V_SCROL == 0x04 - scroll updated vertically
			if (args.Updated == 0x04 || args.Updated == 0x08)
			{
				cancel?.Cancel();
				cancel = new CancellationTokenSource();

				await HighlightRange(Editor.FirstVisibleLine, Editor.LinesOnScreen, cancel.Token);
			}
		}*/
	}
}

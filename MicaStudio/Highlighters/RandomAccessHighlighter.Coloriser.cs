using MicaStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace MicaStudio.Highlighters
{
	/*
	 * Contains code solely for colourising a single line based on the tokens of that line
	 */
	public partial class RandomAccessHighlighter
	{
		// Maps a textmate colour to a scintilla style key
		private Dictionary<int, int> colorToScintillaStyle = new();
		
		private int keyCount = 190; // start index for scintilla styles

		// Colour an individual line with tokens
		private void parseTokens(ITokenizeLineResult result, string line, long linePosition)
		{
			Theme theme = registry.GetTheme();

			foreach (IToken token in result.Tokens)
			{
				int startIndex = (token.StartIndex > line.Length) ? line.Length : token.StartIndex;
				int endIndex = (token.EndIndex > line.Length) ? line.Length : token.EndIndex;
				foreach (var scope in token.Scopes)
				{
					List<ThemeTrieElementRule> themeRules = theme.Match(new string[] { scope });

					foreach (ThemeTrieElementRule themeRule in themeRules)
					{
						DispatcherQueue.TryEnqueue(() =>
						{
							// get position of current line i
							long linePos = Editor.PositionFromLine(linePosition);

							// Register foreground colour to a scintilla style if it does not exist
							if (!colorToScintillaStyle.ContainsKey(themeRule.foreground))
							{
								var color = ColourUtilities.HexToByte(theme.GetColor(themeRule.foreground));
								keyCount++;
								// IMPORTANT: Define a style with a unique KEY mapped to a colour, we will use it to highlight tokens
								Editor.StyleSetFore(keyCount, color);

								// add it to hashmap so we can retrieve it
								colorToScintillaStyle[themeRule.foreground] = keyCount;
							}

							// start styling from token position by using line position and index of token
							Editor.StartStyling(linePos + startIndex, 0);
							// USE the style which we defined earlier for foreground on the token
							// the scintilla style KEYS are in the hashmap
							Editor.SetStyling(endIndex - startIndex, colorToScintillaStyle[themeRule.foreground]);
							/*Debug.WriteLine(
								"      - Matched theme rule: " +
								"[bg: {0}, fg:{1}, fontStyle: {2}]",
								theme.GetColor(themeRule.background),
								theme.GetColor(themeRule.foreground),
								themeRule.fontStyle);*/
						});

					}
				}
			}
		}
	}
}

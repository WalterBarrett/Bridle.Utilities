using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bridle.Utilities.Data
{
	public static class DataLoader
	{
		public static DataNode Load(string file, DataLoaderConfig config = null, DataNode initialNode = null, bool testing = false)
		{
			if (config == null)
			{
				config = DataLoaderConfig.IniFormat2();
			}

			if (string.IsNullOrWhiteSpace(file))
			{
				return initialNode ?? new DataNode();
			}

			return CreateData(file, config, initialNode, testing);
		}

		// TODO: Unsuppress this warning.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static DataNode CreateData(string file, DataLoaderConfig cfg, DataNode initialNode = null, bool testing = false)
		{
			Stack<DataNode> stack = new Stack<DataNode>();
			stack.Push(initialNode ?? new DataNode());
			StringBuilder vague = new StringBuilder();
			StringBuilder spaceTemp = new StringBuilder();
			string key = string.Empty;
			char insideString = '\0';
			bool insideSingleLineComment = false;
			bool insideMultiLineComment = false;
			char lastChar = ' ';
			char eatenChar = ' ';
			bool hadHitNewline = false;
			bool hadHitNewlinePopAfterwards = false;
			bool skipAppendCharacter = false;
			char c;
			for (int index = 0; index <= file.Length+1; index++)
			{
				if (index == file.Length + 1)
				{
					c = '\0';
				}
				else if (index == file.Length)
				{
					c = '\x0A';
				}
				else
				{
					c = file[index];
				}
				if (testing)
				{
					Console.Write(c);
				}
				if (insideSingleLineComment)
				{
					switch (c)
					{
						case '\x0D': // CR
							c = lastChar;
							break;
						case '\x0A': // LF
							insideSingleLineComment = false;
							hadHitNewline = true;
							break;
					}
				}
				else if (insideMultiLineComment)
				{
					switch (c)
					{
						case '/':
							if (eatenChar == '*')
							{
								eatenChar = ' ';
								insideMultiLineComment = false;
							}
							break;
						default:
							eatenChar = c;
							hadHitNewline = true;
							break;
					}
				}
				else if (insideString != '\0')
				{
					if (lastChar == '\\')
					{
						if (c == '\\')
						{
							eatenChar = '\\';
							c = ' '; // So that lastChar will get set to this.
							skipAppendCharacter = true;
							goto LABEL_APPEND;
						}
						if (c == '"' || c == '\'')
						{
							eatenChar = ' ';
						}
						goto LABEL_APPEND;
					}
					if (c == insideString)
					{
						if (cfg.IncludeQuotationMarks)
						{
							vague.Append(insideString);
							eatenChar = ' '; // Is this needed? Could it hurt?
						}
						insideString = '\0';
					}
					else if (c == '\\')
					{
						eatenChar = '\\';
					}
					else
					{
						goto LABEL_APPEND;
					}
					goto LABEL_FALLTHROUGH;

					LABEL_APPEND:
					if (eatenChar != ' ')
					{
						vague.Append(eatenChar);
						eatenChar = ' ';
					}
					if (!skipAppendCharacter)
					{
						vague.Append(c);
					}
					else
					{
						skipAppendCharacter = false;
					}
				}
				else
				{
					switch (c)
					{
						case '\0':
							skipAppendCharacter = true;
							goto default;
						case '\t':
						case ' ':
							if (vague.Length > 0 && !hadHitNewline)
							{
								c = ' ';
								goto default;
							}
							break;
						case '\x0D': // CR
							c = lastChar;
							break;
						case '\x0A': // LF
							if (cfg.NewlineEndsKeyValuePair) // We're exiting a key=value pair.
							{
								if (!(cfg.CommaFollowedByNewlineEatsTheNewline && lastChar == ','))
								{
									goto LABEL_EndKeyValuePair;
								}
							}
							break;
						case '}':
							if (cfg.UseSubnodes) // We're exiting a node.
							{
								hadHitNewline = true;

								hadHitNewlinePopAfterwards = true;
								skipAppendCharacter = true;
								//break;
							}
							goto default;
						case '/':
							if (cfg.UseCSingleLineComments || cfg.UseCMultiLineComments)
							{
								if (cfg.UseCSingleLineComments)
								{
									if (lastChar == '/')
									{
										eatenChar = ' ';
										goto LABEL_SingleLineComment;
									}
									eatenChar = '/';
									break;
								}
								if (cfg.UseCMultiLineComments)
								{
									eatenChar = '/';
									break;
								}
							}
							goto default;
						case '*':
							if (cfg.UseCMultiLineComments && lastChar == '/')
							{
								goto LABEL_MultiLineComment;
							}
							goto default;
						case '{':
							if (cfg.UseSubnodes) // We're entering a Subnode.
							{
								hadHitNewline = false;

								string vagueToUpper = vague.ToString().ToUpperInvariant();
								if (key.Length > 0)
								{
									vague.Clear();
									vague.Append(key);
									key = string.Empty;
									EnteringNode(stack, vague, spaceTemp);
								}
								else
								{
									EnteringNode(stack, vague, spaceTemp);
								}
								break;
							}
							goto default;
						case '[':
							if (cfg.UseSections) // We're entering a section header.
							{
								if (stack.Count > 1)
								{
									stack.Pop();
								}
								vague.Clear();
								break;
							}
							goto default;
						case ']':
							if (cfg.UseSections) // We're entering a section.
							{
								EnteringNode(stack, vague, spaceTemp);
								break;
							}
							goto default;
						case '=':
							if (cfg.EqualsSignMarksEquality) // We're entering a value.
							{
								goto LABEL_EnteringValue;
							}
							goto default;
						case ':':
							if (cfg.ColonMarksEquality) // We're entering a value.
							{
								goto LABEL_EnteringValue;
							}
							goto default;
						case ';':
							if (cfg.SemicolonEndsKeyValuePair) // We're exiting a key=value pair.
							{
								goto LABEL_EndKeyValuePair;
							}
							if (cfg.SemicolonStartsComment) // We're entering a single-line comment
							{
								goto LABEL_SingleLineComment;
							}
							goto default;
						case '#':
							if (cfg.HashtagStartsComment) // We're entering a single-line comment
							{
								goto LABEL_SingleLineComment;
							}
							goto default;
						case '"': // We're entering a string.
							if (cfg.SpaceMarksEquality)
							{
								if (key.Length == 0 && vague.Length > 0 && !vague.ToString().Contains(" "))
								{
									key = vague.ToString();
									vague.Clear();
								}
							}
							insideString = '"';
							spaceTemp.Clear(); // TODO: Does this help or make things worse?
							if (cfg.IncludeQuotationMarks)
							{
								vague.Append('"');
							}
							break;
						case '\'': // We're entering a string.
							if (vague.Length > 0)
							{
								key = vague.ToString();
							}
							insideString = '\'';
							vague.Clear();
							spaceTemp.Clear(); // TODO: Does this help or make things worse?
							if (cfg.IncludeQuotationMarks)
							{
								vague.Append('\'');
							}
							break;
						default:
							if (hadHitNewline)
							{
								hadHitNewline = false;
								if (vague.Length > 0 && key.Length == 0)
								{
									string vagueToString = vague.ToString();
									/*int quotePosition = Math.Min(vagueToString.IndexOf('"'), vagueToString.IndexOf('\''));
									int quotePosition2 = Math.Max(vagueToString.LastIndexOf('"'), vagueToString.LastIndexOf('\''));
									int spacePosition = vagueToString.IndexOf(' ');
									if (vagueToString.Contains("#") || key.Contains("#"))
									{
										Console.Write(' ');
									}*/

									int spacePos = vagueToString.IndexOf(" ", StringComparison.Ordinal);
									if (cfg.SpaceMarksEquality && spacePos != -1) //&& spacePosition != -1 && (quotePosition == -1 || (quotePosition > spacePosition)))
									{
										string newKey = vagueToString.Substring(0, spacePos);
										string newValue = vagueToString.Substring(spacePos, vagueToString.Length - spacePos);

										if (HandlePreprocessor(ref newKey, ref newValue, cfg, stack.Peek()))
										{
											key = string.Empty;
											spaceTemp.Clear();
											vague.Clear();
										}
										else
										{
											key = newKey;
											EndKeyValuePair(newValue, stack, ref key, spaceTemp, cfg);
											vague.Clear();
										}
									}
									else if (cfg.AllowFlags)
									{
										AddFlag(vague, stack, spaceTemp);
									}
								}
								else if (key.Length > 0 || cfg.NewlineEndsKeyValuePairEvenIfValueIsEmpty)
								{
									string newValue = vague.ToString();
									if (HandlePreprocessor(ref key, ref newValue, cfg, stack.Peek()))
									{
										key = string.Empty;
										vague.Clear();
									}
									else
									{
										EndKeyValuePair(newValue, stack, ref key, spaceTemp, cfg);
										vague.Clear();
									}
								}
								if (hadHitNewlinePopAfterwards)
								{
									stack.Pop();
									hadHitNewlinePopAfterwards = false;
								}
							}

							if (skipAppendCharacter)
							{
								skipAppendCharacter = false;
								break;
							}

							//LABEL_defaultSkipNewlineHandling:
							if (eatenChar != ' ')
							{
								AppendCharacter(vague, '/', spaceTemp);
								eatenChar = ' ';
							}

							AppendCharacter(vague, c, spaceTemp);
							break;

							LABEL_SingleLineComment: // HACK: Yup, this is goto. It is being used as intended for C#.
							hadHitNewline = true;
							insideSingleLineComment = true;
							break;

							LABEL_MultiLineComment:
							/*if (key.Length > 0)
							{
								EndKeyValuePair(vague, stack, ref key, spaceTemp);
							}*/
							insideMultiLineComment = true;
							break;

							LABEL_EnteringValue:
							key = EnteringValue(vague, spaceTemp);
							break;

							LABEL_EndKeyValuePair:
							hadHitNewline = true;
							//EndKeyValuePair(vague.ToString(), stack, ref key, spaceTemp, cfg);
							//vague.Clear();
							break;
					}
				}

				LABEL_FALLTHROUGH:
				lastChar = c;
			}

			while (stack.Count > 1)
			{
				stack.Pop();
			}

			return stack.Count > 0 ? stack.Pop() : null;
		}

		private static bool HandlePreprocessor(ref string key, ref string newValue, DataLoaderConfig cfg, DataNode node)
		{
			if (key.ToUpperInvariant() == "#INCLUDE")
			{
				if (cfg.PreprocessorInclude != null)
				{
					DataLoader.Load(cfg.PreprocessorInclude(newValue), cfg, node);
				}
				return true;
			}
			if (key.ToUpperInvariant() == "VAR")
			{
				node.Lines.Add($"{key} {newValue.Trim()}");
				return true;
			}
			return false;
		}

		private static void AppendCharacter(StringBuilder vague, char c, StringBuilder spaceTemp)
		{
			if (c == ' ')
			{
				spaceTemp.Append(c);
			}
			else
			{
				vague.Append(spaceTemp);
				vague.Append(c);
				spaceTemp.Clear();
			}
		}

		private static string EnteringValue(StringBuilder vague, StringBuilder spaceTemp)
		{
			string key = vague.ToString();
			vague.Clear();
			spaceTemp.Clear();
			return key;
		}

		private static void EnteringNode(Stack<DataNode> stack, StringBuilder vague, StringBuilder spaceTemp)
		{
			DataNode node = new DataNode();
			stack.Peek().Subnodes.Add(node);
			stack.Push(node);
			stack.Peek().Header = vague.ToString();
			vague.Clear();
			spaceTemp.Clear();
		}

		private static void EndKeyValuePair(string vague, Stack<DataNode> stack, ref string key, StringBuilder spaceTemp, DataLoaderConfig cfg)
		{
			DataNode curNode = stack.Peek();
			if (key.Length > 0)
			{
				curNode.Values[key.ToUpper(CultureInfo.InvariantCulture).Trim()] = vague.Trim();
				key = string.Empty;
				spaceTemp.Clear();
			}
		}

		private static void AddFlag(StringBuilder flag, Stack<DataNode> stack, StringBuilder spaceTemp)
		{
			if (flag.Length > 0)
			{
				DataNode curNode = stack.Peek();
				curNode.Lines.Add(flag.ToString().ToUpper(CultureInfo.InvariantCulture));
				/*if (curNode.Values.ContainsKey("FLAGS"))
				{
					curNode.Values["FLAGS"] = $"{curNode.Values["FLAGS"]}, {flag.ToString().ToUpper(CultureInfo.InvariantCulture)}";
				}
				else
				{
					curNode.Values["FLAGS"] = flag.ToString().ToUpper(CultureInfo.InvariantCulture);
				}*/
				flag.Clear();
				spaceTemp.Clear();
			}
		}
	}
}

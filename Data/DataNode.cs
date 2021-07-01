using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bridle.Utilities.Data
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ini")]
	public class DataNode
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Subnodes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public List<DataNode> Subnodes = new List<DataNode>();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<string, string> Values = new Dictionary<string, string>();
		public List<string> Lines = new List<string>();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public string Header = string.Empty;

		public static DataType GetValueType(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return DataType.Nil;
			}

			if (value.Contains("\"")) // Search for a string instead of a character because the version that searches for a character uses Linq
			{
				return DataType.QuotedString;
			}

			bool canBeInteger = true;
			bool canBeFloat = true;
			bool discountFloatPossibility = false;
			bool hadDot = false;
			if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				canBeFloat = false;
				for (int i = 2; i < value.Length; i++)
				{
					if ((value[i] >= 0x30 && value[i] <= 0x39) || // 0-9
						(value[i] >= 0x41 && value[i] <= 0x46) || // A-Z
						(value[i] >= 0x61 && value[i] <= 0x66))   // a-z
					{
						continue;
					}

					canBeInteger = false;
					break;
				}
			}
			else
			{
				foreach (char t in value)
				{
					if (discountFloatPossibility)
					{
						canBeFloat = false;
					}

					if ((t >= 0x30 && t <= 0x39) || t == '-')
					{
						continue;
					}

					canBeInteger = false;

					if (t == 'f' || t == 'd')
					{
						discountFloatPossibility = true;
						continue;
					}

					if (t == '.')
					{
						if (hadDot)
						{
							canBeFloat = false;
						}
						else
						{
							hadDot = true;
						}
						continue;
					}

					canBeFloat = false;
					break;
				}
			}

			if (canBeInteger)
			{
				return DataType.Integer;
			}

			if (canBeFloat)
			{
				return DataType.Float;
			}

			return DataType.Keyword;
		}

		public void SetValue(string variableName, bool value)
		{
			if (variableName == null)
			{
				throw new ArgumentNullException(nameof(variableName));
			}

			Values[variableName.Trim().ToUpperInvariant()] = value.ToString();
		}

		public void SetValue(string variableName, int value)
		{
			if (variableName == null)
			{
				throw new ArgumentNullException(nameof(variableName));
			}

			Values[variableName.Trim().ToUpperInvariant()] = value.ToString(CultureInfo.InvariantCulture);
		}

		public void SetValue(string variableName, string value)
		{
			if (variableName == null)
			{
				throw new ArgumentNullException(nameof(variableName));
			}

			Values[variableName.Trim().ToUpperInvariant()] = value;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", Header);
			if (Subnodes.Count > 0)
			{
				s.AppendFormat(CultureInfo.InvariantCulture, ", Subnodes: {0}", Subnodes.Count);
			}

			if (Values.Count > 0)
			{
				s.AppendFormat(CultureInfo.InvariantCulture, ", Values: {0}", Values.Count);
			}

			/*if (Lists.Count > 0)
			{
				s += $", Lists: {Lists.Count}";
			}*/

			return s.ToString();
		}

		public string ToStringVerbose(DataLoaderConfig cfg, bool includeOuter = true)
		{
			string equalsTxt = " ", endTxt = "";
			if (cfg.EqualsSignMarksEquality)
			{
				equalsTxt = " = ";
			}
			if (cfg.SemicolonEndsKeyValuePair)
			{
				endTxt = ";";
			}
			StringBuilder s = new StringBuilder();
			if (includeOuter)
			{
				s.AppendLine(Header);
				s.AppendLine("{");
			}

			if (Lines.Count > 0)
			{
				foreach (string line in Lines)
				{
					s.AppendLine($"{line}{endTxt}");
				}
			}

			/*if (flagsHack && Values.ContainsKey("FLAGS"))
			{
				string flags = Values["FLAGS"];
				if (flags.Length != 0)
				{
					if (flags.IndexOf(',') == -1)
					{
						s.AppendLine(flags.Trim());
					}
					else
					{
						string[] flagsSplit = flags.Split(',');
						foreach (string flag in flagsSplit)
						{
							s.AppendLine(flag.Trim());
						}
					}
				}
			}*/

			if (Values.Count > 0)
			{
				foreach (KeyValuePair<string, string> kvp in Values)
				{
					//if (!(flagsHack && kvp.Key == "FLAGS"))
					{
						if (kvp.Value.IndexOf('{') == 0)
						{
							s.AppendLine($"{kvp.Key}{equalsTxt}{kvp.Value}");
						}
						else
						{
							s.AppendLine($"{kvp.Key}{equalsTxt}{kvp.Value}{endTxt}");
							/*switch (GetValueType(kvp.Value))
							{
								case DataType.Integer:
								case DataType.Float:
									s.AppendLine($"{kvp.Key}{equalsTxt}{kvp.Value}");
									break;
								case DataType.QuotedString:
								case DataType.Keyword:
								case DataType.Nil:
									s.AppendLine($"{kvp.Key}{equalsTxt}\"{kvp.Value}\"");
									break;
							}*/
						}
					}
				}
			}

			if (Subnodes.Count > 0)
			{
				foreach (DataNode subnode in Subnodes)
				{
					s.AppendLine(subnode.ToStringVerbose(cfg));
				}
			}

			/*if (Lists.Count > 0)
			{
				s += $", Lists: {Lists.Count}";
			}*/

			if (includeOuter)
			{
				s.AppendLine("}");
			}
			return s.ToString();
		}

		public DataNode GetSubnode(string subnodeHeader)
		{
			foreach (DataNode dataNode in Subnodes)
			{
				if (dataNode.Header == subnodeHeader)
				{
					return dataNode;
				}
			}
			return null;
		}

		/// <summary>
		/// NOTE: IF THIS DATANODE RECURSES, THIS WILL TURN INTO AN INFINITE LOOP
		/// </summary>
		public DataNode Clone()
		{
			DataNode temp = new DataNode();
			temp.Header = Header;
			temp.Lines = new List<string>(Lines);
			temp.Values = new Dictionary<string, string>(Values);
			foreach (DataNode subnode in Subnodes)
			{
				temp.Subnodes.Add(subnode.Clone());
			}
			return temp;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public bool SetFromNode(string key, ref string variable)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return false; }
			string value = Values[key];
			switch (GetValueType(value))
			{
				case DataType.QuotedString:
					variable = value.Substring(1, value.Length - 2);
					return true;
				default:
					variable = value;
					return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public string GetRawValue(string key)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.Trim().ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return null; }
			string value = Values[key];
			switch (GetValueType(value))
			{
				case DataType.QuotedString:
					return value.Substring(1, value.Length - 2);
				default:
					return value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public bool SetFromNode(string key, ref double variable)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return false; }
			string value = Values[key];
			switch (GetValueType(value))
			{
				case DataType.Integer:
					variable = Convert.ToInt32(value, CultureInfo.InvariantCulture);
					return true;
				case DataType.Float:
					variable = Convert.ToDouble(value, CultureInfo.InvariantCulture);
					return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public bool SetFromNode(string key, ref float variable)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return false; }
			string value = Values[key];
			switch (GetValueType(value))
			{
				case DataType.Integer:
					variable = Convert.ToInt32(value, CultureInfo.InvariantCulture);
					return true;
				case DataType.Float:
					return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out variable);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public bool SetFromNode(string key, ref int variable)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return false; }
			string value = Values[key];
			switch (GetValueType(value))
			{
				case DataType.Integer:
					variable = Convert.ToInt32(value, CultureInfo.InvariantCulture);
					return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
		public bool SetFromNode(string key, ref bool variable)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }
			key = key.ToUpper(CultureInfo.InvariantCulture);
			if (!Values.ContainsKey(key)) { return false; }
			string value = Values[key].ToUpper(CultureInfo.InvariantCulture);
			switch (GetValueType(Values[key]))
			{
				case DataType.Keyword:
					switch (value)
					{
						case "TRUE":
							//case "yes":
							//case "on":
							variable = true;
							return true;
						case "FALSE":
							//case "no":
							//case "off":
							variable = false;
							return true;
						default:
							return false;
					}
				default:
					return false;
			}
		}
	}
}

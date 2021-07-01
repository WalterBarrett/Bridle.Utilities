namespace Bridle.Utilities.Data
{
	public class DataLoaderConfig
	{
		public bool SemicolonEndsKeyValuePair { get; set; }
		public bool SemicolonStartsComment { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Newline")]
		public bool NewlineEndsKeyValuePair { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Newline")]
		public bool NewlineEndsKeyValuePairEvenIfValueIsEmpty { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Subnodes")]
		public bool UseSubnodes { get; set; }
		public bool UseSections { get; set; }
		public bool EqualsSignMarksEquality { get; set; }
		public bool ColonMarksEquality { get; set; }
		public bool HashtagStartsComment { get; set; }
		public bool UseCSingleLineComments { get; set; }
		public bool UseCMultiLineComments { get; set; }
		public bool SpaceMarksEquality { get; set; }
		public bool AllowFlags { get; set; }
		public bool IncludeQuotationMarks { get; set; }
		public bool CommaFollowedByNewlineEatsTheNewline { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ini")]
		public static DataLoaderConfig IniFormat()
		{
			return new DataLoaderConfig
			{
				SemicolonEndsKeyValuePair = false,
				SemicolonStartsComment = true,
				HashtagStartsComment = false,
				NewlineEndsKeyValuePair = true,
				UseSubnodes = false,
				UseSections = true,
				EqualsSignMarksEquality = true,
				ColonMarksEquality = false,
				NewlineEndsKeyValuePairEvenIfValueIsEmpty = true,
				UseCSingleLineComments = false,
				UseCMultiLineComments = false,
				SpaceMarksEquality = false,
				AllowFlags = false,
				IncludeQuotationMarks = false,
				CommaFollowedByNewlineEatsTheNewline = false,
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ini")]
		public static DataLoaderConfig IniFormat2(IncludeDelegate includeDelegate = null)
		{
			return new DataLoaderConfig
			{
				SemicolonEndsKeyValuePair = true,
				SemicolonStartsComment = false,
				HashtagStartsComment = false,
				NewlineEndsKeyValuePair = true,
				UseSubnodes = true,
				UseSections = false,
				EqualsSignMarksEquality = true,
				ColonMarksEquality = false,
				NewlineEndsKeyValuePairEvenIfValueIsEmpty = false,
				UseCSingleLineComments = true,
				UseCMultiLineComments = true,
				SpaceMarksEquality = false,
				AllowFlags = true,
				PreprocessorInclude = includeDelegate,
				IncludeQuotationMarks = true,
				CommaFollowedByNewlineEatsTheNewline = true,
			};
		}

		public IncludeDelegate PreprocessorInclude;

		public delegate string IncludeDelegate(string filename);
	}
}

using System;

namespace OpenIDENet.Languages
{
	public enum SupportedLanguage
	{
		All,
		CSharp
	}

	public class SupportedLanguageConverter
	{
		public string FromEnum(SupportedLanguage language)
		{
			return language.ToString();
		}

		public SupportedLanguage FromString(string language)
		{
			if (FromEnum(SupportedLanguage.All) == language)
				return SupportedLanguage.All;
			if (FromEnum(SupportedLanguage.CSharp) == language)
				return SupportedLanguage.CSharp;
			throw new UnsupportedLanguageException(language);
		}
	}

	public class UnsupportedLanguageException : Exception
	{
		public UnsupportedLanguageException(string language) :
			base ("Unsupported language: " + language)
		{
		}
	}
}

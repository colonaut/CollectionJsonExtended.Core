using System.Text;

namespace CollectionJsonExtended.Core.Extensions
{
    internal static class StringExtensions
    {
        internal static string ToDisplayName(this string propertyName)
        {
            var builder = new StringBuilder();
            if (string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }
            propertyName = PascalCase(propertyName);
            for (int i = 0; i < (propertyName.Length - 1); i++)
            {
                builder.Append(propertyName[i]);
                if (char.IsLower(propertyName[i]) && char.IsUpper(propertyName[i + 1]))
                {
                    builder.Append(' ');
                }
            }
            builder.Append(propertyName[propertyName.Length - 1]);
            return builder.ToString();
        }

        internal static string CamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            bool flag = NoLowerCase(value);
            var builder = new StringBuilder();
            if (!IsSeparatorChar(value[0]))
            {
                builder.Append(char.ToLower(value[0]));
            }
            for (int i = 1; i < value.Length; i++)
            {
                if (!IsSeparatorChar(value[i]))
                {
                    if (IsSeparatorChar(value[i - 1]))
                    {
                        builder.Append(char.ToUpper(value[i]));
                    }
                    else if (flag)
                    {
                        builder.Append(char.ToLower(value[i]));
                    }
                    else
                    {
                        builder.Append(value[i]);
                    }
                }
            }
            return builder.ToString();
        }

        internal static string PascalCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            bool flag = NoLowerCase(value);
            var builder = new StringBuilder();
            if (!IsSeparatorChar(value[0]))
            {
                builder.Append(char.ToUpper(value[0]));
            }
            for (int i = 1; i < value.Length; i++)
            {
                if (!IsSeparatorChar(value[i]))
                {
                    if (IsSeparatorChar(value[i - 1]))
                    {
                        builder.Append(char.ToUpper(value[i]));
                    }
                    else if (flag)
                    {
                        builder.Append(char.ToLower(value[i]));
                    }
                    else
                    {
                        builder.Append(value[i]);
                    }
                }
            }
            return builder.ToString();
        }


        static bool IsSeparatorChar(char value)
        {
            return !char.IsLetterOrDigit(value);
        }

        static bool NoLowerCase(string value)
        {
            foreach (char ch in value)
            {
                if (char.IsLower(ch))
                {
                    return false;
                }
            }
            return true;
        }

    }

}
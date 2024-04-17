using Slugify;

namespace System;

public static class StringExtensions
{
    private static SlugHelper _Slugs = new(new SlugHelperConfiguration
    {
        CollapseDashes = true,
        CollapseWhiteSpace = true,
        ForceLowerCase = true,
        TrimWhitespace = true,
    });

    public static string Slugify(this string value)
    {
        return _Slugs.GenerateSlug(value);
    }
}
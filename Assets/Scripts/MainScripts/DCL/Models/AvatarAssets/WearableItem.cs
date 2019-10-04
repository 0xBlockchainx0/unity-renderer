﻿using System;
using System.Collections.Generic;
using System.Linq;
using DCL;

[System.Serializable]
public class WearableItem : Item
{
    public static string bodyShapeCategory = "body_shape";
    public static string baseWearableTag = "base-wearable";
    public static string nftWearableTag = "exclusive";
    
    [Serializable]
    public class Representation
    {
        public string[] bodyShapes;
        public string mainFile;
        public ContentProvider.MappingPair[] contents;
    }

    public Representation[] representations;
    public string category;
    public string[] tags;
    public string baseUrl;
    public i18n[] i18n;
    public string thumbnail;

    public Representation GetRepresentation(string bodyShapeType)
    {
        if (representations == null) return null;

        for (int i = 0; i < representations.Length; i++)
        {
            if (representations[i].bodyShapes.Contains(bodyShapeType))
            {
                return representations[i];
            }
        }

        return null;
    }

    private readonly Dictionary<string, ContentProvider> cachedContentProviers = new Dictionary<string, ContentProvider>();

    public ContentProvider GetContentProvider(string bodyShapeType)
    {
        var representation = GetRepresentation(bodyShapeType);

        if (representation == null) return null;

        if (!cachedContentProviers.ContainsKey(bodyShapeType))
        {
            var contentProvider = new ContentProvider
            {
                baseUrl = baseUrl,
                contents = representation.contents.ToList()
            };
            contentProvider.BakeHashes();
            cachedContentProviers.Add(bodyShapeType, contentProvider);
        }

        return cachedContentProviers[bodyShapeType];
    }

    public bool SupportsBodyShape(string bodyShapeType)
    {
        if (representations == null) return false;

        for (int i = 0; i < representations.Length; i++)
        {
            if (representations[i].bodyShapes.Contains(bodyShapeType))
            {
                return true;
            }
        }

        return false;
    }
}

[System.Serializable]
public class WearableContent
{
    public string file;
    public string hash;
}

[System.Serializable]
public class i18n
{
    public string code;
    public string text;
}
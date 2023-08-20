using System.Collections.Generic;

namespace DGames.Essentials.Editor
{
    public struct LayoutInfo
    {
        public string Path
        {
            get => string.Join("/",PathSegments);
            set => PathSegments = value.Split('/');
        }
        
        public string[] PathSegments { get; private set; }
        public string[] GroupPathSegments { get; private set; }

        public IEnumerable<string> FullSegments
        {
            get
            {
                for (var i = 0; i < PathSegments.Length; i++)
                {
                    yield return GroupPathSegments[i];
                    yield return PathSegments[i];
                }
            }
        }

        public string GroupPath
        {
            get => string.Join("/",GroupPathSegments);
            set => GroupPathSegments = value.Split("/");
        }

        public object LayoutType { get; set; }
    }
}
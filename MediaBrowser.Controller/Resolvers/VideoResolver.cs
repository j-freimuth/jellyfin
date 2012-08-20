﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Controller.Resolvers
{
    /// <summary>
    /// Resolves a Path into a Video
    /// </summary>
    [Export(typeof(IBaseItemResolver))]
    public class VideoResolver : BaseVideoResolver<Video>
    {
        public override ResolverPriority Priority
        {
            get { return ResolverPriority.Last; }
        }
    }

    /// <summary>
    /// Resolves a Path into a Video or Video subclass
    /// </summary>
    public abstract class BaseVideoResolver<T> : BaseItemResolver<T>
        where T : Video, new()
    {
        protected override T Resolve(ItemResolveEventArgs args)
        {
            // If the path is a file check for a matching extensions
            if (!args.IsFolder)
            {
                if (IsVideoFile(args.Path))
                {
                    return new T()
                    {
                        VideoType = VideoType.VideoFile,
                        Path = args.Path
                    };
                }
            }

            else
            {
                // If the path is a folder, check if it's bluray or dvd
                T item = ResolveFromFolderName(args.Path);

                if (item != null)
                {
                    return item;
                }

                // Also check the subfolders for bluray or dvd
                foreach (KeyValuePair<string, WIN32_FIND_DATA> folder in args.FileSystemChildren)
                {
                    if (!folder.Value.IsDirectory)
                    {
                        continue;
                    }

                    item = ResolveFromFolderName(folder.Key);

                    if (item != null)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private T ResolveFromFolderName(string folder)
        {
            if (folder.IndexOf("video_ts", System.StringComparison.OrdinalIgnoreCase) != -1)
            {
                return new T()
                {
                    VideoType = VideoType.DVD,
                    Path = Path.GetDirectoryName(folder)
                };
            }
            if (folder.IndexOf("bdmv", System.StringComparison.OrdinalIgnoreCase) != -1)
            {
                return new T()
                {
                    VideoType = VideoType.BluRay,
                    Path = Path.GetDirectoryName(folder)
                };
            }

            return null;
        }

        private static bool IsVideoFile(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            switch (extension)
            {
                case ".mkv":
                case ".m2ts":
                case ".iso":
                case ".ts":
                case ".rmvb":
                case ".mov":
                case ".avi":
                case ".mpg":
                case ".mpeg":
                case ".wmv":
                case ".mp4":
                case ".divx":
                case ".dvr-ms":
                case ".wtv":
                case ".ogm":
                case ".ogv":
                case ".asf":
                case ".m4v":
                case ".flv":
                case ".f4v":
                case ".3gp":
                    return true;

                default:
                    return false;
            }
        }
    }
}

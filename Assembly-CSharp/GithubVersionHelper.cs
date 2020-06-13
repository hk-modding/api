using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

// ReSharper disable All (Disabled class anyways)

namespace Modding
{
    /// <summary>
    ///     Class to help determine if your version is out of date.
    /// </summary>
    public class GithubVersionHelper
    {
        private static readonly WebClient WebClient = new WebClient();

        private readonly string _repositoryName;

        /// <summary>
        ///     Provides a convenient method to access the Github Release Information, Allowing for simple checking of released
        ///     versions.
        /// </summary>
        /// <param name="repositoryName">Repository Name such as "seanpr96/HollowKnight.Modding"</param>
        public GithubVersionHelper(string repositoryName)
        {
            _repositoryName = repositoryName;
        }

        private static GithubRelease FromJson(string json) => JsonUtility.FromJson<GithubRelease>(json);

        /// <summary>
        ///     Fetches the Current Release Version From Github
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            // Wyza - Have to disable this.  Unity doesn't support TLS 1.2 and github removed TLS 1.0/1.1 support.  Grumble

            return "0.0";
            // try
            // {
            //     //This needs to be added on every call
            //     WebClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2");
            //     string url = $"https://api.github.com/repos/{_repositoryName}/releases/latest";
            //     Logger.APILogger.LogDebug("Fetching " + url);
            //     string json = WebClient.DownloadString(url);
            //     Logger.APILogger.LogFine("" + json);
            //     string version = FromJson(json)?.tag_name;
            //     Logger.APILogger.LogDebug("Version Found: " + version);
            //     return version;
            // }
            // catch (Exception ex)
            // {
            //     Logger.LogError("Failed to fetch url with error: \n" +ex);
            // }
            // return string.Empty;
        }
        // ReSharper disable All
#pragma warning disable 0649
        private class Author
        {
            public string avatar_url;
            public string events_url;
            public string followers_url;
            public string following_url;
            public string gists_url;
            public string gravatar_id;
            public string html_url;
            public int id;
            public string login;
            public string organizations_url;
            public string received_events_url;
            public string repos_url;
            public bool site_admin;
            public string starred_url;
            public string subscriptions_url;
            public string type;
            public string url;
        }

        private class GithubRelease
        {
            public List<object> assets;
            public string assets_url;
            public Author author;
            public string body;
            public DateTime created_at;
            public bool draft;
            public string html_url;
            public int id;
            public string name;
            public bool prerelease;
            public DateTime published_at;
            public string tag_name;
            public string tarball_url;
            public string target_commitish;
            public string upload_url;
            public string url;
            public string zipball_url;
        }
        // ReSharper enable All
#pragma warning restore 0649
    }
}
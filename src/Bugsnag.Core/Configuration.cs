﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugsnag.Core
{
    /// <summary>
    /// Defines the configuration for a sending notifications to Bugsnag
    /// </summary>
    public class Configuration : IConfiguration
    {
        /// <summary>
        /// Gets the API key used to send notifications to a specific Bugsnag account
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// Gets or sets the application version
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the release stage of the application
        /// </summary>
        public string ReleaseStage { get; set; }

        /// <summary>
        /// Gets or sets the endpoint that defines where to send the notifications
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notifications should be sent using SSL
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets the unique identifier used to identify a user
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the users email
        /// </summary>
        public string UserEmail { get; private set; }

        /// <summary>
        /// Gets the users human readable name
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the current logged in user name
        /// </summary>
        public string LoggedOnUser { get; private set; }

        /// <summary>
        /// Gets or sets the context to apply to the subsequent notifications
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Gets the metadata to send with every error report
        /// </summary>
        public Metadata Metadata { get; private set; }

        /// <summary>
        /// Gets the endpoint URL that notifications will be send to
        /// </summary>
        public Uri EndpointUrl
        {
            get { return new Uri((UseSsl ? @"https://" : "http://") + Endpoint); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether we are auto detecting method calls as 
        /// in project calls in stack traces
        /// </summary>
        public bool AutoDetectInProject { get; set; }

        /// <summary>
        /// Gets or sets a custom function to run just before a notification is sent, the function
        /// operates on an Event and returns a boolean indicating if the notification should
        /// continue to be reported
        /// </summary>
        public Func<Event, bool> BeforeNotifyCallback { get; set; }

        /// <summary>
        /// Internal list of release stages we should notify on
        /// </summary>
        private List<string> notifyReleaseStages;

        /// <summary>
        /// Internal list of file prefixes we should remove from filenames
        /// </summary>
        private List<string> filePrefixes;

        /// <summary>
        /// Internal list of exception class names we should ignore
        /// </summary>
        private List<string> ignoreClasses;

        /// <summary>
        /// Internal list of project namespaces
        /// </summary>
        private List<string> projectNamespaces;

        /// <summary>
        /// Internal list of filters for data
        /// </summary>
        private List<string> filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class. Produces a default configuration with 
        /// default configuration settings
        /// </summary>
        /// <param name="apiKey">The API key linked to a Bugsnag account</param>
        public Configuration(string apiKey)
        {
            ApiKey = apiKey;
            AppVersion = "1.0.0";
            ReleaseStage = "Development";
            Metadata = new Metadata();
            UseSsl = true;
            AutoDetectInProject = true;
            UserId = Environment.UserName;
            LoggedOnUser = Environment.UserDomainName + "\\" + Environment.UserName;
            Context = null;
            Endpoint = "notify.bugsnag.com";
            filePrefixes = new List<string>();
            ignoreClasses = new List<string>();
            projectNamespaces = new List<string>();
            notifyReleaseStages = null;
        }

        /// <summary>
        /// Specifies the release stages that notifications should be sent
        /// </summary>
        /// <param name="releaseStages">The stages to notify on</param>
        public void SetNotifyReleaseStages(params string[] releaseStages)
        {
            notifyReleaseStages = releaseStages.ToList();
        }

        /// <summary>
        /// Resets any restrictions and notifies on all release stages
        /// </summary>
        public void NotifyOnAllReleaseStages()
        {
            notifyReleaseStages = null;
        }

        /// <summary>
        /// Detects if the current release stage is a stage that should be notified on
        /// </summary>
        /// <returns>True if we should notify, otherwise false</returns>
        public bool IsNotifyReleaseStage()
        {
            // Notify if no restrictions have been set or the release stage hasn't been set
            if (notifyReleaseStages == null || ReleaseStage == null)
                return true;

            return notifyReleaseStages.Any(x => x == ReleaseStage);
        }

        /// <summary>
        /// Sets the information about the current user of the system
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="userEmail">The email of the user</param>
        /// <param name="userName">The name of the user</param>
        public void SetUser(string userId, string userEmail, string userName)
        {
            UserId = userId;
            UserEmail = userEmail;
            UserName = userName;
        }

        /// <summary>
        /// Sets the file prefixes that should be removed from frame file paths
        /// </summary>
        /// <param name="prefixes">The prefixes to remove</param>
        public void SetFilePrefix(params string[] prefixes)
        {
            filePrefixes = prefixes.ToList();
        }

        /// <summary>
        /// Removes all file prefixes from a filename
        /// </summary>
        /// <param name="fileName">The filename to modify</param>
        /// <returns>The filename with the prefixes removed</returns>
        public string RemoveFileNamePrefix(string fileName)
        {
            var result = fileName;
            if (result != null)
                filePrefixes.ForEach(x => result = result.Replace(x, string.Empty));
            return result;
        }

        /// <summary>
        /// Sets the project namespaces used to detect local method calls
        /// </summary>
        /// <param name="namespaces">The project namespaces</param>
        public void SetProjectNamespaces(params string[] namespaces)
        {
            projectNamespaces = namespaces.ToList();
        }

        /// <summary>
        /// Indicates if a method name belongs to In Project namespaces
        /// </summary>
        /// <param name="fullMethodName">The fully qualified method name</param>
        /// <returns>True if it belongs to one of the project namespaces, otherwise false</returns>
        public bool IsInProjectNamespace(string fullMethodName)
        {
            return projectNamespaces.Any(x => fullMethodName.StartsWith(x, StringComparison.Ordinal));
        }

        /// <summary>
        /// Sets the exception classes to ignore and not send notifications about
        /// </summary>
        /// <param name="classNames">The exception class names to ignore</param>
        public void SetIgnoreClasses(params string[] classNames)
        {
            ignoreClasses = classNames.ToList();
        }

        /// <summary>
        /// Indicates if an exception class should be ignored based on the previously set ignore
        /// class list
        /// </summary>
        /// <param name="className">The exception class name to check</param>
        /// <returns>True if the class should be ignored, otherwise false</returns>
        public bool IsClassToIgnore(string className)
        {
            return ignoreClasses.Contains(className);
        }

        /// <summary>
        /// Sets the filters that indicate entries that are sensitive 
        /// </summary>
        /// <param name="newFilters">The entries to filter out</param>
        public void SetFilters(params string[] newFilters)
        {
            filters = newFilters.ToList();
        }

        /// <summary>
        /// Indicates if an entry key is a entry that should be filtered
        /// </summary>
        /// <param name="entry">The entry to check</param>
        /// <returns>True if the entry should be filtered, otherwise False</returns>
        public bool IsEntryFiltered(string entry)
        {
            if (filters == null)
                return false;

            return filters.Contains(entry);
        }
    }
}

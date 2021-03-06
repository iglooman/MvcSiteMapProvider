﻿using System;
using MvcSiteMapProvider.Web;

namespace MvcSiteMapProvider.Matching
{
    /// <summary>
    /// Uses an <see cref="T:MvcSiteMapProvider.ISiteMapNode"/>  instance to create a key 
    /// that can be used for matching relative or absolute URLs.
    /// </summary>
    public class SiteMapNodeUrlKey
        : UrlKeyBase
    {
        public SiteMapNodeUrlKey(
            ISiteMapNode node,
            IUrlPath urlPath
            ) 
            : base(urlPath)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            this.node = node;

            // Host name in absolute URL overrides this one.
            this.hostName = node.HostName;
            this.SetUrlValues(node.UnresolvedUrl);
        }
         
        private readonly ISiteMapNode node;

        public override string HostName 
        {
            // The host name of the node can be modified at runtime, so we need to ensure
            // we have the most current value.
            get { return string.IsNullOrEmpty(node.HostName) ? this.hostName : node.HostName; }
        }
    }
}

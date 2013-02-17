﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using System.Linq;
using MvcSiteMapProvider.Web.UrlResolver;
using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// SiteMapNode class. This class represents a node within the SiteMap hierarchy.
    /// It contains all business logic to maintain the node's internal state.
    /// </summary>
    public class SiteMapNode
        : SiteMapNodePositioningBase, ISiteMapNode
    {
        public SiteMapNode(
            ISiteMap siteMap, 
            string key,
            bool isDynamic,
            ISiteMapNodeChildStateFactory siteMapNodeChildStateFactory,
            ILocalizationService localizationService,
            IDynamicNodeProviderStrategy dynamicNodeProviderStrategy,
            ISiteMapNodeUrlResolverStrategy siteMapNodeUrlResolverStrategy,
            ISiteMapNodeVisibilityProviderStrategy siteMapNodeVisibilityProviderStrategy,
            IActionMethodParameterResolver actionMethodParameterResolver,
            IUrlPath urlPath
            )
        {
            if (siteMap == null)
                throw new ArgumentNullException("siteMap");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            if (siteMapNodeChildStateFactory == null)
                throw new ArgumentNullException("siteMapNodeChildStateFactory");
            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (dynamicNodeProviderStrategy == null)
                throw new ArgumentNullException("dynamicNodeProviderStrategy");
            if (siteMapNodeUrlResolverStrategy == null)
                throw new ArgumentNullException("siteMapNodeUrlResolverStrategy");
            if (siteMapNodeVisibilityProviderStrategy == null)
                throw new ArgumentNullException("siteMapNodeVisibilityProviderStrategy");
            if (actionMethodParameterResolver == null)
                throw new ArgumentNullException("actionMethodParameterResolver");
            if (urlPath == null)
                throw new ArgumentNullException("urlPath");

            this.siteMap = siteMap;
            this.key = key;
            this.isDynamic = isDynamic;
            this.localizationService = localizationService;
            this.dynamicNodeProviderStrategy = dynamicNodeProviderStrategy;
            this.siteMapNodeUrlResolverStrategy = siteMapNodeUrlResolverStrategy;
            this.siteMapNodeVisibilityProviderStrategy = siteMapNodeVisibilityProviderStrategy;
            this.actionMethodParameterResolver = actionMethodParameterResolver;
            this.urlPath = urlPath;

            // Initialize child collections
            this.attributes = siteMapNodeChildStateFactory.CreateAttributeCollection(siteMap, localizationService);
            this.routeValues = siteMapNodeChildStateFactory.CreateRouteValueCollection(siteMap);
            this.preservedRouteParameters = siteMapNodeChildStateFactory.CreatePreservedRouteParameterCollection(siteMap);
            this.roles = siteMapNodeChildStateFactory.CreateRoleCollection(siteMap);
        }

        // Services
        protected readonly ILocalizationService localizationService;
        protected readonly IDynamicNodeProviderStrategy dynamicNodeProviderStrategy;
        protected readonly ISiteMapNodeUrlResolverStrategy siteMapNodeUrlResolverStrategy;
        protected readonly ISiteMapNodeVisibilityProviderStrategy siteMapNodeVisibilityProviderStrategy;
        protected readonly IActionMethodParameterResolver actionMethodParameterResolver;
        protected readonly IUrlPath urlPath;

        // Child collections and dictionaries
        protected readonly IAttributeCollection attributes;
        protected readonly IRouteValueCollection routeValues;
        protected readonly IPreservedRouteParameterCollection preservedRouteParameters;
        protected readonly IRoleCollection roles;

        // Object State
        protected readonly string key;
        protected readonly bool isDynamic;
        protected ISiteMap siteMap;
        protected string title = String.Empty;
        protected string description = String.Empty;
        protected DateTime lastModifiedDate = DateTime.MinValue;
        protected ChangeFrequency changeFrequency = ChangeFrequency.Always;
        protected UpdatePriority updatePriority = UpdatePriority.Undefined;
        protected bool clickable = true;
        protected string url = String.Empty;
        protected string resolvedUrl = String.Empty;
        protected string canonicalUrl = String.Empty;
        protected string canonicalKey = String.Empty;

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public override string Key { get { return this.key; } }

        /// <summary>
        /// Gets whether the current node was created from a dynamic source.
        /// </summary>
        /// <value>True if the current node is dynamic.</value>
        public override bool IsDynamic { get { return this.isDynamic; } }

        /// <summary>
        /// Gets whether the current node is read-only.
        /// </summary>
        /// <value>True if the current node is read-only.</value>
        public override bool IsReadOnly { get { return this.SiteMap.IsReadOnly; } }


        /// <summary>
        /// A reference to the root SiteMap object for the current graph.
        /// </summary>
        public override ISiteMap SiteMap
        {
            get { return this.siteMap; }
        }


        /// <summary>
        /// Gets or sets the HTTP method.
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public override string HttpMethod { get; set; }

        /// <summary>
        /// Gets the implicit resource key (optional).
        /// </summary>
        /// <value>The implicit resource key.</value>
        public override string ResourceKey
        {
            get { return this.localizationService.ResourceKey; }
        }

        /// <summary>
        /// Gets or sets the title (optional).
        /// </summary>
        /// <value>The title.</value>
        public override string Title 
        {
            get { return localizationService.GetResourceString("title", this.title, this.SiteMap); }
            set { this.title = localizationService.ExtractExplicitResourceKey("title", value); }
        }

        /// <summary>
        /// Gets or sets the description (optional).
        /// </summary>
        /// <value>The description.</value>
        public override string Description 
        {
            get { return localizationService.GetResourceString("description", this.description, this.SiteMap); }
            set { this.description = localizationService.ExtractExplicitResourceKey("description", value); }
        }

        /// <summary>
        /// Gets or sets the target frame (optional).
        /// </summary>
        /// <value>The target frame.</value>
        public override string TargetFrame { get; set; }

        /// <summary>
        /// Gets or sets the image URL (optional).
        /// </summary>
        /// <value>The image URL.</value>
        public override string ImageUrl { get; set; }

        /// <summary>
        /// Gets the attributes (optional).
        /// </summary>
        /// <value>The attributes.</value>
        public override IAttributeCollection Attributes { get { return this.attributes; } }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public override IRoleCollection Roles { get { return this.roles; } }

        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        /// <value>The last modified date.</value>
        public override DateTime LastModifiedDate 
        { 
            get { return this.lastModifiedDate; } 
            set { this.lastModifiedDate = value; } 
        }

        /// <summary>
        /// Gets or sets the change frequency.
        /// </summary>
        /// <value>The change frequency.</value>
        public override ChangeFrequency ChangeFrequency 
        { 
            get { return this.changeFrequency; }
            set { this.changeFrequency = value; } 
        }

        /// <summary>
        /// Gets or sets the update priority.
        /// </summary>
        /// <value>The update priority.</value>
        public override UpdatePriority UpdatePriority
        { 
            get { return this.updatePriority; } 
            set { this.updatePriority = value; } 
        }


        #region Visibility

        /// <summary>
        /// Gets or sets the name or the type of the visibility provider.
        /// This value will be used to select the concrete type of provider to use to determine
        /// visibility.
        /// </summary>
        /// <value>
        /// The name or type of the visibility provider.
        /// </value>
        public override string VisibilityProvider { get; set; }


        /// <summary>
        /// Determines whether the node is visible.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="sourceMetadata">The source metadata.</param>
        /// <returns>
        /// 	<c>true</c> if the specified node is visible; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsVisible(IDictionary<string, object> sourceMetadata)
        {
            // use strategy factory to provide implementation logic from concrete provider
            // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
            return siteMapNodeVisibilityProviderStrategy.IsVisible(this.VisibilityProvider, this, sourceMetadata);
        }

        #endregion

        #region URL Resolver

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SiteMapNode" /> is clickable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if clickable; otherwise, <c>false</c>.
        /// </value>
        public override bool Clickable
        { 
            get { return this.clickable; }
            set { this.clickable = value; }
        }

        /// <summary>
        /// Gets or sets the name or type of the URL resolver.
        /// </summary>
        /// <value>
        /// The name or type of the URL resolver.
        /// </value>
        public override string UrlResolver { get; set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public override string Url 
        {
            get
            {
                if (!this.Clickable)
                {
                    return string.Empty;
                }
                if (!String.IsNullOrEmpty(this.ResolvedUrl))
                {
                    return this.ResolvedUrl;
                }
                // Only resolve the url if an absolute url is not already set
                if (String.IsNullOrEmpty(this.UnresolvedUrl) || !this.HasAbsoluteUrl())
                {
                    return GetResolvedUrl();
                }
                return this.UnresolvedUrl;
            }
            set
            {
                this.url = value;
            }
        }

        /// <summary>
        /// The raw URL before being evaluated by any URL resovler.
        /// </summary>
        public override string UnresolvedUrl { get { return this.url; } }

        /// <summary>
        /// The resolved url that has been cached, if any.
        /// </summary>
        public override string ResolvedUrl { get { return this.resolvedUrl; } }

        /// <summary>
        /// A value indicating to cache the resolved URL. If false, the URL will be 
        /// resolved every time it is accessed.
        /// </summary>
        public override bool CacheResolvedUrl { get; set; }

        /// <summary>
        /// Sets the ResolvedUrl using the current Url or Url resolver.
        /// </summary>
        public override void ResolveUrl()
        {
            if (this.CacheResolvedUrl && String.IsNullOrEmpty(this.UnresolvedUrl))
            {
                this.resolvedUrl = this.GetResolvedUrl();
            }
        }

        protected string GetResolvedUrl()
        {
            // use strategy factory to provide implementation logic from concrete provider
            // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
            return siteMapNodeUrlResolverStrategy.ResolveUrl(
                this.UrlResolver, this, this.Area, this.Controller, this.Action, this.RouteValues);
        }

        /// <summary>
        /// Gets a boolean value that indicates this is an external URL by checking whether it
        /// looks like an absolute path.
        /// </summary>
        /// <returns></returns>
        public override bool HasAbsoluteUrl()
        {
            return urlPath.IsAbsoluteUrl(this.url);
        }

        /// <summary>
        /// Gets a boolean value that indicates this is an external URL by checking whether it
        /// looks like an absolute path and comparing the DnsSafeHost with the passed in context.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public override bool HasExternalUrl(HttpContextBase httpContext)
        {
            if (!this.HasAbsoluteUrl())
            {
                return false;
            }
            try
            {
                var uri = new Uri(this.url, UriKind.Absolute);
                return !httpContext.Request.Url.DnsSafeHost.Equals(uri.DnsSafeHost);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Canonical Tag

        /// <summary>
        /// Gets or sets the canonical URL.
        /// </summary>
        /// <remarks>May not be used in conjuntion with CanonicalKey. Only 1 canonical value is allowed.</remarks>
        public override string CanonicalUrl 
        {
            get 
            {
                var url = this.canonicalUrl;
                if (!String.IsNullOrEmpty(url))
                {
                    if (urlPath.IsAbsoluteUrl(url))
                    {
                        return url;
                    }
                    return urlPath.MakeRelativeUrlAbsolute(url);
                }
                var key = this.canonicalKey;
                if (!String.IsNullOrEmpty(key))
                {
                    var node = this.SiteMap.FindSiteMapNodeFromKey(key);
                    if (node != null)
                    {
                        return urlPath.MakeRelativeUrlAbsolute(node.Url);
                    }
                }
                return String.Empty;
            }
            set
            {
                if (!this.canonicalUrl.Equals(value))
                {
                    if (!String.IsNullOrEmpty(this.canonicalKey))
                    {
                        throw new ArgumentException(Resources.Messages.SiteMapNodeCanonicalValueAlreadySet, "CanonicalUrl");
                    }
                    this.canonicalUrl = value;
                }
            }
        }

        

        /// <summary>
        /// Gets or sets the canonical key. The key is used to reference another ISiteMapNode to get the canonical URL.
        /// </summary>
        /// <remarks>May not be used in conjuntion with CanonicalUrl. Only 1 canonical value is allowed.</remarks>
        public override string CanonicalKey 
        {
            get { return this.canonicalKey; }
            set
            {
                if (!this.canonicalKey.Equals(value))
                {
                    if (!String.IsNullOrEmpty(this.canonicalUrl))
                    {
                        throw new ArgumentException(Resources.Messages.SiteMapNodeCanonicalValueAlreadySet, "CanonicalKey");
                    }
                    this.canonicalKey = value;
                }
            }
        }

        #endregion

        #region Dynamic Nodes

        /// <summary>
        /// Gets or sets the name or type of the Dynamic Node Provider.
        /// </summary>
        /// <value>
        /// The name or type of the Dynamic Node Provider.
        /// </value>
        public override string DynamicNodeProvider { get; set; }

        /// <summary>
        /// Gets the dynamic node collection.
        /// </summary>
        /// <returns>A dynamic node collection.</returns>
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
        {
            // use strategy factory to provide implementation logic from concrete provider
            // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
            return dynamicNodeProviderStrategy.GetDynamicNodeCollection(this.DynamicNodeProvider);
        }

        /// <summary>
        /// Gets whether the current node has a dynamic node provider.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is a provider; otherwise <c>false</c>.
        /// </value>
        public override bool HasDynamicNodeProvider
        {
            // use strategy factory to provide implementation logic from concrete provider
            // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
            get { return (dynamicNodeProviderStrategy.GetProvider(this.DynamicNodeProvider) != null); }
        }

        #endregion

        #region Route

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>The route.</value>
        public override string Route { get; set; }

        /// <summary>
        /// Gets the route values.
        /// </summary>
        /// <value>The route values.</value>
        public override IRouteValueCollection RouteValues { get { return this.routeValues; } }

        /// <summary>
        /// Gets the preserved route parameter names (= values that will be used from the current request route).
        /// </summary>
        /// <value>The preserved route parameters.</value>
        public override IPreservedRouteParameterCollection PreservedRouteParameters { get { return this.preservedRouteParameters; } }


        /// <summary>
        /// Gets the route data associated with the current node.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The route data associated with the current node.</returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData routeData;
            if (!string.IsNullOrEmpty(this.Route))
            {
                routeData = RouteTable.Routes[this.Route].GetRouteData(httpContext);
            }
            else
            {
                routeData = RouteTable.Routes.GetRouteData(httpContext);
            }
            return routeData;
        }

        /// <summary>
        /// Determines whether this node matches the supplied route values.
        /// </summary>
        /// <param name="routeValues">An IDictionary<string, object> of route values.</param>
        /// <returns><c>true</c> if the route matches this node's RouteValues and Attributes collections; otherwise <c>false</c>.</returns>
        public override bool MatchesRoute(IDictionary<string, object> routeValues)
        {
            var result = this.RouteValues.MatchesRoute(routeValues);
            if (result == true)
            {
                // Find action method parameters?
                IEnumerable<string> actionParameters = new List<string>();
                if (this.IsDynamic == false)
                {
                    actionParameters = actionMethodParameterResolver.ResolveActionMethodParameters(
                        this.Area, this.Controller, this.Action);
                }
                result = this.Attributes.MatchesRoute(actionParameters, routeValues);
            }
            return result;
        }

        #endregion

        #region MVC

        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>The area.</value>
        public override string Area
        {
            get { return RouteValues.ContainsKey("area") && RouteValues["area"] != null ? RouteValues["area"].ToString() : ""; }
            set { RouteValues["area"] = value; }
        }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        /// <value>The controller.</value>
        public override string Controller
        {
            get { return RouteValues.ContainsKey("controller") ? RouteValues["controller"].ToString() : ""; }
            set { RouteValues["controller"] = value; }
        }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>The action.</value>
        public override string Action
        {
            get { return RouteValues.ContainsKey("action") ? RouteValues["action"].ToString() : ""; }
            set { RouteValues["action"] = value; }
        }

        #endregion

        #region CopyTo

        public override void CopyTo(ISiteMapNode node)
        {
            node.ParentNode = this.parentNode;
            foreach (var child in this.ChildNodes)
                node.ChildNodes.Add(child);
            node.HttpMethod = this.HttpMethod;
            node.Title = this.title;
            node.Description = this.description;
            node.TargetFrame = this.TargetFrame;
            node.ImageUrl = this.ImageUrl;
            //foreach (var attribute in this.Attributes)
            //    node.Attributes.Add(attribute);
            //foreach (var role in this.Roles)
            //    node.Roles.Add(role);
            this.Attributes.CopyTo(node.Attributes);
            this.Roles.CopyTo(node.Roles);
            node.LastModifiedDate = this.LastModifiedDate;
            node.ChangeFrequency = this.ChangeFrequency;
            node.UpdatePriority = this.UpdatePriority;
            node.VisibilityProvider = this.VisibilityProvider;
            node.Clickable = this.Clickable;
            node.UrlResolver = this.UrlResolver;
            node.Url = this.url;
            node.CacheResolvedUrl = this.CacheResolvedUrl;
            node.CanonicalUrl = this.canonicalUrl;
            node.CanonicalKey = this.CanonicalKey;
            node.DynamicNodeProvider = this.DynamicNodeProvider;
            node.Route = this.Route;
            //foreach (var routeValue in this.RouteValues)
            //    node.RouteValues.Add(routeValue);
            //foreach (var parameter in this.PreservedRouteParameters)
            //    node.PreservedRouteParameters.Add(parameter);
            this.RouteValues.CopyTo(node.RouteValues);
            this.PreservedRouteParameters.CopyTo(node.PreservedRouteParameters);
            // NOTE: Area, Controller, and Action are covered under RouteValues.
        }

        #endregion


        //#region ICloneable Members

        ///// <summary>
        ///// Creates a new object that is a copy of the current instance.
        ///// </summary>
        ///// <returns>
        ///// A new object that is a copy of this instance.
        ///// </returns>
        //public virtual object Clone()
        //{
        //    //var clone = new SiteMapNode(this.SiteMap, this.key, this.ResourceKey);

        //    var clone = siteMapNodeFactory.Create(this.SiteMap, this.key, this.ResourceKey);
        //    clone.ParentNode = this.ParentNode;

        //    // TODO: implement and cascade call to SiteMapNodeCollection instead of looping here
        //    //clone.ChildNodes = new SiteMapNodeCollection();
        //    //foreach (var childNode in ChildNodes)
        //    //{
        //    //    var childClone = ((SiteMapNode)childNode).Clone() as SiteMapNode;
        //    //    childClone.ParentNode = clone;
        //    //    clone.ChildNodes.Add(childClone);
        //    //}

        //    clone.ChildNodes = (SiteMapNodeCollection)ChildNodes.Clone();
        //    clone.Url = this.Url;
        //    clone.HttpMethod = this.HttpMethod;
        //    clone.Clickable = this.Clickable;
        //    //clone.ResourceKey = this.ResourceKey;
        //    clone.Title = this.Title;
        //    clone.Description = this.Description;
        //    clone.TargetFrame = this.TargetFrame;
        //    clone.ImageUrl = this.ImageUrl;
        //    clone.Attributes = new Dictionary<string, string>(this.Attributes);
        //    clone.Roles = new List<string>(this.Roles);
        //    clone.LastModifiedDate = this.LastModifiedDate;
        //    clone.ChangeFrequency = this.ChangeFrequency;
        //    clone.UpdatePriority = this.UpdatePriority;
        //    clone.VisibilityProvider = this.VisibilityProvider;

        //    // Route
        //    clone.Route = this.Route;
        //    clone.RouteValues = new Dictionary<string, object>(this.RouteValues);
        //    clone.PreservedRouteParameters = this.PreservedRouteParameters;
        //    clone.UrlResolver = this.UrlResolver;

        //    // MVC
        //    clone.Action = this.Action;
        //    clone.Area = this.Area;
        //    clone.Controller = this.Controller;

        //    return clone;
        //}

        //#endregion
    }
}
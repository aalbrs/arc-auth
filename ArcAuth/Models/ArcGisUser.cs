namespace ArcAuth.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    
    public class ArcGisUser
    {
        public string Username { get; set; }
        public object Udn { get; set; }
        public string Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredView { get; set; }
        public object Description { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public object IdpUsername { get; set; }
        public string FavGroupId { get; set; }
        public long LastLogin { get; set; }
        public string Role { get; set; }
        public List<string> Privileges { get; set; }
        public string Level { get; set; }
        public string UserLicenseTypeId { get; set; }
        public bool Disabled { get; set; }
        public string Access { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public string Provider { get; set; }
        public List<ArcGisGroup> Groups { get; set; }
        public AppInfo AppInfo { get; set; }
    }

    public class AppInfo
    {
        public string AppId { get; set; }
        public string ItemId { get; set; }
        public string AppOwner { get; set; }
        public string OrgId { get; set; }
        public string AppTitle { get; set; }
        public List<object> Privileges { get; set; }
    }

    public class ArcGisGroup
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public string Snippet { get; set; }
        public List<string> Tags { get; set; }
        public List<object> TypeKeywords { get; set; }
        public List<string> Capabilities { get; set; }
    }

}

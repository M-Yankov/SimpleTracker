using Android.Content;
using Android.Content.PM;

namespace SimpleTracker.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Returns whether an application is installed or not.
        /// For newer android versions the manifest file must contains defined <paramref name="packageName"/> to return <see langword="true"/>.
        /// </summary>
        /// <param name="packageName">Examples: com.google.android.youtube, com.facebook, com.viber</param>
        /// <returns></returns>
        public static bool IsPackageInstalled(ContextWrapper ctx, string packageName)
        {
            try
            {
                const int PackageInfoFlags = 0;
                ApplicationInfo appInfo = ctx.PackageManager.GetApplicationInfo(packageName, PackageInfoFlags);
                return appInfo != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
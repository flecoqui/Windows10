using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.ApplicationModel.Core;

namespace StartupHelper
{
    class Program
    {
        static AutoResetEvent areWeDone = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            IAsyncOperation<IReadOnlyList<AppListEntry>> result = Windows.ApplicationModel.Package.Current.GetAppListEntriesAsync();
            result.Completed += delegate
            {
                AppListEntry appListEntry = result.GetResults().First();
                appListEntry.LaunchAsync().Completed += delegate
                {
                    areWeDone.Set();
                };
            };
            areWeDone.WaitOne();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace AudioVideoPlayer
{
    class AudioDiscoveryTaskManager
    {
        public const string DiscoveryTaskEntryPoint = "DiscoveryTasks.DiscoveryTask";
        public const string DiscoveryTaskName = "DiscoveryTask";
        public static string DiscoveryTaskProgress = "";
        public static bool DiscoveryTaskRegistered = false;
        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        public static BackgroundTaskRegistration RegisterBackgroundTask(String taskEntryPoint, String name)
        {
            BackgroundTaskRegistration task = null;
            try
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = name;
                builder.TaskEntryPoint = taskEntryPoint;
                DeviceUseTrigger de = new DeviceUseTrigger();
               // de.RequestAsync();
            //    builder.SetTrigger(new TimeTrigger(15, true));
                task = builder.Register();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while registering background task: " + e.Message);
            }
            UpdateBackgroundTaskStatus(name, true);

            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }
        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks(string name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                }
            }

            UpdateBackgroundTaskStatus(name, false);
        }

        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        public static void UpdateBackgroundTaskStatus(String name, bool registered)
        {
            switch (name)
            {
                case DiscoveryTaskName:
                    DiscoveryTaskRegistered = registered;
                    break;
            }
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;
            switch (name)
            {
                case DiscoveryTaskName:
                    registered = DiscoveryTaskRegistered;
                    break;
            }

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }

    }
}

using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ServiceCentreClientApp.Tools
{
    public static class ControlsInteraction
    {
        public static void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(results, current);
            }
        }
        /// <summary>
        /// Отключает все элеметны управления на переданном объекте.
        /// </summary>
        /// <param name="dependencyObject"></param>
        public static void DisableControls(DependencyObject dependencyObject)
        {
            var controls = new List<Control>();
            ControlsInteraction.FindChildren(controls, dependencyObject);

            foreach (var control in controls)
            {
                control.IsEnabled = false;
            }
        }
        /// <summary>
        /// Включает все элеметны управления на переданном объекте.
        /// </summary>
        /// <param name="dependencyObject"></param>
        public static void EnableControls(DependencyObject dependencyObject)
        {
            var controls = new List<Control>();
            ControlsInteraction.FindChildren(controls, dependencyObject);

            foreach (var control in controls)
            {
                control.IsEnabled = true;
            }
        }
    }
}

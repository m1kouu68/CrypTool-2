using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace CrypTool.Plugins.Anonymity
{
    public static class CollectionUpdate
    {

        // order all elements by their left position 
        public static void ReorderElementsByLeftPosition(this UIElementCollection collection)
        {
            var orderedElements = collection.OfType<UIElement>()
                .OrderBy(element => Canvas.GetLeft(element))
                .ToList();



            for (int i = 0; i < orderedElements.Count; i++)
            {
                collection.Remove(orderedElements[i]);
                collection.Insert(i, orderedElements[i]);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Search;
using Object = UnityEngine.Object;

namespace Bewildered.SmartLibrary
{
    /// <summary>
    /// A smart <see cref="LibraryCollection"/> that automatically adds and removes assets based on the rules and folders.
    /// </summary>
    public class SmartCollection : LibraryCollection
    {
        [NonSerialized] private SearchContext _context;
        [NonSerialized] private bool _isSearching;

        [SerializeField] private List<FolderReference> _folders = new List<FolderReference>();
        
        /// <summary>
        /// Folders to search in for valid assets to add to the <see cref="SmartCollection"/>, or to exclude.
        /// </summary>
        public ICollection<FolderReference> Folders
        {
            get { return _folders; }
        }
        /// <summary>
        /// Determines whether the <see cref="SmartCollection"/> is currently searching for assets that match its <see cref="RuleSet"/>.
        /// </summary>
        public bool IsSearching
        {
            get { return _isSearching; }
        }

        public override void UpdateItems()
        {
            // Re-search for all assets.
            
            _context?.Dispose();
            _isSearching = true;

            // Important! We have to move this out to a static class because for some reason,
            // having a #if UNITY_2021_3_OR_NEWER in the method would cause the collection to serialize setting the `m_EditorClassIdentifier`
            // and could not deseialize properly.
            string query = SmartCollectionUtility.GetQuery(_folders, Rules);
           
            if (EditorPrefs.GetBool("DeveloperMode", false))
            {
                Debug.Log("SmartLibrary Query: " + query);
            }
            
            _context = SearchService.CreateContext(query);
            SearchService.Request(_context, OnSearchComplete);
        }

        public override bool IsAddable(LibraryItem item)
        {
            return base.IsAddable(item) && MatchesRequiredFolders(item.AssetPath) && !string.IsNullOrWhiteSpace(Rules.GetSearchQuery());
        }

        /// <summary>
        /// Updates the <see cref="SmartCollection"/> with the specified <see cref="LibraryItem"/>,
        /// either adding it if it is valid and does not arleady contain it, or remove it if it is invalid and contains it.
        /// </summary>
        /// <param name="item">The <see cref="LibraryItem"/> to try to update.</param>
        /// <returns><c>true</c> of <paramref name="item"/> was added or removed from the <see cref="SmartCollection"/>; otherwise, <c>false</c>.</returns>
        internal bool IncrementalUpdateItem(LibraryItem item)
        {
            if (Rules.Count == 0 && _folders.Count == 0)
                return false;
            
            // We don't use IsAddable() because it checks if the collection contains the item.
            if (Rules.Evaluate(item) && MatchesRequiredFolders(item.AssetPath))
            {
                return AddItem(item);
            }
            else
            {
                return RemoveItem(item);
            }
        }

        private void OnSearchComplete(SearchContext context, IList<SearchItem> items)
        {
            _isSearching = false;
            
            // To improve performance, we don't compare to the current items, we simply notify that all items have changed.
            ClearItems();
            var addedItems = new HashSet<LibraryItem>();

            foreach (SearchItem item in items)
            {
                Object obj = item.ToObject();

                // QuickSearch does not remove entries of deleted assets right away.
                // So we need to handle when the SearchItem may not have an asset associated with it any more.
                if (obj == null)
                    continue;
                
                LibraryItem libraryItem = LibraryItem.GetItemInstance(obj);

                // Item is added to this second list to be used when sending the items changed notification.
                addedItems.Add(libraryItem);
                // Now that the item has been fully validated it can be added to the collection.
                AddItem(libraryItem, false);
            }
            
            NotifyItemsChanged(addedItems, LibraryItemsChangeType.Added);
        }

        private bool MatchesRequiredFolders(string path)
        {
            if (_folders.Count == 0)
                return true;
            
            foreach (var folder in _folders)
            {
                bool isValidPath = folder.IsValidPath(path);
                
                // We return if the folder is exclude and the path is in the folder. A.k.a. it is an excluded path.
                if (!folder.DoInclude && !isValidPath)
                    return false;
                
                // We return if the folder is include and the path is in the folder. A.k.a. it is an included path.
                if (folder.DoInclude && isValidPath)
                    return true;
            }

            return false;
        }
    }
}
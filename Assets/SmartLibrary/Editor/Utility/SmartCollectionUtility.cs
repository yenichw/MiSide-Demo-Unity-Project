using System.Collections.Generic;

namespace Bewildered.SmartLibrary
{
    internal static class SmartCollectionUtility
    {
        public static string GetQuery(List<FolderReference> folders, RuleSet rules)
        {
            string query = string.Empty;
            string folderQuery = GetFolderSearchQuery(folders);
            string ruleQuery = rules.GetSearchQuery();

            bool hasFolderQuery = !string.IsNullOrEmpty(folderQuery);
            bool hasRuleQuery = !string.IsNullOrEmpty(ruleQuery);

            if (hasFolderQuery && hasRuleQuery)
                query = $"p: ({folderQuery}) and ({ruleQuery})";
            else if (hasFolderQuery)
                query = $"p: {folderQuery}";
            else if (hasRuleQuery)
                query = $"p: {ruleQuery}";

            return query;
        }
        
        private static string GetFolderSearchQuery(List<FolderReference> folders)
        {
            string includeQuery = string.Empty;
            string excludeQuery = string.Empty;

            foreach (FolderReference folder in folders)
            {
                // We skip folder references that either have are not assigned a folder or is invalid.
                if (string.IsNullOrEmpty(folder.Path))
                    continue;

                // We handle the include an exclude separately as it makes it easier to combine them.
                if (folder.DoInclude)
                    includeQuery = ModifyFolderQuery(includeQuery, folder, "or");
                else
                    excludeQuery = ModifyFolderQuery(excludeQuery, folder, "and");
            }

            string query = string.Empty;

            bool hasIncludeQuery = !string.IsNullOrEmpty(includeQuery);
            bool hasExcludeQuery = !string.IsNullOrEmpty(excludeQuery);

            if (hasIncludeQuery && hasExcludeQuery)
                query = $"({includeQuery}) and ({excludeQuery})";
            else if (hasIncludeQuery)
                query = $"{includeQuery}";
            else if (hasExcludeQuery)
                query = $"assets/ and ({excludeQuery})"; // Gives incorrect results if only has exclude paths.

            return query;
        }

        private static string ModifyFolderQuery(string currentQuery, FolderReference folder, string operand)
        {
            // We only include the operand if there are already other paths in the query.
            // Otherwise it would show as " and some/folder/" instead of "some/folder/".
            if (!string.IsNullOrEmpty(currentQuery))
                currentQuery += $" {operand} "; // Add spaces so it is not combined with the paths.

            // We add the prefix before the '"' otherwise it would be
            // evaluated as part of the path instead of an exclude indicator.
            if (!folder.DoInclude)
                currentQuery += "-";
            
            // TODO: Remove once Unity re-implements path filtering for nested paths.
#if UNITY_2021_3_OR_NEWER 
            // We surround the path with '"' so it is read as a single 'absolute' string,
            // otherwise paths with dashes or spaces would cause them to be evaluated separately.
            currentQuery += $"dir:{'"'}{folder.Path}{'"'}";
#else
            // We surround the path with '"' so it is read as a single 'absolute' string,
            // otherwise paths with dashes or spaces would cause them to be evaluated separately.
            currentQuery += $"{'"'}{folder.Path}/";

            if (folder.MatchOption == FolderMatchOption.TopOnly)
                currentQuery += "[^/]+$"; // Regex that only assets that are directly in the folder match.

            // Add the closing '"', not added with the other one so that if the match is TopOnly, it will be between them.
            currentQuery += '"';
#endif
            
          

            return currentQuery;
        }
    }
}
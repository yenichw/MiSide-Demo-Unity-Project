/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public class ContextMenuManager : StandalonePrefManager<ContextMenuManager>, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return ContextMenuMainManager.GetKeywords()
                        .Concat(ActionsManager.GetKeywords())
                        .Concat(BreadcrumbsManager.GetKeywords())
                        .Concat(PopupWindowManager.GetKeywords());
                }
            }

            public override void Draw()
            {
                ActionsManager.Draw(null);
                BreadcrumbsManager.Draw(null);
                ContextMenuMainManager.Draw(null);
                PopupWindowManager.Draw(null);
            }

            public string GetMenuName()
            {
                return "Context Menu";
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                GetManager<ActionsManager>().SetState(state);
                GetManager<BreadcrumbsManager>().SetState(state);
                GetManager<ContextMenuMainManager>().SetState(state);
                GetManager<PopupWindowManager>().SetState(state);
            }
        }
    }
}
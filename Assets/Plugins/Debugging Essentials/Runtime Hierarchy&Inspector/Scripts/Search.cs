using System;
using System.Reflection;

namespace DebuggingEssentials
{
    public enum SearchMode { Name, Type };
    public enum SearchCondition { Or, And };

    [Serializable]
    public class Search
    {
        public SearchMode mode = SearchMode.Name;
        public SearchCondition condition = SearchCondition.Or;
        public string text = string.Empty;
        public bool useSearch = true;
        public bool hasSearch;

        [NonSerialized] public FastList<Type> types = new FastList<Type>();

        public void MakeArrayTypes()
        {
            for (int i = 0; i < types.Count; i++)
            {
                if (types.items[i] != null) types.items[i].MakeArrayType();
            }
        }
    }

    public struct SearchMember
    {
        public object parent;
        public MemberType memberType;
        public object obj;
        public Type objType;
        public MemberInfo member;
        public RuntimeInspector.DrawInfo info;
        public int level;

        public SearchMember(object parent, object obj, Type objType, MemberInfo member, MemberType memberType, RuntimeInspector.DrawInfo info, int level)
        {
            this.parent = parent;
            this.obj = obj;
            this.objType = objType;
            this.member = member;
            this.memberType = memberType;
            this.info = info;
            this.level = level;
        }
    }
}


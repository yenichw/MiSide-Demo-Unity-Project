using System.Reflection;

namespace DebuggingEssentials
{
    public class EditField
    {
        public MemberInfo parentMember = null;
        public MemberInfo member = null;
        public int arrayIndex = -1;

        public void Reset()
        {
            parentMember = null;
            member = null;
            arrayIndex = -1;
        }

        public bool IsThisEdit(MemberInfo parentMember, MemberInfo member, int arrayIndex)
        {
            return (this.parentMember == parentMember && this.member == member && this.arrayIndex == arrayIndex);
        }

        public void Set(MemberInfo parentMember, MemberInfo member, int arrayIndex = -1)
        {
            this.parentMember = parentMember;
            this.member = member;
            this.arrayIndex = arrayIndex;
        }
    }
}
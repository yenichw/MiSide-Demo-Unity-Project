using System;
using System.Reflection;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum MemberType { Field, Property, Method, ArrayElement }
    public enum Scope { Public, Protected, Private }

    public class SubTypeData
    {
        public Type type;
        public string typeName;
        public int index;

        public FieldInfo[] fields;
        public PropertyInfo[] properties;
        public MethodInfo[] methods;

        public SubTypeData(Type type, int index, BindingFlags bindingFlags)
        {
            this.type = type;
            this.index = index;
            typeName = "(" + type.Name + ")";

            fields = type.GetFields(bindingFlags);
            properties = type.GetProperties(bindingFlags);
            methods = type.GetMethods(bindingFlags);
        }
    }

    public class TypeData
    {
        public FastList<SubTypeData> subTypeDatas = new FastList<SubTypeData>();
        
        public bool isString;
        public bool isClass;
        public bool isStruct;
        public bool isArray;
        public bool isInterface;

        public TypeData(Type type, BindingFlags bindingFlags)
        {
            bindingFlags |= BindingFlags.DeclaredOnly;

            GetMembers(type, bindingFlags);

            isString = (type == typeof(string));
            isClass = type.IsClass;
            isStruct = type.IsValueType && !type.IsPrimitive && !type.IsEnum;
            isArray = type.IsArray;
            isInterface = type.IsInterface;
        }

        public void GetMembers(Type type, BindingFlags bindingFlags)
        {
            int index = 0;
            do
            {
                subTypeDatas.Add(new SubTypeData(type, index++, bindingFlags));

                type = type.BaseType;
            }
            while (type != null);
        }
    }

    public class MemberData
    {
        public MemberType memberType;

        public MemberInfo member;
        public FieldInfo field;
        public PropertyInfo prop;
        public MethodInfo method;
        public ParameterInfo[] parameters;

        public bool validInvokeParameters;
        public bool isStatic;
        public bool isConstant;
        public bool isString;
        public bool isClass;
        public bool isStruct;
        public bool isArray;
        public bool isInterface;

        public Type type;
        public Scope scope;

        public string name;
        public string typeName;
        public string scopeToolTip;
        public Color scopeColor;

        public MemberData(Type objType, MemberInfo member, MemberType memberType, Color colorPublic, Color colorProtected, Color colorPrivate)
        {
            this.member = member;
            this.memberType = memberType;

            if (memberType == MemberType.Field)
            {
                field = (FieldInfo)member;
                name = field.Name;
                type = field.FieldType;
                isStatic = field.IsStatic;
                isConstant = field.IsLiteral;
                if (field.IsPublic) scope = Scope.Public;
                else if (field.IsFamily) scope = Scope.Protected;
                else if (field.IsPrivate) scope = Scope.Private;
            }
            else if (memberType == MemberType.Property)
            {
                prop = (PropertyInfo)member;
                name = prop.Name;
                type = prop.PropertyType;

                method = prop.GetGetMethod(true);
                if (method == null) method = prop.GetSetMethod(true);

                if (method != null)
                {
                    isStatic = method.IsStatic;

                    if (method.IsPublic) scope = Scope.Public;
                    else if (method.IsPrivate) scope = Scope.Private;
                    else if (method.IsFamily) scope = Scope.Protected;
                }
            }
            else
            {
                method = (MethodInfo)member;
                parameters = method.GetParameters();
                validInvokeParameters = RuntimeConsole.ValidParams(type, method, parameters, false);
                name = method.ToString();
                type = method.DeclaringType;
                isStatic = method.IsStatic;
                if (method.IsPublic) scope = Scope.Public;
                else if (method.IsPrivate) scope = Scope.Private;
                else if (method.IsFamily) scope = Scope.Protected;
            }

            isString = (type == typeof(string));
            isClass = type.IsClass;
            isStruct = type.IsValueType && !type.IsPrimitive && !type.IsEnum;
            isArray = type.IsArray;
            isInterface = type.IsInterface;

            typeName = "(" + type.Name + ")";

            if (scope == Scope.Public) { scopeColor = colorPublic; scopeToolTip = "Is Public"; }
            else if (scope == Scope.Protected) { scopeColor = colorProtected; scopeToolTip = "Is Protected"; }
            else { scopeColor = colorPrivate; scopeToolTip = "Is Private"; }
        }
    }

}
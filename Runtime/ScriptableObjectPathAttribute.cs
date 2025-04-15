using System;

namespace DreadZitoTools.ScriptableLovers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ScriptableObjectPathAttribute : Attribute
    {
        public string Path { get; }

        public ScriptableObjectPathAttribute(string path)
        {
            Path = path;
        }
    }
}
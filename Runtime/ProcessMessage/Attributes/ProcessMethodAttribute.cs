using System;

// This attribute can only be placed on fields (variables) inside a class.
[AttributeUsage(AttributeTargets.Method)]
public class ProcessMethodAttribute : Attribute
{
    public string FunctionID { get; }

    public ProcessMethodAttribute(string functionID)
    {
        FunctionID = functionID;
    }
}
using System;

// This attribute can only be placed on fields (variables) inside a class.
[AttributeUsage(AttributeTargets.Field)]
public class ProcessVariableAttribute : Attribute
{ 
    public string VariableID { get; }

    public ProcessVariableAttribute(string variableID)
    {
        VariableID = variableID;
    }
}
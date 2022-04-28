using System.Collections.Generic;

public class OperationTest
{
    public string operation;
    public List<bool> result;
    public string part;

    public OperationTest(string operation, string part, List<bool> result)
    {
        //this.shapeName = null;
        this.operation = operation;
        this.part = part;
        this.result = result;
    }
}

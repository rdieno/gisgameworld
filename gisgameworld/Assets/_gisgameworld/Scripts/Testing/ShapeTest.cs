using System.Collections.Generic;

public class ShapeTest
{
    public string shapeName;
    public List<OperationTest> operationTests;

    public ShapeTest(string shapeName, List<OperationTest> operationTests)
    {
        this.shapeName = shapeName;
        this.operationTests = operationTests;
    }
}

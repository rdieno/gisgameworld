using System.Collections.Generic;

public class ProcessingWrapper
{
    public Dictionary<string, List<Shape>> processsedShapes;
    public List<ShapeTest> testResults;

    public ProcessingWrapper()
    {
        this.processsedShapes = null;
        this.testResults = null;
    }

    public ProcessingWrapper(Dictionary<string, List<Shape>> processsedShapes, List<ShapeTest> testResults)
    {
        this.processsedShapes = processsedShapes;
        this.testResults = testResults;
    }
}

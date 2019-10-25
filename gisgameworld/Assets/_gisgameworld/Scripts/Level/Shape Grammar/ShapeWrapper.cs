using System.Collections.Generic;

public enum ShapeContainerType
{
    Single,
    List,
    Dictionary
}

public class ShapeWrapper
{
    public Shape shape;
    public List<Shape> shapeList;
    public Dictionary<string, List<Shape>> shapeDictionary;
    public ShapeContainerType type;
    public bool removeParentShape;

    public ShapeWrapper(Shape shape, bool removeParentShape = false)
    {
        this.shape = shape;
        type = ShapeContainerType.Single;
        removeParentShape = false;
        this.removeParentShape = removeParentShape;
    }

    public ShapeWrapper(List<Shape> shapeList, bool removeParentShape = false)
    {
        this.shapeList = shapeList;
        type = ShapeContainerType.List;
        removeParentShape = false;
        this.removeParentShape = removeParentShape;
    }

    public ShapeWrapper(Dictionary<string, List<Shape>> shapeDictionary, bool removeParentShape = false)
    {
        this.shapeDictionary = shapeDictionary;
        type = ShapeContainerType.Dictionary;
        this.removeParentShape = removeParentShape;
    }
}

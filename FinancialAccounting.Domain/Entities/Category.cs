using Accounting.Domain.Enums;

namespace Accounting.Domain.Entities;
public class Category
{
    public long Id { get; private set; }
    public CategoryType Type { get; private set; }
    public string Name { get; set; }

    public Category(long id, CategoryType type, string name)
    {
        Id = id;
        Type = type;
        Name = name;
    }

    public override string ToString()
    {
        return $"Категория: Id:{Id}, {Name} ({Type})";
    }
}


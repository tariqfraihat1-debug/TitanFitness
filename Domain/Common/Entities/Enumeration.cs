namespace TitanFitness.Domain.Common.Entities;

public abstract class Enumeration

{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
            return false;

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static IEnumerable<T> GetAll<T>()
        where T : Enumeration
    {
        return typeof(T)
            .GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.DeclaredOnly)
            .Select(field => field.GetValue(null))
            .Cast<T>();
    }
}
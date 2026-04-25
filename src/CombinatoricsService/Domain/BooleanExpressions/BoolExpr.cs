using System.Text.Json.Serialization;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ConstExpr), "const")]
[JsonDerivedType(typeof(BoolVar), "var")]
[JsonDerivedType(typeof(And), "and")]
[JsonDerivedType(typeof(Or), "or")]
[JsonDerivedType(typeof(Not), "not")]
[JsonDerivedType(typeof(Imply), "imply")]
[JsonDerivedType(typeof(Xor), "xor")]
[JsonDerivedType(typeof(Equal), "equal")]
public abstract record BoolExpr
{
    [JsonIgnore]
    public abstract IReadOnlyList<BoolExpr> Children { get; }
    
    [JsonIgnore]
    public abstract int Precedence { get; }

    public abstract IEnumerable<BoolVar> GetVariables();

    protected abstract BoolExpr WithChildren(IReadOnlyList<BoolExpr> children);

    protected abstract int ResolveChildIndex(ChildSelector selector);

    public BoolExpr Replace(Func<BoolExpr, bool> predicate, Func<BoolExpr, BoolExpr> replacement)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(replacement);

        if (predicate(this))
            return replacement(this);

        IReadOnlyList<BoolExpr> children = Children;
        if (children.Count == 0)
            return this;

        BoolExpr[] newChildren = new BoolExpr[children.Count];
        var changed = false;

        for (var i = 0; i < children.Count; i++)
        {
            newChildren[i] = children[i].Replace(predicate, replacement);
            if (!ReferenceEquals(newChildren[i], children[i]))
                changed = true;
        }

        return changed ? WithChildren(newChildren) : this;
    }

    public BoolExpr ReplaceAt(IReadOnlyList<ChildSelector> path, BoolExpr replacement)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(replacement);

        return ReplaceAt(path, 0, replacement);
    }

    public IEnumerable<BoolExpr> DescendantsAndSelf()
    {
        yield return this;

        foreach (BoolExpr child in Children)
        foreach (BoolExpr descendant in child.DescendantsAndSelf())
            yield return descendant;
    }

    private BoolExpr ReplaceAt(IReadOnlyList<ChildSelector> path, int depth, BoolExpr replacement)
    {
        if (depth == path.Count)
            return replacement;

        int index = ResolveChildIndex(path[depth]);
        IReadOnlyList<BoolExpr> children = Children;

        BoolExpr[] newChildren = [.. children];
        newChildren[index] = children[index].ReplaceAt(path, depth + 1, replacement);

        return WithChildren(newChildren);
    }
}
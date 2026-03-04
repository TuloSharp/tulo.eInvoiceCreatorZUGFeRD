using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using tulo.CoreLib.Interfaces.SnapShots;

namespace tulo.CoreLib.Services;

public class SnapShotService : ISnapShotService
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    // ---------------------------------------
    // Deep copy (ALWAYS returns non-null)
    // ---------------------------------------
    public T TryDeepCopy<T>(T? source) where T : class, new()
    {
        if (source is null)
            return CreateEmpty<T>();

        try
        {
            var json = JsonSerializer.Serialize(source, _jsonOpts);
            var copy = JsonSerializer.Deserialize<T>(json, _jsonOpts);

            return copy ?? CreateEmpty<T>();
        }
        catch
        {
            return CreateEmpty<T>();
        }
    }

    // ---------------------------------------
    // Snapshot helpers (never throw)
    // ---------------------------------------
    public string TryTakeSnapshot<T>(T? obj) where T : class
    {
        if (obj is null) return string.Empty;

        try
        {
            return JsonSerializer.Serialize(obj, _jsonOpts);
        }
        catch
        {
            return string.Empty;
        }
    }

    public T TryRestoreSnapshot<T>(string? snapshot) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(snapshot))
            return CreateEmpty<T>();

        try
        {
            var obj = JsonSerializer.Deserialize<T>(snapshot, _jsonOpts);
            return obj ?? CreateEmpty<T>();
        }
        catch
        {
            return CreateEmpty<T>();
        }
    }

    // ---------------------------------------
    // CopyBack (never throw)
    // ---------------------------------------
    public bool TryCopyBackAll<T>(T target, T src) where T : class =>
        TryCopyBackInternal(target, src, include: null, exclude: null);

    public bool TryCopyBackInclude<T>(T target, T src, params Expression<Func<T, object>>[] include) where T : class =>
        TryCopyBackInternal(target, src, include, exclude: null);

    public bool TryCopyBackExclude<T>(T target, T src, params Expression<Func<T, object>>[] exclude) where T : class =>
        TryCopyBackInternal(target, src, include: null, exclude);

    private static bool TryCopyBackInternal<T>(
        T target,
        T src,
        Expression<Func<T, object>>[]? include,
        Expression<Func<T, object>>[]? exclude) where T : class
    {
        if (target is null || src is null) return false;

        try
        {
            var includeNames = include is null ? null : BuildNameSet(include);
            if (include is not null && includeNames is null) return false;

            var excludeNames = exclude is null ? null : BuildNameSet(exclude);
            if (exclude is not null && excludeNames is null) return false;

            var props = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var p in props)
            {
                if (includeNames?.Contains(p.Name) == false) continue;
                if (excludeNames?.Contains(p.Name) == true) continue;

                var value = p.GetValue(src);
                p.SetValue(target, value);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static System.Collections.Generic.HashSet<string>? BuildNameSet<T>(Expression<Func<T, object>>[] exprs)
    {
        try
        {
            var set = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var e in exprs)
            {
                if (!TryGetMemberName(e, out var name) || string.IsNullOrWhiteSpace(name))
                    return null;

                set.Add(name);
            }

            return set;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetMemberName<T>(Expression<Func<T, object>> expr, out string name)
    {
        name = string.Empty;

        try
        {
            if (expr.Body is MemberExpression m)
            {
                name = m.Member.Name;
                return true;
            }

            if (expr.Body is UnaryExpression u && u.Operand is MemberExpression um)
            {
                name = um.Member.Name;
                return true;
            }

            return false;
        }
        catch
        {
            name = string.Empty;
            return false;
        }
    }

    // ---------------------------------------
    // Create empty instance + normalize strings
    // ---------------------------------------
    private static T CreateEmpty<T>() where T : class, new()
    {
        var obj = new T();

        // Make all string props non-null (""), so callers don’t need extra null checks.
        try
        {
            var stringProps = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite && p.PropertyType == typeof(string));

            foreach (var p in stringProps)
                p.SetValue(obj, string.Empty);
        }
        catch
        {
            // swallow (never throw to caller)
        }

        return obj;
    }
}
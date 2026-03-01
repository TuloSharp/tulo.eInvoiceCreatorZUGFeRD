using System.Linq.Expressions;

namespace tulo.CoreLib.Interfaces.SnapShots;

public interface ISnapShotService
{
    // Always returns a non-null instance (empty object on failure or null source)
    T TryDeepCopy<T>(T? source) where T : class, new();

    // Snapshot helpers (never throw)
    string TryTakeSnapshot<T>(T? obj) where T : class;
    T TryRestoreSnapshot<T>(string? snapshot) where T : class, new();

    // CopyBack helpers (never throw; return false on failure)
    bool TryCopyBackAll<T>(T target, T src) where T : class;
    bool TryCopyBackInclude<T>(T target, T src, params Expression<Func<T, object>>[] include) where T : class;
    bool TryCopyBackExclude<T>(T target, T src, params Expression<Func<T, object>>[] exclude) where T : class;
}

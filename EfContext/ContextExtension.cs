using ODataWebserver.Global;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODataWebserver.EfContext
{
    public static class ContextExtension
    {
        #region Get / Create

        public static T GetSingleById<T>(this DbContext c, int id)
            where T : class, IModel, new()
            => c.Set<T>().Where(x => x.Id == id).SingleOrDefault();

        public static bool AddIfNotExists<T>(this DbContext context, T obj, Func<DbSet<T>, T, IQueryable<T>> checkIfExists, bool save = true)
            where T : class, IModel, new()
        {
            var set = context.Set<T>();
            var results = checkIfExists(set, obj);

            if (!results.Any())
            {
                context.Persist(obj, save, false);
                return true;
            }
            return false;
        }

        public static bool AddIfNotExistsWithoutSaving<T>(this DbContext context, T obj, Func<DbSet<T>, T, IQueryable<T>> checkIfExists)
           where T : class, IModel, new()
        {
            return AddIfNotExists(context, obj, checkIfExists, false);
        }

        public static T GetOrCreateOne<T>(this DbContext context, T obj, Func<DbSet<T>, T, IQueryable<T>> checkIfExists)
            where T : class, IModel, new()
        {
            var set = context.Set<T>();
            var results = checkIfExists(set, obj);

            if (!results.Any())
            {
                return context.PersistAndSave(obj);
            }

            return results.Single();
        }

        #endregion

        #region Persist

        public static T PersistAndSave<T>(this DbContext context, T obj)
            where T : class, IModel, new() => context.Persist(obj, true, false);

        public static T PersistNoSave<T>(this DbContext context, T obj)
            where T : class, IModel, new() => context.Persist(obj, false, false);


        public static T PersistAndSaveAsync<T>(this DbContext context, T obj)
            where T : class, IModel, new() => context.Persist(obj, true, true);

        public static T PersistNoSaveAsync<T>(this DbContext context, T obj)
            where T : class, IModel, new() => context.Persist(obj, false, true);

        private static T Persist<T>(this DbContext context, T obj, bool save, bool async)
            where T : class, IModel, new()
        {
            var isNew = obj.Id == 0;

            if (isNew)
            {
                var set = context.Set<T>();
                obj.CreatedUtc = DateTime.UtcNow;
                obj = set.Add(obj).Entity;
            }

            obj.LastChangeUtc = DateTime.UtcNow;

            if (save)
            {
                if (async) context.SaveChangesAsync();
                else context.SaveChanges();
            }

            return obj;
        }

        #endregion

        #region Remove

        public static void RemoveSingleById<T>(this DbContext c, bool save, int id)
            where T : class, IModel, new()
        {
            c.Set<T>().Remove(c.GetSingleById<T>(id));

            if (save) c.SaveChanges();
        }

        public static void RemoveByIds<T>(this DbContext c, bool save, params int[] ids)
            where T : class, IModel, new()
        {
            if (ids.IsNotNullOrEmpty())
            {
                c.Set<T>().RemoveRange(c.Set<T>().Where(x => ids.Contains(x.Id)));
                if (save) c.SaveChanges();
            }
        }

        public static void RemoveAndSave<T>(this DbContext c, T obj, bool async = false)
            where T : class, IModel, new()
        {
            c.Set<T>().Remove(obj);
            if (async) c.SaveChangesAsync();
            else c.SaveChanges();
        }

        #endregion

        #region Transaction and Collection Helper

        public static void WithTransaction(this DbContext c, Action<IDbContextTransaction> withTransaction, SaveAfterTransaction save)
        {
            using (var transaction = c.Database.BeginTransaction())
            {
                try
                {
                    withTransaction(transaction);

                    if (save.Equals(SaveAfterTransaction.Yes))
                    {
                        c.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static IEnumerable<T> WithSaveAfterCount<T>(this IEnumerable<T> enumerable, DbContext context, int interval, bool ignoreCommitFail = false)
        {
            var i = 0;

            var isEfModel = enumerable.FirstOrDefault() is IModel;

            foreach (var item in enumerable)
            {
                yield return item;

                if (isEfModel) (item as IModel).LastChangeUtc = DateTime.UtcNow;

                if (++i % interval == 0)
                {
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {
                        if (!ignoreCommitFail) throw;
                    }
                }
            }

            if (i % interval != 0)
            {
                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    if (!ignoreCommitFail) throw;
                }
            }
        }

        #endregion


        #region Post-Include Functions with Conditions

        /* Includes (insb. für Childentitäten) sind manchmal kritisch, da im Hintergrund ein Inner-Join gebildet wird.
         * Weiterhin ist es fraglich, ob nachträglich noch effizient included werden kann (Eager Loading) nachdem die Query
         * schon ausgeführt wurde.
         * 
         * Aus diesem Grund werden folgende Methoden bereitgestellt. Hiermit lässt sich nachträglich noch kurz und bündig mit der Hilfe von Reflection includen.
         */

        /// <summary>
        /// Wird auf ein Parent-Objekt angewendet: Lädt die dazugehörigen Children anhand des Joinproperties.
        /// </summary>
        /// <typeparam name="TParent">Parent-Typ</typeparam>
        /// <typeparam name="TChild">Child-Typ</typeparam>
        /// <param name="joinIdPropertyName">Name des Properties in der Child-Entität. Immer den Integer-Fremdschlüssel angeben! Z.B. Übergebbar als nameof(Child.JoinId)</param>
        public static ICollection<TChild> IncludeChildren<TParent, TChild>(this TParent obj, DbContext context, string joinIdPropertyName,
                                                                                             Func<IQueryable<TChild>, ICollection<TChild>> additionalConditions = null)
            where TParent : class, IModel, new()
            where TChild : class, IModel, new()
        {
            var query = GetQueryForSpecialInclude<TChild, TParent, TChild>(context, joinIdPropertyName, false, new[] { obj }, null);
            return additionalConditions != null ? additionalConditions(query) : query.ToList();
        }

        /// <summary>
        /// Wird auf eine Parent-Collection angewendet: Lädt die dazugehörigen Children für jedes Element anhand des Joinproperties.
        /// </summary>
        /// <typeparam name="TParent">Parent-Typ</typeparam>
        /// <typeparam name="TChild">Child-Typ</typeparam>
        /// <param name="joinIdPropertyName">Name des Properties in der Child-Entität. Immer den Integer-Fremdschlüssel angeben! Z.B. Übergebbar als nameof(Child.JoinId)</param>
        public static ICollection<TChild> IncludeChildren<TParent, TChild>(this ICollection<TParent> objs, DbContext context, string joinIdPropertyName,
                                                                                             Func<IQueryable<TChild>, ICollection<TChild>> additionalConditions = null)
            where TParent : class, IModel, new()
            where TChild : class, IModel, new()
        {
            var query = GetQueryForSpecialInclude<TChild, TParent, TChild>(context, joinIdPropertyName, false, objs.ToArray(), null);
            return additionalConditions != null ? additionalConditions(query) : query.ToList();
        }

        /// <summary>
        /// Wird auf ein Child-Objekt angewendet: Lädt das dazugehörige Parent anhand des Joinproperties.
        /// </summary>
        /// <typeparam name="TParent">Parent-Typ</typeparam>
        /// <typeparam name="TChild">Child-Typ</typeparam>
        /// <param name="joinIdPropertyName">Name des Properties in der Child-Entität. Immer den Integer-Fremdschlüssel angeben! Z.B. Übergebbar als nameof(Child.JoinId)</param>
        public static TParent IncludeParent<TParent, TChild>(this TChild child, DbContext context, string joinIdPropertyName)
            where TParent : class, IModel, new()
            where TChild : class, IModel, new()
        {
            return GetQueryForSpecialInclude<TChild, TParent, TParent>(context, joinIdPropertyName, true, null, new[] { child }).SingleOrDefault();
        }



        /// <summary>
        /// Wird auf eine Child-Collection angewendet: Lädt die dazugehörigen Parents für jedes Element anhand des Joinproperties.
        /// </summary>
        /// <typeparam name="TParent">Parent-Typ</typeparam>
        /// <typeparam name="TChild">Child-Typ</typeparam>
        /// <param name="joinIdPropertyName">Name des Properties in der Child-Entität. Immer den Integer-Fremdschlüssel angeben! Z.B. Übergebbar als nameof(Child.JoinId)</param>
        public static ICollection<TParent> IncludeParents<TParent, TChild>(this ICollection<TChild> children, DbContext context, string joinIdPropertyName)
            where TParent : class, IModel, new()
            where TChild : class, IModel, new()
        {
            return GetQueryForSpecialInclude<TChild, TParent, TParent>(context, joinIdPropertyName, true, null, children.ToArray()).ToList();
        }

        private static IQueryable<TResult> GetQueryForSpecialInclude<TChild, TParent, TResult>(DbContext context, string joinPropertyName, bool childToParent,
                                                                                                TParent[] parents, TChild[] children)
            where TParent : class, IModel, new()
            where TChild : class, IModel, new()
        {
            var joinProperty = typeof(TChild).GetProperty(joinPropertyName);

            if (childToParent)
            {
                var set = context.Set<TParent>();

                var joinIds = children.Select(x => joinProperty.GetValue(x, null));

                var query = set.Where(x => joinIds.Contains(x.Id));
                return query as IQueryable<TResult>;
            }
            else
            {
                var set = context.Set<TChild>();

                var joinIds = parents.Select(x => x.Id).ToList();

                var query = set.Where(x => joinIds.Contains((int)joinProperty.GetValue(x, null)));
                return query as IQueryable<TResult>;
            }
        }

        #endregion
    }
}

﻿using Ophelia.Data.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Data
{
    public static class ModelExtensions
    {
        public static bool IsDataEntity(this Type type)
        {
            return type.IsSubclassOf(typeof(Model.DataEntity));
        }
        public static bool IsPOCOEntity(this Type type)
        {
            return type.BaseType != null && type.BaseType.Name == "DataEntity";
        }
        public static bool IsQueryableDataSet(this Type type)
        {
            return type.IsSubclassOf(typeof(Model.QueryableDataSet)) || typeof(Model.QueryableDataSet).IsAssignableFrom(type);
        }
        public static bool IsQueryable(this Type type)
        {
            return type.Name.ToLowerInvariant() != "string" && (type.IsSubclassOf(typeof(IQueryable)) || typeof(IEnumerable).IsAssignableFrom(type));
        }
        public static TResult GetValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, TResult>> property, object defaultValue = null)
            where TEntity : Model.DataEntity
        {
            var p = (property.Body as MemberExpression).Member as PropertyInfo;
            if (p.PropertyType.IsDataEntity())
            {
                return entity.GetEntityValue(property);
            }
            else if (p.PropertyType.IsEnumarable())
            {
                return entity.GetCollectionValue(property);
            }
            if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var value = entity.InternalTracker.GetValue(p, defaultValue);
                if (value == null)
                    return default(TResult);

                return (TResult)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(TResult)));
            }
            return (TResult)Convert.ChangeType(entity.InternalTracker.GetValue(p, defaultValue), typeof(TResult));
        }

        public static Model.QueryableDataSet<TResult> GetValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, Model.QueryableDataSet<TResult>>> property)
            where TEntity : Model.DataEntity
            where TResult : class
        {
            return entity.GetValue(property, null);
        }

        public static Model.QueryableDataSet<TResult> GetValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, Model.QueryableDataSet<TResult>>> property, Expression<Func<TResult, bool>> predicate)
            where TEntity : Model.DataEntity
            where TResult : class
        {
            return entity.InternalTracker.GetDataSetValue((property.Body as MemberExpression).Member as PropertyInfo, predicate);
        }

        public static TResult GetEntityValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, TResult>> property) where TEntity : Model.DataEntity
        {
            return entity.InternalTracker.GetEntityValue<TResult>((property.Body as MemberExpression).Member as PropertyInfo);
        }
        public static TResult GetCollectionValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, TResult>> property) where TEntity : Model.DataEntity
        {
            return (TResult)entity.InternalTracker.GetCollectionValue<TResult>((property.Body as MemberExpression).Member as PropertyInfo);
        }
        public static void Load<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> property) where TEntity : Model.DataEntity
        {
            var p = (property.Body as MemberExpression).Member as PropertyInfo;
            entity.InternalTracker.LoadAnyway = true;
            p.GetValue(entity);
            entity.InternalTracker.LoadAnyway = false;
        }

        public static void Load<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, Model.QueryableDataSet<TResult>>> property) where TEntity : Model.DataEntity
            where TResult : class
        {
            var p = (property.Body as MemberExpression).Member as PropertyInfo;
            entity.InternalTracker.LoadAnyway = true;
            p.GetValue(entity);
            entity.InternalTracker.LoadAnyway = false;
        }

        public static void SetValue<TEntity, TResult>(this TEntity entity, Expression<Func<TEntity, TResult>> property, object Value) where TEntity : Model.DataEntity
        {
            entity.InternalTracker.SetValue((property.Body as MemberExpression).Member as PropertyInfo, Value);
        }

        public static IEnumerable CreateDataSet(this Type entityType)
        {
            var types = new Type[] { entityType };
            Type constructedType = typeof(Model.QueryableDataSet<>).MakeGenericType(types);
            try
            {
                return (Model.QueryableDataSet)Activator.CreateInstance(constructedType, DataContext.Current);
            }
            catch (Exception)
            {
                return entityType.CreateList();
            }
        }
        public static Array ToArray(this IList list)
        {
            var array = new Array[list.Count];
            list.CopyTo(array, 0);
            return array;
        }
        public static IEnumerable CreateList(this Type entityType)
        {
            var listType = typeof(List<>).MakeGenericType(entityType);
            return (IEnumerable)Activator.CreateInstance(listType);
        }
        public static Model.DataEntity CreateEntity(this Type entityType, long ID = 0)
        {
            if (entityType.IsSubclassOf(typeof(Model.DataEntity)))
            {
                var entity = (Model.DataEntity)Activator.CreateInstance(entityType);
                entity.ID = ID;
                return entity;
            }
            throw new InvalidCastException(entityType.FullName + " can not cast to Ophelia.Data.Model.DataEntity");
        }
        public static bool SaveChanges<T>(this DataContext ctx, T entity) where T : DataEntity
        {
            var datasource = ctx.GetRepository<T>();
            return datasource.SaveChanges(entity);
        }
    }
}

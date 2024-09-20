﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NLog;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Common.Helpers
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class EnumHelpers
    {
        /// <summary>
        /// Gets the description attribute from an enum.  
        /// Returns the enum.ToString() when no description found.
        /// </summary>
        /// <param name="enumToGetDescriptionFrom">The enum to retrieve the description from.</param>
        /// <returns></returns>
        public static string GetEnumDescriptionFromEnum(Enum enumToGetDescriptionFrom)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                FieldInfo fi = enumToGetDescriptionFrom.GetType().GetField(enumToGetDescriptionFrom.ToString());
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                EnumMemberAttribute[] attributes =
                    (EnumMemberAttribute[])fi.GetCustomAttributes(
                        typeof(EnumMemberAttribute),
                        false);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (attributes != null &&
                    attributes.Length > 0)
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return attributes[0].Value;
#pragma warning restore CS8603 // Possible null reference return.
                }

                return enumToGetDescriptionFrom.ToString();
            }
            catch (Exception ex)
            {
                ThisLogger.Debug($"Error getting description for enum: {enumToGetDescriptionFrom.GetType()}");
                ThisLogger.Debug(ex);
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Gets the enum of type <see cref="T"/> matching the description.
        /// </summary>
        /// <typeparam name="T">The enum type to return/parse descriptions of.</typeparam>
        /// <param name="enumDescription">The description to search the enum for.</param>
        /// <param name="shouldThrow">Should the method throw if the enum is not found?</param>
        /// <returns></returns>
        public static T GetEnumFromEnumDescription<T>(string enumDescription, bool shouldThrow = true)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException("Type is not an enum");
            }

            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                return (T)typeof(T)
                    .GetFields()
                    .First(
                        f => f.GetCustomAttributes<EnumMemberAttribute>()
                            .Any(a => a.Value.Equals(enumDescription, StringComparison.OrdinalIgnoreCase))
                    )
                    .GetValue(null);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (Exception)
            {
                if (shouldThrow)
                {
                    ThisLogger.Error($"Couldn't find an {typeof(T)} matching {nameof(enumDescription)} of \"{enumDescription}\"");
                    throw;
                }

#pragma warning disable IDE0034 // Simplify 'default' expression
                return default(T);
#pragma warning restore IDE0034 // Simplify 'default' expression
            }
        }

        /// <summary>
        /// Gets the description attributes from an enum type.
        /// If a description is not found for any items in the enum, 
        /// the ToString representation of that item is returned.
        /// </summary>
        /// <typeparam name="T">The enum type to get descriptions from</typeparam>
        /// <returns></returns>
        public static List<string> GetEnumDescriptions<T>()
            where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException("Only Enum types allowed");
            }

            List<string> descriptions = [];
            foreach (var value in Enum.GetValues(type).Cast<Enum>())
            {
                descriptions.Add(GetEnumDescriptionFromEnum(value));
            }

            return descriptions;
        }

        public static List<T> GetEnums<T>()
            where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException("Only Enum types allowed");
            }

            var enums = new List<T>();
            foreach (var value in Enum.GetValues(type).Cast<T>())
            {
                enums.Add(value);
            }

            return enums;
        }

        public static List<T> GetEnumsWithoutDefault<T>()
            where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
            {
                throw new ArgumentException("Only Enum types allowed");
            }

            var enums = new List<T>();
            foreach (var value in Enum.GetValues(type).Cast<T>())
            {
                if (value.Equals(default(T))) continue;

                enums.Add(value);
            }

            return enums;
        }

        private static Logger ThisLogger = LogManager.GetCurrentClassLogger();
    }
}

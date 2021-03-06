﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    class ActorInterfaceMapping : IEquatable<ActorInterfaceMapping>
    {
        public static ActorInterfaceMapping Of(string typeName) => new ActorInterfaceMapping(typeName, null, null);

        public static ActorInterfaceMapping Of(Type type, IEnumerable<Assembly> assemblies)
        {
            var name = ActorTypeName.Of(type);

            Type @interface = null;
            Type @class = null;

            if (type.IsClass)
            {
                @class = type;
                @interface = ActorTypeName.CustomInterface(type);
            }

            if (type.IsInterface)
            {
                var classes = assemblies.SelectMany(a => a.GetTypes()
                    .Where(x => x.IsClass && type.IsAssignableFrom(x)))
                    .ToArray();

                if (classes.Length > 1)
                    throw new InvalidOperationException(
                        $"Custom actor interface [{type.FullName}] is implemented by " +
                        $"multiple classes: {string.Join(" ; ", classes.Select(x => x.ToString()))}");

                @interface = type;
                @class = classes.Length != 0 ? classes[0] : null;
            }

            return new ActorInterfaceMapping(name, @interface, @class);
        }

        public readonly string TypeName;
        public readonly Type CustomInterface;
        public readonly Type ImplementationClass;
        public readonly Type[] Types;
        public readonly string Key;

        ActorInterfaceMapping(string typeName, Type @interface, Type @class)
        {
            TypeName = typeName;
            CustomInterface = @interface;
            ImplementationClass = @class;
            Types = new[]{@interface, @class}.Where(x => x != null).ToArray();
            Key = $"{typeName} -> {string.Join(";", Types.OrderBy(x => x.AssemblyQualifiedName).Select(x => x.AssemblyQualifiedName))}";
        }

        public bool Equals(ActorInterfaceMapping other) => 
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || 
            string.Equals(Key, other.Key));

        public override bool Equals(object obj) => 
            !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
            obj.GetType() == GetType() && Equals((ActorInterfaceMapping) obj));

        public static bool operator ==(ActorInterfaceMapping left, ActorInterfaceMapping right) => Equals(left, right);
        public static bool operator !=(ActorInterfaceMapping left, ActorInterfaceMapping right) => !Equals(left, right);

        public override int GetHashCode() => Key.GetHashCode();
    }
}
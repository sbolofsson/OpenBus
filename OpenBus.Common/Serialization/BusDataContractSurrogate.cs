using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenBus.Common.Serialization
{
    /// <summary>
    /// Class for shifting between regular types and types used for serialization.
    /// Not used at the moment.
    /// http://msdn.microsoft.com/en-us/library/system.runtime.serialization.idatacontractsurrogate.aspx
    /// </summary>
    public class BusDataContractSurrogate : IDataContractSurrogate
    {
        /// <summary>
        /// During serialization, deserialization, and schema import and export, returns a data contract type that substitutes the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Type GetDataContractType(Type type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// During serialization, returns an object that substitutes the specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object GetObjectToSerialize(object obj, Type targetType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// During deserialization, returns an object that is a substitute for the specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object GetDeserializedObject(object obj, Type targetType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// During schema export operations, inserts annotations into the schema for non-null return values.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="dataContractType"></param>
        /// <returns></returns>
        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// During schema export operations, inserts annotations into the schema for non-null return values.
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="dataContractType"></param>
        /// <returns></returns>
        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the collection of known types to use for serialization and deserialization of the custom data objects.
        /// </summary>
        /// <param name="customDataTypes"></param>
        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// During schema import, returns the type referenced by the schema.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="typeNamespace"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the type that has been generated from the imported schema.
        /// </summary>
        /// <param name="typeDeclaration"></param>
        /// <param name="compileUnit"></param>
        /// <returns></returns>
        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            throw new NotImplementedException();
        }
    }
}
